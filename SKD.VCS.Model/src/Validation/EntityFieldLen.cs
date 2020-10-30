namespace SKD.VCS.Model {
    public static class EntityFieldLen {

        public static int Id = 36;
        public static int Email = 320;
        public static int Component_Code = 10;
        public static int Component_Name = 100;
        public static int Component_FordComponentType = 5;

        public static int ProductionStation_Code = 15;
        public static int ProductionStation_Name = 100;

        public static int ProductionPlant_Code = 10;
        public static int ProductionPlant_Name = 100;

        public static int Vehicle_VIN = 17;
        public static int Vehicle_LotNo = 15;
        public static int Vehicle_KitNo = 17;

        public static int VehicleModel_Code = 11;
        public static int VehicleModel_Name = 100;
        public static int VehicleModel_Type = 4;

        public static int ComponentScan_ScanEntry_Min = 5;
        public static int ComponentScan_ScanEntry = 100;
        public static int ComponentScan_DCWS_ResponseCode = 100;

        public static int DCWSResponse_Code = 50;
        public static int DCWS_ErrorMessage = 1000;

        public static int CreatedBy = 255;
        public static int VehicleComponent_PrerequisiteSequence = 50;

        public static int Shipment_SequenceNo = 4;
        public static int Shipment_LotNo = 15;
        public static int Shipment_InvoiceNo = 11;

        public static int Shipment_PartNo = 30;
        public static int Shipment_CustomerPartNo = 30;
		public static int Shipment_CustomerPartDesc = 30;
    }
}