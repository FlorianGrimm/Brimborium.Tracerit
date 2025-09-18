using Brimborium.Tracerit.TracorActivityListener;
using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brimborium.Tracerit.Test;

public class ActivitySourceBaseTests {
    [Test]
    public async Task AddActivitySourceByTypeTest() {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile(GetActivitySourceBaseTestsJson(), optional: false);
        var configuration = configurationBuilder.Build();

        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddOptions();
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddLogging();
        serviceBuilder.AddTracorLogger();
        serviceBuilder.AddTracor(true, (options) => {
        });
        serviceBuilder.AddTracorActivityListener(false, (options) => {
            options.AddActivitySourceByType<SampleTest1Instrumentation>();
        });
        serviceBuilder.AddTracorActivityListener(true, (options) => {
            options.AddActivitySourceByType<SampleTest2Instrumentation>();
            options.AddActivitySourceByType<SampleTest3Instrumentation>();
        });

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();
        var testtimeTracorActivityListener = serviceProvider.GetRequiredService<TesttimeTracorActivityListener>();
        var act = testtimeTracorActivityListener.GetActivitySourceBase();

        await Assert.That(act.Count).IsEqualTo(3);
        await Assert.That(act).Contains((t) => t is SampleTest1Instrumentation);
        await Assert.That(act).Contains((t) => t is SampleTest2Instrumentation);
        await Assert.That(act).Contains((t) => t is SampleTest3Instrumentation);

        var sampleTest1Instrumentation = act.First(t => t is SampleTest1Instrumentation);
        var sampleTest2Instrumentation = act.First(t => t is SampleTest2Instrumentation);
        var sampleTest3Instrumentation = act.First(t => t is SampleTest3Instrumentation);

        await Assert.That(sampleTest1Instrumentation.LogLevel).IsEqualTo(LogLevel.Trace);
        await Assert.That(sampleTest2Instrumentation.LogLevel).IsEqualTo(LogLevel.Warning);
        await Assert.That(sampleTest3Instrumentation.LogLevel).IsEqualTo(LogLevel.Information);

        serviceProvider.TracorActivityListenerStop();
    }
    private static string GetActivitySourceBaseTestsJson([CallerFilePath] string callerFilePath = "") {
        return System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(callerFilePath) ?? "",
            "ActivitySourceBaseTests.json");
    }
}
