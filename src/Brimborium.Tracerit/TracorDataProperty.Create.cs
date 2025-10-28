#pragma warning disable IDE0009 // Member access should be qualified.
#pragma warning disable IDE0079

using System.Diagnostics.Tracing;

namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    public const string TypeNameAny = "any";
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameDouble = "dbl";
    public const string TypeNameEnum = "enum";
    public const string TypeNameUuid = "uuid";
    public const string TypeNameNull = "null";

    public static TracorDataProperty Create(string argName, object? argValueQ) {
        if (argValueQ is not { } argValueNotNull) {
            return CreateNull(argName);
        }
        
        {
            if (TracorDataUtility.TryCastObjectToStringValue(argValueNotNull, out var resultValue)) {
                return CreateStringValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToInteger(argValueNotNull, out var resultValue)) {
                return CreateIntegerValue(argName, resultValue, argValueNotNull);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDoubleValue(argValueNotNull, out var resultValue)) {
                return CreateDoubleValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToLogLevelValue(argValueNotNull, out var resultValue)) {
                return CreateLevelValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDateTimeValue(argValueNotNull, out var resultValue)) {
                return CreateDateTimeValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToDateTimeOffsetValue(argValueNotNull, out var resultValue)) {
                return CreateDateTimeOffsetValue(argName, resultValue);
            }
        }
        {
            if (TracorDataUtility.TryCastObjectToUuidValue(argValueNotNull, out var resultValue)) {
                return CreateUuidValue(argName, resultValue);
            }
        }
        {
            if (argValueNotNull is { } && argValueNotNull.GetType().IsEnum) {
                return CreateEnumValue(argName, argValueNotNull);
            }
        }
        {
            var result = new TracorDataProperty(argName);
            result.SetAnyValue(argValueNotNull);
            return result;
        }
    }

    public static TracorDataProperty CreateNull(string argName)
        => new TracorDataProperty(
            name: argName,
            typeValue: TracorDataPropertyTypeValue.Null,
            textValue: string.Empty
        );

    public static TracorDataProperty CreateStringValue(string argName, string argValue) {
        var result = new TracorDataProperty(argName);
        result.SetStringValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateIntegerValue(string argName, long argValue, object? objectArgValue = default) {
        var result = new TracorDataProperty(argName);
        result.SetIntegerValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateBoolean(string argName, bool argValue) {
        var result = new TracorDataProperty(argName);
        result.SetBooleanValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateEnumValue(string argName, object argValue) {
        var result = new TracorDataProperty(argName);
        result.SetEnumValue(argValue.ToString()!);
        return result;
    }

    public static TracorDataProperty CreateLevelValue(string argName, LogLevel argValue) {
        var result = new TracorDataProperty(argName);
        result.SetLevelValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateDoubleValue(string argName, double argValue) {
        var result = new TracorDataProperty(argName);
        result.SetDoubleValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateDateTimeValue(string argName, DateTime argValue) {
        var result = new TracorDataProperty(argName);
        result.SetDateTimeValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateDateTimeOffsetValue(string argName, DateTimeOffset argValue) {
        var result = new TracorDataProperty(argName);
        result.SetDateTimeOffsetValue(argValue);
        return result;
    }

    public static TracorDataProperty CreateUuidValue(string argName, Guid argValue) {
        var result = new TracorDataProperty(argName);
        result.SetUuidValue(argValue);
        return result;
    }
}