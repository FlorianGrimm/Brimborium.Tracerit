#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Microsoft.AspNetCore.Builder;

public static class AngularFileServiceExtension {
    public static OptionsBuilder<AngularFileServiceOptions> AddAngularFileService(this IServiceCollection services) {
        OptionsBuilder<AngularFileServiceOptions> result =services.AddOptions<AngularFileServiceOptions>();
        result.BindConfiguration("");
        services.AddSingleton<AngularFileService>();

        return result;
    }

    public static WebApplication UseAngularFileService(this WebApplication app) {
        var angularFileService = app.Services.GetRequiredService<AngularFileService>();
        angularFileService.Initialize();
        ((IEndpointRouteBuilder)app).DataSources.Add(angularFileService);
        app.Use(angularFileService.Execute);
        app.UseStaticFiles();
        return app;
    }
}