#pragma warning disable IDE0130 // Namespace does not match folder structure
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA1873 // Avoid potentially expensive logging

namespace Brimborium_Tracerit_Logger;
#pragma warning restore IDE0079 // Remove unnecessary suppression

using static Brimborium.Tracerit.TracorExtension;

public class TracorLoggerTests {
    [Test]
    public async Task MagicConstantOwnNamespaceLength() {
#pragma warning disable TUnitAssertions0005 // Assert.That(...) should not be used with a constant value
        await Assert.That(typeof(ITracor).Namespace).IsEqualTo(TracorLogger.OwnNamespace);
        await Assert.That(typeof(ITracor).Namespace?.Length).IsEqualTo(TracorLogger.OwnNamespaceLength);
#pragma warning restore TUnitAssertions0005 // Assert.That(...) should not be used with a constant value
    }

    [Test]
    public async Task M() {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTesttimeTracor();
        serviceCollection.AddLogging(loggingBuilder => {
            loggingBuilder.AddTracorLogger();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory!.CreateLogger<TracorLoggerTests>();
        var tracorValidator = serviceProvider.GetRequiredService<ITracorValidator>();
        using (var validateLog = tracorValidator.Add(
            new CalleeCondition(
                TracorIdentitfier.Create("Brimborium_Tracerit_Logger.TracorLoggerTests/456"),
                Wrap((LoggerTracorData tracorData) =>
                    tracorData.TryGetPropertyValue<int>("abc", out var abc) && (123 == abc)
                ).PredicateTracorData()
                ).AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        using (var validateLog = tracorValidator.Add(
                Wrap((LoggerTracorData tracorData) =>
                    tracorData.TryGetPropertyValue<string>("Source", out var source) && ("Brimborium_Tracerit_Logger.TracorLoggerTests" == source)
                    && tracorData.TryGetPropertyValue<int>("Event.Id", out var eventId) && (456 == eventId)
                    && tracorData.TryGetPropertyValue<int>("abc", out var abc) && (123 == abc)
                ).PredicateTracorData().AsMatch()
            )) {
            logger.LogInformation(new EventId(456), "Test {abc}", 123);
            await Assert.That(validateLog.GetFinished(null)).IsNotNull();
        }

        await ValueTask.CompletedTask;
    }
}
