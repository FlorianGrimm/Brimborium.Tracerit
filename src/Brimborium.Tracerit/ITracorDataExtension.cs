namespace Brimborium.Tracerit;

/// <summary>
/// Provides extension methods for <see cref="ITracorData"/> to enhance functionality with strongly-typed operations.
/// </summary>
public static class ITracorDataExtension {
    public static bool IsEqual<T>(this ITracorData data, string propertyName, T value)
        where T:notnull{
        return data.TryGetPropertyValue(propertyName, out var currentValue)
            && TracorDataProperty.Create(propertyName, value).HasEqualValue(currentValue);
    }

    /// <summary>
    /// Attempts to get a strongly-typed property value from the trace data.
    /// </summary>
    /// <typeparam name="T">The expected type of the property value.</typeparam>
    /// <param name="tracorData">The trace data to query.</param>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="value">When this method returns, contains the strongly-typed property value if found and can be cast to the specified type; otherwise, the default value.</param>
    /// <returns>True if the property was found and can be cast to the specified type; otherwise, false.</returns>
    public static bool TryGetPropertyValue<T>(this ITracorData tracorData, string propertyName, [MaybeNullWhen(false)] out T value) {
        if (tracorData.TryGetPropertyValue(propertyName, out var propertyValue)) {
            if (propertyValue is T propertyValueTyped) {
                value = propertyValueTyped;
                return true;
            }
        }
        value = default;
        return false;
    }
}