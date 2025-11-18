namespace Brimborium.Tracerit;

/// <summary>
/// Represents a record of trace data, including properties and identifiers.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public sealed class TracorDataRecord
    : ReferenceCountObject
    , ITracorData {
    private readonly List<TracorDataProperty> _ListProperty = new(8);

    public TracorDataRecord() : base(null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorDataRecord"/> class.
    /// </summary>
    public TracorDataRecord(IReferenceCountPool? owner) : base(owner) { }

    /// <summary>
    /// Gets or sets the identifier associated with this trace data record.
    /// </summary>
    public TracorIdentifier TracorIdentifier { get; set; }

    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets the list of properties associated with this trace data record.
    /// </summary>
    public List<TracorDataProperty> ListProperty {
        get=> this._ListProperty; 
        set {
            this._ListProperty.AddRange(value);
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
        var listProperty = this.ListProperty;
        for (int index = 0; index < listProperty.Count; index++) {
            if (string.Equals(listProperty[index].Name, propertyName, StringComparison.Ordinal)) {
                propertyValue = listProperty[index].Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }

    public bool TryGetDataProperty(string propertyName, out TracorDataProperty result) {
        var listProperty = this.ListProperty;
        for (int index = 0; index < listProperty.Count; index++) {
            if (string.Equals(listProperty[index].Name, propertyName, StringComparison.Ordinal)) { 
                result = listProperty[index];
                return true;
            }
        }
        result = new TracorDataProperty(string.Empty);
        return false;
    }


    /// <inheritdoc/>
    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        listProperty.AddRange(this.ListProperty);
    }

    public void CopyPropertiesToSink(TracorPropertySinkTarget target) {
        ITracorDataExtension.CopyPropertiesToSinkBase<TracorDataRecord>(this, target);
        target.ListPropertyFromTracorData = this.ListProperty;
    }

    /// <summary>
    /// Determines whether the current trace data partially equals the expected data.
    /// </summary>
    /// <param name="currentData">The current trace identifier data.</param>
    /// <param name="expectedData">The expected trace data record.</param>
    /// <returns>True if the current data matches the expected data; otherwise, false.</returns>
    public static bool IsPartialEquals(ITracorData currentData, TracorDataRecord expectedData) {
        if (expectedData.TracorIdentifier is { } expectedTracorIdentifier) {
            var currentTracorIdentifier = currentData.TracorIdentifier;
            if (!MatchEqualityComparerTracorIdentifier.Default.Equals(
                    currentTracorIdentifier,
                    expectedTracorIdentifier)) {
                return false;
            }
        }
        if (0 < expectedData.ListProperty.Count) {
            foreach (var expectedProperty in expectedData.ListProperty) {
                if (currentData.TryGetPropertyValue(expectedProperty.Name, out var currentPropertyValue)) {
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

    protected override bool IsStateReset() {
        return this.ListProperty.Count == 0 && this.ListProperty.Capacity <= 128;
    }

    public static TracorDataRecord Convert(ITracorData src, TracorDataRecordPool? tracorDataRecordPool) {
        if (src is TracorDataRecord result) {
            return result;
        }
        {
            if (tracorDataRecordPool is { }) {
                result = tracorDataRecordPool.Rent();
            } else { 
                result = new TracorDataRecord();
            }

            result.TracorIdentifier = src.TracorIdentifier;
            result.Timestamp = src.Timestamp;
            src.ConvertProperties(result.ListProperty);

            return result;
        }
    }

    internal string GetDebuggerDisplay() {
        return $"{this.TracorIdentifier} {this.ListProperty.FirstOrDefault().GetDebuggerDisplay()} {this.Timestamp:o}";
    }

    public TracorDataRecord Add(TracorDataProperty value) {
        this.ListProperty.Add(value);
        return this;
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
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public TracorDataRecordPool() : this(0) { }

    public TracorDataRecordPool(int capacity) : base(capacity) { 
    }

    protected override TracorDataRecord Create() => new TracorDataRecord(this);
}