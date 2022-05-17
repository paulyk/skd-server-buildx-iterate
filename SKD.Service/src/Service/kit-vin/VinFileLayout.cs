namespace SKD.Service;

/// <summary>VIN Import File Layout</summary>
public class VinFileLayout {
    public class HeaderLine {
        public readonly int HDR_RECORD_TYPE = 3;
        public readonly int HDR_FILE_NAME = 20;
        public readonly int HDR_KD_PLANT_GSDB = 5;  // Kitting Plant GSDB
        public readonly int HDR_PARTNER_GSDB = 5;   // Partner Plant GSDB
        public readonly int HDR_PARTNER_TYPE = 3;
        public readonly int HDR_SEQ_NBR = 6;        // Sequential number identifying= file number for 
                                                    // KD Plant and Patner Plant combination
        public readonly int HDR_BATCH_DATE = 10;
        public readonly int HDR_FILLER = 48;

    }

    public class DetailLine {
        public readonly int KVM_RECORD_TYPE = 3;
        public readonly int KVM_LOT_NUMBER = 15;
        public readonly int KVM_KIT_NUMBER = 17;
        public readonly int KVM_PHYSICAL_VIN = 17;
        public readonly int KVM_BUILD_DATE = 10;
        public readonly int KVM_FILLER = 38;
    }
}
