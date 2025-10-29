#pragma warning disable IDE0009 // Member access should be qualified.


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

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public partial struct TracorDataProperty : IEquatable<TracorDataProperty> {
    private TracorDataPropertyTypeValue _TypeValue;

    public TracorDataProperty(string name) {
        Name = name;
        _TypeValue = TracorDataPropertyTypeValue.Null;
        InnerObjectValue = null;
    }

    public TracorDataProperty(string name, string textValue, TracorDataPropertyTypeValue typeValue = TracorDataPropertyTypeValue.String) {
        if (typeValue is not TracorDataPropertyTypeValue.String or TracorDataPropertyTypeValue.Enum) {
            throw new ArgumentException("must be string or Enum", nameof(typeValue));
        }
        _TypeValue = typeValue;
        Name = name;
        InnerObjectValue = textValue;
    }

    public TracorDataProperty(string name, long longValue) {
        _TypeValue = TracorDataPropertyTypeValue.Integer;
        Name = name;
        LongValue = longValue;
    }

    public TracorDataProperty(string name, bool boolValue) {
        Name = name;
        SetBooleanValue(boolValue);
    }

    public TracorDataProperty(string name, LogLevel levelValue) {
        Name = name;
        SetLevelValue(levelValue);
    }

    public TracorDataProperty(string name, double doubleValue) {
        Name = name;
        SetDoubleValue(doubleValue);
    }

    public TracorDataProperty(string name, DateTime dateTimeValue) {
        Name = name;
        SetDateTimeValue(dateTimeValue);
    }

    public TracorDataProperty(string name, DateTimeOffset dateTimeOffsetValue) {
        Name = name;
        SetDateTimeOffsetValue(dateTimeOffsetValue);
    }

    public TracorDataProperty(string name, Guid uuidValue) {
        Name = name;
        SetUuidValue(uuidValue);
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

            if (value is not { } argValueNotNull) {
                SetNullValue();
                return;
            }

            {
                if (TracorDataUtility.TryCastObjectToStringValue(argValueNotNull, out var resultValue)) {
                    SetStringValue(resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToInteger(argValueNotNull, out var resultValue)) {
                    SetIntegerValue(resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToDoubleValue(argValueNotNull, out var resultValue)) {
                    SetDoubleValue( resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToLogLevelValue(argValueNotNull, out var resultValue)) {
                    SetLevelValue(resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToDateTimeValue(argValueNotNull, out var resultValue)) {
                    SetDateTimeValue( resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToDateTimeOffsetValue(argValueNotNull, out var resultValue)) {
                    SetDateTimeOffsetValue( resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToUuidValue(argValueNotNull, out var resultValue)) {
                    SetUuidValue(resultValue);
                    return;
                }
            }
            {
                if (TracorDataUtility.TryCastObjectToEnumValue(argValueNotNull, out var enumValue)) {
                    SetEnumValue(enumValue);
                    return;
                }
            }
            {
                SetAnyValue(argValueNotNull);
                return;
            }
        }
    }
    public object? InnerObjectValue { get; set; }
    public long LongValue { get; set; }
    public double DoubleValue { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetNullValue(out object? value) {
        value = null;
        return TracorDataPropertyTypeValue.Null == _TypeValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetNullValue() {
        TypeValue = TracorDataPropertyTypeValue.Null;
        InnerObjectValue = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetStringValue(out string value) {
        if (TracorDataPropertyTypeValue.String == TypeValue) {
            value = InnerObjectValue is string textValue ? textValue : string.Empty;
            return true;
        }
        value = string.Empty;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetStringValue(string value) {
        _TypeValue = TracorDataPropertyTypeValue.String;
        InnerObjectValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetIntegerValue(out long value) {
        if (TracorDataPropertyTypeValue.Integer == _TypeValue) {
            value = LongValue;
            return true;
        }
        value = 0L;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetIntegerValue(long value) {
        TypeValue = TracorDataPropertyTypeValue.Integer;
        LongValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetBooleanValue(out bool value) {
        if (TypeValue == TracorDataPropertyTypeValue.Boolean) {
            value = (LongValue == 0 ? false : true);
            return true;
        }
        value = false;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBooleanValue(bool value) {
        TypeValue = TracorDataPropertyTypeValue.Boolean;
        LongValue = value ? 1L : 0L;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetEnumValue(string txtValue) {
        TypeValue = TracorDataPropertyTypeValue.Enum;
        InnerObjectValue = txtValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDoubleValue(out double value) {
        if (TypeValue == TracorDataPropertyTypeValue.Double) {
            value = DoubleValue;
            return true;
        }
        value = double.NaN;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDoubleValue(double value) {
        TypeValue = TracorDataPropertyTypeValue.Double;
        DoubleValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDateTimeValue(DateTime value) {
        TypeValue = TracorDataPropertyTypeValue.DateTime;
        LongValue = TracorDataUtility.DateTimeToUnixTimeNanoseconds(value);
        InnerObjectValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetDateTimeOffsetValue(out DateTimeOffset value) {
        if (TypeValue == TracorDataPropertyTypeValue.DateTimeOffset) {
            if (InnerObjectValue is DateTimeOffset dateTimeOffsetValue) {
                value = dateTimeOffsetValue;
                return true;
            }
            if (InnerObjectValue is string { Length: > 0 } stringValue) {
                if (DateTimeOffset.TryParseExact(
                    stringValue,
                    "o",
                    TracorConstants.TracorCulture.DateTimeFormat,
                    System.Globalization.DateTimeStyles.None,
                    out var result)) {
                    value = result;
                    return true;
                }
            }
            value = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(LongValue, 0);
            return true;
        }
        value = new DateTimeOffset(new DateTime(0, DateTimeKind.Utc), TimeSpan.Zero);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDateTimeOffsetValue(DateTimeOffset value) {
        TypeValue = TracorDataPropertyTypeValue.DateTimeOffset;
        var (ticksValue, _) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(value);
        LongValue = ticksValue;
        InnerObjectValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUuidValue(Guid value) {
        TypeValue = TracorDataPropertyTypeValue.Uuid;
        InnerObjectValue = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryGetAnyValue(out object? value) {
        value = this.InnerObjectValue;
        return TracorDataPropertyTypeValue.Any == _TypeValue;
    }

    public void SetAnyValue(object? value) {
        TypeValue = TracorDataPropertyTypeValue.Any;
        InnerObjectValue = value;
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
                => TracorDataUtility.TryConvertObjectToEnumValue(currentPropertyValue, out var otherTextEnumValue)
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
                    return longValue.ToString(TracorConstants.TracorCulture.NumberFormat);
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
                    return floatValue.ToString(TracorConstants.TracorCulture.NumberFormat);
                }
            case TracorDataPropertyTypeValue.DateTime: {
                    TryGetDateTimeValue(out var dtValue);
                    return dtValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat);
                }
            case TracorDataPropertyTypeValue.DateTimeOffset: {
                    TryGetDateTimeOffsetValue(out var dtoValue);
                    return dtoValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat);
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

    public override readonly bool Equals([NotNullWhen(true)] object? obj) {
        if (obj is null) { return false; }
        if (obj is not TracorDataProperty other) { return false; }
        return TracorDataPropertyEqualityComparer.EqualsRef(in this, in other);
    }

    public readonly bool Equals(TracorDataProperty other) {
        return TracorDataPropertyEqualityComparer.EqualsRef(in this, in other);
    }

    public readonly override int GetHashCode() {
        return TracorDataPropertyEqualityComparer.GetHashCodeRef(in this);
    }

    public static bool operator ==(TracorDataProperty a, TracorDataProperty b ) {
        return TracorDataPropertyEqualityComparer.EqualsRef(in a, in b);
    }

    public static bool operator !=(TracorDataProperty a, TracorDataProperty b) {
        return !TracorDataPropertyEqualityComparer.EqualsRef(in a, in b);
    }

    internal string GetDebuggerDisplay() {
        return $"{this.Name} {this.TypeName} {this.LongValue} {this.DoubleValue} {this.InnerObjectValue}";
    }
}
