using System.Diagnostics;
using Microsoft.AspNetCore.Builder;

namespace Brimborium.Tracerit.Test.Expression;

public class DataExpressionTests {
    [Test, Explicit]
    public async Task DataExpressionUsage() {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddOptions();
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddLogging((builder) => {
            builder.AddTracorLogger();
        });
        serviceBuilder.AddTracor(
            addEnabledServices: true,
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default)
            .AddTracorActivityListener(
                enabled:true,
                configuration: default,
                configure: default)
            .AddTracorInstrumentation<SampleTestInstrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();

        var sampleTestInstrumentation = serviceProvider.GetRequiredService<SampleTestInstrumentation>();

        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        RecordExpressionResult reportExpressionResult = new();
        using (var validatorPath = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                    new DataExpression(
                        """
                        [
                          [
                             ["source", "str", "Activity"],
                             ["scope", "str", "sample.test.Stop"],
                             ["operation", "str", "test2"]
                          ],
                          [
                            ["source", "str", "Activity"],
                            ["scope", "str", "sample.test.Stop"],
                            ["operation", "str", "test3"]
                          ],
                          [
                            ["source", "str", "Activity"],
                            ["scope", "str", "sample.test.Stop"],
                            ["operation", "str", "test1"]
                          ]
                        ]
                        """
                )))) {
            using (var rootActivity = sampleTestInstrumentation.StartRoot(name: "aaa")) {
                var activity0 = rootActivity.Activity;
                await Assert.That(activity0).IsNotNull();
                activity0?.SetTag("operation", "test1");

                using (var subActivity0 = sampleTestInstrumentation.Start(name: "bbb")) {
                    await Assert.That(subActivity0).IsNotNull();
                    subActivity0?.SetTag("operation", "test2");
                }

                using (var subActivity1 = sampleTestInstrumentation.Start(name: "ccc")) {
                    await Assert.That(subActivity1).IsNotNull();
                    subActivity1?.SetTag("operation", "test3");
                }
            }
            var finished = await validatorPath.GetFinishedAsync(null, TimeSpan.FromMilliseconds(100));
            await Assert.That(finished).IsNotNull();
        }
        serviceProvider.TracorActivityListenerStop();

        /*
        VerifySettings verifySettings = new VerifySettings();
        verifySettings.IgnoreMember<TracorDataProperty>(i => i.TextValue);
        await Verify(reportExpressionResult.ToTracorListData(), verifySettings);
        */
    }
}
