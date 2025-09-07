namespace Brimborium.Tracerit;

public static class ITracorDataExtension {
    public static bool TryGetPropertyValue<T>(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out T value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)
            && propertyValue is T propertyValueTyped) {
            value = propertyValueTyped;
            return true;
        }
        value = default;
        return false;
    }
}