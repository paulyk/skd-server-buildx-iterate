
namespace SKD.Service;

public class KitSnapshotRunDTO {
    public DateTime RunDate { get; set; }
    public int Sequence { get; set; }
    public string PlantCode { get; set; }
    public string PartnerPlantCode { get; set; }
    public string PartnerPlantType { get; set; }
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    public class Entry {
        public PartnerStatus_ChangeStatus TxType { get; set; }
        public TimeLineEventCode CurrentTimeLineCode { get; set; }
        public string LotNo { get; set; }
        public string KitNo { get; set; }
        public string VIN { get; set; }
        public string DealerCode { get; set; }
        public string EngineSerialNumber { get; set; }
        public DateTime? CustomReceived { get; set; }
        public DateTime? OriginalPlanBuild { get; set; }
        public DateTime? PlanBuild { get; set; }
        public DateTime? BuildCompleted { get; set; }
        public DateTime? GateRelease { get; set; }
        public DateTime? Wholesale { get; set; }
    }
}
