

namespace SKD.VCS.Seed {
    public class Vehicle_Seed_DTO {
        public string vin { get; set; }
        public string kitNo { get; set; }
        public string lotNo { get; set; }
        public string modelId { get; set; }
    }

    public class VehicleModel_Seed_DTO{
        public string code { get; set; }
        public string name { get; set; }
    }
    public class Component_Seed_DTO {
        public string code { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class VehicleModelComponent_Seed_DTO {
        public string modelCode { get; set; }
        public string componentCode { get; set; }
        public int sequence { get; set; }
        public string prerequisite { get; set; }
    }
}