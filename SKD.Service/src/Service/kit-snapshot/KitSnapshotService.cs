#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SKD.Model;

namespace SKD.Service {

    public class KitSnapshotService {

        private readonly SkdContext context;

        private readonly int wholeSateCutOffDays = 7;
        public KitSnapshotService(SkdContext ctx) {
            this.context = ctx;
        }

        public async Task<MutationPayload<SnapshotDTO>> GenerateSnapshot(KitSnapshotInput input) {
            // set to current date if null
            input.RunDate = input.RunDate.HasValue ? input.RunDate.Value : DateTime.UtcNow.Date;

            
            var payload = new MutationPayload<SnapshotDTO>(null);
            // validate
            payload.Errors = await ValidateGenerateKitSnapshot(input);
            if (payload.Errors.Any()) {
                return payload;
            }

            // get qualifying kit list
            var query = GetPartnerStatusQualifyingKitsQuery(input);
            var qualifyingKits = await query
                .Include(t => t.Lot)
                .Include(t => t.Snapshots.OrderBy(t => t.KitTimeLineEventType.Sequence))
                .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
                .ToListAsync();

            // if no kits
            if (qualifyingKits.Count == 0) {
                var dto = new SnapshotDTO {
                    RunDate = input.RunDate.Value.Date,
                    PlantCode = input.PlantCode,
                    SnapshotCount = 0
                };

                payload.Payload = dto;
                return payload;
            }

            // create entity
            var kitSnapshotRun = new KitSnapshotRun {
                Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
                RunDate = input.RunDate.Value.Date,
                Sequence = await context.KitSnapshotRuns
                    .Where(t => t.Plant.Code == input.PlantCode)
                    .OrderByDescending(t => t.Sequence)
                    .Select(t => t.Sequence)
                    .FirstOrDefaultAsync() + 1
            };

            foreach (var kit in qualifyingKits) {
                var timeLineEventType =  Get_KitLastestTimelineEventType(kit);

                var kitStatusSnapshot = new KitSnapshot {
                    Kit = kit,
                    ChangeStatusCode = await GetKit_TxSatus(kit),
                    // TimelineEventCode = timeLineEventType.Code,
                    KitTimeLineEventType = timeLineEventType,
                    VIN = Get_KitVIN_IfBuildComplete(kit),
                    DealerCode = GetDealerCode(kit),
                    EngineSerialNumber = await GetEngineSerialNumber(kit, input.EngineComponentCode),

                    OrginalPlanBuild = await GetKit_OriginalPlanBuildDate(kit),
                    CustomReceived = GetKitTimelineEventDate(kit, TimeLineEventCode.CUSTOM_RECEIVED),
                    PlanBuild = GetKitTimelineEventDate(kit, TimeLineEventCode.PLAN_BUILD),
                    BuildCompleted = GetKitTimelineEventDate(kit, TimeLineEventCode.BUILD_COMPLETED),
                    GateRelease = GetKitTimelineEventDate(kit, TimeLineEventCode.GATE_RELEASED),
                    Wholesale = GetKitTimelineEventDate(kit, TimeLineEventCode.WHOLE_SALE),
                };

                kitSnapshotRun.KitSnapshots.Add(kitStatusSnapshot);
            }

            // save
            context.KitSnapshotRuns.Add(kitSnapshotRun);
            var entity = await context.SaveChangesAsync();
            
            // input
            payload.Payload = new SnapshotDTO {
                RunDate = input.RunDate.Value.Date,
                PlantCode = input.PlantCode,                
                SnapshotCount = kitSnapshotRun.KitSnapshots.Count(),
                Sequence = kitSnapshotRun.Sequence
            };

            return payload;
        }

        public async Task<KitSnapshotRunDTO?> GetSnapshotRunBySequence(string plantCode, int sequence) {

            var snapshotRun = await context.KitSnapshotRuns
                .Include(t => t.Plant)
                .Include(t => t.KitSnapshots.OrderBy(u => u.Kit.Lot.LotNo).ThenBy(u => u.Kit.KitNo))                    
                    .ThenInclude(t => t.Kit)
                    .ThenInclude(t => t.Lot)
                .Where(t => t.Plant.Code == plantCode)
                .Where(t => t.Sequence == sequence).FirstOrDefaultAsync();

            if (snapshotRun == null) {
                return null;
            }

            return BuildKitSnapshotgRunDTO(snapshotRun);
        }

        public async Task<KitSnapshotRunDTO?> GetSnapshotRunByDate(string plantCode, DateTime runDate) {

            var snapshotRun = await context.KitSnapshotRuns
                .Include(t => t.Plant)
                .Include(t => t.KitSnapshots)
                    .ThenInclude(t => t.Kit)
                    .ThenInclude(t => t.Lot)
                .Include(t => t.KitSnapshots)
                    .ThenInclude(t => t.KitTimeLineEventType)
                .Where(t => t.Plant.Code == plantCode)
                .Where(t => t.RunDate == runDate).FirstOrDefaultAsync();

            if (snapshotRun == null) {
                return null;
            }

            return BuildKitSnapshotgRunDTO(snapshotRun);
        }

        private KitSnapshotRunDTO BuildKitSnapshotgRunDTO(KitSnapshotRun snapshotRun) {
            var dto = new KitSnapshotRunDTO {
                PlantCode = snapshotRun.Plant.Code,
                PartnerPlantCode= snapshotRun.Plant.PartnerPlantCode,
                PartnerPlantType = snapshotRun.Plant.PartnerPlantType,
                RunDate = snapshotRun.RunDate.Date,
                Sequence = snapshotRun.Sequence,
                Entries = new List<KitSnapshotRunDTO.Entry>()
            };

            foreach (var entry in snapshotRun.KitSnapshots) {
                dto.Entries.Add(new KitSnapshotRunDTO.Entry {
                    TxType = entry.ChangeStatusCode,
                    CurrentTimelineEvent = entry.KitTimeLineEventType.Code,
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
            return await context.KitSnapshotRuns
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

            if (!input.RunDate.HasValue) {
                errors.Add(new Error("", "Run date required"));
                return errors;
            }

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

            // already generated
            var alreadyGenerated = await context.KitSnapshotRuns
                .AnyAsync(t => t.Plant.Code == input.PlantCode && t.RunDate.Date == input.RunDate.Value.Date);

            if (alreadyGenerated) {
                errors.Add(new Error("", $"already generated kit snapshot for plant {input.PlantCode},  date ${DateTime.UtcNow.Date}"));
            }
        
            return errors;
        }


        #region helper methods
        private IQueryable<Kit> GetPartnerStatusQualifyingKitsQuery(KitSnapshotInput input) {
            // filter by plant code
            var query = context.Kits.Where(t => t.Lot.Plant.Code == input.PlantCode).AsQueryable();

            // filter by custome recived
            query = query
                .Where(t => t.TimelineEvents.Any(ev => ev.RemovedAt == null && ev.EventType.Code == TimeLineEventCode.CUSTOM_RECEIVED))
                .AsQueryable();

            // filter by wholesale null or whilesalte < runDate + 7
            query = query
                .Where(t =>
                    // no wholesale time line event
                    !t.TimelineEvents.Any(
                        ev => ev.RemovedAt == null &&
                        ev.EventType.Code == TimeLineEventCode.WHOLE_SALE)

                    ||

                    // wholesale timeline event before cut-off date
                    t.TimelineEvents.Any(ev =>
                        ev.RemovedAt == null &&
                        ev.EventType.Code == TimeLineEventCode.WHOLE_SALE &&
                        ev.EventDate.AddDays(wholeSateCutOffDays) > input.RunDate
                    )
                ).AsQueryable();

            return query;
        }

        private string Get_KitVIN_IfBuildComplete(Kit kit) {
            var buildCompletedEvent = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventCode.BUILD_COMPLETED);

            if (buildCompletedEvent == null) {
                return "";
            }
            return kit.VIN;
        }

        private async Task<string> GetEngineSerialNumber(Kit kit, string engineComponentCode) {
            if (engineComponentCode == null) {
                throw new Exception("GetEngineSerialNumber: Engine component code required");
            }

            var buildCompletedEvent = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .FirstOrDefault(t => t.EventType.Code == TimeLineEventCode.BUILD_COMPLETED);

            if (buildCompletedEvent == null) {
                return "";
            }

            var componentScan = await context.ComponentSerials
                .Where(t => t.KitComponent.Kit.KitNo == kit.KitNo)
                .Where(t => t.KitComponent.Component.Code == engineComponentCode)
                .Where(t => t.VerifiedAt != null && t.RemovedAt == null)
                .FirstOrDefaultAsync();

            return (componentScan?.Serial1 + " " + componentScan?.Serial2).Trim();
        }

        private DateTime? GetKitTimelineEventDate(Kit kit, TimeLineEventCode eventType) {
            var timeLineEvnet = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == eventType)
                .FirstOrDefault();

            return timeLineEvnet != null
                ? timeLineEvnet.EventDate
                : (DateTime?)null;
        }

        private async Task<DateTime?> GetKit_OriginalPlanBuildDate(Kit kit) {
            // find prior OriginalPlanBuild
            var originalPlanBuild = await context.KitSnapshots
                .OrderBy(t => t.CreatedAt)
                .Where(t => t.Kit.Id == kit.Id)
                .Where(t => t.OrginalPlanBuild != null)
                .Select(t => t.OrginalPlanBuild)
                .FirstOrDefaultAsync();
                
            
            if (originalPlanBuild != null) {
                return originalPlanBuild;
            }

            // Use PlanBuild date from timeline events
            var planBuild = GetKitTimelineEventDate(kit, TimeLineEventCode.PLAN_BUILD);
            return planBuild;
        }

        private string? GetDealerCode(Kit kit) {
            var timeLineEvnet = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == TimeLineEventCode.WHOLE_SALE)
                .FirstOrDefault();


            return timeLineEvnet != null
                ? timeLineEvnet.EventNote
                : null;
        }

        private KitTimelineEventType Get_KitLastestTimelineEventType(
            Kit kit
        ) {
            var latestTimelineEvent = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.CreatedAt).FirstOrDefault();

            if (latestTimelineEvent == null) {
                throw new Exception("Should have at least custom received event");
            }

            return latestTimelineEvent.EventType;
        }

        private async Task<PartnerStatus_ChangeStatus> GetKit_TxSatus(Kit kit) {
            var latestTimelineEvent = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.EventType.Sequence)
                .FirstOrDefault(t => t.RemovedAt == null);

            if (latestTimelineEvent == null) {
                throw new Exception("timeline event should not be null");
            }

            var currentEventCode = latestTimelineEvent.EventType.Code;

            var priorKitSnapshotEntry = await context.KitSnapshots
                .OrderByDescending(t => t.KitSnapshotRun.RunDate)
                .FirstOrDefaultAsync(t => t.KitId == kit.Id);

            TimeLineEventCode? priorEventCode = priorKitSnapshotEntry != null
                ? priorKitSnapshotEntry.KitTimeLineEventType.Code
                : null;

            // ADDED
            // if no prior event 
            if (priorKitSnapshotEntry == null) {
                return PartnerStatus_ChangeStatus.Added;
            }

            // CHANGED
            // f anything but wholesale and not same as prior event
            if (currentEventCode != TimeLineEventCode.WHOLE_SALE &&
                currentEventCode != priorEventCode) {
                return PartnerStatus_ChangeStatus.Changed;
            }

            // NO_CHANGE
            // if wholesale
            if (currentEventCode == TimeLineEventCode.WHOLE_SALE) {
                return PartnerStatus_ChangeStatus.Final;
            }
            return PartnerStatus_ChangeStatus.NoChange;
        }

        #endregion
    }
}