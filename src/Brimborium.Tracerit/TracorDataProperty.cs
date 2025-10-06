using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    private string? _TypeName;
    private TracorDataPropertyTypeValue _TypeValue;

    public TracorDataProperty(
       string name,
       TracorDataPropertyTypeValue typeValue,
       string textValue
       ) {
        this.Name = name;
        this._TypeValue = typeValue;
        this._TypeName = null;
        this.TextValue = textValue;
    }

    public TracorDataProperty(
        string name,
        string typeName,
        string textValue
        ) {
        this.Name = name;
        (this._TypeValue, this._TypeName) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(typeName);
        this.TextValue = textValue;
    }

    [JsonInclude]
    public string Name { get; set; }

#warning here
    public object? Value { get; set; }

    [JsonIgnore]
    public string TypeName {
        readonly get => TracorDataUtility.TracorDataPropertyConvertTypeValueToString(this._TypeValue, this._TypeName);
        set => this._TypeName = value;
    }

    [JsonInclude]
    public TracorDataPropertyTypeValue TypeValue {
        readonly get {
            return this._TypeValue;
        }
        set {
            this._TypeValue = value;
            this._TypeName = null;
        }
    }

    [JsonInclude]
    public string TextValue { get; set; }

    // Boolean
    [JsonIgnore]
    internal long LongValue;

    [JsonIgnore]
    internal double FloatValue;

    [JsonIgnore]
    internal Guid UuidValue;
        
    [JsonIgnore]
    public object? AnyValue { get; set; }

    private const char _SeperationJsonChar = ':';

    public readonly void ToMinimizeString(StringBuilder sbOut) {
        sbOut
            .Append(this.Name)
            .Append(_SeperationJsonChar)
            .Append(this.TypeName)
            .Append(_SeperationJsonChar)
            .Append(this.TextValue)
            ;
    }

    public readonly bool HasEqualValue(object? currentPropertyValue) {
        switch (this.TypeValue) {
            case TracorDataPropertyTypeValue.Any:
                return false;
            case TracorDataPropertyTypeValue.String:
                return (currentPropertyValue is string stringValue)
                    && string.Equals(this.TextValue, stringValue);
            case TracorDataPropertyTypeValue.Integer:
                return (currentPropertyValue is int intValue)
                    && (intValue == this.LongValue);
            case TracorDataPropertyTypeValue.LevelValue:
                return (currentPropertyValue is LogLevel logLevelValue)
                    && ((long)logLevelValue == this.LongValue);
            case TracorDataPropertyTypeValue.DateTime:
                return (currentPropertyValue is DateTime dateTimeValue)
                    && (dateTimeValue.Ticks == this.LongValue);
            case TracorDataPropertyTypeValue.DateTimeOffset:
                return (currentPropertyValue is DateTimeOffset dateTimeOffsetValue)
                    && (dateTimeOffsetValue.Ticks == this.LongValue);
            case TracorDataPropertyTypeValue.Boolean:
                return (currentPropertyValue is bool booleanValue)
                && ((booleanValue ? 1 : 0) == this.LongValue);
            case TracorDataPropertyTypeValue.Long:
                return (currentPropertyValue is long longValue)
                && (longValue == this.LongValue);
            case TracorDataPropertyTypeValue.Float:
                return (currentPropertyValue is double floatValue)
                && (floatValue == this.FloatValue);
            default:
                throw new NotSupportedException($"{this.TypeValue} is unknown");
        }
    }
}


[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(TracorDataProperty))]
internal partial class SourceGenerationContext : JsonSerializerContext {
}