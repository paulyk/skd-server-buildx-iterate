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

        public async Task<MutationPayload<VehicleSnapshoDTO>> GenerateSnapshot(VehicleSnapshotInput input) {
            var dto = new VehicleSnapshoDTO {
                RunDate = input.RunDate.Date,
                PlantCode = input.PlantCode,
                Entries = new List<VehicleSnapshoDTO.Entry>()
            };
            var payload = new MutationPayload<VehicleSnapshoDTO>(dto);
            payload.Errors = await ValidateGenPartnerStatus(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            var query = GetPartnerStatusQualifyingVehiclesQuery(input);
            var qualifyingVehicles = await query
                .Include(t => t.Lot)
                .Include(t => t.StatusSnapshots)
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

            var vehicleStatusSnapshots = new List<VehicleStatusSnapshots>();
            // for each vehilce
            foreach (var vehicle in qualifyingVehicles) {
                var vehicleStatusSnapshot = new VehicleStatusSnapshots {
                    RunDate = input.RunDate.Date,
                    Vehicle = vehicle,
                    PlantId = vehicle.Lot.PlantId,
                    ChangeStatusCode = GetVehicle_TxSatus(vehicle),
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
                vehicleStatusSnapshots.Add(vehicleStatusSnapshot);
            }
            
            context.VehicleStatusSnapshots.AddRange(vehicleStatusSnapshots);
            await context.SaveChangesAsync();
            return payload;
        }

        public async Task<QueryPayload<VehicleSnapshoDTO>> GetSnapshot(VehicleSnapshotInput input) {
            var dto = new VehicleSnapshoDTO {
                PlantCode = input.PlantCode,
                RunDate= input.RunDate.Date,
                Entries = new List<VehicleSnapshoDTO.Entry>()
            };
            var payload = new QueryPayload<VehicleSnapshoDTO>(dto);

            var entries = await context.VehicleStatusSnapshots.AsNoTracking()
                .Include(t => t.Vehicle).ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == input.PlantCode)
                .Where(t => t.RunDate == input.RunDate.Date)
                .ToListAsync();
            

            foreach(var entry in entries) {
                dto.Entries.Add(new VehicleSnapshoDTO.Entry{
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
        
            return payload;
        }

        private IQueryable<Vehicle> GetPartnerStatusQualifyingVehiclesQuery(VehicleSnapshotInput input) {
            var wholesaleCutoff = input.RunDate.AddDays(7);

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
                        ev.EventDate <= wholesaleCutoff
                    )
                ).AsQueryable();

            return query;
        }

        public string Get_VehicleVIN_IfBuildComplete(Vehicle vehicle) {
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

        private string GetDealerCode(Vehicle vehicle) {
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

        private PartnerStatus_ChangeStatus GetVehicle_TxSatus(Vehicle vehicle) {
            var latestTimelineEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault(t => t.RemovedAt == null);

            var eventCode = latestTimelineEvent.EventType.Code;

            var priorPartnerStatusEntry = vehicle.StatusSnapshots
                .OrderByDescending(t => t.RunDate)
                .FirstOrDefault();

            // 1:  if custom received and no previous status entry
            if (eventCode == TimeLineEventType.CUSTOM_RECEIVED.ToString() &&
                priorPartnerStatusEntry == null) {
                return PartnerStatus_ChangeStatus.Added;
            }

            if (priorPartnerStatusEntry == null) {
                throw new Exception("The first event should have been custom received");
            }

            // if anything but wholesale and not same as prior event
            if (eventCode != TimeLineEventType.WHOLE_SALE.ToString() &&
                priorPartnerStatusEntry.TimelineEventCode != TimeLineEventType.WHOLE_SALE) {
                return PartnerStatus_ChangeStatus.Changed;
            }

            // if wholesale
            if (eventCode == TimeLineEventType.WHOLE_SALE.ToString()) {
                return PartnerStatus_ChangeStatus.Final;
            }
            return PartnerStatus_ChangeStatus.NoChange;
        }

        public async Task<List<Error>> ValidateGenPartnerStatus(VehicleSnapshotInput input) {
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

            var alreadyGenerated = await context.VehicleStatusSnapshots
                .AnyAsync(t => t.RunDate.Date == input.RunDate.Date);

            if (alreadyGenerated) {
                errors.Add(new Error("", $"already generated for utc date ${DateTime.UtcNow.Date}"));
            }

            return errors;
        }
    }
}