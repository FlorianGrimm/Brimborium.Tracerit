#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1873 // Avoid potentially expensive logging

namespace Brimborium_Tracerit_Logger;
#pragma warning restore IDE0079 // Remove unnecessary suppression

using static Brimborium.Tracerit.TracorExtension;

public class TracorLoggerTests {
    [Test]
    public async Task MagicConstantOwnNamespaceLength() {
        await Assert.That(typeof(ITracorServiceSink).Namespace).IsEqualTo(TracorLogger.OwnNamespace);
        await Assert.That(typeof(ITracorServiceSink).Namespace?.Length).IsEqualTo(TracorLogger.OwnNamespaceLength);
    }

    [Test]
    public async Task TracorValidatorSimpleTest() {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddEnabledTracor();
        serviceCollection.AddLogging(loggingBuilder => {
            loggingBuilder.AddTracorLogger();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var publisher = serviceProvider.GetRequiredService<ITracorCollectivePublisher>();
        await Assert.That(publisher.IsEnabled).IsEqualTo(true);

        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory!.CreateLogger<TracorLoggerTests>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        using (var validateLog = tracorValidator.Add(
            new CalleeCondition(
                TracorIdentifier.Create("Brimborium_Tracerit_Logger.TracorLoggerTests.456"),
                Wrap((ITracorData tracorData) =>
                    tracorData.TryGetPropertyValue<int>("abc", out var abc) && (123 == abc)
                ).Predicate()
                ).AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        using (var validateLog = tracorValidator.Add(
                Wrap((ITracorData tracorData) =>
                    tracorData.TryGetPropertyValue<string>("source", out var source) && ("Brimborium_Tracerit_Logger.TracorLoggerTests" == source)
                    && tracorData.TryGetPropertyValue<int>("event.id", out var eventId) && (456 == eventId)
                    && tracorData.TryGetPropertyValue<int>("abc", out var abc) && (123 == abc)
                ).Predicate().AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        await ValueTask.CompletedTask;
    }
}
