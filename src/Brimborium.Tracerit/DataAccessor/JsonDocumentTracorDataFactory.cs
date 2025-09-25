namespace Brimborium.Tracerit.DataAccessor;

public sealed class JsonDocumentTracorDataFactory
    : ITracorDataAccessorFactory<System.Text.Json.JsonDocument> {
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is System.Text.Json.JsonDocument document) {
            tracorData = new JsonDocumentTracorData(document);
            return true;
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(System.Text.Json.JsonDocument value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new JsonDocumentTracorData(value);
        return true;
    }
}
