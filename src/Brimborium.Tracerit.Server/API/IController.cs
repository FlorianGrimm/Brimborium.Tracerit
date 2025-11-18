namespace Brimborium.Tracerit.API;

public interface IController {
    void MapEndpoints(WebApplication app);
}

public static class ControllerExtensions {
    public static void MapEndpoints(
        this WebApplication app
        ) {
        var listController = app.Services.GetServices<IController>();
        foreach (var controller in listController) {
            controller.MapEndpoints(app);
        }
    }
}