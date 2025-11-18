namespace Brimborium.Tracerit.API;

public class CollectorTracorEndpoints : IController {
    private readonly TracorCollectorHttpService _CollectorHttpService;
    //private readonly TracorCollectorWebSocketService _CollectorWebSocketService;

    public CollectorTracorEndpoints(
        TracorCollectorHttpService collectorHttpService
        //TracorCollectorWebSocketService collectorWebSocketService
        ) {
        this._CollectorHttpService = collectorHttpService;
        //this._CollectorWebSocketService = collectorWebSocketService;
    }
        
    public void MapEndpoints(WebApplication app) {
        // /_api/tracerit/v1/collector.http
        var group = app.MapGroup("/_api/tracerit/v1");
        group.MapPost("collector.http", async (HttpContext httpContext) => {
            await this._CollectorHttpService.HandlePostAsync(httpContext);
            return Results.Ok();
        }).AllowAnonymous();
        //group.MapPost("collector.ws", async (HttpContext httpContext) => {
        //    await this._CollectorWebSocketService.HandlePostAsync(httpContext);
        //});
    }
}