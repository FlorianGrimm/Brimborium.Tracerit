namespace Brimborium.Tracerit.Service;

/*
 
([System.DateTime]::UtcNow.Ticks % [int]::MaxValue).ToString() | Set-Clipboard

*/

public partial class LoggerUtility {
    private readonly ILogger _Logger;

    public LoggerUtility(ILogger logger) {
        this._Logger = logger;
    }

    //[LoggerMessage(
    //    EventId = 1450604493,
    //    EventName = "Condition",
    //    Level = LogLevel.Debug,
    //    Message = "Result {result} Callee {callee} fnConditionDisplay:{fnConditionDisplay}")]
    //static partial void LogCondition(ILogger logger, TracorIdentifier callee, bool result, string? fnConditionDisplay);

    public void LogCondition(TracorIdentifier callee, bool result, string? fnConditionDisplay) {
        this._Logger.LogCondition(callee, result, fnConditionDisplay);
    }
}