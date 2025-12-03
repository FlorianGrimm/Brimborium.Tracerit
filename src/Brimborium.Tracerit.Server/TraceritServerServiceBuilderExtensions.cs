// MIT - Florian Grimm

#pragma warning disable IDE0130 // Namespace does not match folder structure

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class TraceritServerServiceBuilderExtensions {
    //public static IServiceCollection AddTracorCollector(
    //    this IServiceCollection serviceBuilder
    //    ) {
    //    serviceBuilder.AddSingleton<TracorDataRecordPool>();
    //    serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
    //    serviceBuilder.AddSingleton<ITracorCollector, TracorCollectorService>();
    //    serviceBuilder.AddSingleton<ITracorCollectorHttpService, TracorCollectorHttpService>();
    //    //serviceBuilder.AddSingleton<TracorCollectorWebSocketService>();
    //    //serviceBuilder.AddSingleton<WebSocketConnectionManager>();
    //    //serviceBuilder.AddTransient<WebSocketHandler>();
    //    serviceBuilder.AddOptions<AspNetCore.Builder.WebSocketOptions>();
    //    serviceBuilder.AddOptions<AspNetCore.Http.Connections.WebSocketOptions>();
    //    return serviceBuilder;
    //}

    public static WebApplicationBuilder AddTracorCollectorServer(
        this WebApplicationBuilder webApplicationBuilder,
        string? configSectionPath,
        Action<TracorLogFileServiceOptions>? configure
        ) {
        webApplicationBuilder.AddTracorCollectorMinimal();

        webApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IController, UIEndpoints>());
        webApplicationBuilder.Services.AddSingleton<IReadLogFileService, ReadLogFileService>();

        webApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IController, CollectorTracorEndpoints>());

        var optionBuilder = webApplicationBuilder.Services.AddOptions<TracorLogFileServiceOptions>();
        optionBuilder.BindConfiguration(configSectionPath ?? string.Empty);
        if (configure is not null) {
            optionBuilder.Configure(configure);
        }

        return webApplicationBuilder;
    }

    public static WebApplicationBuilder AddTracorCollectorMinimal(
        this WebApplicationBuilder webApplicationBuilder
        ) {
        var serviceBuilder = webApplicationBuilder.Services;
        serviceBuilder.AddSingleton<TracorDataRecordPool>();
        serviceBuilder.AddSingleton<TracorMemoryPoolManager>();
        serviceBuilder.Add(ServiceDescriptor.Singleton<IController, CollectorTracorEndpoints>());
        serviceBuilder.AddSingleton<ITracorCollectorHttpService, TracorCollectorHttpService>();
        
        serviceBuilder.AddSingleton<TracorServerCollectorServiceReadAndWrite>();
        serviceBuilder.Add(ServiceDescriptor.Singleton<ITracorServerCollectorWrite>(
            (sp) => sp.GetRequiredService<TracorServerCollectorServiceReadAndWrite>()));
        serviceBuilder.AddSingleton<ITracorServerCollectorReadAndWrite>(
            (sp)=>sp.GetRequiredService<TracorServerCollectorServiceReadAndWrite>());
        
        return webApplicationBuilder;
    }
    public static WebApplicationBuilder AddTracorCollectorClient(
        this WebApplicationBuilder webApplicationBuilder
        ) {
        webApplicationBuilder.Services.AddSingleton<TracorServerCollectorToPublisherService>();
        webApplicationBuilder.Services.Add(ServiceDescriptor.Singleton<ITracorServerCollectorWrite>(
            (serviceProvider) => serviceProvider.GetRequiredService<TracorServerCollectorToPublisherService>()));
        //serviceBuilder.AddSingleton<ITracorCollectivePublisher, TracorCollectivePublisher>();
        //serviceBuilder.TryAdd(ServiceDescriptor.Singleton<TracorEmergencyLogging>(sp=>new TracorEmergencyLogging(sp.GetRequiredService<IOptions<TracorOptions>>())));
        return webApplicationBuilder;
    }
}
