// MIT - Florian Grimm

namespace SampleForTesting;

public class Program {
    public static async Task<int> Main(string[] args) {
        try {
            // modify the services via StartupActions
            // e.g. add a Testing Authentication
            string pathStaticAssets = GetPathStaticAssets();
            var contentRoot = global::Sample.WebApp.Program.GetContentRoot();
            var extendedArgs = new string[] {
                    @"--environment=Development",
                    $"--contentRoot={contentRoot}",
                    @"--applicationName=Sample",
                    $"--StaticAssets={pathStaticAssets}"
                };
            var nextArgs = args.Concat(extendedArgs).ToArray();
            await global::Sample.WebApp.Program.RunAsync(
                nextArgs,
                new()
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
            "Sample.staticwebassets.endpoints.json");
        return result;
    }
}
