namespace Brimborium.Tracerit.Utility;

public static partial class TracorDataUtility {
    public const string PrefixProp = "Prop";
    public const string PrefixTag = "Tag";
    public readonly static char[] SplitChar = [':'];

    public static (string mode, string name) SplitPropertyName(string propertyName) {
        var propertyNameParts = propertyName.Split(SplitChar, 2);
        if (PrefixProp == propertyNameParts[0]) {
            return (PrefixProp, propertyNameParts[1]);
        } else if (PrefixTag == propertyNameParts[0]) {
            return (PrefixTag, propertyNameParts[1]);
        } else {
            return (string.Empty, propertyName);
        }
    }

    private readonly static object[] _BoolValueBoxes = [(object)false, (object)true];

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
        return (dt.Ticks - _UnixEpochTicks) * _NanosecondsPerTicks;
    }

    public static DateTime UnixTimeNanosecondsToDateTime(long ns) {
        return new DateTime((ns / _NanosecondsPerTicks) + _UnixEpochTicks);
    }

    public static long DateTimeOffsetToUnixTimeNanoseconds(DateTimeOffset dto) {
        return (dto.Ticks - _UnixEpochTicks) * _NanosecondsPerTicks;
    }

    public static DateTimeOffset UnixTimeNanosecondsToDateTimeOffset(long ns) {
        return new DateTimeOffset(new DateTime((ns / _NanosecondsPerTicks) + _UnixEpochTicks), TimeSpan.Zero);
    }

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
            nameof(TracorDataProperty.TypeNameString) => (TracorDataPropertyTypeValue.String, null),
            nameof(TracorDataProperty.TypeNameInteger) => (TracorDataPropertyTypeValue.Integer, null),
            nameof(TracorDataProperty.TypeNameLevelValue) => (TracorDataPropertyTypeValue.LevelValue, null),
            nameof(TracorDataProperty.TypeNameDateTime) => (TracorDataPropertyTypeValue.DateTime, null),
            nameof(TracorDataProperty.TypeNameDateTimeOffset) => (TracorDataPropertyTypeValue.DateTimeOffset, null),
            nameof(TracorDataProperty.TypeNameBoolean) => (TracorDataPropertyTypeValue.Boolean, null),
            nameof(TracorDataProperty.TypeNameLong) => (TracorDataPropertyTypeValue.Long, null),
            nameof(TracorDataProperty.TypeNameDouble) => (TracorDataPropertyTypeValue.Float, null),
            _ => (TracorDataPropertyTypeValue.Any, name)
        };
    }
    public static string TracorDataPropertyConvertTypeValueToString(TracorDataPropertyTypeValue value, string? name) {
        return value switch {
            TracorDataPropertyTypeValue.String => TracorDataProperty.TypeNameString,
            TracorDataPropertyTypeValue.Integer => TracorDataProperty.TypeNameInteger,
            TracorDataPropertyTypeValue.LevelValue => TracorDataProperty.TypeNameLevelValue,
            TracorDataPropertyTypeValue.DateTime => TracorDataProperty.TypeNameDateTime,
            TracorDataPropertyTypeValue.DateTimeOffset => TracorDataProperty.TypeNameDateTimeOffset,
            TracorDataPropertyTypeValue.Boolean => TracorDataProperty.TypeNameBoolean,
            TracorDataPropertyTypeValue.Long => TracorDataProperty.TypeNameLong,
            TracorDataPropertyTypeValue.Float => TracorDataProperty.TypeNameDouble,
            TracorDataPropertyTypeValue.Any => TracorDataProperty.TypeNameAny,
            _ => name ?? TracorDataProperty.TypeNameAny
        };
    }

    public static TracorDataRecordOperation ParseTracorDataRecordOperation(string? value) {
        return value switch {
            null => TracorDataRecordOperation.Data,
            "Data" => TracorDataRecordOperation.Data,
            "Filter" => TracorDataRecordOperation.Filter,
            "Get" => TracorDataRecordOperation.VariableGet,
            "Set" => TracorDataRecordOperation.VariableSet,
            _ => TracorDataRecordOperation.Data,
        };
    }

    public static string TracorDataRecordOperationToString(TracorDataRecordOperation value) {
        return value switch {
            TracorDataRecordOperation.Data => "Data",
            TracorDataRecordOperation.Filter => "Filter",
            TracorDataRecordOperation.VariableGet => "Get",
            TracorDataRecordOperation.VariableSet => "Set",
            _ => "Data",
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