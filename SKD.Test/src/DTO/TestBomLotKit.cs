
namespace SKD.Test;

public class TestBomLotKit {
    public string PlantCode { get; set; }
    public int BomSequence { get; set; }

    public List<Lot> Lots { get; set; } = new List<Lot>();

    public class Lot {
        public string LotNo { get; set; }
        public List<Vehicle> Vehicles { get; set; } = new List<Vehicle>();


        public class Vehicle {
            public string VIN { get; set; }
            public string KitNo { get; set; }
            public string ModelCode { get; set; }
        }
    }
}
