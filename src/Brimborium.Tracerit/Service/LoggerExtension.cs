namespace Brimborium.Tracerit.Service;

public static partial class LoggerExtension {
    [LoggerMessage(
        EventName = "ConditionBool",
        Level = LogLevel.Debug,
        Message = "Result {result} Callee {callee} fnConditionDisplay:{fnConditionDisplay}")]
    public static partial void LogConditionBool(
        this ILogger logger,
        TracorIdentifier callee, bool result, string? fnConditionDisplay);

    [LoggerMessage(
        EventName = "ConditionBoolOTR",
        Level = LogLevel.Debug,
        Message = "Result {result} Callee {callee} fnConditionDisplay:{fnConditionDisplay}")]
    public static partial void LogConditionOTR(
    this ILogger logger,
    TracorIdentifier callee, TracorValidatorOnTraceResult result, string? fnConditionDisplay);

    [LoggerMessage(LogLevel.Debug, "{activitySourceIdentifier} returns {result}")]
    public static partial void OnShouldListenToReturns(
        this ILogger logger,
        ActivitySourceIdentifier activitySourceIdentifier, bool result);
}