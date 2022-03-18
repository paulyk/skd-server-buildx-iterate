#nullable enable

namespace SKD.Service;

public class NextKitSnapshotService {

    public class NextSnapshotInput {
        public Kit Kit = new Kit();
        public KitSnapshot? PriorSnapshot = null;
        public string VIN = "";
        public string DealerCode = "";
        public string EngineSerialNumber = "";
        public IEnumerable<KitTimelineEventType> TimelineEventTypes = new List<KitTimelineEventType>();
        public IEnumerable<KitTimelineEvent> KitTimelineEvents = new List<KitTimelineEvent>();
    }


    public static KitSnapshot CreateNextSnapshot(NextSnapshotInput input) {
        // setup
        var customReceiveEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.CUSTOM_RECEIVED);
        var buildCompletedEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.BUILD_COMPLETED);
        var wholeSaleEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.WHOLE_SALE);

        //
        var startSequence = input.TimelineEventTypes.OrderBy(t => t.Sequence).Select(t => t.Sequence).First();

        var priorEventType = input.PriorSnapshot?.KitTimeLineEventType;
        var priorEventSequence = priorEventType != null ? priorEventType.Sequence : startSequence - 1;

        // get next event sequence
        var nextEventSequence = priorEventSequence > wholeSaleEventType.Sequence
                ? wholeSaleEventType.Sequence
                : priorEventSequence + 1;

        var kitTimelineEventForSequence = input.KitTimelineEvents.FirstOrDefault(t => t.EventType.Sequence == nextEventSequence);
        nextEventSequence = kitTimelineEventForSequence != null ? nextEventSequence : priorEventSequence;

        KitTimelineEventType nextKitTimelineEventType = input.TimelineEventTypes.First(t => t.Sequence == nextEventSequence);

        SnapshotChangeStatus nextChangeStatusCode = priorEventSequence < startSequence
            ? SnapshotChangeStatus.Added
            : nextEventSequence == wholeSaleEventType.Sequence
                ? SnapshotChangeStatus.Final
                : nextEventSequence != priorEventSequence
                    ? SnapshotChangeStatus.Changed
                    : SnapshotChangeStatus.NoChange;

        // next snapshot from prior snapshot
        var nextSnapshot = new KitSnapshot {
            KitId = input.Kit.Id,
            Kit = input.Kit,
            KitTimeLineEventType = nextKitTimelineEventType,
            ChangeStatusCode = nextChangeStatusCode,

            VIN = input.PriorSnapshot?.VIN ?? "",
            DealerCode = input.PriorSnapshot?.DealerCode ?? "",
            EngineSerialNumber = input.PriorSnapshot?.EngineSerialNumber,

            CustomReceived = input.PriorSnapshot?.CustomReceived,
            PlanBuild = input.PriorSnapshot?.PlanBuild,
            VerifyVIN = input.PriorSnapshot?.VerifyVIN,
            BuildCompleted = input.PriorSnapshot?.BuildCompleted,
            GateRelease = input.PriorSnapshot?.GateRelease,
            Wholesale = input.PriorSnapshot?.Wholesale,

            OrginalPlanBuild = input.PriorSnapshot?.OrginalPlanBuild
        };

        var nextEventCode = input.TimelineEventTypes.First(t => t.Sequence == nextEventSequence).Code;

        switch (nextEventCode) {
            case TimeLineEventCode.CUSTOM_RECEIVED: {
                    nextSnapshot.CustomReceived = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    break;
                }
            case TimeLineEventCode.PLAN_BUILD: {
                    nextSnapshot.PlanBuild = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    nextSnapshot.OrginalPlanBuild = nextSnapshot.PlanBuild;
                    break;
                }
            case TimeLineEventCode.VERIFY_VIN: {
                    nextSnapshot.VerifyVIN = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    break;
                }
            case TimeLineEventCode.BUILD_COMPLETED: {
                    nextSnapshot.BuildCompleted = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    nextSnapshot.VIN = input.VIN;
                    nextSnapshot.EngineSerialNumber = input.EngineSerialNumber;
                    break;
                }
            case TimeLineEventCode.GATE_RELEASED: {
                    nextSnapshot.GateRelease = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    break;
                }
            case TimeLineEventCode.WHOLE_SALE: {
                    nextSnapshot.Wholesale = input.KitTimelineEvents.First(t => t.EventType.Code == nextEventCode).EventDate;
                    nextSnapshot.DealerCode = input.DealerCode;
                    break;
                }
            default: {
                    break;
                }

        }

        return nextSnapshot;
    }

    public static bool DuplicateKitSnapshot(KitSnapshot snapshot_1, KitSnapshot snapshot_2) {
        if (snapshot_1.Kit.Id != snapshot_2.Kit.Id) {
            throw new Exception($"Kit IDs different");
        }

        var duplicate = 
            snapshot_1.KitId == snapshot_2.KitId
            && snapshot_1.ChangeStatusCode == snapshot_2.ChangeStatusCode
            && snapshot_1.KitTimeLineEventType.Code == snapshot_2.KitTimeLineEventType.Code
            
            && (snapshot_1.VIN ?? "") == (snapshot_2.VIN ?? "") 
            && (snapshot_1.DealerCode ?? "") == (snapshot_2.DealerCode ?? "")
            && (snapshot_1.EngineSerialNumber ?? "") == (snapshot_2.EngineSerialNumber ?? "")

            && snapshot_1.OrginalPlanBuild == snapshot_2.OrginalPlanBuild

            && snapshot_1.CustomReceived == snapshot_2.CustomReceived
            && snapshot_1.PlanBuild == snapshot_2.PlanBuild
            && snapshot_1.VerifyVIN == snapshot_2.VerifyVIN
            && snapshot_1.BuildCompleted == snapshot_2.BuildCompleted
            && snapshot_1.GateRelease == snapshot_2.GateRelease
            && snapshot_1.Wholesale == snapshot_2.Wholesale
            ;

        return duplicate;
    }
}