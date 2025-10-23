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

public partial struct TracorDataProperty {
    private string? _TypeName;
    private TracorDataPropertyTypeValue _TypeValue;
    private ValueBuffer _ValueBuffer;
    private string? _TextValue;

    public TracorDataProperty(string name) {
        Name = name;
        _TypeName = null;
        _TypeValue = TracorDataPropertyTypeValue.Null;
        _TextValue = null;
    }

    public TracorDataProperty(
       string name,
       TracorDataPropertyTypeValue typeValue,
       string textValue
       ) {
        Name = name;
        _TypeValue = typeValue;
        _TypeName = null;
        _TextValue = textValue;
    }

    public TracorDataProperty(
        string name,
        string typeName,
        string textValue
        ) {
        Name = name;
        (_TypeValue, _TypeName) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(typeName);
        _TextValue = textValue;
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
                        return _TextValue;
                    case TracorDataPropertyTypeValue.LevelValue:
                        TryGetLevelValueValue(out var logLevel);
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
                        return AnyValue;
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



    [JsonIgnore]
    public object? AnyValue { get; set; }

    public readonly bool TryGetNullValue(out object? value) {
        value = null;
        return TracorDataPropertyTypeValue.Null == _TypeValue;
    }

    public void SetAnyValue() {
        TypeValue = TracorDataPropertyTypeValue.Null;
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetStringValue(out string value) {
        if (TracorDataPropertyTypeValue.String == TypeValue) {
            value = _TextValue ?? string.Empty;
            return true;
        }
        value = string.Empty;
        return false;
    }

    public void SetStringValue(string value) {
        _TypeValue = TracorDataPropertyTypeValue.String;
        _TextValue = value;
        AnyValue = null;
    }

    public readonly bool TryGetIntegerValue(out long value) {
        if (TracorDataPropertyTypeValue.Integer == _TypeValue) {
            if (MemoryMarshal.TryRead(GetValueReadSpan(), out value)) {
                return true;
            }
        }
        value = 0L;
        return false;
    }

    public void SetIntegerValue(long value) {
        TypeValue = TracorDataPropertyTypeValue.Integer;
        MemoryMarshal.Write(GetValueWriteSpan(), value);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetBooleanValue(out bool value) {
        if (TypeValue == TracorDataPropertyTypeValue.Boolean) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
            value = (longValue == 0 ? false : true);
            return true;
        }
        value = false;
        return false;
    }

    public void SetBooleanValue(bool value) {
        TypeValue = TracorDataPropertyTypeValue.Boolean;
        long longValue = value ? 1L : 0L;
        MemoryMarshal.Write(GetValueWriteSpan(), longValue);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetEnumTypedValue<T>(out T value, out string? textValue) where T : struct, Enum {
        if (TypeValue == TracorDataPropertyTypeValue.Enum) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
            value = TracorDataUtility.ConvertLongToEnum<T>(longValue);
            textValue = _TextValue;
            return true;
        }
        value = default;
        textValue = _TextValue;
        return false;
    }

    public readonly bool TryGetEnumUntypedValue(out long longValue, out string? textValue) {
        if (TypeValue == TracorDataPropertyTypeValue.Enum) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out longValue);
            textValue = _TextValue;
            return true;
        }
        longValue = default;
        textValue = _TextValue;
        return false;
    }

    public void SetEnumValue(long longValue, string? txtValue) {
        TypeValue = TracorDataPropertyTypeValue.Enum;
        MemoryMarshal.Write(GetValueWriteSpan(), longValue);
        _TextValue = txtValue;
        AnyValue = null;
    }


    public readonly bool TryGetLevelValueValue(out LogLevel value) {
        if (TracorDataPropertyTypeValue.LevelValue == TypeValue) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);

            switch (longValue) {
                case (long)LogLevel.Trace: value = LogLevel.Trace; return true;
                case (long)LogLevel.Debug: value = LogLevel.Debug; return true;
                case (long)LogLevel.Information: value = LogLevel.Information; return true;
                case (long)LogLevel.Warning: value = LogLevel.Warning; return true;
                case (long)LogLevel.Error: value = LogLevel.Error; return true;
                case (long)LogLevel.Critical: value = LogLevel.Critical; return true;
                case (long)LogLevel.None: value = LogLevel.None; return true;
                default: value = LogLevel.None; return false;
            }
        }
        value = 0;
        return false;
    }

    public void SetLevelValueValue(LogLevel value) {
        TypeValue = TracorDataPropertyTypeValue.LevelValue;
        long longValue = (long)value;
        MemoryMarshal.Write(GetValueWriteSpan(), longValue);
        _TextValue = value switch {
            LogLevel.Trace => "trace",
            LogLevel.Debug => "debug",
            LogLevel.Information => "information",
            LogLevel.Warning => "warning",
            LogLevel.Error => "error",
            LogLevel.Critical => "critical",
            LogLevel.None => "none",
            _ => string.Empty
        };
        AnyValue = null;
    }

    public readonly bool TryGetDoubleValue(out double value) {
        if (TypeValue == TracorDataPropertyTypeValue.Double) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out value);
            return true;
        }
        value = double.NaN;
        return false;
    }

    public void SetDoubleValue(double value) {
        TypeValue = TracorDataPropertyTypeValue.Double;
        MemoryMarshal.Write(GetValueWriteSpan(), value);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetDateTimeValue(out DateTime value) {
        if (TypeValue == TracorDataPropertyTypeValue.DateTime) {
            MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
            value = TracorDataUtility.UnixTimeNanosecondsToDateTime(longValue);
            return true;
        }
        value = new DateTime(0);
        return false;
    }

    public void SetDateTimeValue(DateTime value) {
        TypeValue = TracorDataPropertyTypeValue.DateTime;
        var longValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(value);
        MemoryMarshal.Write(GetValueWriteSpan(), longValue);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetDateTimeOffsetValue(out DateTimeOffset value) {
        if (TypeValue == TracorDataPropertyTypeValue.DateTimeOffset) {
            var readSpan = GetValueReadSpan();
            MemoryMarshal.TryRead(readSpan, out long ticksValue);
            MemoryMarshal.TryRead(readSpan[8..], out long offsetValue);
            if (-240L <= offsetValue && offsetValue <= 240L) {
                value = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(ticksValue, (int)offsetValue);
            }

            if (_TextValue is { Length: > 0 } textValue) {
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
        }
        value = new DateTimeOffset(new DateTime(0, DateTimeKind.Utc), TimeSpan.Zero);
        return false;
    }

    public void SetDateTimeOffsetValue(DateTimeOffset value) {
        TypeValue = TracorDataPropertyTypeValue.DateTimeOffset;
        var (ticksValue, offsetValue) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(value);
        var valueWriteSpan = GetValueWriteSpan();
        MemoryMarshal.Write(valueWriteSpan, ticksValue);
        MemoryMarshal.Write(valueWriteSpan[8..], offsetValue);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetUuidValue(out Guid value) {
        if (TypeValue == TracorDataPropertyTypeValue.Uuid) {
            value = new Guid(GetValueReadSpan());
            return true;
        }
        value = Guid.Empty;
        return false;
    }

    public void SetUuidValue(Guid value) {
        TypeValue = TracorDataPropertyTypeValue.Uuid;
        var valueWriteSpan = GetValueWriteSpan();
        value.TryWriteBytes(valueWriteSpan);
        _TextValue = null;
        AnyValue = null;
    }

    public readonly bool TryGetAnyValue(out object? value) {
        value = this.AnyValue;
        return TracorDataPropertyTypeValue.Any == _TypeValue;
    }

    public void SetAnyValue(object? value) {
        TypeValue = TracorDataPropertyTypeValue.Any;
        _TextValue = null;
        AnyValue = value;
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
                && TryGetEnumUntypedValue(out var thisEnumValue, out var thisEnumTextValue)
                && ((string.Equals(otherTextEnumValue, thisEnumTextValue, StringComparison.OrdinalIgnoreCase))
                    || (otherEnumValue == thisEnumValue)),

            TracorDataPropertyTypeValue.LevelValue
                => TracorDataUtility.TryConvertObjectToLogLevelValue(currentPropertyValue, out var otherLogLevelValue)
                    && TryGetLevelValueValue(out var thisLogLevelValue)
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

            case TracorDataPropertyTypeValue.String:
                return _TextValue;

            case TracorDataPropertyTypeValue.Integer: {
                    MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
                    return longValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
            case TracorDataPropertyTypeValue.Boolean: {
                    MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
                    //TracorDataUtility.GetBoolString(longValue!=0)
                    return (longValue == 0) ? "false" : "true";
                }
            case TracorDataPropertyTypeValue.Enum: {
                    if (_TextValue is { } textValue) {
                        return textValue;
                    }
                    MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
                    return longValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                }
            case TracorDataPropertyTypeValue.LevelValue: {
                    if (_TextValue is { } textValue) {
                        return textValue;
                    }
                    MemoryMarshal.TryRead(GetValueReadSpan(), out long longValue);
                    return longValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
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
                    return AnyValue?.ToString();
                }
            default:
                return null;
        }
    }
}

/*
[JsonSourceGenerationOptions(WriteIndented = false)]
[JsonSerializable(typeof(TracorDataProperty))]
internal partial class SourceGenerationContext : JsonSerializerContext {
}
*/