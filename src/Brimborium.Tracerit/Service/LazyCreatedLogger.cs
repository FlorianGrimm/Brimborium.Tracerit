
namespace Brimborium.Tracerit.Service;
public sealed class LazyCreatedLogger<T> : ILogger {
    private readonly IServiceProvider _ServiceProvider;

    public LazyCreatedLogger(IServiceProvider serviceProvider) {
        this._ServiceProvider = serviceProvider;
    }

    private ILogger? _Logger;
    private ILogger GetLogger() {
        if (this._Logger is { } logger) { return logger; }
        var loggerFactory = this._ServiceProvider.GetRequiredService<ILoggerFactory>();
        logger = loggerFactory.CreateLogger<T>();
        this._Logger = logger;
        return logger;
    }


    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return this.GetLogger().BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel) {
        return this.GetLogger().IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        this.GetLogger().Log<TState>(logLevel, eventId, state, exception, formatter);
    }
}
