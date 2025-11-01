namespace Brimborium.Tracerit;

public static class TracorConstants {
    /// <summary>
    /// TracorPrivate
    /// </summary>
    public const string SourceTracorPrivate = "TracorPrivate";

    /// <summary>
    /// TracorPublic
    /// </summary>
    public const string SourceTracorPublic = "TracorPublic";

    /// <summary>
    /// Activity
    /// </summary>
    public const string SourceActivity = "Activity";
    public const string MessageActivityStart = "Start";
    public const string MessageActivityStop = "Stop";

    public const string TracorDataPropertyNameTimestamp = "timestamp";
    public const string TracorDataPropertyNameSource = "source";
    public const string TracorDataPropertyNameScope = "scope";
    public const string TracorDataPropertyNameMessage = "message";
    public const string TracorDataPropertyNameValue = "value";
    public const string TracorDataPropertyNameEventId = "event.id";
    public const string TracorDataPropertyNameEventName = "event.name";
    public const string TracorDataPropertyNameActivityTraceId = "activity.traceId";
    public const string TracorDataPropertyNameActivityParentTraceId = "activity.parentTraceId";
    public const string TracorDataPropertyNameActivityParentTraceId2 = "activity.parentTraceId.2";
    public const string TracorDataPropertyNameActivityParentTraceId3 = "activity.parentTraceId.3";
    public const string TracorDataPropertyNameActivitySpanId = "activity.spanId";
    public const string TracorDataPropertyNameActivityParentSpanId = "activity.parentSpanId";
    public const string TracorDataPropertyNameActivityParentSpanId2 = "activity.parentSpanId.2";
    public const string TracorDataPropertyNameActivityParentSpanId3 = "activity.parentSpanId.3";
    public const string TracorDataPropertyNameActivityTraceFlags = "activity.traceFlags";
    public const string TracorDataPropertyNameLogLevel = "logLevel";
    public const string TracorDataPropertyNameExceptionTypeName = "exception.typeName";
    public const string TracorDataPropertyNameExceptionMessage = "exception.message";
    public const string TracorDataPropertyNameExceptionHResult = "exception.hResult";
    public const string TracorDataPropertyNameExceptionVerboseMessage = "exception.VerboseMessage";
    public const string TracorDataPropertyNameOperationName = "activity.OperationName";
    public const string TracorDataPropertyNameDisplayName = "activity.DisplayName";
    public const string TracorDataPropertyNameStartTimeUtc = "activity.StartTimeUtc";
    public const string TracorDataPropertyNameStopTimeUtc = "activity.StopTimeUtc";

    private static CultureInfo? _TracorCulture;
    public static CultureInfo TracorCulture => _TracorCulture ??= System.Globalization.CultureInfo.InvariantCulture;

    public const string TypeNameNull = "null";
    public const string TypeNameString = "str";
    public const string TypeNameInteger = "int";
    public const string TypeNameBoolean = "bool";
    public const string TypeNameEnum = "enum";
    public const string TypeNameLevelValue = "lvl";
    public const string TypeNameDouble = "dbl";
    public const string TypeNameDateTime = "dt";
    public const string TypeNameDateTimeOffset = "dto";
    public const string TypeNameDuration = "dur";
    public const string TypeNameUuid = "uuid";
    public const string TypeNameAny = "any";

}
