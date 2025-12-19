// MIT - Florian Grimm

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Brimborium.Tracerit.Collector.OOP.Test")]

namespace Brimborium.Tracerit.Collector.ForTesting;

public sealed class Program {
    public const string ApplicationName = "Brimborium.Tracerit.Collector";

    public static async Task<int> Main(string[] args) {
        try {
            // modify the services via StartupActions
            // e.g. add a Testing Authentication
            string pathStaticAssets = GetPathStaticAssets();
            var contentRoot = global::Brimborium.Tracerit.Collector.Program.GetContentRoot();
            var extendedArgs = new string[] {
                    $"--StaticAssets={pathStaticAssets}"
                };
            var nextArgs = args.Concat(extendedArgs).ToArray();
            WebApplicationOptions webApplicationOptions = new() {
                ApplicationName = ApplicationName,
                EnvironmentName = "Development",
                ContentRootPath = contentRoot,
                Args = nextArgs
            };
            var builder = WebApplication.CreateBuilder(webApplicationOptions);

            // needed to kill this process while tunit runs
            Brimborium.Tracerit.Utility.TracorTestingUtility.WireParentTestingProcessForTesting(builder.Configuration);

            await global::Brimborium.Tracerit.Collector.Program.RunAsync(
                builder: builder,
                startupActions: new()
                ).ConfigureAwait(false);
            return 0;
        } catch (Microsoft.Extensions.Hosting.HostAbortedException) {
            return 0;
        } catch (AggregateException error) {
            System.Console.Error.WriteLine(error.ToString());
            error.Handle((e) => {
                System.Console.Error.WriteLine(e.ToString());
                return true;
            });
            return 1;
        } catch (Exception error) {
            System.Console.Error.WriteLine(error.ToString());
            return 1;
        }
    }

    private static string GetPathStaticAssets() {
        var assemblyLocation = Path.GetDirectoryName(
                typeof(Program).Assembly.Location ?? throw new Exception("")
            ) ?? throw new Exception("");
        var result = Path.Combine(
            assemblyLocation,
            $"{ApplicationName}.staticwebassets.endpoints.json");
        return result;
    }
    
    internal static string GetBrimboriumTraceritCollectorForTestingCsproj() {
        return System.IO.Path.Combine(GetProjectSourceCodeFolder(), "Brimborium.Tracerit.Collector.ForTesting.csproj");

        static string GetProjectSourceCodeFolder([System.Runtime.CompilerServices.CallerFilePath] string thisFilePath = "") {
            return System.IO.Path.GetDirectoryName(thisFilePath) ?? throw new Exception("no source code");
        }
    }
}
