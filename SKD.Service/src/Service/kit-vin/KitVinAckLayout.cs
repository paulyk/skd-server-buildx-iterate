
namespace SKD.Service {

    /// <summary>Kit VIN import acknowledgement file layout</summary>
    /// <remarks>Ford interface requires acknowledgment when VINs are imported</remarks>
    public class KitVinAckLayout {
        // filename
        public readonly static string FILENAME_DATE_FORMAT = "yyMMdd_HHmmss";

        public static readonly string HDR_RECORD_TYPE_VAL = "HDR";
        public static readonly string HDR_FILE_NAME_VAL = "KIT_VIN_MAP_ACK";
        public static readonly string HDR_BATCH_DATE_FORMAT = "yyyy-MM-dd";

        public static readonly string DTL_KVM_ACK_RECORD_TYPE_VAL = "DTL";
        public static readonly string DTL_KVM_ACK_FILE_STATUS_ACCEPTED = "ACCEPTED  ";
        public static readonly string DTL_KVM_ACK_FILE_STATUS_REJECTED = "REJECTED  ";

        public class Header {
            public int HDR_RECORD_TYPE = 3;   // 'HDR'
            public int HDR_FILE_NAME = 20;    // 'KIT_VIN_MAP_ACK'
            public int HDR_KD_PLANT_GSDB = 5; // Kitting Plant GSDB  (plant code)
            public int HDR_PARTNER_GSDB = 5;  // Partner Plant GSDB  (Partner code should be in in DB)
            public int HDR_PARTNER_TYPE = 3;  // blank
            public int HDR_SEQ_NBR = 6;       // Same sequence number as received from Ford
            public int HDR_BATCH_DATE = 10;   // Date on which the interface file was created. YYYY-MM-DD
            public int HDR_FILLER = 8;        // blank
        }

        public class Detail {
            public int KVM_ACK_RECORD_TYPE = 3;           // 'DTL'
            public int KVM_ACK_FILE_STATUS = 10;          // ACCEPTED / REJECTED
            public int KVM_ACK_TOTAL_DTL_RECORD = 10;     // kit/vin count
            public int KVM_ACK_TOTAL_DTL_ACCEPTED = 10;   // number of records in kit vin payload
            public int KVM_ACK_TOTAL_DTL_REJECTED = 10;   // number of records in kit vin payload
            public int KVM_FILLER = 17;                   // blank
        }


        public class Trailer {
            public static readonly string TLR_RECORD_TYPE_VAL = "TLR";
            public static readonly string TLR_FILE_NAME_VAL = "KIT_VIN_MAP_ACK";

            public int TLR_RECORD_TYPE = 3;       // 'TLR'
            public int TLR_FILE_NAME = 20;        // 'KIT_VIN_MAP_ACK'
            public int TLR_KD_PLANT_GSDB = 5;     // plant code
            public int TLR_PARTNER_GSDB = 5;      // partner code
            public int TLR_TOTAL_RECORDS = 10;    // 3 (hard coded)
            public int TLR_FILLER = 17;           // blank
        }
    }
}