namespace SKD.Service;

public record ComponentSerialInput(
    Guid KitComponentId,
    string Serial1,
    string Serial2 = "",
    Boolean Replace = false
);
