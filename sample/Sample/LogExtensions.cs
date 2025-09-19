namespace Sample.WebApp;

public static partial class LogExtensions {
    [LoggerMessage(Level=LogLevel.Information, Message = "ping result {now}")]
    public static partial void PingResult(this ILogger logger, DateTimeOffset now);
}
