using System;

namespace SKD.Model {
    public record ComponentSerialInput (
        Guid KitComponentId,
        string Serial1, 
        string Serial2 = "",
        Boolean Replace = false
    );
}