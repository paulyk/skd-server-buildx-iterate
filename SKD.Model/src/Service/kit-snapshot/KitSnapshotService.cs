#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SKD.Model {

    public class KitSnapshotService {

        private readonly SkdContext context;

        public KitSnapshotService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<SnapshotDTO>> GenerateSnapshot(KitSnapshotInput input) {

            var payload = new MutationPayload<SnapshotDTO>(null);
            payload.Errors = await ValidateGenerateKitSnapshot(input);
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

            // if no vehicles
            if (qualifyingVehicles.Count == 0) {
                var dto = new SnapshotDTO {
                    RunDate = input.RunDate.Date,
                    PlantCode = input.PlantCode,
                    SnapshotCount = 0
                };

                payload.Entity = dto;
                return payload;
            }

            // create entity
            var vehicleSnapshotRun = new kitSnapshotRun {
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                RunDate = input.RunDate.Date,
                Sequence = await context.VehicleSnapshotRuns
                    .Where(t => t.Plant.Code == input.PlantCode)
                    .OrderByDescending(t => t.Sequence)
                    .Select(t => t.Sequence)
                    .FirstOrDefaultAsync() + 1
            };

            foreach (var vehicle in qualifyingVehicles) {
                var vehicleStatusSnapshot = new KitSnapshot {
                    Kit = vehicle,
                    ChangeStatusCode = await GetVehicle_TxSatus(vehicle),
                    TimelineEventCode = Get_VehicleLastestTimelineEventType(vehicle),
                    VIN = Get_VehicleVIN_IfBuildComplete(vehicle),
                    DealerCode = GetDealerCode(vehicle),
                    EngineSerialNumber = await GetEngineSerialNumber(vehicle, input.EngineComponentCode),
                    CustomReceived = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.CUSTOM_RECEIVED),
                    PlanBuild = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.PLAN_BUILD),
                    OrginalPlanBuild = await GetVehicle_OriginalPlanBuildDate(vehicle),
                    BuildCompleted = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.BULD_COMPLETED),
                    GateRelease = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.GATE_RELEASED),
                    Wholesale = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.WHOLE_SALE),
                };

                vehicleSnapshotRun.KitSnapshots.Add(vehicleStatusSnapshot);
            }

            // save
            context.VehicleSnapshotRuns.Add(vehicleSnapshotRun);
            var entity = await context.SaveChangesAsync();

            // dto
            payload.Entity = new SnapshotDTO {
                RunDate = input.RunDate.Date,
                PlantCode = input.PlantCode,                
                SnapshotCount = vehicleSnapshotRun.KitSnapshots.Count(),
                Sequence = vehicleSnapshotRun.Sequence
            };

            return payload;
        }

        public async Task<VehicleSnapshotRunDTO?> GetSnapshotRunBySequence(string plantCode, int sequence) {

            var snapshotRun = await context.VehicleSnapshotRuns
                .Include(t => t.Plant)
                .Include(t => t.KitSnapshots)
                    .ThenInclude(t => t.Kit)
                    .ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == plantCode)
                .Where(t => t.Sequence == sequence).FirstOrDefaultAsync();

            if (snapshotRun == null) {
                return null;
            }

            return BuildVehicleSnapshotgRunDTO(snapshotRun);
        }

        public async Task<VehicleSnapshotRunDTO?> GetSnapshotRunByDate(string plantCode, DateTime runDate) {

            var snapshotRun = await context.VehicleSnapshotRuns
                .Include(t => t.Plant)
                .Include(t => t.KitSnapshots)
                    .ThenInclude(t => t.Kit)
                    .ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == plantCode)
                .Where(t => t.RunDate == runDate).FirstOrDefaultAsync();

            if (snapshotRun == null) {
                return null;
            }

            return BuildVehicleSnapshotgRunDTO(snapshotRun);
        }

        private VehicleSnapshotRunDTO BuildVehicleSnapshotgRunDTO(kitSnapshotRun snapshotRun) {
            var dto = new VehicleSnapshotRunDTO {
                PlantCode = snapshotRun.Plant.Code,
                RunDate = snapshotRun.RunDate.Date,
                Sequence = snapshotRun.Sequence,
                Entries = new List<VehicleSnapshotRunDTO.Entry>()
            };

            foreach (var entry in snapshotRun.KitSnapshots) {
                dto.Entries.Add(new VehicleSnapshotRunDTO.Entry {
                    TxType = entry.ChangeStatusCode,
                    CurrentTimelineEvent = entry.TimelineEventCode,
                    LotNo = entry.Kit.Lot.LotNo,
                    KitNo = entry.Kit.KitNo,
                    VIN = entry.VIN,
                    DealerCode = entry.DealerCode,
                    EngineSerialNumber = entry.EngineSerialNumber,
                    CustomReceived = entry.CustomReceived,
                    OriginalPlanBuild = entry.OrginalPlanBuild,
                    PlanBuild = entry.PlanBuild,
                    BuildCompleted = entry.BuildCompleted,
                    GateRelease = entry.GateRelease,
                    Wholesale = entry.Wholesale
                });
            }

            return dto;
        }

        public async Task<List<SnapshotDTO>> GetSnapshotRuns(string plantCode, int count = 50) {
            return await context.VehicleSnapshotRuns
                .OrderByDescending(t => t.RunDate)
                .Select(t => new SnapshotDTO {
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    RunDate = t.RunDate,
                    SnapshotCount = t.KitSnapshots.Count()
                })
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Error>> ValidateGenerateKitSnapshot(KitSnapshotInput input) {
            var errors = new List<Error>();

            var plantExists = await context.Plants.AnyAsync(t => t.Code == input.PlantCode);
            if (!plantExists) {
                errors.Add(new Error("plantCode", "plant code not found"));
            }

            var engineComponent = await context.Components.FirstOrDefaultAsync(t => t.Code == input.EngineComponentCode);
            if (engineComponent == null) {
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
        private IQueryable<Kit> GetPartnerStatusQualifyingVehiclesQuery(KitSnapshotInput input) {
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

        private string Get_VehicleVIN_IfBuildComplete(Kit vehicle) {
            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }
            return vehicle.VIN;
        }

        private async Task<string> GetEngineSerialNumber(Kit vehicle, string engineComponentCode) {
            if (engineComponentCode == null) {
                throw new Exception("GetEngineSerialNumber: Engine component code required");
            }

            var buildCompletedEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventType.BULD_COMPLETED.ToString());

            if (buildCompletedEvent == null) {
                return "";
            }

            var componentScan = await context.ComponentSerials
                .Where(t => t.VehicleComponent.Kit.KitNo == vehicle.KitNo)
                .Where(t => t.VehicleComponent.Component.Code == engineComponentCode)
                .Where(t => t.VerifiedAt != null && t.RemovedAt == null)
                .FirstOrDefaultAsync();

            return (componentScan?.Serial1 + " " + componentScan?.Serial2).Trim();
        }

        private DateTime? GetVehicleTimelineEventDate(Kit vehicle, TimeLineEventType eventType) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == eventType.ToString())
                .FirstOrDefault();

            return timeLineEvnet != null
                ? timeLineEvnet.EventDate
                : (DateTime?)null;
        }

        private async Task<DateTime?> GetVehicle_OriginalPlanBuildDate(Kit vehicle) {
            // find prior OriginalPlanBuild
            var originalPlanBuild = await context.VehicleSnapshots
                .OrderBy(t => t.CreatedAt)
                .Where(t => t.Kit.Id == vehicle.Id)
                .Where(t => t.OrginalPlanBuild != null)
                .Select(t => t.OrginalPlanBuild)
                .FirstOrDefaultAsync();
                
            
            if (originalPlanBuild != null) {
                return originalPlanBuild;
            }

            // Use PlanBuild date from timeline events
            var planBuld = GetVehicleTimelineEventDate(vehicle, TimeLineEventType.PLAN_BUILD);
            return planBuld;
        }

        private string? GetDealerCode(Kit vehicle) {
            var timeLineEvnet = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == TimeLineEventType.WHOLE_SALE.ToString())
                .FirstOrDefault();


            return timeLineEvnet != null
                ? timeLineEvnet.EventNote
                : null;
        }

        private TimeLineEventType Get_VehicleLastestTimelineEventType(Kit vehicle) {
            var latestTimelineEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt).FirstOrDefault();

            if (latestTimelineEvent == null) {
                throw new Exception("Should have at least custom received event");
            }

            return Enum.Parse<TimeLineEventType>(latestTimelineEvent.EventType.Code);
        }

        private async Task<PartnerStatus_ChangeStatus> GetVehicle_TxSatus(Kit vehicle) {
            var latestTimelineEvent = vehicle.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.EventType.Sequecne)
                .FirstOrDefault(t => t.RemovedAt == null);

            if (latestTimelineEvent == null) {
                throw new Exception("timeline event should not be null");
            }

            var currentEventCode = latestTimelineEvent.EventType.Code;

            var priorVehicleSnapshotEntry = await context.VehicleSnapshots
                .OrderByDescending(t => t.VehicleSnapshotRun.RunDate)
                .FirstOrDefaultAsync(t => t.KitId == vehicle.Id);

            var priorEventCode = priorVehicleSnapshotEntry != null
                ? priorVehicleSnapshotEntry.TimelineEventCode.ToString()
                : null;

            // 1.  if not prior event the ADDED
            if (priorVehicleSnapshotEntry == null) {
                return PartnerStatus_ChangeStatus.Added;
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