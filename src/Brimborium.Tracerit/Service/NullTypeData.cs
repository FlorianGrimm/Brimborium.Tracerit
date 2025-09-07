namespace Brimborium.Tracerit.Service;

public sealed class NullTypeData : ITracorData {
    public List<string> GetListPropertyName() => [];

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        propertyValue = default;
        return false;
    }
}