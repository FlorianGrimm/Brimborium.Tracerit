using Microsoft.AspNetCore.Builder;

namespace Brimborium.Tracerit.Test.Expression;

public class DataExpressionTests {
    [Test]
    public async Task DataExpressionUsage() {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddOptions();
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddLogging((builder) => {
            builder.AddTracorLogger();
        });
        serviceBuilder.AddTracor(true);
        serviceBuilder.AddTracorActivityListener(true);
        serviceBuilder.AddActivitySourceBase<SampleTest1Instrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();

        var sampleTest1Instrumentation = serviceProvider.GetRequiredService<SampleTest1Instrumentation>();

        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        RecordExpressionResult reportExpressionResult = new();
        using (var validatorPath = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                    new DataExpression(
                        """
                        [
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:str:test2"
                          ],
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:str:test3"
                          ],
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:str:test1"
                          ]
                        ]
                        """
                )))) {
            using (var rootActivity = sampleTest1Instrumentation.StartRootActivity(name: "aaa")) {
                var activity0 = rootActivity.Activity;
                await Assert.That(activity0).IsNotNull();
                activity0?.SetTag("operation", "test1");

                using (var subActivity0 = sampleTest1Instrumentation.StartActivity(name: "bbb")) {
                    await Assert.That(subActivity0).IsNotNull();
                    subActivity0?.SetTag("operation", "test2");
                }

                using (var subActivity1 = sampleTest1Instrumentation.StartActivity(name: "ccc")) {
                    await Assert.That(subActivity1).IsNotNull();
                    subActivity1?.SetTag("operation", "test3");
                }
            }
            var finished = await validatorPath.GetFinishedAsync(null, TimeSpan.FromSeconds(1));
            await Assert.That(finished).IsNotNull();
        }
        serviceProvider.TracorActivityListenerStop();

        /*
        VerifySettings verifySettings = new VerifySettings();
        verifySettings.IgnoreMember<TracorDataProperty>(i => i.TextValue);
        await Verify(reportExpressionResult.ToTracorListData(), verifySettings);
        */
    }



    [Test]
    public async Task DataExpressionUsageOps() {
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        var serviceBuilder = new ServiceCollection();
        serviceBuilder.AddOptions();
        serviceBuilder.AddSingleton<IConfiguration>(configuration);
        serviceBuilder.AddLogging((builder) => {
            builder.AddTracorLogger();
        });
        serviceBuilder.AddTracor(true);
        serviceBuilder.AddTracorActivityListener(true);
        serviceBuilder.AddActivitySourceBase<SampleTest1Instrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();

        var sampleTest1Instrumentation = serviceProvider.GetRequiredService<SampleTest1Instrumentation>();

        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        RecordExpressionResult reportExpressionResult = new();
        using (var validatorPath = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                    new DataExpression(
                        """
                        [
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:str:test2",
                            "operation:equal"
                          ],
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:str:test3",
                            "operation:set:something"
                          ],
                          [
                            "Source:str:Activity",
                            "Scope:str:sample.test1/Stop",
                            "operation:equal:stop"
                          ]
                        ]
                        """
                        )
                    ), new TracorGlobalState(new() { { "stop", "test1" } })
                )
            ) {
            using (var rootActivity = sampleTest1Instrumentation.StartRootActivity(name: "aaa")) {
                var activity0 = rootActivity.Activity;
                await Assert.That(activity0).IsNotNull();
                activity0?.SetTag("operation", "test1");

                using (var subActivity0 = sampleTest1Instrumentation.StartActivity(name: "bbb")) {
                    await Assert.That(subActivity0).IsNotNull();
                    subActivity0?.SetTag("operation", "test2");
                }

                using (var subActivity1 = sampleTest1Instrumentation.StartActivity(name: "ccc")) {
                    await Assert.That(subActivity1).IsNotNull();
                    subActivity1?.SetTag("operation", "test3");
                }
            }
            var finished = await validatorPath.GetFinishedAsync(null);
            
            await Assert.That(finished).IsNotNull()
                .And.HasMember(f => f["something"]).EqualTo("test3");
        }
        serviceProvider.TracorActivityListenerStop();

        /*
        VerifySettings verifySettings = new VerifySettings();
        verifySettings.IgnoreMember<TracorDataProperty>(i => i.TextValue);
        await Verify(reportExpressionResult.ToTracorListData(), verifySettings);
        */
    }
}
