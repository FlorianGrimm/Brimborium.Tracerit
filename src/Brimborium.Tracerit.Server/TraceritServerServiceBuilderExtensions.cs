// MIT - Florian Grimm

#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.Extensions.DependencyInjection;

public static class TraceritServerServiceBuilderExtensions {
    public static IServiceCollection AddTracorCollector(
        this IServiceCollection serviceBuilder
        ) {
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.AddSingleton<ITracorCollector, TracorCollectorService>();
        serviceBuilder.AddSingleton<ITracorCollectorHttpService, TracorCollectorHttpService>();
        //serviceBuilder.AddSingleton<TracorCollectorWebSocketService>();
        //serviceBuilder.AddSingleton<WebSocketConnectionManager>();
        //serviceBuilder.AddTransient<WebSocketHandler>();
        serviceBuilder.AddOptions<AspNetCore.Builder.WebSocketOptions>();
        serviceBuilder.AddOptions<AspNetCore.Http.Connections.WebSocketOptions>();
        return serviceBuilder;
    }

    public static WebApplicationBuilder AddTracorServerControllers(
        this WebApplicationBuilder webApplicationBuilder,
        string configSectionPath = "",
        Action<TracorLogFileServiceOptions>? configure = null
        ) {
        webApplicationBuilder.Services.AddSingleton<IController, UIEndpoints>();
        webApplicationBuilder.Services.AddSingleton<IController, CollectorTracorEndpoints>();
        // webApplicationBuilder.Services.AddSingleton<LogFileService>();

        var optionBuilder = webApplicationBuilder.Services.AddOptions<TracorLogFileServiceOptions>();
        optionBuilder.BindConfiguration(configSectionPath);
        if (configure is not null) {
            optionBuilder.Configure(configure);
        }
        return webApplicationBuilder;
    }


    public static WebApplicationBuilder AddTracorCollectorMinimal(
        this WebApplicationBuilder webApplicationBuilder,
        string configSectionPath = "",
        Action<TracorLogFileServiceOptions>? configure = null
        ) {
        var serviceBuilder=webApplicationBuilder.Services;
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.AddSingleton<ITracorCollectorHttpService, TracorCollectorHttpService>();
        serviceBuilder.AddSingleton<IController, CollectorTracorEndpoints>();
        serviceBuilder.AddSingleton<TracorCollectorToPublisherService>();
        serviceBuilder.AddTransient<ITracorCollector>(
            (serviceProvider)=> serviceProvider.GetRequiredService<TracorCollectorToPublisherService>());

        var optionBuilder = webApplicationBuilder.Services.AddOptions<TracorLogFileServiceOptions>();
        optionBuilder.BindConfiguration(configSectionPath);
        if (configure is not null) {
            optionBuilder.Configure(configure);
        }
        return webApplicationBuilder;
    }
}
