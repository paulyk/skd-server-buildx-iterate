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
            input.RunDate = input.RunDate ?? DateTime.UtcNow.Date;

            MutationPayload<SnapshotDTO> payload = new();
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
                KitTimelineEvent selectedKitTimeLineEvent = Select_KitTimeLineEvent_ForSnapshot(kit);
                PartnerStatus_ChangeStatus changeStatus = await GetKit_ChangeStatus(kit, selectedKitTimeLineEvent);

                KitSnapshot ks = new();
                ks.Kit = kit;
                ks.ChangeStatusCode = changeStatus;
                ks.KitTimeLineEventType = selectedKitTimeLineEvent.EventType;
                ks.VIN = Get_KitVIN_IfBuildComplete(kit);
                ks.DealerCode = GetDealerCode(kit);
                ks.EngineSerialNumber = await GetEngineSerialNumber(kit, input.EngineComponentCode);

                ks.OrginalPlanBuild = await GetKit_OriginalPlanBuildDate(kit, selectedKitTimeLineEvent);

                ks.CustomReceived = Get_EventDate_ForEventType(kit, TimeLineEventCode.CUSTOM_RECEIVED, selectedKitTimeLineEvent);
                ks.PlanBuild = Get_EventDate_ForEventType(kit, TimeLineEventCode.PLAN_BUILD, selectedKitTimeLineEvent);
                ks.BuildCompleted = Get_EventDate_ForEventType(kit, TimeLineEventCode.BUILD_COMPLETED, selectedKitTimeLineEvent);
                ks.GateRelease = Get_EventDate_ForEventType(kit, TimeLineEventCode.GATE_RELEASED, selectedKitTimeLineEvent);
                ks.Wholesale = Get_EventDate_ForEventType(kit, TimeLineEventCode.WHOLE_SALE, selectedKitTimeLineEvent);


                kitSnapshotRun.KitSnapshots.Add(ks);
            }

            // reject if no changes
            if (input.RejectIfNoChanges) {
                bool hasChanges = kitSnapshotRun.KitSnapshots.Any(x => x.ChangeStatusCode != PartnerStatus_ChangeStatus.NoChange);
                if (!hasChanges) {
                    payload.Errors.Add(new Error("", "No changes since last snapshot"));
                    return payload;
                }
            }

            // save
            context.KitSnapshotRuns.Add(kitSnapshotRun);
            var entity = await context.SaveChangesAsync();

            // input
            payload.Payload = new SnapshotDTO {
                RunDate = input.RunDate.Value.Date,
                PlantCode = input.PlantCode,
                SnapshotCount = kitSnapshotRun.KitSnapshots.Count,
                ChangedCount = kitSnapshotRun.KitSnapshots.Count(x => x.ChangeStatusCode != PartnerStatus_ChangeStatus.NoChange),
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
                .Where(t => t.Plant.Code == plantCode)
                .Select(t => new SnapshotDTO {
                    PlantCode = t.Plant.Code,
                    Sequence = t.Sequence,
                    RunDate = t.RunDate,
                    SnapshotCount = t.KitSnapshots.Count
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

        ///<remarks>
        /// Find the corresponding kitTimeLineEvent for the TimeLineEventCode
        /// e.g. CUSTOM_RECEIVE, PLAN_BUILD etc.
        /// return the EventDate of that entry if found
        /// other wise return a null DateTime
        ///</remarks>
        private DateTime? Get_EventDate_ForEventType(
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


        ///<remarks>
        /// Return OrginalPlanBuild date entry of the immediately preceeding snapshot
        // If no prior snapshot, plan build date from the kit timeline entrie if any
        ///</remarks>
        private async Task<DateTime?> GetKit_OriginalPlanBuildDate(Kit kit, KitTimelineEvent selectedTimeLineEvent) {
            // find prior OriginalPlanBuild
            var priorKitSnapshot = await context.KitSnapshots
                .OrderBy(t => t.CreatedAt)
                .Where(t => t.RemovedAt == null)
                .Where(t => t.Kit.Id == kit.Id)
                .Where(t => t.OrginalPlanBuild != null)
                .FirstOrDefaultAsync();

            if (priorKitSnapshot != null) {
                return priorKitSnapshot.OrginalPlanBuild;
            }

            // Use PlanBuild date from timeline events
            return Get_EventDate_ForEventType(kit, TimeLineEventCode.PLAN_BUILD, selectedTimeLineEvent);
        }

        private string? GetDealerCode(Kit kit) {
            var timeLineEvnet = kit.TimelineEvents
                .Where(t => t.RemovedAt == null)
                .Where(t => t.EventType.Code == TimeLineEventCode.WHOLE_SALE)
                .FirstOrDefault();


            return timeLineEvnet?.EventNote;
        }

        ///<remarks>
        /// Loop through each kit timeline event entry until one is found with no corresponding snapshot
        /// return that one
        ///</remarks>
        private KitTimelineEvent Select_KitTimeLineEvent_ForSnapshot(Kit kit) {
            var kitSnapshots = kit.Snapshots.OrderByDescending(t => t.CreatedAt).Where(t => t.RemovedAt == null);
            var kitTimeLineEventEntries = kit.TimelineEvents.OrderBy(t => t.EventType.Sequence).Where(t => t.RemovedAt == null).ToList();

            KitTimelineEvent selected = kitTimeLineEventEntries.First();
            foreach (var kitTimelineEvent in kitTimeLineEventEntries) {
                selected = kitTimelineEvent;
                var hasKitSnapshot = kitSnapshots.Any(t => t.KitTimeLineEventType.Code == kitTimelineEvent.EventType.Code);
                if (!hasKitSnapshot) {
                    break;
                }
            }
            return selected;
        }

        ///<remarks>
        /// Change since last snapshot
        /// Can be Added, Changed, NoChange, or Final
        ///</remarks>
        private async Task<PartnerStatus_ChangeStatus> GetKit_ChangeStatus(Kit kit, KitTimelineEvent selectedTimeLineEvent) {

            var selectedEventCode = selectedTimeLineEvent.EventType.Code;

            var priorKitSnapshot = await context.KitSnapshots
                .OrderByDescending(t => t.KitSnapshotRun.Sequence)
                .Where(t => t.RemovedAt == null)
                .FirstOrDefaultAsync(t => t.KitId == kit.Id);

            TimeLineEventCode? priorEventCode = priorKitSnapshot?.KitTimeLineEventType?.Code;

            // ADDED
            // if no prior snapshot 
            if (priorKitSnapshot == null) {
                return PartnerStatus_ChangeStatus.Added;
            }

            // CHANGED
            // If anything but wholesale and not equal prior snapshot
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


    }
}