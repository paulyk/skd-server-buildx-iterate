namespace SKD.Model;
public static class EntityFieldLen {

    public readonly static int Id = 36;
    public readonly static int Email = 320;

    public readonly static int LotNo = 15;
    public readonly static int KitNo = 17;
    public readonly static int VIN = 17;
    public readonly static int LotNote = 100;

    public readonly static int Component_Code = 10;
    public readonly static int Component_Name = 100;
    public readonly static int Component_SerialRule = 30;
    public readonly static int Component_FordComponentType = 5;

    public readonly static int ProductionStation_Code = 15;
    public readonly static int ProductionStation_Name = 100;

    public readonly static int Plant_Code = 5;
    public readonly static int PartnerPlant_Code = 5;
    public readonly static int PartnerPlant_Type = 2;
    public readonly static int Plant_Name = 100;


    public readonly static int Part_No = 30;
    public readonly static int Part_Desc = 50;


    public readonly static int VehicleModel_Code = 7;
    public readonly static int VehicleModel_Description = 100;
    public readonly static int VehicleModel_ModelYear = 10;
    public readonly static int VehicleModel_Model = 30;
    public readonly static int VehicleModel_Series = 30;
    public readonly static int VehicleModel_Body = 30;

    public readonly static int ComponentSerial_Min = 5;
    public readonly static int ComponentSerial = 100;
    public readonly static int ComponentSerial_DCWS_ResponseCode = 100;

    public readonly static int DCWSResponse_Code = 50;
    public readonly static int DCWS_ErrorMessage = 1000;

    public readonly static int CreatedBy = 255;
    public readonly static int VehicleComponent_PrerequisiteSequence = 50;

    public readonly static int Shipment_InvoiceNo = 11;
    public readonly static int HandlingUnit_Code = 7;

    public readonly static int Bom_SequenceNo = 4;
    public readonly static int KitVinImport_Sequence = 6;

    public readonly static int BomPart_LotNo = 15;
    public readonly static int BomPart_PartNo = 30;
    public readonly static int BomPart_PartDesc = 34;

    public readonly static int Event_Code = 30;
    public readonly static int Event_Description = 200;
    public readonly static int Event_Note = 200;
}
