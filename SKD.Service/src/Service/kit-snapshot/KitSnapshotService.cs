#nullable enable

namespace SKD.Service;

public class KitSnapshotService {

    private readonly SkdContext context;
    public static readonly int WholeSateCutOffDays = 7;
    public static readonly int PlanBuildLeadTimeDays = 7;

    public KitSnapshotService(SkdContext ctx) {
        this.context = ctx;
    }

    public async Task<MutationResult<SnapshotDTO>> GenerateSnapshot(KitSnapshotInput input) {
        // set to current date if null
        input.RunDate = input.RunDate ?? DateTime.UtcNow;

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
            RunDate = input.RunDate.Value,
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

            // check for gap in snapshot timeline
            if (priorSnapshot != null) {
                var (hasGap, eventCode) = SnapshotHasTimelineGap(priorSnapshot);
                if (hasGap) {
                    throw new Exception($"kit {kit.KitNo} snapshot has missing date for {eventCode}");
                }
            }

            var snapshotGenerator = new NextKitSnapshotGenerator(new NextKitSnapshotGenerator.GenNextSnapshotInput{
                KitId = kit.Id,
                PriorSnapshot = priorSnapshot,
                VIN = kit.VIN,
                DealerCode = kit.Dealer?.Code ?? "",
                EngineSerialNumber = await GetEngineSerialNumber(kit, input.EngineComponentCode),
                TimelineEventTypes = await context.KitTimelineEventTypes.ToListAsync(),
                KitTimelineEvents = kit.TimelineEvents.Where(t => t.RemovedAt == null).ToList()
            });
            
            var snapshot = snapshotGenerator.GenerateNextSnapshot();

            kitSnapshotRun.KitSnapshots.Add(snapshot);
        }

        // reject if no changes
        if (input.RejectIfNoChanges) {
            bool hasChanges = kitSnapshotRun.KitSnapshots.Any(x => x.ChangeStatusCode != SnapshotChangeStatus.NoChange);
            if (!hasChanges) {
                result.Errors.Add(new Error("", "No changes since last snapshot"));
                return result;
            }
        }

        context.KitSnapshotRuns.Add(kitSnapshotRun);
        // save
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
            .Include(t => t.Lot).ThenInclude(t => t.ShipmentLots.Where(t => t.RemovedAt == null))
            .Include(t => t.Snapshots.Where(t => t.RemovedAt == null).OrderBy(t => t.KitTimeLineEventType.Sequence))
            .Include(t => t.TimelineEvents.Where(t => t.RemovedAt == null)).ThenInclude(t => t.EventType)
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
                    ev.CreatedAt.AddDays(WholeSateCutOffDays) > runDate
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
                VerifyVIN = entry.VerifyVIN,
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

        // already generated for snapshot for this runDate
        if (!input.AllowMultipleSnapshotsPerDay) {
            var snapshotForRunDateExists = await context.KitSnapshotRuns
                .AnyAsync(t => t.Plant.Code == input.PlantCode && t.RunDate.Date == input.RunDate.Value.Date);

            if (snapshotForRunDateExists) {
                errors.Add(new Error("", $"Snapshot already take for this date:  {input.PlantCode} - {DateTime.UtcNow.Date.ToString("yyyy-MM-dd")}"));
            }
        }

        return errors;
    }


    /// <remark>
    /// returns VIN if pending status is BUILD_COMPLETED event otherwise return empty string
    /// <remark>
    private async Task<string> GetEngineSerialNumber(Kit kit, string engineComponentCode) {

        var verifiedComponentSerial = await context.ComponentSerials
            .Where(t => t.KitComponent.Kit.KitNo == kit.KitNo)
            .Where(t => t.KitComponent.Component.Code == engineComponentCode)
            .Where(t => t.VerifiedAt != null && t.RemovedAt == null)
            .FirstOrDefaultAsync();

        return (verifiedComponentSerial?.Serial1 + " " + verifiedComponentSerial?.Serial2).Trim();
    }

    private async Task<KitSnapshot?> GetPriorKitSnapshot(Guid kitId)
        => await context.KitSnapshots
            .OrderByDescending(t => t.KitSnapshotRun.Sequence)
            .Where(t => t.RemovedAt == null)
            .Where(t => t.KitSnapshotRun.RemovedAt == null)
            .FirstOrDefaultAsync(t => t.KitId == kitId);

    public static DateTime? SnapshotTimelineEventDate(KitSnapshot snapshot, TimeLineEventCode eventCode) {
        switch (eventCode) {
            case TimeLineEventCode.CUSTOM_RECEIVED: return snapshot.CustomReceived;
            case TimeLineEventCode.PLAN_BUILD: return snapshot.PlanBuild;
            case TimeLineEventCode.VERIFY_VIN: return snapshot.VerifyVIN;
            case TimeLineEventCode.BUILD_COMPLETED: return snapshot.BuildCompleted;
            case TimeLineEventCode.GATE_RELEASED: return snapshot.GateRelease;
            case TimeLineEventCode.WHOLE_SALE: return snapshot.Wholesale;
            default: throw new Exception("never");
        }
    }

    public (bool hasGap, TimeLineEventCode? eventCode) SnapshotHasTimelineGap(KitSnapshot snapshot) {
        var eventCodeDates = Enum.GetValues<TimeLineEventCode>()
            .Select(t => new {
                eventCode = t,
                date = SnapshotTimelineEventDate(snapshot, t)
            }).ToArray();

        // find gap in array
        for (var i = 0; i < eventCodeDates.Length; i++) {
            if (i - 1 >= 0) {
                if (eventCodeDates[i - 1].date == null && eventCodeDates[i].date != null) {
                    return (hasGap: true, eventCode: eventCodeDates[i - 1].eventCode);
                }
            }
        }
        return (false, null);
    }
}
