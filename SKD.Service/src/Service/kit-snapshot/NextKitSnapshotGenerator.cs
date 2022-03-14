#nullable enable

namespace SKD.Service;

public class NextKitSnapshotGenerator {


    private KitTimelineEventType CustomReceiveEventType;
    private KitTimelineEventType BuildCompletedEventType;
    private KitTimelineEventType WholeSaleEventType;

    public class GenNextSnapshotInput {
        public Guid KitId = Guid.NewGuid();
        public KitSnapshot? PriorSnapshot = null;
        public string VIN = "";
        public string DealerCode = "";
        public string EngineSerialNumber = "";
        public IEnumerable<KitTimelineEventType> TimelineEventTypes = new List<KitTimelineEventType>();
        public IEnumerable<KitTimelineEvent> KitTimelineEvents = new List<KitTimelineEvent>();
    }

    private GenNextSnapshotInput input;
    public NextKitSnapshotGenerator(GenNextSnapshotInput input) {

        this.input = input;

        this.CustomReceiveEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.CUSTOM_RECEIVED);
        this.BuildCompletedEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.BUILD_COMPLETED);
        this.WholeSaleEventType = input.TimelineEventTypes.First(t => t.Code == TimeLineEventCode.WHOLE_SALE);

        if (input.KitTimelineEvents.Count() == 0) {
            throw new Exception("No KitTimelineEvents");
        }
    }

    public KitSnapshot GenerateNextSnapshot() {
        var startSequence = input.TimelineEventTypes.OrderBy(t => t.Sequence).Select(t => t.Sequence).First();

        var priorEventType = input.PriorSnapshot?.KitTimeLineEventType;
        var priorEventSequence = priorEventType != null ? priorEventType.Sequence : startSequence - 1;

        // get next event sequence
        var nextEventSequence = priorEventSequence > WholeSaleEventType.Sequence
                ? WholeSaleEventType.Sequence
                : priorEventSequence + 1;

        var kitTimelineEventForSequence = input.KitTimelineEvents.FirstOrDefault(t => t.EventType.Sequence == nextEventSequence);
        nextEventSequence = kitTimelineEventForSequence != null ? nextEventSequence : priorEventSequence;

        KitTimelineEventType nextKitTimelineEventType = input.TimelineEventTypes.First(t => t.Sequence == nextEventSequence);

        SnapshotChangeStatus nextChangeStatusCode = priorEventSequence < startSequence
            ? SnapshotChangeStatus.Added
            : nextEventSequence == WholeSaleEventType.Sequence
                ? SnapshotChangeStatus.Final
                : nextEventSequence != priorEventSequence
                    ? SnapshotChangeStatus.Changed
                    : SnapshotChangeStatus.NoChange;

        // next snapshot from prior snapshot
        var nextSnapshot = new KitSnapshot {
            KitId = input.KitId,
            KitTimeLineEventType = nextKitTimelineEventType,
            ChangeStatusCode = nextChangeStatusCode,

            VIN = input.PriorSnapshot?.VIN,
            DealerCode = input.PriorSnapshot?.DealerCode,
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
}