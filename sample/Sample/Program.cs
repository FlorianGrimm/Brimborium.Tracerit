using System.Threading.Tasks;
namespace Sample.WebApp;

public partial class Program {
    public static async Task Main(string[] args) {
        await Run(args);
    }
    public static async Task Run(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddAngularFileService()
            .Configure(options => {
                options.AngularPathPrefix.Add("home");
                options.AngularPathPrefix.Add("page1");
                options.AngularPathPrefix.Add("page2");
            });
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseRouting();

        app.UseAuthorization();

        app.UseAngularFileService();

        await app.RunAsync();
    }
}