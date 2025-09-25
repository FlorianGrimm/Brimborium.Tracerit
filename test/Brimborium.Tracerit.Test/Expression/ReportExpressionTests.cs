using Microsoft.AspNetCore.Builder;

namespace Brimborium.Tracerit.Test.Expression;

public class ReportExpressionTests {
    [Test]
    public async Task ReportExpressionUsage() {
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
        serviceBuilder.AddInstrumentation<SampleTestInstrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();
        var testtimeTracorActivityListener = serviceProvider.GetRequiredService<TesttimeTracorActivityListener>();

        var sampleTestInstrumentation = serviceProvider.GetRequiredService<SampleTestInstrumentation>();

        RecordExpressionResult reportExpressionResult = new();
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        using (var validatorPath = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                new SequenceExpression(
                    listChild: [
                        Wrap(static(ActivityTracorData data)
                            => data.TryGetTagValue<string>("operation", out var tagValue)
                            && ("test2"==tagValue)).PredicateTracorData().AsMatch(),
                        Wrap(static(ActivityTracorData data)
                            => data.TryGetTagValue<string>("operation", out var tagValue)
                            && ("test3"==tagValue)).PredicateTracorData().AsMatch(),
                        Wrap(static(ActivityTracorData data)
                            => data.TryGetTagValue<string>("operation", out var tagValue)
                            && ("test1"==tagValue)).PredicateTracorData().AsMatch()
                    ])))
           ) {
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
        }
        serviceProvider.TracorActivityListenerStop();
        await Assert.That(reportExpressionResult.ListData.Count).IsEqualTo(3);
        if (reportExpressionResult.ListData[0].TracorData is ActivityTracorData activityTracorData0) {
            await Assert.That(activityTracorData0.TryGetTagValue<string>("operation", out var tagValue) ? tagValue : "").IsEqualTo("test2");
        }
        if (reportExpressionResult.ListData[1].TracorData is ActivityTracorData activityTracorData1) {
            await Assert.That(activityTracorData1.TryGetTagValue<string>("operation", out var tagValue) ? tagValue : "").IsEqualTo("test3");
        }
        if (reportExpressionResult.ListData[2].TracorData is ActivityTracorData activityTracorData2) {
            await Assert.That(activityTracorData2.TryGetTagValue<string>("operation", out var tagValue) ? tagValue : "").IsEqualTo("test1");
        }

        var json = reportExpressionResult.ToTracorDataCollectionJson(
            new System.Text.Json.JsonSerializerOptions() { WriteIndented = true });
        await Verify(json);
    }
}
