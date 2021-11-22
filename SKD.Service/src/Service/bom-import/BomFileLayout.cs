namespace SKD.Service;

public class BomFileLayout {

    public class Header {
        public int HDR_RECORD_TYPE = 7;
        public int HDR_KD_PLANT_GSDB = 5;
        public int HDR_FILE_NAME = 8;
        public int HDR_DATE_CREATED = 8;
        public int HDR_TIME_CREATED = 6;
        public int HDR_PARTNER_GSDB = 5;    // production plant code
        public int HDR_FTC_GSDB = 5;
        public int HDR_BRIDGE_SEQ_NBR = 4;   // sequnce number
        public int HDR_PROGRAM_NAME = 8;
        // public int HDR_FILLER = 144;
    }

    public class Detail {

        public int KBM_RECORD_TYPE = 3;
        public int KBM_LOT_NUMBER = 15;            // group by lot
        public int KBM_KIT_NUMBER = 2;             // kit sequence number
        public int KBM_RVSD_TGT_SHP_DTE = 10;
        public int KBM_ORDER_STATUS = 3;
        public int KBM_MARKET_TERRITORY = 5;
        public int KBM_VEHICLE_LINE = 3;
        public int KBM_COMMODITY_NAME = 22;
        public int KBM_KIT_PART_TYPE = 3;           // filter part type == 'kit' + 'lot'
        public int KBM_NO_PART = 30;                // group by part number & sum quantity
        public int KBM_PART_DESCRIPTION = 34;       // description
        public int KBM_NET_PART_QTY = 16;           // quantity
                                                    // public int FILLER = 54;

    }
}
