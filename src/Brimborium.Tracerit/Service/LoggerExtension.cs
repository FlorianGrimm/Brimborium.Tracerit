namespace Brimborium.Tracerit.Service;

public static partial class LoggerExtension {
    [LoggerMessage(
        EventId = 1450604493,
        EventName = "Condition",
        Level = LogLevel.Debug,
        Message = "Result {result} Callee {callee} fnConditionDisplay:{fnConditionDisplay}")]
    public static partial void LogCondition(
        this ILogger logger,
        TracorIdentifier callee, bool result, string? fnConditionDisplay);

    [LoggerMessage(LogLevel.Debug, "{activitySourceIdentifier} returns {result}")]
    public static partial void OnShouldListenToReturns(
        this ILogger logger,
        ActivitySourceIdentifier activitySourceIdentifier, bool result);
}