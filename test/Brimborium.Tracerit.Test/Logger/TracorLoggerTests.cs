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
        serviceCollection.AddEnabledTracor(
            configuration: default,
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
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
                Predicate((tracorData) => tracorData.IsEqualInteger("abc", 123)))
            .AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        using (var validateLog = tracorValidator.Add(
                Predicate((ITracorData tracorData) =>
                    string.Equals(tracorData.TracorIdentifier.Scope, "Brimborium_Tracerit_Logger.TracorLoggerTests.456", StringComparison.Ordinal)
                    && tracorData.IsEqualInteger(TracorConstants.TracorDataPropertyNameEventId, 456)
                    && tracorData.IsEqualInteger("abc", 123)
                ).AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        await ValueTask.CompletedTask;
    }
}
