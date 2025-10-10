namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    public const string TypeNameAny = "any";
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameFloat = "flt";
    public const string TypeNameEnum = "enum";
    public const string TypeNameUuid = "uuid";
    public const string TypeNameNull = "null";

    public static TracorDataProperty Create(string argName, object? argValue) {
        {
            if (argValue is null) {
                return CreateNull(argName);
            }
        }
        {
            if (TracorDataUtility.TryConvertToStringValue(argValue, out var resultValue)) {
                return CreateString(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToIntValue(argValue, out var resultValue)) {
                return CreateInteger(argName, resultValue, argValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToFloatValue(argValue, out var resultValue)) {
                return CreateFloat(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToLevelValue(argValue, out var resultValue)) {
                return CreateLevelValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToDateTimeValue(argValue, out var resultValue)) {
                return CreateDateTime(argName, resultValue, argValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToDateTimeOffsetValue(argValue, out var resultValue)) {
                return CreateDateTimeOffset(argName, resultValue, argValue);
            }
        }
        {
            if (TracorDataUtility.TryConvertToUuidValue(argValue, out var resultValue)) {
                return CreateGuid(argName, resultValue);
            }
        }
        {
            if (argValue is { } && argValue.GetType().IsEnum) {
                return CreateEnum(argName, argValue);
            }
        }
        {
            return new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: argValue?.ToString() ?? string.Empty
            ) {
                AnyValue = argValue
            };
        }
    }

    public static TracorDataProperty CreateNull(string argName)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Null,
            textValue: string.Empty
        );

    public static TracorDataProperty CreateString(string argName, string argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.String,
            textValue: argValue
        );

    public static TracorDataProperty CreateLevelValue(string argName, LogLevel argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.LevelValue,
            textValue: argValue.ToString()
        ) {
            InnerLongValue = (long)argValue
        };


    public static TracorDataProperty CreateEnum(string argName, object argValue)
        => new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Enum,
                textValue: argValue.ToString() ?? string.Empty
            ) {
            InnerLongValue = argValue.GetType().IsEnum ? (long)argValue : 0
        };

    public static TracorDataProperty CreateEnum<T>(string argName, T argValue)
        where T : struct, Enum
        => new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Enum,
                textValue: argValue.ToString() ?? string.Empty
            ) {
            InnerLongValue = System.Convert.ToInt64(argValue)
        };

    public static TracorDataProperty CreateDateTime(string argName, DateTime argValue, object? objectArgValue = default) {
        var ns = TracorDataUtility.DateTimeToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTime,
            textValue: argValue.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)
        ) {
            InnerLongValue = ns,
            InnerObjectValue = objectArgValue
        };
    }

    public static TracorDataProperty CreateDateTimeOffset(string argName, DateTimeOffset argValue, object? objectArgValue = default) {
        var (ns, o) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTimeOffset,
            textValue: argValue.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)
        ) {
            InnerLongValue = ns,
            InnerFloatValue = o,
            InnerObjectValue = objectArgValue
        };
    }

    public static TracorDataProperty CreateBoolean(string argName, bool argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Boolean,
            textValue: TracorDataUtility.GetBoolString(argValue)
        ) {
            InnerLongValue = argValue ? 1 : 0
        };

    public static TracorDataProperty CreateInteger(string argName, long argValue, object? objectArgValue = default)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Integer,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
        ) {
            InnerLongValue = argValue,
            InnerObjectValue = objectArgValue
        };

    public static TracorDataProperty CreateFloat(string argName, double argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Float,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
        ) {
            InnerFloatValue = argValue
        };

    public static TracorDataProperty CreateGuid(string argName, Guid argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Uuid,
            textValue: argValue.ToString()
        ) {
            InnerUuidValue = argValue
        };
}