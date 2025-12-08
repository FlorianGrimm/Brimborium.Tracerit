namespace Brimborium.Tracerit.API;

public class CollectorTracorEndpoints : IController {
    private readonly ITracorCollectorHttpService _CollectorHttpService;
    //private readonly TracorCollectorWebSocketService _CollectorWebSocketService;

    public CollectorTracorEndpoints(
        ITracorCollectorHttpService collectorHttpService
        //TracorCollectorWebSocketService collectorWebSocketService
        ) {
        this._CollectorHttpService = collectorHttpService;
        //this._CollectorWebSocketService = collectorWebSocketService;
    }
        
    public void MapEndpoints(WebApplication app) {
        // /_api/tracerit/v1/collector.http
        var group = app.MapGroup("/_api/tracerit/v1");
        group.MapPost("collector.http/{*applicationName}", async (HttpContext httpContext, string? applicationName) => {
            await this._CollectorHttpService.HandlePostAsync(httpContext, applicationName);
            return Results.Ok();
        }).AllowAnonymous();
        //group.MapPost("collector.ws", async (HttpContext httpContext) => {
        //    await this._CollectorWebSocketService.HandlePostAsync(httpContext);
        //});
    }
}