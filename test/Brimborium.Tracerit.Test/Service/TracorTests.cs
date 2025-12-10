namespace Brimborium.Tracerit.Test.Service;

/// <summary>
/// Unit tests for ITracor implementations including TracorServiceSink and DisabledTracorServiceSink.
/// </summary>
public class TracorTests {

    [Test]
    public async Task DisabledTracorServiceSink_ShouldAlwaysReturnFalseForEnabled() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();

        // Act & Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsFalse();
        await Assert.That(tracor.IsCurrentlyEnabled()).IsFalse();
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldNotDisposeDisposableValues() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();
        var disposableValue = new TestDisposable();

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", disposableValue);

        // Assert
        await Assert.That(disposableValue.IsDisposed).IsFalse();
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldNotThrowForNonDisposableValues() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();

        // Act & Assert - Should not throw
        tracor.TracePublic("test", LogLevel.Information, "test", "test string");
        tracor.TracePublic("test", LogLevel.Information, "test", 42);
        tracor.TracePublic("test", LogLevel.Information, "test", new object());

        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldNotConvertValuesToTracorData() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddDisabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentifier("Test", "Method");

        bool tracorDataReceived = false;
        ITracorData? receivedData = null;

        // Create a custom validator to capture the trace data
        var validatorPath = validator.Add(
            new MatchExpression(
                "Test",
                Predicate((data, state) => {
                    tracorDataReceived = true;
                    receivedData = data;
                    return true;
                })));

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", "test value");

        // Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsFalse();
        await Assert.That(tracorDataReceived).IsFalse();
        await Assert.That(receivedData).IsNull();
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldHandleReferenceCountObjects() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddDisabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        TracorDataRecordPool pool = new(0);
        using (var refCountObject = pool.Rent()) {
            var validatorPath = validator.Add(new MatchExpression());

            await Assert.That(((IReferenceCountObject)refCountObject).CanBeReturned()).IsEqualTo(1);

            // Act
            tracor.TracePublic("test", LogLevel.Information, "test", refCountObject);

            // Assert
            await Assert.That(((IReferenceCountObject)refCountObject).CanBeReturned()).IsEqualTo(1);
        }
    }

    [Test]
    public async Task EnabledTracorServiceSink_ShouldHandleReferenceCountObjects() {
        // Arrange
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>() { { "a", "a" } });

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configuration: default,
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        TracorDataRecordPool pool = new(0);
        TracorDataRecord tracorDataRecord;
        using (var data = pool.Rent()) {
            tracorDataRecord = data;
            data.ListProperty.Add(new TracorDataProperty("a", "a"));
            await Assert.That(((IReferenceCountObject)data).CanBeReturned()).IsEqualTo(1);
            long innerCanBeReturned = 0;
            var validatorPath = validator.Add(
                Predicate((data) => {
                    innerCanBeReturned = ((IReferenceCountObject)data).CanBeReturned();
                    return true;
                }).AsMatch()
                );

            // Act
            tracor.TracePublic("test", LogLevel.Information, "test", data);

            // Assert
            await Assert.That(innerCanBeReturned).IsEqualTo(2);
            await Assert.That(((IReferenceCountObject)data).CanBeReturned()).IsEqualTo(1);
        }
        await Assert.That(
                ((IReferenceCountObject)tracorDataRecord).CanBeReturned()
            ).IsLessThanOrEqualTo(0);
    }

    /// <summary>
    /// Test helper class that implements IDisposable for testing disposal behavior.
    /// </summary>
    private class TestDisposable : IDisposable {
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            this.IsDisposed = true;
        }
    }

    /// <summary>
    /// Test helper expression that allows custom condition logic for testing.
    /// </summary>
    private class TestMatchExpression : ValidatorExpression {
        private readonly Func<TracorIdentifier, ITracorData, OnTraceStepCurrentContext, bool> _Condition;

        public TestMatchExpression(Func<TracorIdentifier, ITracorData, OnTraceStepCurrentContext, bool> condition)
            : base(null) {
            this._Condition = condition;
        }

        public override TracorValidatorOnTraceResult OnTrace(
            ITracorData tracorData,
            OnTraceStepCurrentContext currentContext) {
            var state = currentContext.GetState<TestMatchState>();
            if (state.Result.IsComplete()) {
                return state.Result;
            }

            if (this._Condition(tracorData.TracorIdentifier, tracorData, currentContext)) {
                return currentContext.SetStateSuccessful(this, state, tracorData.Timestamp);
            }

            return TracorValidatorOnTraceResult.None;
        }

        private class TestMatchState : ValidatorExpressionState {
            protected internal override ValidatorExpressionState Copy()
                => new TestMatchState();
        }
    }
}
