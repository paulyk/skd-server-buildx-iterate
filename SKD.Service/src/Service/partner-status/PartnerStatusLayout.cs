
namespace SKD.Service {
    public class PartnerStatusLayout {

        public static string HDR_RECORD_TYPE_VAL = "HDR";
        public static string HDR_FILE_NAME_VAL = "PARTNER_STATUS";
        public static string PST_RECORD_TYPE_VAL = "DTL";
        public static string HDR_BATCH_DATE_FORMAT = "yyyy-MM-dd";
        public static string PST_DATE_FORMAT = "yyyy-MM-dd";
        public static string PST_STATUS_DATE_FORMAT = "yyyy-MM-dd hh:mm:ss";
        public class Header {
            public int HDR_RECORD_TYPE = 3;  // HDR
            public int HDR_FILE_NAME = 20;   // PARTNER_STATUS
            public int HDR_KD_PLANT_GSDB = 5; // Plant Code - HPUDB
            public int HDR_PARTNER_GSDB = 5;  // Partner Code -  GQQLA
            public int HDR_PARTNER_TYPE = 3;   // Partner Type`FP `
            public int HDR_SEQ_NBR = 6;       // run date sequnce number
            public int HDR_BATCH_DATE = 10;    // run date
            public int HDR_FILLER = 248;       // chars
        }

        public class Trailer {
            public int TLR_RECORD_TYPE = 3;      // TLR
            public int TLR_FILE_NAME = 20;       // PARTNER_STATUS
            public int TLR_KD_PLANT_GSDB = 5;    // Plant Code - HPUDB
            public int TLR_PARTNER_GSDB = 5;      // Partner Code -  GQQLA
            public int TLR_TOTAL_RECORDS = 10;   // Record count
            public int TLR_FILLER = 257;          //
        }

        public class Detail {
            public int PST_RECORD_TYPE = 3;          // DTL
            public int PST_TRAN_TYPE = 1;            // TxType
            public int PST_LOT_NUMBER = 15;           // lot no
            public int PST_KIT_NUMBER = 17;           // kit no
            public int PST_PHYSICAL_VIN = 17;         // vin 
            public int PST_BUILD_DATE = 10;           // plan build date
            public int PST_ACTUAL_DEALER_CODE = 9;
            public int PST_ENGINE_SERIAL_NUMBER = 20;
            public int PST_CURRENT_STATUS = 4;       // Ford= Timeline Event Code 
            public int PST_IP1R_STATUS_DATE = 20;    // blank 
            public int PST_IP1S_STATUS_DATE = 20;    // blank
            public int PST_IP2R_STATUS_DATE = 20;    // blank
            public int PST_IP2S_STATUS_DATE = 20;    // blank
            public int PST_FPRE_STATUS_DATE = 20;    // custom receive
            public int PST_FPBP_STATUS_DATE = 20;    // plan build
            public int PST_FPBC_STATUS_DATE = 20;    // build completed
            public int PST_FPGR_STATUS_DATE = 20;    // gate release 
            public int PST_FPWS_STATUS_DATE = 20;    // whole sale
            public int PST_FILLER = 24;               //            
        }
    }


}