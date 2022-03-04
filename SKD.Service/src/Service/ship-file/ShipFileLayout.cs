namespace SKD.Service;

/// <summary>Shipment Import File Layout</summary>
public class ShipFileLayout {
    public class HeaderLine {
        public readonly int HDR_RECORD_ID = 7;
        public readonly int HDR_CD_PLANT = 5;
        public readonly int HDR_FILE_NAME = 8;
        public readonly int HDR_DATE_CREATED = 8;
        public readonly int HDR_TIME_CREATED = 6;
        public readonly int HDR_BRIG_SEQ_NO = 4;   // Shipment.shipmentSequence
        public readonly int HDR_PROGRAM = 8;
    }

    public class LotLine {
        public readonly int NO_SEQUENCE = 6;
        public readonly int CD_TYPE = 2;    // 01
        public readonly int LOT_NUMBER = 15;   // ShipmentLot.lotNo
        public readonly int SHIP_TO_LOC = 5;
        public readonly int SOLD_TO_LOC = 5;
        public readonly int ALT_SHIP_TO_LOC = 5;
        public readonly int CUSTOMER = 5;   // Shipment.productionPlant 
        public readonly int NUMBER_OF_INVOICES = 2;
        public readonly int CD_AIAG_TRANSIT = 2;
        public readonly int IN_ASN_CORRECTION = 1;
    }

    public class InvoiceLine {
        public readonly int NO_SEQUENCE = 6;
        public readonly int CD_TYPE = 2;      // 02
        public readonly int DS_CONV_INITIALS = 4;
        public readonly int NO_CONVEYANCE = 7;
        public readonly int NO_INVOICE = 11;    // ShipmentInvoice.invoiceNo
        public readonly int DT_SHIPPED = 10;    // ShipmentInvoice.shipDate
    }

    public class PartLine {
        public readonly int NO_SEQUENCE = 6;
        public readonly int CD_TYPE = 2;               // 03
        public readonly int NO_CNTR_BASE = 9;
        public readonly int NO_CNTR_SUFFIX = 8;
        public readonly int QT_CONTAINER = 1;
        public readonly int FILLER_1 = 1;
        public readonly int HANDLER_UNIT_CODE = 7;     // [a-z0-9]
        public readonly int CNTR_LENGTH = 8;
        public readonly int FILLER_2 = 1;
        public readonly int CNTR_WIDTH = 8;
        public readonly int FILLER_3 = 1;
        public readonly int CNTR_HEIGHT = 8;

        public readonly int NO_PART = 30;       // ShipmentPart.partNo
        public readonly int NO_PART_BUS = 30;   // ShipmentPart.customerPartNo
        public readonly int DS_PART = 30;       // ShipmentPart.customerPartDesc
        public readonly int QT_SHIPPED = 7;     // ShipmentPart.quantity
        public readonly int WT_PART = 10;
        public readonly int CD_UNIT_MEAS_USAGE = 2;
        public readonly int CD_PACKING_SLIP = 20;
    }
}

