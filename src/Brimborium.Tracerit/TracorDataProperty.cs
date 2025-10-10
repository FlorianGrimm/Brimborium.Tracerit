using System.ComponentModel;
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
        this.InnerTextValue = textValue;
    }

    public TracorDataProperty(
        string name,
        string typeName,
        string textValue
        ) {
        this.Name = name;
        (this._TypeValue, this._TypeName) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(typeName);
        this.InnerTextValue = textValue;
    }

    [JsonPropertyName("name"), JsonInclude]
    public string Name { get; set; }

    public object? Value {
        readonly get {
            if (this.InnerObjectValue is null) {
                switch (this.TypeValue) {
                    case TracorDataPropertyTypeValue.Any:
                        return this.AnyValue;
                    case TracorDataPropertyTypeValue.String:
                        if (this.TryGetStringValue(out var stringValue)) { return stringValue; }
                        break;
                    case TracorDataPropertyTypeValue.LevelValue:
                        if (this.TryGetLevelValueValue(out var logLevel)) { return logLevel; }
                        break;
                    case TracorDataPropertyTypeValue.Enum:
                        return this.InnerTextValue;
                    case TracorDataPropertyTypeValue.DateTime:
                        if (this.TryGetDateTimeValue(out var dtValue)) { return dtValue; }
                        break;
                    case TracorDataPropertyTypeValue.DateTimeOffset:
                        if (this.TryGetDateTimeOffsetValue(out var dtoValue)) { return dtoValue; }
                        break;
                    case TracorDataPropertyTypeValue.Boolean:
                        if (this.TryGetBooleanValue(out var boolValue)) { return boolValue; }
                        break;
                    case TracorDataPropertyTypeValue.Integer:
                        if (this.TryGetIntegerValue(out var longValue)) { return longValue; }
                        break;
                    case TracorDataPropertyTypeValue.Float:
                        if (this.TryGetFloatValue(out var floatValue)) { return floatValue; }
                        break;
                    case TracorDataPropertyTypeValue.Uuid:
                        if (this.TryGetUuidValue(out var uuidValue)) { return uuidValue; }
                        break;
                    default:
                        break;
                }
            }
            return this.InnerObjectValue;
        }
        set {
            this.InnerObjectValue = value;
            if (value is null) {
                this.TypeValue = TracorDataPropertyTypeValue.Any;
                return;
            }
            if (value is string stringValue) {
                this.TypeValue = TracorDataPropertyTypeValue.String;
                this.InnerTextValue = stringValue;
                return;
            }
            if (value is int intValue) {
                this.TypeValue = TracorDataPropertyTypeValue.Integer;
                this.InnerLongValue = intValue;
                return;
            }
            if (value is LogLevel logLevelValue) {
                this.TypeValue = TracorDataPropertyTypeValue.LevelValue;
                this.InnerTextValue = logLevelValue.ToString();
                this.InnerLongValue = (long)logLevelValue;
                return;
            }
            if (value is DateTime dtValue) {
                this.TypeValue = TracorDataPropertyTypeValue.DateTime;
                this.InnerLongValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dtValue);
                return;
            }
            if (value is DateTimeOffset dtoValue) {
                var dto = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(dtoValue);
                this.TypeValue = TracorDataPropertyTypeValue.DateTime;
                this.InnerLongValue = dto.longVaule;
                this.InnerFloatValue = dto.floatValue;
                return;
            }
            if (value is long longValue) {
                this.TypeValue = TracorDataPropertyTypeValue.DateTime;
                this.InnerLongValue = longValue;
                return;
            }
            if (value is double doubleValue) {
                this.TypeValue = TracorDataPropertyTypeValue.Float;
                this.InnerFloatValue = doubleValue;
                return;
            }
            if (value is float floatValue) {
                this.TypeValue = TracorDataPropertyTypeValue.Float;
                this.InnerFloatValue = floatValue;
                return;
            }
            if (value is Guid guidValue) {
                this.TypeValue = TracorDataPropertyTypeValue.Uuid;
                this.InnerUuidValue = guidValue;
                return;
            }
            if (value.GetType().IsEnum) {
                this.TypeValue = TracorDataPropertyTypeValue.Float;
                this.InnerTextValue = value.ToString() ?? string.Empty;
                this.InnerFloatValue = (long)value;
                return;
            }
            {
                this.TypeValue = TracorDataPropertyTypeValue.Any;
                this.AnyValue = value;
                return;
            }
        }
    }
    public object? InnerObjectValue { get; set; }

    [JsonIgnore]
    public string TypeName {
        readonly get => TracorDataUtility.TracorDataPropertyConvertTypeValueToString(this._TypeValue, this._TypeName);
        set => this._TypeName = value;
    }

    [JsonPropertyName("type_Value"), JsonInclude]
    public TracorDataPropertyTypeValue TypeValue {
        readonly get {
            return this._TypeValue;
        }
        set {
            this._TypeValue = value;
            this._TypeName = null;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("text_Value"), JsonInclude()]
    public string? InnerTextValue { get; set; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("long_Value"), JsonInclude()]
    public long InnerLongValue;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("float_Value"), JsonInclude()]
    public double InnerFloatValue;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("uuid_Value"), JsonInclude()]
    public Guid InnerUuidValue;

    [JsonIgnore]
    public object? AnyValue { get; set; }

    public readonly bool TryGetStringValue(out string value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.String) {
            value = this.InnerTextValue;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public readonly bool TryGetIntegerValue(out long value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.Integer) {
            value = this.InnerLongValue;
            return true;
        }
        value = 0;
        return false;
    }

    public readonly bool TryGetLevelValueValue(out LogLevel value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.LevelValue) {
            if (this.InnerLongValue == 0 && this.InnerTextValue is { Length: > 0 } textValue) {
                switch (textValue) {
                    case "Trace": value = LogLevel.Trace; return true;
                    case "Debug": value = LogLevel.Debug; return true;
                    case "Information": value = LogLevel.Information; return true;
                    case "Warning": value = LogLevel.Warning; return true;
                    case "Error": value = LogLevel.Error; return true;
                    case "Critical": value = LogLevel.Critical; return true;
                    case "trace": value = LogLevel.Trace; return true;
                    case "debug": value = LogLevel.Debug; return true;
                    case "information": value = LogLevel.Information; return true;
                    case "warning": value = LogLevel.Warning; return true;
                    case "error": value = LogLevel.Error; return true;
                    case "critical": value = LogLevel.Critical; return true;
                    default: break;
                }
            }
            switch (this.InnerLongValue) {
                case (long)LogLevel.Trace: value = LogLevel.Trace; return true;
                case (long)LogLevel.Debug: value = LogLevel.Debug; return true;
                case (long)LogLevel.Information: value = LogLevel.Information; return true;
                case (long)LogLevel.Warning: value = LogLevel.Warning; return true;
                case (long)LogLevel.Error: value = LogLevel.Error; return true;
                case (long)LogLevel.Critical: value = LogLevel.Critical; return true;
                default: break;
            }
            value = 0;
            return false;
        }
        value = 0;
        return false;
    }

    /*
     ,
    ,
    */
    public readonly bool TryGetEnumValue<T>(out T value) where T : struct, Enum {
        if (this.TypeValue == TracorDataPropertyTypeValue.Enum) {
            value = (T)(object)this.InnerLongValue;
            return true;
        }
        value = default;
        return false;
    }

    public readonly bool TryGetDateTimeValue(out DateTime value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.DateTime) {
            value = TracorDataUtility.UnixTimeNanosecondsToDateTime(this.InnerLongValue);
            return true;
        }
        value = new DateTime(0);
        return false;
    }

    public readonly bool TryGetDateTimeOffsetValue(out DateTimeOffset value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.DateTimeOffset) {
            if (this.InnerLongValue == 0 && this.InnerTextValue is { Length: > 0 } textValue) {
                if (DateTimeOffset.TryParseExact(
                    textValue,
                    "o",
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                    System.Globalization.DateTimeStyles.None,
                    out var result)) {
                    value = result;
                    return true;
                }
            }
            int offset;
            if (-240 <= this.InnerFloatValue && this.InnerFloatValue <= 240) {
                offset = (int)this.InnerFloatValue;
            } else {
                offset = 0;
            }
            value = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(this.InnerLongValue, offset);
            return true;
        }
        value = new DateTimeOffset(new DateTime(0, DateTimeKind.Utc), TimeSpan.Zero);
        return false;
    }

    public readonly bool TryGetBooleanValue(out bool value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.Boolean) {
            value = (this.InnerLongValue == 0 ? false : true);
            return true;
        }
        value = false;
        return false;
    }

    public readonly bool TryGetFloatValue(out double value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.Float) {
            value = this.InnerFloatValue;
            return true;
        }
        value = double.NaN;
        return false;
    }

    public readonly bool TryGetUuidValue(out Guid value) {
        if (this.TypeValue == TracorDataPropertyTypeValue.Uuid) {
            value = this.InnerUuidValue;
            return true;
        }
        value = Guid.Empty;
        return false;
    }



    private const char _SeperationJsonChar = ':';

    public readonly void ToMinimizeString(StringBuilder sbOut) {
        sbOut
            .Append(this.Name)
            .Append(_SeperationJsonChar)
            .Append(this.TypeName)
            .Append(_SeperationJsonChar)
            .Append(this.InnerTextValue)
            ;
    }

    public readonly bool HasEqualValue(object? currentPropertyValue) {
        switch (this.TypeValue) {
            case TracorDataPropertyTypeValue.Any:
                return false;
            case TracorDataPropertyTypeValue.String:
                return (currentPropertyValue is string stringValue)
                    && string.Equals(this.InnerTextValue, stringValue);
            case TracorDataPropertyTypeValue.LevelValue:
                return (currentPropertyValue is LogLevel logLevelValue)
                    && ((long)logLevelValue == this.InnerLongValue);
            case TracorDataPropertyTypeValue.DateTime:
                return (currentPropertyValue is DateTime dateTimeValue)
                    && (dateTimeValue.Ticks == this.InnerLongValue);
            case TracorDataPropertyTypeValue.DateTimeOffset:
                return (currentPropertyValue is DateTimeOffset dateTimeOffsetValue)
                    && (dateTimeOffsetValue.Ticks == this.InnerLongValue);
            case TracorDataPropertyTypeValue.Boolean:
                return (currentPropertyValue is bool booleanValue)
                && ((booleanValue ? 1 : 0) == this.InnerLongValue);
            case TracorDataPropertyTypeValue.Integer:
                if (currentPropertyValue is int intValue) { return (intValue == this.InnerLongValue); }
                if (currentPropertyValue is long longValue) { return (longValue == this.InnerLongValue); }
                return false;
            case TracorDataPropertyTypeValue.Float:
                return (currentPropertyValue is double floatValue)
                && (floatValue == this.InnerFloatValue);
            default:
                throw new NotSupportedException($"{this.TypeValue} is unknown");
        }
    }
}

/*
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(TracorDataProperty))]
internal partial class SourceGenerationContext : JsonSerializerContext {
}
*/