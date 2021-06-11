namespace SKD.Common{
    public record ReceiveHandlingUnitInput (
        string HandlingUnitCode,
        bool Remove = false
    );
}