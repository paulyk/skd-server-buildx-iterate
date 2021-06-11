using System;

namespace SKD.Common {
    public record ComponentSerialInput (
        Guid KitComponentId,
        string Serial1, 
        string Serial2 = "",
        Boolean Replace = false
    );
}