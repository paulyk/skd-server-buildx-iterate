

namespace SKD.Seed {
   
    public class VehicleModel_MockData_DTO{
        public string code { get; set; }
        public string name { get; set; }
    }
    public class Component_MockData_DTO {
        public string code { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class CmponentStation_McckData_DTO {
        public string componentCode { get; set; }
        public string stationCode { get; set; }
    }

    public class ProductionStation_Mock_DTO {
        public string code { get; set; }
        public string name { get; set; }
        public int sortOrder { get; set; }
    }
}