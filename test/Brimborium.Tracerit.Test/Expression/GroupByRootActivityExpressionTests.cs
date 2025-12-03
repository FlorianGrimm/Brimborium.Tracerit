using Microsoft.AspNetCore.Builder;

namespace Brimborium.Tracerit.Test.Expression;

public class GroupByRootActivityExpressionTests {
    [Test]
    public async Task GroupByRootActivityTest() {
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
                configuration: configuration,
                configure: (options) => {
                    options.ActivitySourceStartEventEnabled = options.ActivitySourceStopEventEnabled = true;
                })
            .AddTracorInstrumentation<SampleTestInstrumentation>();

        var serviceProvider = serviceBuilder.BuildServiceProvider();
        serviceProvider.TracorActivityListenerStart();
        var enabledTracorActivityListener = serviceProvider.GetRequiredService<EnabledTracorActivityListener>();

        var sampleTestInstrumentation = serviceProvider.GetRequiredService<SampleTestInstrumentation>();

        RecordExpressionResult reportExpressionResult = new();
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        using (var validatorPathRecord = tracorValidator.Add(
            new RecordExpression(
                reportExpressionResult,
                NeverCondition.Instance.AsMatch()
                ))) {
            using (var validatorPath = tracorValidator.Add(

                    new GroupByRootActivityExpression(
                        label: "group",
                        onStart: Wrap((ITracorData data)
                                    => data.TracorIdentifier.DoesMatch(sampleTestInstrumentation.GetTracorIdentifier())
                                        ? TracorValidatorOnTraceResult.Successful
                                        : TracorValidatorOnTraceResult.Failed)
                                    .Predicate().AsMatch(),
                        onItem: new SequenceExpression(
                            listChild: [
                                Wrap(static(ITracorData data, TracorGlobalState tracorGlobalState)
                                => {
                                    if (data.IsEqualString("tag.something", "test2")){
                                        tracorGlobalState.SetValue(TracorDataProperty.CreateIntegerValue("see", 1));
                                        return true;
                                    }
                                    return false;
                                }
                                )
                                .Predicate().AsMatch(),
                            Wrap(static(ITracorData data, TracorGlobalState tracorGlobalState)
                                => {
                                    if(data.IsEqualString("tag.something", "test2")){
                                        if (tracorGlobalState.TryGetValue("see", out var see)
                                            && see.TryGetIntegerValue(out var seeValue)){
                                            see.SetIntegerValue(seeValue+2);
                                            tracorGlobalState.SetValue(see);
                                        }
                                        return true;
                                    }
                                return false;
                                }
                                ).Predicate().AsMatch(),
                            Wrap(static(ITracorData data, TracorGlobalState tracorGlobalState)
                                => {
                                    if(data.IsEqualString("tag.something", "test1")){
                                        if (tracorGlobalState.TryGetValue("see", out var see)
                                            && see.TryGetIntegerValue(out var seeValue)){
                                            see.SetIntegerValue(seeValue+2);
                                            tracorGlobalState.SetValue(see);
                                        }
                                        return true;
                                    }
                                return false;
                                }
                                ).Predicate().AsMatch()
                            ]),
                        onStop: Wrap(static (ITracorData data)
                                    => "Stop" == data.TracorIdentifier.Message
                                    ).Predicate().AsMatch()
                        )
                        )

               ) {
                using (var rootActivity = sampleTestInstrumentation.StartRoot(name: "aaa", tags: [new("operation", "test1")])) {
                    var activity0 = rootActivity.Activity;
                    await Assert.That(activity0).IsNotNull();
                    activity0?.SetTag("something", "test1");

                    using (var subActivity0 = sampleTestInstrumentation.Start(name: "bbb")) {
                        await Assert.That(subActivity0).IsNotNull();
                        subActivity0?.SetTag("something", "test2");
                    }

                    using (var subActivity1 = sampleTestInstrumentation.Start(name: "ccc")) {
                        await Assert.That(subActivity1).IsNotNull();
                        subActivity1?.SetTag("something", "test3");
                    }
                }
                using (var rootActivity = sampleTestInstrumentation.StartRoot(name: "aaa2", tags: [new("", "")])) {
                    var activity0 = rootActivity.Activity;
                    await Assert.That(activity0).IsNotNull();
                    activity0?.SetTag("something", "test1");

                    using (var subActivity0 = sampleTestInstrumentation.Start(name: "bbb2")) {
                        await Assert.That(subActivity0).IsNotNull();
                        subActivity0?.SetTag("something", "test2");
                    }

                    using (var subActivity1 = sampleTestInstrumentation.Start(name: "ccc2")) {
                        await Assert.That(subActivity1).IsNotNull();
                        subActivity1?.SetTag("something", "test3");
                    }
                }
                List<TracorFinishState> listFinished = validatorPath.GetListFinished();
                await Assert.That(listFinished.Count).IsEqualTo(2);
            }
        }
        serviceProvider.TracorActivityListenerStop();

        await Assert.That(reportExpressionResult.ListData.Count).IsEqualTo(12);

        // TODO
        if (reportExpressionResult.ListData[2] is TracorDataRecord activityTracorData0) {
            await Assert.That(activityTracorData0.TryGetPropertyValueString("tag.something", out var tagValue) ? tagValue : "").IsEqualTo("test2");
        }
        if (reportExpressionResult.ListData[4] is TracorDataRecord activityTracorData1) {
            await Assert.That(activityTracorData1.TryGetPropertyValueString("tag.something", out var tagValue) ? tagValue : "").IsEqualTo("test3");
        }
        if (reportExpressionResult.ListData[5] is TracorDataRecord activityTracorData2) {
            await Assert.That(activityTracorData2.TryGetPropertyValueString("tag.something", out var tagValue) ? tagValue : "").IsEqualTo("test1");
        }

        /*
        foreach (var data in reportExpressionResult.ListData) {
            data.Timestamp = new DateTime(0);
        }
        var json = TracorDataSerialization.SerializeSimple(
            reportExpressionResult.ListData,
            new System.Text.Json.JsonSerializerOptions(
                TracorDataSerialization.GetMinimalJsonSerializerOptions(null, null)) {
                WriteIndented = true
            });
        await Verify(json);
        */
    }
}
