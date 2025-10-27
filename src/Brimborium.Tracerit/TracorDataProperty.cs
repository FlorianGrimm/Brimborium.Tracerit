#pragma warning disable IDE0009 // Member access should be qualified.

using System.Runtime.InteropServices;

namespace Brimborium.Tracerit;

/*
new pbr::GeneratedClrTypeInfo(
    typeof(global::OpenTelemetry.Proto.Common.V1.AnyValue), 
    global::OpenTelemetry.Proto.Common.V1.AnyValue.Parser, 
    new[]{ 
        "StringValue",
        "BoolValue", 
        "IntValue", 
        "DoubleValue", 
        "ArrayValue", 
        "KvlistValue", 
        "BytesValue" 
    }, new[]{ "Value" }, null, null, null),
*/

public partial struct TracorDataProperty : IEquatable<TracorDataProperty> {
    private TracorDataPropertyTypeValue _TypeValue;

    public TracorDataProperty(string name) {
        Name = name;
        _TypeValue = TracorDataPropertyTypeValue.Null;
        InnerObjectValue = null;
    }

    public TracorDataProperty(
       string name,
       TracorDataPropertyTypeValue typeValue,
       string textValue
       ) {
        Name = name;
        _TypeValue = typeValue;
        InnerObjectValue = textValue;
    }

    [JsonPropertyName("name"), JsonInclude]
    public string Name { get; set; }

    public object? Value {
        readonly get {
            if (InnerObjectValue is null) {
                switch (TypeValue) {
                    case TracorDataPropertyTypeValue.Null:
                        return null;
                    case TracorDataPropertyTypeValue.String:
                        TryGetStringValue(out var stringValue);
                        return stringValue;
                    case TracorDataPropertyTypeValue.Integer:
                        TryGetIntegerValue(out var longValue);
                        return longValue;
                    case TracorDataPropertyTypeValue.Boolean:
                        TryGetBooleanValue(out var boolValue);
                        return boolValue;
                    case TracorDataPropertyTypeValue.Enum:
                        TryGetEnumValue(out var enumValue);
                        return enumValue;
                    case TracorDataPropertyTypeValue.Level:
                        TryGetLevelValue(out var logLevel);
                        return logLevel;
                    case TracorDataPropertyTypeValue.Double:
                        TryGetDoubleValue(out var doubleValue);
                        return doubleValue;
                    case TracorDataPropertyTypeValue.DateTime:
                        TryGetDateTimeValue(out var dtValue);
                        return dtValue;
                    case TracorDataPropertyTypeValue.DateTimeOffset:
                        TryGetDateTimeOffsetValue(out var dtoValue);
                        return dtoValue;
                    case TracorDataPropertyTypeValue.Uuid:
                        TryGetUuidValue(out var uuidValue);
                        return uuidValue;
                    case TracorDataPropertyTypeValue.Any:
                        return InnerObjectValue;
                    default:
                        break;
                }
            }
            return InnerObjectValue;
        }
        set {
            InnerObjectValue = value;
#warning TODO
            /*
            if (value is null) {
                TypeValue = TracorDataPropertyTypeValue.Null;
                return;
            }
            if (value is string stringValue) {
                TypeValue = TracorDataPropertyTypeValue.String;
                InnerTextValue = stringValue;
                return;
            }
            if (value is int intValue) {
                TypeValue = TracorDataPropertyTypeValue.Integer;
                InnerLongValue = intValue;
                return;
            }
            if (value is LogLevel logLevelValue) {
                TypeValue = TracorDataPropertyTypeValue.LevelValue;
                InnerTextValue = logLevelValue.ToString();
                InnerLongValue = (long)logLevelValue;
                return;
            }
            if (value is DateTime dtValue) {
                TypeValue = TracorDataPropertyTypeValue.DateTime;
                InnerLongValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dtValue);
                return;
            }
            if (value is DateTimeOffset dtoValue) {
                var dto = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(dtoValue);
                TypeValue = TracorDataPropertyTypeValue.DateTime;
                InnerLongValue = dto.longVaule;
                InnerFloatValue = dto.floatValue;
                return;
            }
            if (value is long longValue) {
                TypeValue = TracorDataPropertyTypeValue.DateTime;
                InnerLongValue = longValue;
                return;
            }
            if (value is double doubleValue) {
                TypeValue = TracorDataPropertyTypeValue.Float;
                InnerFloatValue = doubleValue;
                return;
            }
            if (value is float floatValue) {
                TypeValue = TracorDataPropertyTypeValue.Float;
                InnerFloatValue = floatValue;
                return;
            }
            if (value is Guid guidValue) {
                TypeValue = TracorDataPropertyTypeValue.Uuid;
                InnerUuidValue = guidValue;
                return;
            }
            if (value.GetType().IsEnum) {
                TypeValue = TracorDataPropertyTypeValue.Enum;
                InnerTextValue = value.ToString() ?? string.Empty;
                InnerFloatValue = (long)value;
                return;
            }
            {
                TypeValue = TracorDataPropertyTypeValue.Any;
                AnyValue = value;
                return;
            }
            */
        }
    }
    public object? InnerObjectValue { get; set; }

    public readonly bool TryGetNullValue(out object? value) {
        value = null;
        return TracorDataPropertyTypeValue.Null == _TypeValue;
    }

    public void SetNullValue() {
        TypeValue = TracorDataPropertyTypeValue.Null;
        InnerObjectValue = null;
    }

    public readonly bool TryGetStringValue(out string value) {
        if (TracorDataPropertyTypeValue.String == TypeValue) {
            value = InnerObjectValue is string textValue ? textValue : string.Empty;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public void SetStringValue(string value) {
        _TypeValue = TracorDataPropertyTypeValue.String;
        InnerObjectValue = value;
    }

    public readonly bool TryGetIntegerValue(out long value) {
        if (TracorDataPropertyTypeValue.Integer == _TypeValue) {
            if (InnerObjectValue is long integerValue) {
                value = integerValue;
                return true;
            }
        }
        value = 0L;
        return false;
    }

    public void SetIntegerValue(long value) {
        TypeValue = TracorDataPropertyTypeValue.Integer;
        InnerObjectValue = value;
    }

    public readonly bool TryGetBooleanValue(out bool value) {
        if (TypeValue == TracorDataPropertyTypeValue.Boolean) {
            if (InnerObjectValue is bool boolValue) {
                value = boolValue;
                return true;
            }
            if (InnerObjectValue is long longValue) {
                value = (longValue == 0 ? false : true);
                return true;
            }
        }
        value = false;
        return false;
    }

    public void SetBooleanValue(bool value) {
        TypeValue = TracorDataPropertyTypeValue.Boolean;
        InnerObjectValue = value;
    }

    public readonly bool TryGetEnumValue(out string textValue) {
        if (TypeValue == TracorDataPropertyTypeValue.Enum) {
            if (InnerObjectValue is string stringValue) {
                textValue = stringValue;
                return true;
            }
        }
        textValue = string.Empty;
        return false;
    }

    public void SetEnumValue(string txtValue) {
        TypeValue = TracorDataPropertyTypeValue.Enum;
        InnerObjectValue = txtValue;
    }

    public readonly bool TryGetLevelValue(out LogLevel value) {
        if (TracorDataPropertyTypeValue.Level == TypeValue) {
            if (InnerObjectValue is string stringValue) {

                switch (stringValue) {
                    case "trace": value = LogLevel.Trace; return true;
                    case "debug": value = LogLevel.Debug; return true;
                    case "information": value = LogLevel.Information; return true;
                    case "warning": value = LogLevel.Warning; return true;
                    case "error": value = LogLevel.Error; return true;
                    case "critical": value = LogLevel.Critical; return true;
                    case "none": value = LogLevel.None; return true;
                    default: value = LogLevel.None; return false;
                }
            }
        }
        value = 0;
        return false;
    }

    public void SetLevelValue(LogLevel value) {
        TypeValue = TracorDataPropertyTypeValue.Level;
        InnerObjectValue = value switch {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "information",
            LogLevel.Warning => "warning",
            LogLevel.Error => "error",
            LogLevel.Critical => "critical",
            LogLevel.None => "none",
            _ => string.Empty
        };
    }

    public readonly bool TryGetDoubleValue(out double value) {
        if (TypeValue == TracorDataPropertyTypeValue.Double) {
            if (InnerObjectValue is double doubleValue) {
                value = doubleValue;
                return true;
            }
        }
        value = double.NaN;
        return false;
    }

    public void SetDoubleValue(double value) {
        TypeValue = TracorDataPropertyTypeValue.Double;
        InnerObjectValue = value;
    }

    public readonly bool TryGetDateTimeValue(out DateTime value) {
        if (TypeValue == TracorDataPropertyTypeValue.DateTime) {
            if (InnerObjectValue is DateTime dateTimeValue) {
                value = dateTimeValue;
                return true;
            }
            if (InnerObjectValue is long longValue) {
                value = TracorDataUtility.UnixTimeNanosecondsToDateTime(longValue);
                return true;
            }
        }
        value = new DateTime(0);
        return false;
    }

    public void SetDateTimeValue(DateTime value) {
        TypeValue = TracorDataPropertyTypeValue.DateTime;
        // var longValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(value);
        InnerObjectValue = value;
    }

    public readonly bool TryGetDateTimeOffsetValue(out DateTimeOffset value) {
        if (TypeValue == TracorDataPropertyTypeValue.DateTimeOffset) {
            if (InnerObjectValue is DateTimeOffset dateTimeOffsetValue) {
                value = dateTimeOffsetValue;
                return true;
            }
            if (InnerObjectValue is long ticksValue) {
                value = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(ticksValue, 0);
                return true;
            }
            if (InnerObjectValue is string { Length: > 0 } stringValue) {
                if (DateTimeOffset.TryParseExact(
                    stringValue,
                    "o",
                    System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                    System.Globalization.DateTimeStyles.None,
                    out var result)) {
                    value = result;
                    return true;
                }
            }
        }
        value = new DateTimeOffset(new DateTime(0, DateTimeKind.Utc), TimeSpan.Zero);
        return false;
    }

    public void SetDateTimeOffsetValue(DateTimeOffset value) {
        TypeValue = TracorDataPropertyTypeValue.DateTimeOffset;
        //var (ticksValue, offsetValue) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(value);
        InnerObjectValue = value;
    }

    public readonly bool TryGetUuidValue(out Guid value) {
        if (TypeValue == TracorDataPropertyTypeValue.Uuid) {
            if (InnerObjectValue is Guid uuidValue) {
                value = uuidValue;
                return true;
            }
        }
        value = Guid.Empty;
        return false;
    }

    public void SetUuidValue(Guid value) {
        TypeValue = TracorDataPropertyTypeValue.Uuid;
        InnerObjectValue = value;
    }

    public readonly bool TryGetAnyValue(out object? value) {
        value = this.InnerObjectValue;
        return TracorDataPropertyTypeValue.Any == _TypeValue;
    }

    public void SetAnyValue(object? value) {
        TypeValue = TracorDataPropertyTypeValue.Any;
        InnerObjectValue = null;
    }
    /*
    private const char _SeparationJsonChar = ':';
    public readonly void ToMinimizeString(StringBuilder sbOut) {
        sbOut
            .Append(Name)
            .Append(_SeparationJsonChar)
            .Append(TypeName)
            .Append(_SeparationJsonChar)
            .Append(InnerTextValue)
            ;
    }
    */

    public readonly bool HasEqualValue(object? currentPropertyValue) {
        return (TypeValue) switch {
            TracorDataPropertyTypeValue.Null
                => (TracorDataPropertyTypeValue.Null == _TypeValue)
                    && (currentPropertyValue is null),

            TracorDataPropertyTypeValue.String
                => (currentPropertyValue is string otherStringValue)
                && TryGetStringValue(out var thisStringValue)
                && string.Equals(thisStringValue, otherStringValue),

            TracorDataPropertyTypeValue.Integer
                => TracorDataUtility.TryConvertObjectToIntegerValue(currentPropertyValue, out var otherIntegerValue)
                && TryGetIntegerValue(out var thisIntegerValue)
                && (thisIntegerValue == otherIntegerValue),

            TracorDataPropertyTypeValue.Boolean
                => TracorDataUtility.TryConvertObjectToBooleanValue(currentPropertyValue, out var otherBooleanValue)
                && TryGetBooleanValue(out var thisBooleanValue)
                && (otherBooleanValue == thisBooleanValue),

            TracorDataPropertyTypeValue.Enum
                => TracorDataUtility.TryConvertObjectToEnumValue(currentPropertyValue, out var otherEnumValue, out var otherTextEnumValue)
                && TryGetEnumValue(out var thisEnumTextValue)
                && string.Equals(otherTextEnumValue, thisEnumTextValue, StringComparison.OrdinalIgnoreCase),

            TracorDataPropertyTypeValue.Level
                => TracorDataUtility.TryConvertObjectToLogLevelValue(currentPropertyValue, out var otherLogLevelValue)
                    && TryGetLevelValue(out var thisLogLevelValue)
                    && (thisLogLevelValue == otherLogLevelValue),

            TracorDataPropertyTypeValue.Double
                => TracorDataUtility.TryConvertObjectToDoubleValue(currentPropertyValue, out var otherFloatValue)
                && TryGetDoubleValue(out var thisFloatValue)
                && (thisFloatValue == otherFloatValue),

            TracorDataPropertyTypeValue.DateTime
                => (currentPropertyValue is DateTime otherDateTimeValue)
                && TryGetDateTimeValue(out var thisDateTimeValue)
                && (otherDateTimeValue == thisDateTimeValue),

            TracorDataPropertyTypeValue.DateTimeOffset
                => (currentPropertyValue is DateTimeOffset otherDateTimeOffsetValue)
                && TryGetDateTimeOffsetValue(out var thisDateTimeOffsetValue)
                && (otherDateTimeOffsetValue == thisDateTimeOffsetValue),

            TracorDataPropertyTypeValue.Any
                => false,
            _ => throw new NotSupportedException($"{TypeValue} is unknown")
        };
    }

    public readonly string? GetValueAsString() {
        switch (_TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                return string.Empty;

            case TracorDataPropertyTypeValue.String: {
                    return InnerObjectValue is string textValue ? textValue : string.Empty;
                }

            case TracorDataPropertyTypeValue.Integer: {
                    TryGetIntegerValue(out long longValue);
                    return longValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
            case TracorDataPropertyTypeValue.Boolean: {
                    TryGetBooleanValue(out bool boolValue);
                    return (boolValue) ? "true" : "false";
                }
            case TracorDataPropertyTypeValue.Enum: {
                    if (InnerObjectValue is string textValue) {
                        return textValue;
                    }
                    return string.Empty;
                }
            case TracorDataPropertyTypeValue.Level: {
                    if (InnerObjectValue is string textValue) {
                        return textValue;
                    }
                    return string.Empty;
                }
            case TracorDataPropertyTypeValue.Double: {
                    TryGetDoubleValue(out double floatValue);
                    return floatValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
            case TracorDataPropertyTypeValue.DateTime: {
                    TryGetDateTimeValue(out var dtValue);
                    return dtValue.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                }
            case TracorDataPropertyTypeValue.DateTimeOffset: {
                    TryGetDateTimeOffsetValue(out var dtoValue);
                    return dtoValue.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);
                }
            case TracorDataPropertyTypeValue.Uuid: {
                    TryGetUuidValue(out var uuidValue);
                    return uuidValue.ToString("d");
                }
            case TracorDataPropertyTypeValue.Any: {
                    return InnerObjectValue?.ToString();
                }
            default:
                return null;
        }
    }

    private static EqualityComparer<TracorDataProperty>? _TracorDataPropertyEqualityComparer;
    public override bool Equals([NotNullWhen(true)] object? obj) {
        if (ReferenceEquals(null, obj)) { return false; }
        if (obj is not TracorDataProperty other) { return false; }
        var ec = _TracorDataPropertyEqualityComparer ??= TracorDataPropertyEqualityComparer.Default;
        return ec.Equals(this, other);
    }

    public bool Equals(TracorDataProperty other) {
        var ec = _TracorDataPropertyEqualityComparer ??= TracorDataPropertyEqualityComparer.Default;
        return ec.Equals(other);
    }

    public override int GetHashCode() {
        var ec = _TracorDataPropertyEqualityComparer ??= TracorDataPropertyEqualityComparer.Default;
        return base.GetHashCode();
    }
}
