// MIT - Florian Grimm
namespace Microsoft.AspNetCore.Builder;

public static class TraceritServerAspNetCoreBuilderExtensions {

    public static WebApplication UseTraceritCollector(this WebApplication app) {
        /*
        var webSocketOptions = new Microsoft.AspNetCore.Builder.WebSocketOptions {
            KeepAliveInterval = TimeSpan.FromSeconds(120),
        };
        app.UseWebSockets();
        */

        return app;
    }
}