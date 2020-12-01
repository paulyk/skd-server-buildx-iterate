#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SKD.Model {

    public class VehicleSnapshotService {

        private readonly SkdContext context;

        public VehicleSnapshotService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<GenarateSnapshotDTO>> GenerateSnapshot(VehicleSnapshotInput input) {

            var payload = new MutationPayload<GenarateSnapshotDTO>(null);
            payload.Errors = await ValidateGenerateVehicleSnapshot(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            // get qualifying vehicle list
            var query = GetPartnerStatusQualifyingVehiclesQuery(input);
            var qualifyingVehicles = await query
                .Include(t => t.Lot)
                .Include(t => t.Snapshots)
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

            // if no vehicles return with { WasGenerated = False }
            if (qualifyingVehicles.Count == 0) {
                var dto = new GenarateSnapshotDTO {
                    RunDate = input.RunDate.Date,
                    PlantCode = input.PlantCode,
                    WasGenerated = false
                };

                payload.Entity = dto;
                return payload;
            }

            // create entity
            var vehicleSnapshotRun = new VehicleSnapshotRun {
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                RunDate = input.RunDate.Date,
                Sequence = await context.VehicleSnapshotRuns
                    .Where(t => t.Plant.Code == input.PlantCode)
                    .OrderByDescending(t => t.Sequence)
                    .Select(t => t.Sequence)
                    .FirstOrDefaultAsync() + 1
            };

            foreach (var vehicle in qualifyingVehicles) {
                var vehicleStatusSnapshot = new VehicleSnapshot {
                    Vehicle = vehicle,
                    ChangeStatusCode = await GetVehicle_TxSatus(vehicle),
                    TimelineEventCode = Get_VehicleLastestTimelineEventType(vehicle),
                    VIN = Get_VehicleVIN_IfBuildComplete(vehicle),
                    DealerCode = GetDealerCode(vehicle),
                    EngineSerialNumber = await GetEngineSerialNumber(vehicle, input.EngineComponentCode),
                    CustomReceived = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.CUSTOM_RECEIVED),
                    PlanBuild = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.PLAN_BUILD),
                    BuildCompleted = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.BULD_COMPLETED),
                    GateRelease = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.GATE_RELEASED),
                    Wholesale = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.WHOLE_SALE),
                };

                vehicleSnapshotRun.VehicleSnapshots.Add(vehicleStatusSnapshot);
            }

            // save
            context.VehicleSnapshotRuns.Add(vehicleSnapshotRun);
            var entity = await context.SaveChangesAsync();

            // dto
            payload.Entity = new GenarateSnapshotDTO {
                RunDate = input.RunDate.Date,
                PlantCode = input.PlantCode,
                SnapshotCount = vehicleSnapshotRun.VehicleSnapshots.Count(),
                WasGenerated = true,
                Sequence = vehicleSnapshotRun.Sequence
            };

            return payload;
        }

        public async Task<VehicleSnapshotRunDTO?> GetSnapshot(string plantCode, DateTime runDate) {

            var snapsthoRun = await context.VehicleSnapshotRuns
                .Include(t => t.Plant)
                .Include( t => t.VehicleSnapshots)
                    .ThenInclude(t => t.Vehicle)
                    .ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == plantCode)
                .Where(t => t.RunDate == runDate).FirstOrDefaultAsync();

            if (snapsthoRun == null) {
                return null;
            }

            var dto = new VehicleSnapshotRunDTO {
                PlantCode = snapsthoRun.Plant.Code,
                RunDate = snapsthoRun.RunDate.Date,
                Sequence = snapsthoRun.Sequence,
                Entries = new List<VehicleSnapshotRunDTO.Entry>()
            };

            foreach (var entry in snapsthoRun.VehicleSnapshots) {
                dto.Entries.Add(new VehicleSnapshotRunDTO.Entry {
                    TxType = entry.ChangeStatusCode,
                    CurrentTimelineEvent = entry.TimelineEventCode,
                    LotNo = entry.Vehicle.Lot.LotNo,
                    KitNo = entry.Vehicle.KitNo,
                    VIN = entry.VIN,
                    DealerCode = entry.DealerCode,
                    EngineSerialNumber = entry.EngineSerialNumber,
                    CustomReceived = entry.CustomReceived,
                    PlanBuild = entry.PlanBuild,
                    BuildCompleted = entry.BuildCompleted,
                    GateRelease = entry.GateRelease,
                    Wholesale = entry.Wholesale
                });
            }

            return dto;
        }

        public async Task<List<DateTime>> GetSnapshotDates(string plantCode) {
            return await context.VehicleSnapshotRuns
                .OrderByDescending(t => t.RunDate)
                .Select(t => t.RunDate)
                .ToListAsync();
        }

        public async Task<List<Error>> ValidateGenerateVehicleSnapshot(VehicleSnapshotInput input) {
            var errors = new List<Error>();

            var plantExists = await context.Plants.AnyAsync(t => t.Code == input.PlantCode);
            if (!plantExists) {
                errors.Add(new Error("plantCode", "plant code not found"));
            }

            var component = await context.Components.FirstOrDefaultAsync(t => t.Code == input.EngineComponentCode);
            if (component == null) {
                errors.Add(new Error("EngineComponentCode", $"engine component not found for {input.EngineComponentCode}"));
            }

            if (errors.Any()) {
                return errors;
            }

            var alreadyGenerated = await context.VehicleSnapshotRuns
                .AnyAsync(t => t.Plant.Code == input.PlantCode && t.RunDate.Date == input.RunDate.Date);

            if (alreadyGenerated) {
                errors.Add(new Error("", $"already generated vehicle snapshot for plant {input.PlantCode},  date ${DateTime.UtcNow.Date}"));
            }

            return errors;
        }


        #region helper methods
        private IQueryable<Vehicle> GetPartnerStatusQualifyingVehiclesQuery(VehicleSnapshotInput input) {
            // filter by plant code
            var query = context.Vehicles.Where(t => t.Lot.Plant.Code == input.PlantCode).AsQueryable();

            // filter by custome recived
            query = query
                .Where(t => t.TimelineEvents.Any(ev => ev.RemovedAt == null && ev.EventType.Code == TimeLineEventType.CUSTOM_RECEIVED.ToString()))
                .AsQueryable();

            // filter by wholesale null or whilesalte < runDate + 7
            query = query
                .Where(t =>
                    // no wholesale time line event
                    !t.TimelineEvents.Any(
                        ev => ev.RemovedAt == null &&
                        ev.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString())

                    ||

                    // wholesale timeline event before cut-off date
                    t.TimelineEvents.Any(ev =>
                        ev.RemovedAt == null &&
                        ev.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString() &&
                        ev.EventDate.AddDays(7) > input.RunDate
                    )
                ).AsQueryable();

            return query;
        }

        private string Get_VehicleVIN_IfBuildComplete(Vehicle vehicle) {
            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }
            return vehicle.VIN;
        }

        private async Task<string> GetEngineSerialNumber(Vehicle vehicle, string engineComponentCode) {
            if (engineComponentCode == null) {
                throw new Exception("GetEngineSerialNumber: Engine component code required");
            }

            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }

            var componentScan = await context.ComponentScans
                .Where(t => t.VehicleComponent.Vehicle.KitNo == vehicle.KitNo)
                .Where(t => t.VehicleComponent.Component.Code == engineComponentCode)
                .Where(t => t.AcceptedAt != null && t.RemovedAt == null)
                .FirstOrDefaultAsync();

            return (componentScan?.Scan1 + " " + componentScan?.Scan2).Trim();
        }

        private DateTime? GetVehicleTimelineEventDate(Vehicle vehicle, TimeLineEventType eventType) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == eventType.ToString())
                .FirstOrDefault();

            return timeLineEvnet != null
                ? timeLineEvnet.EventDate
                : (DateTime?)null;
        }

        private string? GetDealerCode(Vehicle vehicle) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString())
                .FirstOrDefault();


            return timeLineEvnet != null
                ? timeLineEvnet.EventNote
                : null;
        }

        private TimeLineEventType Get_VehicleLastestTimelineEventType(Vehicle vehicle) {
            var latestTimelineEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt).FirstOrDefault();

            if (latestTimelineEvent == null) {
                throw new Exception("Should have at least custom received event");
            }

            return Enum.Parse<TimeLineEventType>(latestTimelineEvent.EventType.Code);
        }

        private async Task<PartnerStatus_ChangeStatus> GetVehicle_TxSatus(Vehicle vehicle) {
            var latestTimelineEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.RemovedAt == null);

            if (latestTimelineEvent == null) {
                throw new Exception("timeline event should not be null");
            }

            var currentEventCode = latestTimelineEvent.EventType.Code;

            var priorVehicleSnapshotEntry = await context.VehicleSnapshots
                .OrderByDescending(t => t.VehicleSnapshotRun.RunDate)
                .FirstOrDefaultAsync(t => t.VehicleId == vehicle.Id);
            
            var priorEventCode = priorVehicleSnapshotEntry != null
                ? priorVehicleSnapshotEntry.TimelineEventCode.ToString()
                : null;

            // 1:  if custom received and no previous status entry
            if (currentEventCode == TimeLineEventType.CUSTOM_RECEIVED.ToString() &&
                priorVehicleSnapshotEntry == null) {
                return PartnerStatus_ChangeStatus.Added;
            }

            if (priorVehicleSnapshotEntry == null) {
                throw new Exception("The first event should have been custom received");
            }

            // if anything but wholesale and not same as prior event
            if (currentEventCode != TimeLineEventType.WHOLE_SALE.ToString() &&
                currentEventCode != priorEventCode) {
                return PartnerStatus_ChangeStatus.Changed;
            }

            // if wholesale
            if (currentEventCode == TimeLineEventType.WHOLE_SALE.ToString()) {
                return PartnerStatus_ChangeStatus.Final;
            }
            return PartnerStatus_ChangeStatus.NoChange;
        }

        #endregion
    }
}