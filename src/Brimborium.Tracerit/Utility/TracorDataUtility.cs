namespace Brimborium.Tracerit.Utility;

public static partial class TracorDataUtility {

    private readonly static object[] _BoolValueBoxes = [(object)false, (object)true];

    public static bool GetBoolValue(string? value) {
        return value switch {
            null => false,
            "1" => true,
            "0" => false,
            _ => false
        };
    }

    public static object GetBoolValueBoxes(string? value) {
        return value switch {
            null => _BoolValueBoxes[0],
            "1" => _BoolValueBoxes[1],
            "0" => _BoolValueBoxes[0],
            _ => _BoolValueBoxes[0]
        };
    }

    public static object GetBoolValueBoxes(bool value) {
        return value ? _BoolValueBoxes[1] : _BoolValueBoxes[0];
    }

    private readonly static string[] _BoolValueString = ["0", "1"];

    public static string GetBoolString(bool value)
        => value ? _BoolValueString[1] : _BoolValueString[0];

    // opentelemetry-dotnet/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/Implementation/TimestampHelpers.cs
    /*
        ns = (Ticks - UnixEpochTicks) * NanosecondsPerTicks;
        (ns / NanosecondsPerTicks) = (Ticks - UnixEpochTicks)
        (ns / NanosecondsPerTicks) + UnixEpochTicks = Ticks
    */

    private const long _NanosecondsPerTicks = 100;
    private const long _UnixEpochTicks = 621355968000000000; // = DateTimeOffset.FromUnixTimeMilliseconds(0).Ticks

    public static long DateTimeToUnixTimeNanoseconds(DateTime dt) {
        long ticks = (dt.Kind == DateTimeKind.Utc) ? dt.Ticks : dt.ToUniversalTime().Ticks;
        return (ticks - _UnixEpochTicks) * _NanosecondsPerTicks;
    }

    public static DateTime UnixTimeNanosecondsToDateTime(long ns) {
        return new DateTime((ns / _NanosecondsPerTicks) + _UnixEpochTicks, DateTimeKind.Utc);
    }

    public static long DateTimeOffsetToUnixTimeNanoseconds(DateTimeOffset dto) {
        return (dto.UtcTicks - _UnixEpochTicks) * _NanosecondsPerTicks;
    }

    public static (long longVaule, double floatValue) DateTimeOffsetToUnixTimeNanosecondsAndOffset(DateTimeOffset dto) {
        return (
            longVaule: (dto.UtcTicks - _UnixEpochTicks) * _NanosecondsPerTicks,
            floatValue: (double)dto.Offset.TotalMinutes
            );
    }

    public static DateTimeOffset UnixTimeNanosecondsToDateTimeOffset(long ns) {
        return new DateTimeOffset(new DateTime((ns / _NanosecondsPerTicks) + _UnixEpochTicks), TimeSpan.Zero);
    }

    public static DateTimeOffset UnixTimeNanosecondsAndOffsetToDateTimeOffset(long ns, int offset) {
        var tsOffset = TimeSpan.FromMinutes(offset);
        return new DateTimeOffset(
            (ns / _NanosecondsPerTicks) + _UnixEpochTicks + tsOffset.Ticks,
            tsOffset);
    }


    public static bool TryConvertToStringValue(object? value, out string resultValue) {
        bool success;
        (success, resultValue) = TryConvertToStringValue(value);
        return success;
    }

    public static (bool success, string resultValue) TryConvertToStringValue(object? value)
        => (value) switch {
            string v => (true, v),
            _ => (false, string.Empty)
        };

    public static bool TryConvertToIntValue(object? value, out long resultValue) {
        bool success;
        (success, resultValue) = TryConvertToIntValue(value);
        return success;
    }
    public static (bool success, long resultValue) TryConvertToIntValue(object? value)
        => (value) switch {
            byte v => (true, (long)v),
            sbyte v => (true, (long)v),
            short v => (true, (long)v),
            ushort v => (true, (long)v),
            int v => (true, (long)v),
            uint v => (true, (long)v),
            long v => (true, v),
            ulong v => (true, (long)v),
            _ => (false, 0L)
        };

    public static bool TryConvertToFloatValue(object? value, out double resultValue) {
        bool success;
        (success, resultValue) = TryConvertToFloatValue(value);
        return success;
    }

    public static (bool success, double resultValue) TryConvertToFloatValue(object? value)
        => (value) switch {
            float v => (true, (double)v),
            double v => (true, (double)v),
            decimal v => (true, (double)v),
            _ => (false, 0L)
        };

    public static bool TryConvertToLevelValue(object? value, out LogLevel resultValue) {
        bool success;
        (success, resultValue) = TryConvertToLevelValue(value);
        return success;
    }

    public static (bool success, LogLevel resultValue) TryConvertToLevelValue(object? value)
        => (value) switch {
            0 => (true, LogLevel.Trace),
            1 => (true, LogLevel.Debug),
            2 => (true, LogLevel.Information),
            3 => (true, LogLevel.Warning),
            4 => (true, LogLevel.Error),
            5 => (true, LogLevel.Critical),

            LogLevel.Trace => (true, LogLevel.Trace),
            LogLevel.Debug => (true, LogLevel.Debug),
            LogLevel.Information => (true, LogLevel.Information),
            LogLevel.Warning => (true, LogLevel.Warning),
            LogLevel.Error => (true, LogLevel.Error),
            LogLevel.Critical => (true, LogLevel.Critical),

            "Trace" => (true, LogLevel.Trace),
            "Debug" => (true, LogLevel.Debug),
            "Information" => (true, LogLevel.Information),
            "Warning" => (true, LogLevel.Warning),
            "Error" => (true, LogLevel.Error),
            "Critical" => (true, LogLevel.Critical),

            "trace" => (true, LogLevel.Trace),
            "debug" => (true, LogLevel.Debug),
            "information" => (true, LogLevel.Information),
            "warning" => (true, LogLevel.Warning),
            "error" => (true, LogLevel.Error),
            "critical" => (true, LogLevel.Critical),

            _ => (false, LogLevel.None)
        };

    public static bool TryConvertToBoolValue(object? value, out bool resultValue) {
        bool success;
        (success, resultValue) = TryConvertToBoolValue(value);
        return success;
    }

    public static (bool success, bool resultValue) TryConvertToBoolValue(object? value)
        => (value) switch {
            false => (true, false),
            true => (true, true),
            _ => (false, false)
        };

    public static bool TryConvertToDateTimeValue(object? value, out DateTime resultValue) {
        bool success;
        (success, resultValue) = TryConvertToDateTimeValue(value);
        return success;
    }

    public static (bool success, DateTime resultValue) TryConvertToDateTimeValue(object? value)
        => (value) switch {
            DateTime v => (true, (DateTime)v),
            DateOnly v => (true, v.ToDateTime(new TimeOnly())),
            _ => (false, new DateTime(0))
        };

    public static bool TryConvertToDateTimeOffsetValue(object? value, out DateTimeOffset resultValue) {
        bool success;
        (success, resultValue) = TryConvertToDateTimeOffsetValue(value);
        return success;
    }

    public static (bool success, DateTimeOffset resultValue) TryConvertToDateTimeOffsetValue(object? value)
        => (value) switch {
            DateTimeOffset v => (true, v),
            _ => (false, new DateTimeOffset(0, TimeSpan.Zero))
        };

    public static bool TryConvertToEnumValue<T>(object? value, out T resultValue) where T:struct,Enum{
        if (value is T) { 
            resultValue = (T)value;
            return true;
        }
        resultValue = default;
        return false;
    }
    public static bool TryConvertToEnumValue(object? value, out string resultValue) {
        if (value is { } && value.GetType().IsEnum) {
            resultValue = value.ToString() ?? string.Empty;
            return true;
        }
        resultValue = string.Empty;
        return false;
    }


    public static bool TryConvertToUuidValue(object? value, out Guid resultValue) {
        bool success;
        (success, resultValue) = TryConvertToUuidValue(value);
        return success;
    }

    public static (bool success, Guid resultValue) TryConvertToUuidValue(object? value)
        => (value) switch {
            Guid v => (true, v),
            _ => (false, Guid.Empty)
        };


#if false
    public static long ToNanoseconds(TimeSpan duration) {
        return duration.Ticks * NanosecondsPerTicks;
    }
#endif
}


public static partial class TracorDataUtility {
    private readonly static ConcurrentDictionary<string, string> _PropertyName = new(StringComparer.Ordinal);
    private static int _PropertyNameCount;
    public static string GetPropertyName(ReadOnlySpan<char> value) {
        var key = value.ToString();
        if (_PropertyName.TryGetValue(key, out var result)) {
            return result;
        } else {
            result = key;
            if (1024 <= _PropertyNameCount) {
                _PropertyNameCount = 0;
                _PropertyName.Clear();
            }
            if (_PropertyName.TryAdd(key, result)) {
                _PropertyNameCount++;
            }
            return result;
        }
    }

    private readonly static ConcurrentDictionary<string, string> _PropertyValue = new(StringComparer.Ordinal);
    private static int _PropertyValueCount;
    public static string GetPropertyValue(ReadOnlySpan<char> value) {
        var key = value.ToString();
        if (_PropertyValue.TryGetValue(key, out var result)) {
            return result;
        } else {
            result = key;
            if (1024 <= _PropertyValueCount) {
                _PropertyValueCount = 0;
                _PropertyValue.Clear();
            }
            if (_PropertyValue.TryAdd(key, result)) {
                _PropertyValueCount++;
            }
            return result;
        }
    }

    public static (TracorDataPropertyTypeValue value, string? name) TracorDataPropertyConvertStringToTypeName(string? name) {
        return name switch {
            TracorDataProperty.TypeNameNull => (TracorDataPropertyTypeValue.Null, null),
            TracorDataProperty.TypeNameString => (TracorDataPropertyTypeValue.String, null),
            TracorDataProperty.TypeNameInteger => (TracorDataPropertyTypeValue.Integer, null),
            TracorDataProperty.TypeNameLevelValue => (TracorDataPropertyTypeValue.LevelValue, null),
            TracorDataProperty.TypeNameDateTime => (TracorDataPropertyTypeValue.DateTime, null),
            TracorDataProperty.TypeNameDateTimeOffset => (TracorDataPropertyTypeValue.DateTimeOffset, null),
            TracorDataProperty.TypeNameBoolean => (TracorDataPropertyTypeValue.Boolean, null),
            TracorDataProperty.TypeNameFloat => (TracorDataPropertyTypeValue.Float, null),
            TracorDataProperty.TypeNameAny => (TracorDataPropertyTypeValue.Any, null),
            TracorDataProperty.TypeNameEnum => (TracorDataPropertyTypeValue.Enum, null),
            TracorDataProperty.TypeNameUuid => (TracorDataPropertyTypeValue.Uuid, null),
            _ => (TracorDataPropertyTypeValue.Any, name)
        };
    }
    public static string TracorDataPropertyConvertTypeValueToString(TracorDataPropertyTypeValue value, string? name) {
        return value switch {
            TracorDataPropertyTypeValue.Null=> TracorDataProperty.TypeNameNull,
            TracorDataPropertyTypeValue.String => TracorDataProperty.TypeNameString,
            TracorDataPropertyTypeValue.Integer => TracorDataProperty.TypeNameInteger,
            TracorDataPropertyTypeValue.LevelValue => TracorDataProperty.TypeNameLevelValue,
            TracorDataPropertyTypeValue.DateTime => TracorDataProperty.TypeNameDateTime,
            TracorDataPropertyTypeValue.DateTimeOffset => TracorDataProperty.TypeNameDateTimeOffset,
            TracorDataPropertyTypeValue.Boolean => TracorDataProperty.TypeNameBoolean,
            TracorDataPropertyTypeValue.Float => TracorDataProperty.TypeNameFloat,
            TracorDataPropertyTypeValue.Any => TracorDataProperty.TypeNameAny,
            TracorDataPropertyTypeValue.Enum => TracorDataProperty.TypeNameEnum,
            TracorDataPropertyTypeValue.Uuid => TracorDataProperty.TypeNameUuid,
            _ => name ?? TracorDataProperty.TypeNameAny
        };
    }


    public static TracorDataPropertyOpertation ParseTracorDataPropertyOpertationParse(string value) {
        return value switch {
            "" => TracorDataPropertyOpertation.Data,
            "Data" => TracorDataPropertyOpertation.Data,
            "Ignore" => TracorDataPropertyOpertation.Ignore,
            "==" => TracorDataPropertyOpertation.Equal,
            "!=" => TracorDataPropertyOpertation.NotEqual,
            "<" => TracorDataPropertyOpertation.Less,
            "<=" => TracorDataPropertyOpertation.LessOrEqual,
            ">" => TracorDataPropertyOpertation.Greater,
            ">=" => TracorDataPropertyOpertation.GreaterOrEqual,
            "Like" => TracorDataPropertyOpertation.Like,
            _ => TracorDataPropertyOpertation.Data,
        };
    }
    public static string TracorDataPropertyOpertationToString(TracorDataPropertyOpertation value) {
        return value switch {
            TracorDataPropertyOpertation.Data => "",
            TracorDataPropertyOpertation.Ignore => "Ignore",
            TracorDataPropertyOpertation.Equal => "==",
            TracorDataPropertyOpertation.NotEqual => "!=",
            TracorDataPropertyOpertation.Less => "<",
            TracorDataPropertyOpertation.LessOrEqual => "<=",
            TracorDataPropertyOpertation.Greater => ">",
            TracorDataPropertyOpertation.GreaterOrEqual => ">=",
            TracorDataPropertyOpertation.Like => "Like",
            _ => "Data",
        };
    }
}