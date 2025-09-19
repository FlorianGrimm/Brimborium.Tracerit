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
}