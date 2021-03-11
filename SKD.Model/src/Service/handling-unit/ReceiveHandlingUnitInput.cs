namespace SKD.Model {
    public record ReceiveHandlingUnitInput (
        string HandlingUnitCode,
        bool Remove = false
    );
}