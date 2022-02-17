#nullable enable

namespace SKD.Service;

public class KitSnapshotService {

    private readonly SkdContext context;
    private readonly int wholeSateCutOffDays = 7;

    public KitSnapshotService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<SnapshotDTO>> GenerateSnapshot(KitSnapshotInput input) {
        // set to current date if null
        input.RunDate = input.RunDate ?? DateTime.UtcNow.Date;

        MutationResult<SnapshotDTO> result = new() {
            Payload = new SnapshotDTO {
                RunDate = input.RunDate.Value.Date,
                PlantCode = input.PlantCode,
                SnapshotCount = 0
            }
        };

        // validate
        result.Errors = await ValidateGenerateKitSnapshot(input);
        if (result.Errors.Any()) {
            return result;
        }

        // get qualifying kit list
        var qualifyingKits = await GetQualifyingKits(input.PlantCode, input.RunDate.Value);

        // if no kits
        if (qualifyingKits.Count == 0) {
            return result;
        }

        // create entity, set sequence number
        var kitSnapshotRun = new KitSnapshotRun {
            Plant = await context.Plants.FirstOrDefaultAsync(t => t.Code == input.PlantCode),
            RunDate = input.RunDate.Value.Date,
            Sequence = await context.KitSnapshotRuns
                .Where(t => t.Plant.Code == input.PlantCode)
                .OrderByDescending(t => t.Sequence)
                .Select(t => t.Sequence)
                .FirstOrDefaultAsync() + 1
        };

        // generate kitSnapshots
        foreach (var kit in qualifyingKits) {
            // get prior snapshot used to determine differences
            var priorSnapshot = await GetPriorKitSnapshot(kit.Id);

            // Set snapshot properties
            KitSnapshot snapshot = new() { Kit = kit };
            snapshot.VIN = Get_VIN_If_BuildCompleted(kit);
            snapshot.DealerCode = kit.Dealer?.Code;
            snapshot.EngineSerialNumber = await GetEngineSerialNumber(kit, input.EngineComponentCode);

            snapshot.CustomReceived = Get_EventDate_For_Timeline_EventCode(kit, priorSnapshot, TimeLineEventCode.CUSTOM_RECEIVED);
            snapshot.PlanBuild = Get_EventDate_For_Timeline_EventCode(kit, priorSnapshot, TimeLineEventCode.PLAN_BUILD);
            snapshot.BuildCompleted = Get_EventDate_For_Timeline_EventCode(kit, priorSnapshot, TimeLineEventCode.BUILD_COMPLETED);
            snapshot.GateRelease = Get_EventDate_For_Timeline_EventCode(kit, priorSnapshot, TimeLineEventCode.GATE_RELEASED);
            snapshot.Wholesale = Get_EventDate_For_Timeline_EventCode(kit, priorSnapshot, TimeLineEventCode.WHOLE_SALE);

            snapshot.OrginalPlanBuild = snapshot.PlanBuild;

            snapshot.KitTimeLineEventType = GetLatestSnapshotEventType(kit, snapshot);
            snapshot.ChangeStatusCode = GetKit_PartnerStatus_ChangeStatus(snapshot, priorSnapshot);            
            
            kitSnapshotRun.KitSnapshots.Add(snapshot);
        }


        // ks.KitTimeLineEventType.EventType.Code == TimeLineEventCode.WHOLE_SALE

        // reject if no changes
        if (input.RejectIfNoChanges) {
            bool hasChanges = kitSnapshotRun.KitSnapshots.Any(x => x.ChangeStatusCode != SnapshotChangeStatus.NoChange);
            if (!hasChanges) {
                result.Errors.Add(new Error("", "No changes since last snapshot"));
                return result;
            }
        }

        // save
        context.KitSnapshotRuns.Add(kitSnapshotRun);
        await context.SaveChangesAsync();

        // result payload
        result.Payload = new SnapshotDTO {
            RunDate = input.RunDate.Value.Date,
            PlantCode = input.PlantCode,
            SnapshotCount = kitSnapshotRun.KitSnapshots.Count,
            ChangedCount = kitSnapshotRun.KitSnapshots.Count(x => x.ChangeStatusCode != SnapshotChangeStatus.NoChange),
            Sequence = kitSnapshotRun.Sequence
        };

        return result;
    }

    ///<summary>
    /// Return kits to be included in this snapshot
    ///</summay>
    public async Task<List<Kit>> GetQualifyingKits(string plantCode, DateTime runDate) {
        var query = GetKitSnapshotQualifyingKitsQuery(plantCode, runDate);
        return await query
            .Include(t => t.Lot).ThenInclude(t => t.ShipmentLots)
            .Include(t => t.Snapshots.Where(t => t.RemovedAt == null).OrderBy(t => t.KitTimeLineEventType.Sequence))
            .Include(t => t.TimelineEvents).ThenInclude(t => t.EventType)
            .Include(t => t.Dealer)
            .ToListAsync();
    }

    ///Z<summary>
    /// Only kits that have at least one TimelineEvent 
    /// and whose final TimelineEvent is date is within wholeSateCutOffDays days of the currente date
    ///<summary>
    private IQueryable<Kit> GetKitSnapshotQualifyingKitsQuery(string plantCode, DateTime runDate) {
        // filter by plant code
        var query = context.Kits.Where(t => t.Lot.Plant.Code == plantCode).AsQueryable();

        // filter by custome recived
        query = query
            .Where(t => t.TimelineEvents.Any(ev => ev.RemovedAt == null && ev.EventType.Code == TimeLineEventCode.CUSTOM_RECEIVED))
            .AsQueryable();

        // filter by wholesale null or whilesalte < runDate + 7 or whole sale date not found in any kit snapshot
        query = query
            .Where(t =>
                // no wholesale time line event
                !t.TimelineEvents.Any(
                    ev => ev.RemovedAt == null &&
                    ev.EventType.Code == TimeLineEventCode.WHOLE_SALE)

                ||

                // wholesale kit timeline event created date 
                // is within wholeSateCutOffDays of the runDate
                t.TimelineEvents.Any(ev =>
                    ev.RemovedAt == null &&
                    ev.EventType.Code == TimeLineEventCode.WHOLE_SALE &&
                    ev.CreatedAt.AddDays(wholeSateCutOffDays) > runDate
                )

                ||

                // wholesale of most recent snapshot for this kit is null
                t.Snapshots
                    .OrderByDescending(t => t.CreatedAt)
                    .Select(t => t.Wholesale)
                    .First() == null
            ).AsQueryable();

        return query;
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

        return BuildKitSnapshotRunDTO(snapshotRun);
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

        return BuildKitSnapshotRunDTO(snapshotRun);
    }

    private KitSnapshotRunDTO BuildKitSnapshotRunDTO(KitSnapshotRun snapshotRun) {
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
            errors.Add(new Error("", $"Already generated kit snapshot for plant {input.PlantCode}, UTC date {DateTime.UtcNow.Date.ToString("yyyy-MM-dd")}"));
        }

        return errors;
    }


    /// <remark>
    /// returns VIN if kit has the build completed event.
    /// <remark>
    private string Get_VIN_If_BuildCompleted(Kit kit) {
        if (!KitHasTimelineEvent(kit, TimeLineEventCode.BUILD_COMPLETED)) {
            return "";
        }

        return kit.VIN;
    }

    private async Task<string> GetEngineSerialNumber(Kit kit, string engineComponentCode) {
        if (engineComponentCode == null) {
            throw new Exception("GetEngineSerialNumber: Engine component code required");
        }

        var buildCompletedEvent = GetKitTimelineEvent(kit, TimeLineEventCode.BUILD_COMPLETED);
        if (buildCompletedEvent == null) {
            return "";
        }

        var verifiedComponentSerial = await context.ComponentSerials
            .Where(t => t.KitComponent.Kit.KitNo == kit.KitNo)
            .Where(t => t.KitComponent.Component.Code == engineComponentCode)
            .Where(t => t.VerifiedAt != null && t.RemovedAt == null)
            .FirstOrDefaultAsync();

        return (verifiedComponentSerial?.Serial1 + " " + verifiedComponentSerial?.Serial2).Trim();
    }

    ///<remarks>
    /// Get timeline event date from prior snapshot if exists Otherwise gets from kit, 
    /// but only for the latest pending event
    /// Even if kit has timeline events after pending, they are not added to the snapshot/
    ///</remarks>
    private DateTime? Get_EventDate_For_Timeline_EventCode(
        Kit kit,
        KitSnapshot? priorSnapshot,
        TimeLineEventCode eventCode) {

        // if no prior snapshot then we start with custom receive if eventCode == CUSTOM_RECEIVED 
        if (priorSnapshot == null) {
            return eventCode == TimeLineEventCode.CUSTOM_RECEIVED
                ? GetKitTimelineEventDate(kit, eventCode)
                : (DateTime?)null;
        }

        var pendingCode = GetNextPendingSnapshotTimeLineEventCode(priorSnapshot);
        if (pendingCode == null) {
            // if nothing pending the return snapshot date
            return SnapshotTimelineEventDate(priorSnapshot, eventCode);
        }

        var result = eventCode.CompareTo(pendingCode);

        if (result < 0) {
            // return snapshot date
            return SnapshotTimelineEventDate(priorSnapshot, eventCode);
        } else if (result == 0) {
            // same and pending:  return kit event date 
            return GetKitTimelineEventDate(kit, eventCode);
        } else {
            // return null for now even if kit has timeine event. 
            return (DateTime?)null;
        }
    }

    ///<remarks>
    /// Compare prior to current snapshot to determin change statusL: Added, Changed, NoChange, or Final
    /// Oonce status set ot Final it stays there.
    ///</remarks>
    private SnapshotChangeStatus GetKit_PartnerStatus_ChangeStatus(KitSnapshot currentSnapshot, KitSnapshot? priorSnapshot) {

        // ADDED if no prior snapshot 
        if (priorSnapshot == null) {
            return SnapshotChangeStatus.Added;
        }

        // FINAL if has wholesate then Final
        if (currentSnapshot.KitTimeLineEventType.Code == TimeLineEventCode.WHOLE_SALE)  {
            return SnapshotChangeStatus.Final;
        }

        // CHANGE: if current different from prior
        if (currentSnapshot.KitTimeLineEventType.Code != priorSnapshot.KitTimeLineEventType.Code) {
            return SnapshotChangeStatus.Changed;
        }

        // NO-CHANGE otherwise no change
        return SnapshotChangeStatus.NoChange;

    }
    private DateTime? GetKitTimelineEventDate(Kit kit, TimeLineEventCode eventCode) {
        var result = kit.TimelineEvents
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == eventCode)
            .Select(t => t.EventDate)
            .FirstOrDefault();

        return result == DateTime.MinValue
            ? (DateTime?)null
            : result;
    }

    private bool KitHasTimelineEvent(Kit kit, TimeLineEventCode eventCode)
        => kit.TimelineEvents
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == eventCode)
            .Any();

    private async Task<KitSnapshot?> GetPriorKitSnapshot(Guid kitId)
        => await context.KitSnapshots
            .OrderByDescending(t => t.KitSnapshotRun.Sequence)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefaultAsync(t => t.KitId == kitId);

    private bool GetSnapshotTimelineEventDate(KitSnapshot snapshot, TimeLineEventCode timelineEventCode) {
        switch (timelineEventCode) {
            case TimeLineEventCode.CUSTOM_RECEIVED: return snapshot.CustomReceived != null;
            case TimeLineEventCode.PLAN_BUILD: return snapshot.PlanBuild != null;
            case TimeLineEventCode.BUILD_COMPLETED: return snapshot.BuildCompleted != null;
            case TimeLineEventCode.GATE_RELEASED: return snapshot.GateRelease != null;
            case TimeLineEventCode.WHOLE_SALE: return snapshot.Wholesale != null;
            default: return false;
        }
    }

    ///<remarks>
    /// Get most recenet kit timeline event for specified eventCode
    ///</remarks>
    private KitTimelineEvent? GetKitTimelineEvent(Kit kit, TimeLineEventCode eventCode) {
        return kit.TimelineEvents
            .OrderByDescending(t => t.CreatedAt)
            .Where(t => t.RemovedAt == null)
            .FirstOrDefault(t => t.EventType.Code == eventCode);
    }


    ///<remarks>
    /// Get most rececnt KitTimelineEventType matchin snapshot event
    ///</remarks>
    public static KitTimelineEventType GetLatestSnapshotEventType(Kit kit, KitSnapshot snapshot) {
        var eventCode = LatestSnapshotEventCode(snapshot);
        if (eventCode == null) {
            throw new Exception("shold have timeline event code");
        }
        return kit.TimelineEvents
            .Where(t => t.RemovedAt == null)
            .Where(t => t.EventType.Code == eventCode)
            .Select(t => t.EventType).First();
    }

    public static TimeLineEventCode? GetNextPendingSnapshotTimeLineEventCode(KitSnapshot? snapshot) {
        if (snapshot == null) {
            return TimeLineEventCode.CUSTOM_RECEIVED;
        }

        var eventCodes = Enum.GetValues<TimeLineEventCode>();
        for (var i = 0; i < eventCodes.Length; i++) {
            var code = eventCodes[i];
            if (!SnapshotHasTimelineEvent(snapshot, code)) {
                return code;
            }
        }

        return (TimeLineEventCode?)null;
    }

    public static bool SnapshotHasTimelineEvent(KitSnapshot snapshot, TimeLineEventCode eventCode) {
        switch (eventCode) {
            case TimeLineEventCode.CUSTOM_RECEIVED: return snapshot.CustomReceived != null;
            case TimeLineEventCode.PLAN_BUILD: return snapshot.PlanBuild != null;
            case TimeLineEventCode.BUILD_COMPLETED: return snapshot.BuildCompleted != null;
            case TimeLineEventCode.GATE_RELEASED: return snapshot.GateRelease != null;
            case TimeLineEventCode.WHOLE_SALE: return snapshot.Wholesale != null;
            default: throw new Exception("never");
        }
    }

    public static DateTime? SnapshotTimelineEventDate(KitSnapshot snapshot, TimeLineEventCode eventCode) {
        switch (eventCode) {
            case TimeLineEventCode.CUSTOM_RECEIVED: return snapshot.CustomReceived;
            case TimeLineEventCode.PLAN_BUILD: return snapshot.PlanBuild;
            case TimeLineEventCode.BUILD_COMPLETED: return snapshot.BuildCompleted;
            case TimeLineEventCode.GATE_RELEASED: return snapshot.GateRelease;
            case TimeLineEventCode.WHOLE_SALE: return snapshot.Wholesale;
            default: throw new Exception("never");
        }
    }

    public static TimeLineEventCode? LatestSnapshotEventCode(KitSnapshot snapshot) {
        var eventCodes = Enum.GetValues<TimeLineEventCode>().Reverse();
        foreach (var eventCode in eventCodes) {
            if (SnapshotHasTimelineEvent(snapshot, eventCode)) {
                return eventCode;
            }
        }
        return (TimeLineEventCode?)null;
    }
}
