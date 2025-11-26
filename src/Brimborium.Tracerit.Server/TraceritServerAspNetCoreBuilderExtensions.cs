// MIT - Florian Grimm
namespace Microsoft.AspNetCore.Builder;

public static class TraceritServerAspNetCoreBuilderExtensions {

    public static void MapTracorControllerEndpoints(
        this WebApplication app
        ) {
        var listController = app.Services.GetServices<IController>();
        foreach (var controller in listController) {
            controller.MapEndpoints(app);
        }
    }
}