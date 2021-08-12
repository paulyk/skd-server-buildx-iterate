
namespace SKD.Service {
    public class PartnerStatusLayout {

        // filename
        public static readonly string FILENAME_PREFIX = "PARTNER_STATUS";
        public static readonly string FILENAME_DATE_FORMAT = "yyMMdd_HHmmss";

        // header 
        //
        public static readonly string PST_RECORD_TYPE_VAL = "DTL";
        public static readonly string PST_DATE_FORMAT = "yyyy-MM-dd";
        public static readonly string PST_STATUS_DATE_FORMAT = "yyyy-MM-dd HH:mm:ss";

        // Trailer
        public static readonly string TLR_RECORD_TYPE_VAL = "TLR";
        public static readonly string TLR_FILE_NAME_VAL = "PARTNER_STATUS";

        public class Header {

            public static readonly string HDR_RECORD_TYPE_VAL = "HDR";
            public static readonly string HDR_FILE_NAME_VAL = "PARTNER_STATUS";
            public static readonly string HDR_BATCH_DATE_FORMAT = "yyyy-MM-dd";


            //
            public readonly int HDR_RECORD_TYPE = 3;  // HDR
            public readonly int HDR_FILE_NAME = 20;   // PARTNER_STATUS
            public readonly int HDR_KD_PLANT_GSDB = 5; // Plant Code - HPUDB
            public readonly int HDR_PARTNER_GSDB = 5;  // Partner Code -  GQQLA
            public readonly int HDR_PARTNER_TYPE = 3;   // Partner Type`FP `
            public readonly int HDR_SEQ_NBR = 6;       // run date sequnce number
            public readonly int HDR_BATCH_DATE = 10;    // run date
            public readonly int HDR_FILLER = 248;       // chars
        }

        public class Trailer {
            public readonly int TLR_RECORD_TYPE = 3;      // TLR
            public readonly int TLR_FILE_NAME = 20;       // PARTNER_STATUS
            public readonly int TLR_KD_PLANT_GSDB = 5;    // Plant Code - HPUDB
            public readonly int TLR_PARTNER_GSDB = 5;      // Partner Code -  GQQLA
            public readonly int TLR_TOTAL_RECORDS = 10;   // Record count
            public readonly int TLR_FILLER = 257;          //
        }

        public class Detail {
            public readonly int PST_RECORD_TYPE = 3;          // DTL
            public readonly int PST_TRAN_TYPE = 1;            // TxType
            public readonly int PST_LOT_NUMBER = 15;           // lot no
            public readonly int PST_KIT_NUMBER = 17;           // kit no
            public readonly int PST_PHYSICAL_VIN = 17;         // vin 
            public readonly int PST_BUILD_DATE = 10;           // plan build date (original ?)
            public readonly int PST_ACTUAL_DEALER_CODE = 9;
            public readonly int PST_ENGINE_SERIAL_NUMBER = 20;
            public readonly int PST_CURRENT_STATUS = 4;       // Ford= Timeline Event Code 
            public readonly int PST_IP1R_STATUS_DATE = 20;    // blank 
            public readonly int PST_IP1S_STATUS_DATE = 20;    // blank
            public readonly int PST_IP2R_STATUS_DATE = 20;    // blank
            public readonly int PST_IP2S_STATUS_DATE = 20;    // blank
            public readonly int PST_FPRE_STATUS_DATE = 20;    // custom receive
            public readonly int PST_FPBP_STATUS_DATE = 20;    // plan build
            public readonly int PST_FPBC_STATUS_DATE = 20;    // build completed
            public readonly int PST_FPGR_STATUS_DATE = 20;    // gate release 
            public readonly int PST_FPWS_STATUS_DATE = 20;    // whole sale
            public readonly int PST_FILLER = 24;               //            
        }
    }


}