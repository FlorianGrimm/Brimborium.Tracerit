namespace Brimborium.Tracerit.Logger;

public static class TracorLoggerFactoryExtensions {
    public static ILoggingBuilder AddTracor(this ILoggingBuilder builder) {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TracorLoggerProvider>());
        return builder;
    }
}
