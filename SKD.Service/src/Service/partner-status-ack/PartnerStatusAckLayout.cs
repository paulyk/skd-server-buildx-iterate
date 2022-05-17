namespace SKD.Service;

public class PartnerStatusAckLayout {
    public class HeaderLine {

        public readonly int HDR_RECORD_TYPE = 3;
        public readonly int HDR_FILE_NAME = 20;
        public readonly int HDR_KD_PLANT_GSDB = 5;
        public readonly int HDR_PARTNER_GSDB = 5;
        public readonly int HDR_PARTNER_TYPE = 3;
        public readonly int HDR_SEQ_NBR = 6;
        public readonly int HDR_BATCH_DATE = 10;
        public readonly int HDR_FILLER = 8;

    }

    public class DetailLine {
        public readonly int PST_ACK_RECORD_TYPE = 3;
        public readonly int PST_ACK_FILE_STATUS = 10;
        public readonly int PST_ACK_TOTAL_DTL_RECORD = 10;
        public readonly int PST_ACK_TOTAL_DTL_ACCEPTED = 10;
        public readonly int PST_ACK_TOTAL_DTL_REJECTED = 10;
        public readonly int PST_FILLER = 17;
    }
}