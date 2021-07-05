namespace SKD.Model {
    public static class EntityFieldLen {

        public static int Id = 36;
        public static int Email = 320;

        public static int LotNo = 15;
        public static int KitNo = 17;
        public static int VIN = 17;
        public static int LotNote = 100;

        public static int Component_Code = 10;
        public static int Component_Name = 100;
        public static int Component_SerialCaptureRequirement = 30;
        public static int Component_FordComponentType = 5;

        public static int ProductionStation_Code = 15;
        public static int ProductionStation_Name = 100;

        public static int Plant_Code = 5;
        public static int PartnerPlant_Code = 5;
        public static int PartnerPlant_Type = 2;
        public static int Plant_Name = 100;


        public static int Part_No = 30;
        public static int Part_Desc = 50;


        public static int VehicleModel_Code = 7;
        public static int VehicleModel_Description = 100;
        public static int VehicleModel_ModelYear = 4;
        public static int VehicleModel_Model = 30;
        public static int VehicleModel_Series = 30;
        public static int VehicleModel_Body = 30;

        public static int ComponentSerial_Min = 5;
        public static int ComponentSerial = 100;
        public static int ComponentSerial_DCWS_ResponseCode = 100;

        public static int DCWSResponse_Code = 50;
        public static int DCWS_ErrorMessage = 1000;

        public static int CreatedBy = 255;
        public static int VehicleComponent_PrerequisiteSequence = 50;

        public static int Shipment_InvoiceNo = 11;
        public static int HandlingUnit_Code = 7;

        public static int Bom_SequenceNo = 4;
        public static int KitVinImport_Sequence = 6;

        public static int BomPart_LotNo = 15;
        public static int BomPart_PartNo = 30;
        public static int BomPart_PartDesc = 34;

        public static int Event_Code = 30;
        public static int Event_Description = 200;
        public static int Event_Note = 200;
    }
}