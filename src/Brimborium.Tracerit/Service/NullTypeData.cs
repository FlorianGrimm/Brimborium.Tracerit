
namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents trace data for null values or empty data scenarios.
/// This implementation provides no properties and always returns false for property value queries.
/// </summary>
public sealed class NullTypeData : ITracorData {
    public object? this[string propertyName] {
        get {
            return null;
        }
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
    }

    /// <inheritdoc />
    /// <summary>
    /// Returns an empty list since null data has no properties.
    /// </summary>
    /// <returns>An empty list of property names.</returns>
    public List<string> GetListPropertyName() => [];

    /// <inheritdoc />
    /// <summary>
    /// Always returns false since null data has no property values.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <param name="propertyValue">Always set to null.</param>
    /// <returns>Always returns false.</returns>
    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        propertyValue = default;
        return false;
    }
}