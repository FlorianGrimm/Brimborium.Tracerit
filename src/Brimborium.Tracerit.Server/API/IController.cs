namespace Brimborium.Tracerit.API;

/// <summary>
/// Interface for ASP.NET Core endpoint controllers to map their endpoints.
/// </summary>
public interface IController {
    /// <summary>
    /// Maps the controller's endpoints to the web application.
    /// </summary>
    /// <param name="app">The web application to map endpoints to.</param>
    void MapEndpoints(WebApplication app);
}
