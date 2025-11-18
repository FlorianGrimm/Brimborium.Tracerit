// MIT - Florian Grimm

namespace Brimborium.Tracerit.API;

public class OtlpReceiver : IController {
    public void MapEndpoints(WebApplication app) {
        // /v1/traces
        // /v1/logs
        var group=app.MapGroup("/v1");
        group.MapPost("/traces", async (httpContext) => {
            using (var target = System.IO.File.Create(@"C:\temp\traces.proto")) {
                await httpContext.Request.Body.CopyToAsync(target);
            }
            httpContext.Response.StatusCode = 200;
        }).AllowAnonymous();
        group.MapPost("/logs", async (httpContext) => {
            using (var target = System.IO.File.Create(@"C:\temp\logs.proto")) {
                await httpContext.Request.Body.CopyToAsync(target);
            }
            httpContext.Response.StatusCode = 200;
        }).AllowAnonymous();
    }
}
