namespace Brimborium.Tracerit.Logger;

/// <summary>
/// Provides extension methods for integrating Tracor logging with the Microsoft.Extensions.Logging framework.
/// </summary>
public static class TracorLoggerLoggingBuilderExtensions {
    /// <summary>
    /// Adds the Tracor logger provider to the logging builder, enabling integration between logging and tracing.
    /// </summary>
    /// <param name="builder">The logging builder to add the Tracor provider to.</param>
    /// <returns>The logging builder for method chaining.</returns>
    public static ILoggingBuilder AddTracorLogger(
        this ILoggingBuilder builder,
        Action<TracorLoggerOptions>? configure = default) {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TracorLoggerProvider>());
        var optionsBuilder = builder.Services.AddOptions<TracorLoggerOptions>();
        if (configure is { }) {
            optionsBuilder.Configure(configure);
        }
        return builder;
    }
}