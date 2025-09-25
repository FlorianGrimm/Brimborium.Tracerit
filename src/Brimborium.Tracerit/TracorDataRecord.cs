using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

/// <summary>
/// Represents a record of trace data, including properties and identifiers.
/// </summary>
public sealed class TracorDataRecord
    : ReferenceCountObject
    , ITracorData {
    public TracorDataRecord() : base(null) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorDataRecord"/> class.
    /// </summary>
    public TracorDataRecord(IReferenceCountPool? owner) : base(owner) { }

    /// <summary>
    /// Gets or sets the operation type for this trace data record.
    /// </summary>
    public TracorDataRecordOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentitfier? TracorIdentitfier { get; set; }

    /// <summary>
    /// Gets the list of properties associated with this trace data record.
    /// </summary>
    public List<TracorDataProperty> ListProperty { get; } = new(64);

    /// <summary>
    /// Converts this record to a <see cref="TracorIdentitfierData"/> object.
    /// </summary>
    /// <returns>A <see cref="TracorIdentitfierData"/> containing the identifier and this record.</returns>
    public TracorIdentitfierData ToTracorIdentitfierData()
        => new TracorIdentitfierData(
            this.TracorIdentitfier ?? new(string.Empty, string.Empty),
            this);

    /// <summary>
    /// Gets the value of a property by name.
    /// </summary>
    /// <param name="propertyName">The name of the property to retrieve.</param>
    /// <returns>The value of the property if found; otherwise, null.</returns>
    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var result)) {
                return result;
            } else {
                return null;
            }
        }
    }

    /// <inheritdoc/>
    public List<string> GetListPropertyName() {
        List<string> result = new(this.ListProperty.Count);
        foreach (var property in this.ListProperty) {
            result.Add(property.Name);
        }
        return result;
    }

    /// <inheritdoc/>
    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        foreach (var property in this.ListProperty) {
            if (propertyName == property.Name) {
                propertyValue = property.Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }

    /// <inheritdoc/>
    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        listProperty.AddRange(this.ListProperty);
    }

    /// <summary>
    /// Determines whether the current trace data partially equals the expected data.
    /// </summary>
    /// <param name="currentData">The current trace identifier data.</param>
    /// <param name="expectedData">The expected trace data record.</param>
    /// <returns>True if the current data matches the expected data; otherwise, false.</returns>
    public static bool IsPartialEquals(TracorIdentitfierData currentData, TracorDataRecord expectedData) {
        if (expectedData.TracorIdentitfier is { } expectedtracorIdentitfier) {
            var currentTracorIdentitfier = currentData.TracorIdentitfier;
            if (!MatchEqualityComparerTracorIdentitfier.Default.Equals(
                    currentTracorIdentitfier,
                    expectedtracorIdentitfier)) {
                return false;
            }
        }
        if (0 < expectedData.ListProperty.Count) {
            foreach (var expectedProperty in expectedData.ListProperty) {
                if (currentData.TracorData.TryGetPropertyValue(expectedProperty.Name, out var currentPropertyValue)) {
                    if (expectedProperty.HasEqualValue(currentPropertyValue)) {
                        // equal -> ok
                    } else {
                        // not equal
                        return false;
                    }
                } else {
                    // not found
                    return false;
                }
            }
        }

        // no diff found
        return true;
    }

    protected override void ResetState() {
        this.ListProperty.Clear();
    }

    protected override bool IsStateReseted() {
        return this.ListProperty.Count == 0 && this.ListProperty.Capacity <= 128;
    }
}

/// <summary>
/// Factory for creating <see cref="TracorDataRecord"/> trace data from objects.
/// </summary>
public sealed class TracorDataRecordAccessorFactory : ITracorDataAccessorFactory<TracorDataRecord> {
    /// <inheritdoc/>
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is TracorDataRecord tracorDataValue) {
            tracorData = tracorDataValue; return true;
        }
        tracorData = default; return false;
    }

    /// <inheritdoc/>
    public bool TryGetDataTyped(TracorDataRecord value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = value;
        return true;
    }
}
public sealed class TracorDataRecordPool : ReferenceCountPool<TracorDataRecord> {
    public static TracorDataRecordPool Create(IServiceProvider provider) => new(0);

    public TracorDataRecordPool(int capacity) : base(capacity) { }

    protected override TracorDataRecord Create() => new TracorDataRecord(this);
}
/*
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(TracorDataCollection))]
[JsonSerializable(typeof(TracorDataRecord))]
[JsonSerializable(typeof(TracorDataProperty))]
[JsonSerializable(typeof(TracorIdentitfier))]
internal partial class TracorDataJsonSerializerContext : JsonSerializerContext {
}
*/