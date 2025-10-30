using System.Runtime.InteropServices;

namespace Brimborium.Tracerit.Utility;

public static partial class TracorDataUtility {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCastObjectToStringValue(object? value, out string resultValue) {
        bool success;
        (success, resultValue) = (value) switch {
            string v => (true, v),
            _ => (false, string.Empty)
        };
        return success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryConvertObjectToStringValue(object? value, out string resultValue) {
        bool success;
        (success, resultValue) = (value) switch {
            string v => (true, v),
            //TODO: more better
            object o => (true, o.ToString() ?? string.Empty),
            _ => (false, string.Empty)
        };
        return success;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryCastObjectToInteger(object? value, out long result) {
        bool matched;
        (matched, result) = value switch {
            byte byteValue => (true, (long)byteValue),
            sbyte byteValue => (true, (long)byteValue),
            short shortValue => (true, (long)shortValue),
            ushort ushortValue => (true, (long)ushortValue),
            int intValue => (true, (long)intValue),
            uint uintValue => (true, (long)uintValue),
            long longValue => (true, (long)longValue),
            ulong ulongValue => (true, (long)ulongValue),
            _ => (false, 0L)
        };
        return matched;
    }

    public static bool TryConvertObjectToIntegerValue(object? value, out long result) {
        bool matched;
        (matched, result) = value switch {
            byte byteValue => (true, (long)byteValue),
            sbyte byteValue => (true, (long)byteValue),
            short shortValue => (true, (long)shortValue),
            ushort ushortValue => (true, (long)ushortValue),
            int intValue => (true, (long)intValue),
            uint uintValue => (true, (long)uintValue),
            long longValue => (true, (long)longValue),
            ulong ulongValue => (true, (long)ulongValue),
            double doubleValue => (true, (long)doubleValue),
            float floatValue => (true, (long)floatValue),
            decimal decimalValue => (true, (long)decimalValue),
            string textValue => (long.TryParse(textValue, TracorConstants.TracorCulture, out var intValue))
                ? (true, intValue)
                : (false, 0L),
            _ => (false, 0L)
        };
        return matched;
    }

    public static bool TryCastObjectToBoolean(object? value, out bool result) {
        if (value is null) {
            result = false;
            return false;
        }
        if (value is bool boolValue) {
            result = boolValue;
            return true;
        }

        result = false;
        return false;
    }
    public static bool TryConvertObjectToBooleanValue(object? value, out bool result) {
        if (value is null) {
            result = false;
            return false;
        }
        if (value is bool boolValue) {
            result = boolValue;
            return true;
        }
        if (value is string stringValue) {
            if (stringValue is "1" or "true" or "True" or "TRUE") {
                result = true;
                return true;
            }
            if (stringValue is "0" or "false" or "False" or "FALSE") {
                result = false;
                return true;
            }
        }
        result = false;
        return false;
    }

    public static bool TryCastObjectToEnumValue(object? value, out string textResult) {
        if (value is null) { textResult = string.Empty; return false; }

        if (value.GetType().IsEnum
            && value.ToString() is { } textValue) {
            textResult = textValue; return true;
        }

        {
            textResult = string.Empty; return false;
        }
    }

    public static bool TryConvertObjectToEnumValue(object? value, out string? textResult) {
        if (value is null) { textResult = null; return false; }

        if (value.GetType().IsEnum) {
            textResult = value.ToString();
            return true;
        }

        if (value is string txtValue) {
            textResult = txtValue;
            return true;
        }

        { textResult = null; return false; }
    }

    public static bool TryCastObjectToLogLevelValue(object? value, out LogLevel result) {
        if (value is null) {
            result = LogLevel.None;
            return false;
        }
        if (value is LogLevel logLevelValue) {
            result = logLevelValue;
            return true;
        }
        if (value is string textValue) {

            switch (textValue) {
                case "Trace": result = LogLevel.Trace; return true;
                case "Debug": result = LogLevel.Debug; return true;
                case "Information": result = LogLevel.Information; return true;
                case "Warning": result = LogLevel.Warning; return true;
                case "Error": result = LogLevel.Error; return true;
                case "Critical": result = LogLevel.Critical; return true;
                case "trace": result = LogLevel.Trace; return true;
                case "debug": result = LogLevel.Debug; return true;
                case "information": result = LogLevel.Information; return true;
                case "warning": result = LogLevel.Warning; return true;
                case "error": result = LogLevel.Error; return true;
                case "critical": result = LogLevel.Critical; return true;
                default: break;
            }

        }
        result = LogLevel.None;
        return false;
    }

    public static bool TryConvertObjectToLogLevelValue(object? value, out LogLevel result) {
        if (value is null) {
            result = LogLevel.None;
            return false;
        }
        if (value is LogLevel logLevelValue) {
            result = logLevelValue;
            return true;
        }
        if (value is string textValue) {

            switch (textValue) {
                case "Trace": result = LogLevel.Trace; return true;
                case "Debug": result = LogLevel.Debug; return true;
                case "Information": result = LogLevel.Information; return true;
                case "Warning": result = LogLevel.Warning; return true;
                case "Error": result = LogLevel.Error; return true;
                case "Critical": result = LogLevel.Critical; return true;
                case "trace": result = LogLevel.Trace; return true;
                case "debug": result = LogLevel.Debug; return true;
                case "information": result = LogLevel.Information; return true;
                case "warning": result = LogLevel.Warning; return true;
                case "error": result = LogLevel.Error; return true;
                case "critical": result = LogLevel.Critical; return true;
                default: break;
            }

        }
        result = LogLevel.None;
        return false;
    }

    public static bool TryCastObjectToDoubleValue(object? value, out double result) {
        if (value is null) { result = 0.0d; return false; }
        bool matched;
        (matched, result) = value switch {
            double doubleValue => (true, (double)doubleValue),
            float floatValue => (true, (double)floatValue),
            decimal decimalValue => (true, (double)decimalValue),
            _ => (false, 0.0d)
        };
        return matched;
    }

    public static bool TryConvertObjectToDoubleValue(object? value, out double result) {
        if (value is null) { result = 0.0d; return false; }
        bool matched;
        (matched, result) = value switch {
            double doubleValue => (true, (double)doubleValue),
            float floatValue => (true, (double)floatValue),
            decimal decimalValue => (true, (double)decimalValue),
            byte byteValue => (true, (double)byteValue),
            sbyte byteValue => (true, (double)byteValue),
            short shortValue => (true, (double)shortValue),
            ushort ushortValue => (true, (double)ushortValue),
            int intValue => (true, (double)intValue),
            uint uintValue => (true, (double)uintValue),
            long longValue => (true, (double)longValue),
            ulong ulongValue => (true, (double)ulongValue),
            _ => (false, 0.0d)
        };
        return matched;
    }

    public static bool TryCastObjectToDateTimeValue(object? value, out DateTime result) {
        if (value is null) { result = new DateTime(0); return false; }
        bool matched;
        (matched, result) = value switch {
            DateTime dateTimeValue => (true, (DateTime)dateTimeValue),
            DateOnly dateOnlyValue => (true, dateOnlyValue.ToDateTime(new TimeOnly())),
            TimeSpan timeSpan => (true, new DateTime(timeSpan.Ticks)),
            _ => (false, new DateTime(0))
        };
        return matched;
    }

    public static bool TryConvertObjectToDateTimeValue(object? value, out DateTime result) {
        if (value is null) { result = new DateTime(0, DateTimeKind.Utc); return false; }
        bool matched;
        (matched, result) = value switch {
            DateTimeOffset dateTimeOffsetValue => (true, new DateTime(dateTimeOffsetValue.UtcTicks, DateTimeKind.Utc)),
            DateTime dateTimeValue => (true, dateTimeValue),
            DateOnly dateOnlyValue => (true, dateOnlyValue.ToDateTime(new TimeOnly())),
            TimeSpan timeSpanValue => (true, new DateTime(timeSpanValue.Ticks, DateTimeKind.Utc)),
            string textValue => DateTime.TryParseExact(
                    s: textValue,
                    format: "O",
                    provider: TracorConstants.TracorCulture.DateTimeFormat,
                    style: DateTimeStyles.AdjustToUniversal,
                    out var dtValue)
                ? (true, dtValue)
                : (false, new DateTime(0, DateTimeKind.Utc)),
            _ => (false, new DateTime(0, DateTimeKind.Utc))
        };
        return matched;
    }

    public static bool TryCastObjectToDateTimeOffsetValue(object? value, out DateTimeOffset result) {
        if (value is null) { result = new DateTimeOffset(0, TimeSpan.Zero); return false; }
        bool matched;
        (matched, result) = value switch {
            DateTimeOffset dateTimeOffsetValue => (true, dateTimeOffsetValue),
            _ => (false, new DateTimeOffset(0, TimeSpan.Zero))
        };
        return matched;
    }

    public static bool TryConvertObjectToDateTimeOffsetValue(object? value, out DateTimeOffset result) {
        if (value is null) { result = new DateTimeOffset(0, TimeSpan.Zero); return false; }
        bool matched;
        (matched, result) = value switch {
            DateTimeOffset dateTimeOffsetValue => (true, dateTimeOffsetValue),
            DateTime dateTimeValue => (true, new DateTimeOffset(dateTimeValue, TimeSpan.Zero)),
            DateOnly dateOnlyValue => (true, new DateTimeOffset(dateOnlyValue.ToDateTime(new TimeOnly()), TimeSpan.Zero)),
            TimeSpan timeSpan => (true, new DateTime(timeSpan.Ticks)),
            string textValue => (DateTimeOffset.TryParseExact(
                input: textValue,
                format: "O",
                formatProvider: TracorConstants.TracorCulture.DateTimeFormat,
                styles: DateTimeStyles.AssumeUniversal,
                out var dtoValue))
                ? (true, dtoValue)
                : (false, new DateTimeOffset(0, TimeSpan.Zero)),
            _ => (false, new DateTimeOffset(0, TimeSpan.Zero))
        };
        return matched;
    }

    public static bool TryCastObjectToUuidValue(object? value, out Guid resultValue) {
        bool success;
        (success, resultValue) = (value) switch {
            Guid v => (true, v),
            _ => (false, Guid.Empty)
        };
        return success;
    }

    public static bool TryConvertObjectToUuidValue(object? value, out Guid resultValue) {
        bool success;
        (success, resultValue) = (value) switch {
            Guid v => (true, v),
            string textValue => (Guid.TryParse(textValue, out var uuidValue)
                ? (true, uuidValue)
                : (false, Guid.Empty)),
            _ => (false, Guid.Empty)
        };
        return success;
    }

#if false
    //

    public static bool TryConvertObjectToIntValue(object? value, out long resultValue) {
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

    public static bool TryConvertToEnumValue<T>(object? value, out T resultValue) where T : struct, Enum {
        if (value is T valueT) {
            resultValue = valueT;
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
#endif

    public static TEnum ConvertLongToEnum<TEnum>(long longValue)
        where TEnum : struct, Enum {
        if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<byte>()) {
            var byteValue = (byte)longValue;
            return Unsafe.As<byte, TEnum>(ref byteValue);
        } else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<short>()) {
            var shortValue = (short)longValue;
            return Unsafe.As<short, TEnum>(ref shortValue);
        } else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<int>()) {
            var intValue = (int)longValue;
            return Unsafe.As<int, TEnum>(ref intValue);
        } else if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<long>()) {
            return Unsafe.As<long, TEnum>(ref longValue);
        }
        return (TEnum)(object)longValue;
    }

    public static long ConvertEnumToLong<TEnum>(TEnum enumValue)
        where TEnum : struct, Enum {
        if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<byte>()) {
            return Unsafe.As<TEnum, byte>(ref enumValue);
        }
        if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<short>()) {
            return Unsafe.As<TEnum, short>(ref enumValue);
        }
        if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<int>()) {
            return Unsafe.As<TEnum, int>(ref enumValue);
        }
        if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<long>()) {
            return Unsafe.As<TEnum, long>(ref enumValue);
        }
        return Convert.ToInt64(enumValue);
    }


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

    public static (long longVaule, long offsetValue) DateTimeOffsetToUnixTimeNanosecondsAndOffset(DateTimeOffset dto) {
        return (
            longVaule: (dto.UtcTicks - _UnixEpochTicks) * _NanosecondsPerTicks,
            offsetValue: (long)dto.Offset.TotalMinutes
            );
    }

    public static DateTimeOffset UnixTimeNanosecondsToDateTimeOffset(long ns) {
        return new DateTimeOffset(new DateTime((ns / _NanosecondsPerTicks) + _UnixEpochTicks), TimeSpan.Zero);
    }

    public static DateTimeOffset UnixTimeNanosecondsAndOffsetToDateTimeOffset(long ns, long offset) {
        var tsOffset = TimeSpan.FromMinutes(offset);
        return new DateTimeOffset(
            (ns / _NanosecondsPerTicks) + _UnixEpochTicks + tsOffset.Ticks,
            tsOffset);
    }

#if false
    public static long ToNanoseconds(TimeSpan duration) {
        return duration.Ticks * NanosecondsPerTicks;
    }
#endif

    public static void SetActivity(List<TracorDataProperty> listProperty) {
        if (Activity.Current is { } activity) {
            SetActivity(listProperty, activity);
        }
    }

    public static void SetActivityIfNeeded(List<TracorDataProperty> listProperty) {
        if (Activity.Current is { } activity) {
            bool found = false;
            {
                var listSpan = CollectionsMarshal.AsSpan(listProperty);
                for (int index = 0; index < listProperty.Count; index++) {
                    ref TracorDataProperty property = ref listSpan[index];
                    if (string.Equals(
                        TracorConstants.TracorDataPropertyNameActivitySpanId,
                        property.Name,
                        StringComparison.OrdinalIgnoreCase)) {
                        found = true; break;
                    }
                }
            }
            if (!found) {
                SetActivity(listProperty, activity);
            }
        }
    }

    public static void SetActivity(List<TracorDataProperty> listProperty, Activity activity) {
        listProperty.Add(
            TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameActivitySpanId,
                activity.Id ?? string.Empty));

        listProperty.Add(
            TracorDataProperty.CreateStringValue(
                TracorConstants.TracorDataPropertyNameActivityTraceId,
                activity.TraceId.ToString()));

        if (activity.ParentId is { Length: > 0 } parentId) {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityParentTraceId,
                    parentId ?? string.Empty));
        }

        var parentSpanId = activity.ParentSpanId;
        if ("0000000000000000" != parentSpanId.ToHexString()) {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityParentSpanId,
                    parentSpanId.ToHexString()));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConvertObjectToListProperty(
        bool isPublic,
        int levelWatchDog,
        string key, 
        object? value, 
        List<TracorDataProperty> targetListProperty, 
        ITracorDataConvertService tracorDataConvertService) {
        if (value is null){ return; }
        var tracorDataProperty = TracorDataProperty.Create(key, value);
        if (TracorDataPropertyTypeValue.Any != tracorDataProperty.TypeValue) {
            targetListProperty.Add(tracorDataProperty);
        } else {
            tracorDataConvertService.ConvertObjectToListProperty(
                isPublic: isPublic,
                levelWatchDog: levelWatchDog,
                name: key,
                value: value,
                listProperty: targetListProperty);
        }

    }
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
            TracorDataProperty.TypeNameLevelValue => (TracorDataPropertyTypeValue.Level, null),
            TracorDataProperty.TypeNameDateTime => (TracorDataPropertyTypeValue.DateTime, null),
            TracorDataProperty.TypeNameDateTimeOffset => (TracorDataPropertyTypeValue.DateTimeOffset, null),
            TracorDataProperty.TypeNameBoolean => (TracorDataPropertyTypeValue.Boolean, null),
            TracorDataProperty.TypeNameDouble => (TracorDataPropertyTypeValue.Double, null),
            TracorDataProperty.TypeNameAny => (TracorDataPropertyTypeValue.Any, null),
            TracorDataProperty.TypeNameEnum => (TracorDataPropertyTypeValue.Enum, null),
            TracorDataProperty.TypeNameUuid => (TracorDataPropertyTypeValue.Uuid, null),
            _ => (TracorDataPropertyTypeValue.Any, name)
        };
    }
    public static string TracorDataPropertyConvertTypeValueToString(TracorDataPropertyTypeValue value, string? name) {
        return value switch {
            TracorDataPropertyTypeValue.Null => TracorDataProperty.TypeNameNull,
            TracorDataPropertyTypeValue.String => TracorDataProperty.TypeNameString,
            TracorDataPropertyTypeValue.Integer => TracorDataProperty.TypeNameInteger,
            TracorDataPropertyTypeValue.Level => TracorDataProperty.TypeNameLevelValue,
            TracorDataPropertyTypeValue.DateTime => TracorDataProperty.TypeNameDateTime,
            TracorDataPropertyTypeValue.DateTimeOffset => TracorDataProperty.TypeNameDateTimeOffset,
            TracorDataPropertyTypeValue.Boolean => TracorDataProperty.TypeNameBoolean,
            TracorDataPropertyTypeValue.Double => TracorDataProperty.TypeNameDouble,
            TracorDataPropertyTypeValue.Any => TracorDataProperty.TypeNameAny,
            TracorDataPropertyTypeValue.Enum => TracorDataProperty.TypeNameEnum,
            TracorDataPropertyTypeValue.Uuid => TracorDataProperty.TypeNameUuid,
            _ => name ?? TracorDataProperty.TypeNameAny
        };
    }

#if false
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
#endif
}