namespace SKD.Service;

public record ReceiveHandlingUnitInput(
    string HandlingUnitCode,
    bool Remove = false
);
