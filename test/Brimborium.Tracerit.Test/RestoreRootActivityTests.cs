using Microsoft.AspNetCore.Builder;

namespace Brimborium.Tracerit.Test;

public class RestoreRootActivityTests {
    [Test]
    public async Task RestoreRootActivity() {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddOptions();
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddLogging((builder) => {
            builder.AddConsole();
        });
        serviceBuilder.AddTracorLogger();
        serviceBuilder.AddTracor(true);
        serviceBuilder.AddTracorActivityListener(true);
        serviceBuilder.AddActivitySourceBase<SampleTest1Instrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();
        var testtimeTracorActivityListener = serviceProvider.GetRequiredService<TesttimeTracorActivityListener>();
        var act = testtimeTracorActivityListener.GetActivitySourceBase();

        var sampleTest1Instrumentation = serviceProvider.GetRequiredService<SampleTest1Instrumentation>();

        RecordExpressionResult reportExpressionResult = new();
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        using (var validatorPath = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                new SequenceExpression(
                    listChild: [
                        Wrap(static(ITracorData data) => data.IsEqual("operation", "test3")).Predicate().AsMatch()
                        ])))
            ) {
            using (var rootActivity = sampleTest1Instrumentation.StartRootActivity(name:"aaa")) {
                var activity0 = rootActivity.Activity;
                await Assert.That(activity0).IsNotNull();
                activity0?.SetTag("operation", "test1");

                using (var subActivity0 = sampleTest1Instrumentation.StartActivity(name:"bbb")) {
                    await Assert.That(subActivity0).IsNotNull();
                    subActivity0?.SetTag("operation", "test2");
                }

                using (var subActivity1 = sampleTest1Instrumentation.StartActivity(name: "ccc")) {
                    await Assert.That(subActivity1).IsNotNull();
                    subActivity1?.SetTag("operation", "test3");
                }
            }

            await validatorPath.GetFinishedAsync(null);
        }
        serviceProvider.TracorActivityListenerStop();
    }
}
