namespace Sample.WebApp;

public static partial class LogExtension {
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Ping {i}")]
    public static partial void PingReceived(this ILogger logger, int i);
}