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

    public static WebApplicationBuilder AddTraceritServerControllers(
        this WebApplicationBuilder webApplicationBuilder,
        string configSectionPath = "",
        Action<TracorLogFileServiceOptions>? configure = null
        ) {
        webApplicationBuilder.Services.AddSingleton<IController, UIEndpoints>();
        webApplicationBuilder.Services.AddSingleton<IController, CollectorTracorEndpoints>();
        webApplicationBuilder.Services.AddSingleton<LogFileService>();

        var optionBuilder = webApplicationBuilder.Services.AddOptions<TracorLogFileServiceOptions>();
        optionBuilder.BindConfiguration(configSectionPath);
        if (configure is not null) {
            optionBuilder.Configure(configure);
        }

        return webApplicationBuilder;
    }
}
