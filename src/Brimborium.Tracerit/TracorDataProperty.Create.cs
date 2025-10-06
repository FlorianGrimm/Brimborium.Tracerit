namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    public const string TypeNameAny = "any";
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameLong = "long";
    public const string TypeNameDouble = "dbl";

    public static TracorDataProperty Create(string argName, object argValue) {
        if (argValue is string stringValue) {
            return CreateString(argName, stringValue);
        }
        if (argValue is int intValue) {
            return CreateInteger(argName, intValue);
        }
        if (argValue is LogLevel logLevelValue) {
            return CreateLevelValue(argName, logLevelValue);
        }
        if (argValue is DateTime dtValue) {
            return CreateDateTime(argName, dtValue);
        }
        if (argValue is DateTimeOffset dtoValue) {
            return CreateDateTimeOffset(argName, dtoValue);
        }
        if (argValue is long longValue) {
            return CreateLong(argName, longValue);
        }
        if (argValue is double doubleValue) {
            return CreateFloat(argName, doubleValue);
        }
        if (argValue is float floatValue) {
            return CreateFloat(argName, floatValue);
        }
        if (argValue is Guid guidValue) {
            return CreateGuid(argName, guidValue);
        }
        if (argValue.GetType().IsEnum) {
            return CreateEnum(argName, argValue);
        }
        {
            return new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: argValue.ToString() ?? string.Empty
            ) {
                AnyValue = argValue
            };
        }
    }

    public static TracorDataProperty CreateString(string argName, string argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.String,
            textValue: argValue
        );

    public static TracorDataProperty CreateInteger(string argName, int argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Integer,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
        ) {
            LongValue = argValue
        };

    public static TracorDataProperty CreateLevelValue(string argName, LogLevel argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.LevelValue,
            textValue: argValue.ToString()
        ) {
            LongValue = (long)argValue
        };


    public static TracorDataProperty CreateEnum(string argName, object argValue) 
        => new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: argValue.ToString() ?? string.Empty
            ) {
                LongValue = argValue.GetType().IsEnum ? (long)argValue : 0
            };

    public static TracorDataProperty CreateEnum<T>(string argName, T argValue)
        where T : struct, Enum
        => new TracorDataProperty(
                name: argName,
                typeValue: TracorDataPropertyTypeValue.Any,
                textValue: argValue.ToString() ?? string.Empty
            ) {
            LongValue = System.Convert.ToInt64(argValue)
        };

    public static TracorDataProperty CreateDateTime(string argName, DateTime argValue) {
        var ns = TracorDataUtility.DateTimeToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTime,
            textValue: argValue.ToString("O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)
        ) {
            LongValue = ns
        };
    }

    public static TracorDataProperty CreateDateTimeOffset(string argName, DateTimeOffset argValue) {
        var ns = TracorDataUtility.DateTimeOffsetToUnixTimeNanoseconds(argValue);
        return new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.DateTime,
            textValue: argValue.ToString("O", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)
        ) {
            LongValue = ns
        };
    }

    public static TracorDataProperty CreateBoolean(string argName, bool argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Boolean,
            textValue: TracorDataUtility.GetBoolString(argValue)
        ) {
            LongValue = argValue ? 1 : 0
        };

    public static TracorDataProperty CreateLong(string argName, long argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Long,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
        ) {
            LongValue = argValue
        };

    public static TracorDataProperty CreateFloat(string argName, double argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Float,
            textValue: argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
        ) { 
            FloatValue = argValue
        };

    public static TracorDataProperty CreateGuid(string argName, Guid argValue)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Uuid,
            textValue: argValue.ToString()
        ) {
            UuidValue = argValue
        };
}