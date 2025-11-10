namespace Brimborium.Tracerit.Server;

public class TracorCollectorWebSocketService {
    public TracorCollectorWebSocketService() {
    }

    public async Task HandlePostAsync(HttpContext httpContext) {
        if (httpContext.WebSockets.IsWebSocketRequest) {
            using var webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            var handler = httpContext.RequestServices.GetRequiredService<WebSocketHandler>();
            await handler.HandleAsync(httpContext, webSocket);
        } else {
            httpContext.Response.StatusCode = 400;
        }
    }

    public void X() {
        //global::OpenTelemetry.Proto.Collector.Logs.V1.LogsService
    }

}
