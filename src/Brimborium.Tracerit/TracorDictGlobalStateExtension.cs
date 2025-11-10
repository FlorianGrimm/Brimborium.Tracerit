namespace Brimborium.Tracerit;

public static class TracorDictGlobalStateExtension {
    // StringValue
    public static bool TryGetStringValue(
       this TracorRunningState? that,
       string key,
       out string stringValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetStringValue(out stringValue)) {
            return true;
        }

        { stringValue = string.Empty; return false; }
    }
    public static bool TryGetStringValue(
        this TracorFinishState? that,
        string key,
        out string stringValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetStringValue(out stringValue)) {
            return true;
        }

        { stringValue = string.Empty; return false; }
    }

    public static bool TryGetStringValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out string stringValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetStringValue(out stringValue)) {
            return true;
        }

        { stringValue = string.Empty; return false; }
    }
    public static bool TryCopyStringValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueString(sourcePropertyName, out var stringValue)) {
            globalState.SetValue(new(targetPropertyName, stringValue));
            return true;
        } else {
            return false;
        }
    }

    // Integer
    public static bool TryGetIntegerValue(
       this TracorRunningState? that,
       string key,
       out long integerValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetIntegerValue(out integerValue)) {
            return true;
        }

        { integerValue = 0L; return false; }
    }
  
    public static bool TryGetIntegerValue(
        this TracorFinishState? that,
        string key,
        out long integerValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetIntegerValue(out integerValue)) {
            return true;
        }

        { integerValue = 0L; return false; }
    }

    public static bool TryGetIntegerValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out long integerValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetIntegerValue(out integerValue)) {
            return true;
        }

        { integerValue = 0L; return false; }
    }
   
    public static bool TryCopyIntegerValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueInteger(sourcePropertyName, out long integerValue)) {
            globalState.SetValue(new(targetPropertyName, integerValue));
            return true;
        } else {
            return false;
        }
    }

    // BooleanValue
    public static bool TryGetBooleanValue(
       this TracorRunningState? that,
       string key,
       out bool booleanValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetBooleanValue(out booleanValue)) {
            return true;
        }

        { booleanValue = false; return false; }
    }

    public static bool TryGetBooleanValue(
        this TracorFinishState? that,
        string key,
        out bool booleanValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetBooleanValue(out booleanValue)) {
            return true;
        }

        { booleanValue = false; return false; }
    }

    public static bool TryGetBooleanValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out bool booleanValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetBooleanValue(out booleanValue)) {
            return true;
        }

        { booleanValue = false; return false; }
    }

    public static bool TryCopyBooleanValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueBoolean(sourcePropertyName, out bool booleanValue)) {
            globalState.SetValue(new(targetPropertyName, booleanValue));
            return true;
        } else {
            return false;
        }
    }

    // EnumValue
    public static bool TryGetEnumValue(
       this TracorRunningState? that,
       string key,
       out string enumValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetEnumValue(out enumValue)) {
            return true;
        }

        { enumValue = string.Empty; return false; }
    }


    public static bool TryGetEnumValue(
        this TracorFinishState? that,
        string key,
        out string enumValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetEnumValue(out enumValue)) {
            return true;
        }

        { enumValue = string.Empty; return false; }
    }

    public static bool TryGetEnumValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out string enumValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetEnumValue(out enumValue)) {
            return true;
        }

        { enumValue = string.Empty; return false; }
    }

    public static bool TryCopyEnumValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueEnum(sourcePropertyName, out var enumValue)) {
            globalState.SetValue(new(targetPropertyName, enumValue));
            return true;
        } else {
            return false;
        }
    }

    // LevelValue
    public static bool TryGetLevelValue(
       this TracorRunningState? that,
       string key,
       out LogLevel levelValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetLevelValue(out levelValue)) {
            return true;
        }

        { levelValue = LogLevel.None; return false; }
    }

    public static bool TryGetLevelValue(
        this TracorFinishState? that,
        string key,
        out LogLevel levelValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetLevelValue(out levelValue)) {
            return true;
        }

        { levelValue = LogLevel.None; return false; }
    }

    public static bool TryGetLevelValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out LogLevel levelValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetLevelValue(out levelValue)) {
            return true;
        }

        { levelValue = LogLevel.None; return false; }
    }

    public static bool TryCopyLevelValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueLevelValue(sourcePropertyName, out LogLevel levelValue)) {
            globalState.SetValue(new(targetPropertyName, levelValue));
            return true;
        } else {
            return false;
        }
    }

    // DoubleValue
    public static bool TryGetDoubleValue(
       this TracorRunningState? that,
       string key,
       out double doubleValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDoubleValue(out doubleValue)) {
            return true;
        }

        { doubleValue = 0.0d; return false; }
    }

    public static bool TryGetDoubleValue(
        this TracorFinishState? that,
        string key,
        out double doubleValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDoubleValue(out doubleValue)) {
            return true;
        }

        { doubleValue = 0.0d; return false; }
    }

    public static bool TryGetDoubleValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out double doubleValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetDoubleValue(out doubleValue)) {
            return true;
        }

        { doubleValue = 0.0d; return false; }
    }

    public static bool TryCopyDoubleValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueDouble(sourcePropertyName, out double doubleValue)) {
            globalState.SetValue(new(targetPropertyName, doubleValue));
            return true;
        } else {
            return false;
        }
    }

    // DateTimeValue
    public static bool TryGetDateTimeValue(
       this TracorRunningState? that,
       string key,
       out DateTime dateTimeValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeValue(out dateTimeValue)) {
            return true;
        }

        { dateTimeValue = TracorDataUtility.UnixTimeNanosecondsToDateTime(0); return false; }
    }

    public static bool TryGetDateTimeValue(
        this TracorFinishState? that,
        string key,
        out DateTime dateTimeValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeValue(out dateTimeValue)) {
            return true;
        }

        { dateTimeValue = TracorDataUtility.UnixTimeNanosecondsToDateTime(0); return false; }
    }

    public static bool TryGetDateTimeValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out DateTime dateTimeValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeValue(out dateTimeValue)) {
            return true;
        }

        { dateTimeValue = TracorDataUtility.UnixTimeNanosecondsToDateTime(0); return false; }
    }
   
    public static bool TryCopyDateTimeValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueDateTime(sourcePropertyName, out DateTime dateTimeValue)) {
            globalState.SetValue(new(targetPropertyName, dateTimeValue));
            return true;
        } else {
            return false;
        }
    }

    // DateTimeOffsetValue
    public static bool TryGetDateTimeOffsetValue(
       this TracorRunningState? that,
       string key,
       out DateTimeOffset dateTimeOffsetValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeOffsetValue(out dateTimeOffsetValue)) {
            return true;
        }

        { dateTimeOffsetValue = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(0); return false; }
    }
   
    public static bool TryGetDateTimeOffsetValue(
        this TracorFinishState? that,
        string key,
        out DateTimeOffset dateTimeOffsetValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeOffsetValue(out dateTimeOffsetValue)) {
            return true;
        }

        { dateTimeOffsetValue = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(0); return false; }
    }

    public static bool TryGetDateTimeOffsetValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out DateTimeOffset dateTimeOffsetValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetDateTimeOffsetValue(out dateTimeOffsetValue)) {
            return true;
        }

        { dateTimeOffsetValue = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(0); return false; }
    }
   
    public static bool TryCopyDateTimeOffsetValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueDateTimeOffset(sourcePropertyName, out DateTimeOffset dateTimeOffsetValue)) {
            globalState.SetValue(new(targetPropertyName, dateTimeOffsetValue));
            return true;
        } else {
            return false;
        }
    }

    // UuidValue
    public static bool TryGetUuidValue(
        this TracorRunningState? that,
        string key,
        out Guid uuidValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetUuidValue(out uuidValue)) {
            return true;
        }

        { uuidValue = Guid.Empty; return false; }
    }
   
    public static bool TryGetUuidValue(
        this TracorFinishState? that,
        string key,
        out Guid uuidValue) {
        if (that is { DictGlobalState: { } dictGlobalState }
            && dictGlobalState.TryGetValue(key, out var tdp)
            && tdp.TryGetUuidValue(out uuidValue)) {
            return true;
        }

        { uuidValue = Guid.Empty; return false; }
    }

    public static bool TryGetUuidValue(
        this ImmutableDictionary<string, TracorDataProperty>? that,
        string key,
        out Guid uuidValue) {
        if (that is not null
            && that.TryGetValue(key, out var tdp)
            && tdp.TryGetUuidValue(out uuidValue)) {
            return true;
        }

        { uuidValue = Guid.Empty; return false; }
    }
   
    public static bool TryCopyUuidValue(
        this TracorGlobalState globalState,
        string sourcePropertyName,
        ITracorData tracorData,
        string targetPropertyName
        ) {
        if (tracorData.TryGetPropertyValueUuid(sourcePropertyName, out Guid uuidValue)) {
            globalState.SetValue(new(targetPropertyName, uuidValue));
            return true;
        } else {
            return false;
        }
    }
}
