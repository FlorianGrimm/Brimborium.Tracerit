// MIT - Florian Grimm
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
}
