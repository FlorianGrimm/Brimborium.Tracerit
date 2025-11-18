// MIT - Florian Grimm

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.DependencyInjection;

public static class TraceritServerServiceBuilderExtensions {
    public static IServiceCollection AddTraceritCollector(
        this IServiceCollection serviceBuilder
        ) {
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.AddSingleton<ITracorCollector, TracorCollectorService>();
        serviceBuilder.AddSingleton<TracorCollectorHttpService>();
        //serviceBuilder.AddSingleton<TracorCollectorWebSocketService>();
        //serviceBuilder.AddSingleton<WebSocketConnectionManager>();
        //serviceBuilder.AddTransient<WebSocketHandler>();
        serviceBuilder.AddOptions<AspNetCore.Builder.WebSocketOptions>();
        serviceBuilder.AddOptions<AspNetCore.Http.Connections.WebSocketOptions>();
        return serviceBuilder;
    }

    public static IServiceCollection AddTraceritServerControllers(
        this IServiceCollection serviceBuilder
        ) {
        serviceBuilder.AddSingleton<IController, UIEndpoints>();
        serviceBuilder.AddSingleton<IController, CollectorTracorEndpoints>();

        return serviceBuilder;
    }
}
