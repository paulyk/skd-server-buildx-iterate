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
                .Include(t => t.Snapshots.Where(t => t.RemovedAt == null).OrderBy(t => t.KitTimeLineEventType.Sequence))
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

            var timeLineEventTypes = await context.KitTimelineEventTypes.OrderBy(t => t.Sequence).ToListAsync();

            foreach (var kit in qualifyingKits) {
                KitTimelineEvent selectedKitTimeLineEvent = Get_Next_KitTimeLineEvent_ForThisSnapshot(kit, timeLineEventTypes);
                PartnerStatus_ChangeStatus changeStatus = await GetKit_TxSatus(kit, selectedKitTimeLineEvent);

                var ks = new KitSnapshot();
                ks.Kit = kit;
                ks.ChangeStatusCode = changeStatus;
                ks.KitTimeLineEventType = selectedKitTimeLineEvent.EventType;
                ks.VIN = Get_KitVIN_IfBuildComplete(kit);
                ks.DealerCode = GetDealerCode(kit);
                ks.EngineSerialNumber = await GetEngineSerialNumber(kit, input.EngineComponentCode);

                ks.OrginalPlanBuild = await GetKit_OriginalPlanBuildDate(kit, selectedKitTimeLineEvent);

                ks.CustomReceived = Get_Date_ForEventType(kit, TimeLineEventCode.CUSTOM_RECEIVED, selectedKitTimeLineEvent);
                ks.PlanBuild = Get_Date_ForEventType(kit, TimeLineEventCode.PLAN_BUILD, selectedKitTimeLineEvent);
                ks.BuildCompleted = Get_Date_ForEventType(kit, TimeLineEventCode.BUILD_COMPLETED, selectedKitTimeLineEvent);
                ks.GateRelease = Get_Date_ForEventType(kit, TimeLineEventCode.GATE_RELEASED, selectedKitTimeLineEvent);
                ks.Wholesale = Get_Date_ForEventType(kit, TimeLineEventCode.WHOLE_SALE, selectedKitTimeLineEvent);


                kitSnapshotRun.KitSnapshots.Add(ks);
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
                .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null))
                    .ThenInclude(t => t.Kit).ThenInclude(t => t.Lot)
                .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null))
                    .ThenInclude(t => t.KitTimeLineEventType)
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
                .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null))
                    .ThenInclude(t => t.Kit).ThenInclude(t => t.Lot)
                .Include(t => t.KitSnapshots.Where(u => u.RemovedAt == null))
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
                PartnerPlantCode = snapshotRun.Plant.PartnerPlantCode,
                PartnerPlantType = snapshotRun.Plant.PartnerPlantType,
                RunDate = snapshotRun.RunDate.Date,
                Sequence = snapshotRun.Sequence,
                Entries = new List<KitSnapshotRunDTO.Entry>()
            };

            foreach (var entry in snapshotRun.KitSnapshots) {
                dto.Entries.Add(new KitSnapshotRunDTO.Entry {
                    TxType = entry.ChangeStatusCode,
                    CurrentTimeLineCode = entry.KitTimeLineEventType.Code,
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

            dto.Entries = dto.Entries.OrderBy(t => t.LotNo).ThenBy(t => t.KitNo).ToList();

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

        private DateTime? Get_KitimelineEventDate_ForEventCode(Kit kit, TimeLineEventCode timeLineEventCode) {
            var date = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == timeLineEventCode)
                .Select(t => t.EventDate).FirstOrDefault();

            if (date == DateTime.MinValue) {
                return (DateTime?)null;
            }

            return date;            
        }

        ///<remarks>
        /// 
        ///</remarks>
        private DateTime? Get_Date_ForEventType(
            Kit kit,
            TimeLineEventCode eventTypeForDate,
            KitTimelineEvent timeLineEventForSnapshot) {

            var kitTimeLineEventEntry = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == eventTypeForDate)
                .FirstOrDefault();

            if (kitTimeLineEventEntry == null) {
                return (DateTime?)null;
            }

            var entryForDate_Sequence = kitTimeLineEventEntry.EventType.Sequence;
            var currentStatus_Sequence = timeLineEventForSnapshot.EventType.Sequence;

            if (entryForDate_Sequence > currentStatus_Sequence) {
                return (DateTime?)null;
            }

            return kitTimeLineEventEntry.EventDate;
        }

        private async Task<DateTime?> GetKit_OriginalPlanBuildDate(Kit kit, KitTimelineEvent selectedTimeLineEvent) {
            // find prior OriginalPlanBuild
            var originalPlanBuild = await context.KitSnapshots                
                .OrderBy(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.Kit.Id == kit.Id)
                .Where(t => t.OrginalPlanBuild != null)
                .Select(t => t.OrginalPlanBuild)
                .FirstOrDefaultAsync();

            if (originalPlanBuild != null) {
                return originalPlanBuild;
            }

            // Use PlanBuild date from timeline events
            var planBuild = Get_Date_ForEventType(kit, TimeLineEventCode.PLAN_BUILD, selectedTimeLineEvent);
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

        ///<remarks>
        /// Find all kit timeline events entries that have not been added to a kit_snaphost
        /// If one or more select the first one in timeline event type sequence
        /// Otherwiser return the last timeleine event
        ///</remarks>
        private KitTimelineEvent Get_Next_KitTimeLineEvent_ForThisSnapshot(
            Kit kit,
            List<KitTimelineEventType> kitTimelineEventTypes
        ) {
            var allKitTimelineEventEntries = kit.TimelineEvents.Where(t => t.RemovedAt == null);
            var kitSnapshots = kit.Snapshots.OrderByDescending(t => t.CreatedAt).Where(t => t.RemovedAt == null);

            foreach (var timeLineEventType in kitTimelineEventTypes.OrderBy(t => t.Sequence)) {

                var hasKitSnapshot = kitSnapshots.Any(t => t.KitTimeLineEventType.Code == timeLineEventType.Code);
                
                if (!hasKitSnapshot) {
                    var existingTimeLineEntry = allKitTimelineEventEntries.FirstOrDefault(t => t.EventType == timeLineEventType);
                    if (existingTimeLineEntry != null) {

                        // not snap shot for this time line event type
                        return existingTimeLineEntry;
                    }
                }
            }


            // get the latest timeline event entry for this kit
            return allKitTimelineEventEntries.OrderByDescending(t => t.CreatedAt).First();
        }

        ///<summary></summary>
        private async Task<PartnerStatus_ChangeStatus> GetKit_TxSatus(Kit kit, KitTimelineEvent selectedTimeLineEvent) {

            var selectedEventCode = selectedTimeLineEvent.EventType.Code;

            var priorKitSnapshotEntry = await context.KitSnapshots.Where(t => t.RemovedAt == null)
                .OrderByDescending(t => t.KitSnapshotRun.Sequence)
                .FirstOrDefaultAsync(t => t.KitId == kit.Id);

            TimeLineEventCode? priorEventCode = priorKitSnapshotEntry != null
                ? priorKitSnapshotEntry.KitTimeLineEventType.Code
                : null;

            // ADDED
            // if no prior snapshot 
            if (priorKitSnapshotEntry == null) {
                return PartnerStatus_ChangeStatus.Added;
            }

            // CHANGED
            // ff anything but wholesale and not equal prior snapshot
            if (selectedEventCode != TimeLineEventCode.WHOLE_SALE &&
                selectedEventCode != priorEventCode) {
                return PartnerStatus_ChangeStatus.Changed;
            }

            // NO_CHANGE
            // if wholesale
            if (selectedEventCode == TimeLineEventCode.WHOLE_SALE) {
                return PartnerStatus_ChangeStatus.Final;
            }
            return PartnerStatus_ChangeStatus.NoChange;
        }

        #endregion
    }
}