namespace Brimborium.Tracerit.Test.Service;

/// <summary>
/// Unit tests for ITracor implementations including TracorServiceSink and DisabledTracorServiceSink.
/// </summary>
public class TracorTests {
    
    [Test]
    public async Task TracorServiceSink_ShouldAlwaysReturnFalseForEnabled() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();

        // Act & Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsFalse();
        await Assert.That(tracor.IsCurrentlyEnabled()).IsFalse();
    }

    [Test]
    public async Task TracorServiceSink_ShouldNotDisposeDisposableValues() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();
        var disposableValue = new TestDisposable();

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", disposableValue);

        // Assert
        await Assert.That(disposableValue.IsDisposed).IsFalse();
    }

    [Test]
    public async Task TracorServiceSink_ShouldNotThrowForNonDisposableValues() {
        // Arrange
        var tracor = new DisabledTracorServiceSink();
        
        // Act & Assert - Should not throw
        tracor.TracePublic("test", LogLevel.Information, "test", "test string");
        tracor.TracePublic("test", LogLevel.Information, "test", 42);
        tracor.TracePublic("test", LogLevel.Information, "test", new object());

        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldReturnTrueForGeneralEnabled() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();

        // Act & Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsTrue();
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldProcessTraceData() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        // Create a simple validation path
        var validatorPath = validator.Add(new MatchExpression());

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", "test value");

        // Assert - Should not throw and validator should be processing
        await Assert.That(tracor.IsCurrentlyEnabled()).IsTrue();
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldConvertValuesToTracorData() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentifier("Test", "Method");

        bool tracorDataReceived = false;
        ITracorData? receivedData = null;

        // Create a custom validator to capture the trace data
        var validatorPath = validator.Add(new TestMatchExpression((c, data, ctx) => {
            tracorDataReceived = true;
            receivedData = data;
            return true;
        }));

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", "test value");

        // Assert
        await Assert.That(tracorDataReceived).IsTrue();
        await Assert.That(receivedData).IsNotNull();
        await Assert.That(receivedData!.TryGetPropertyValue("Value", out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("test value");
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldHandleITracorDataDirectly() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentifier("Test", "Method");

        var originalTracorData = new ValueTracorData<string>("direct trace data");
        ITracorData? receivedData = null;

        var validatorPath = validator.Add(new TestMatchExpression((c, data, ctx) => {
            receivedData = data;
            return true;
        }));

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", originalTracorData);

        // Assert
        await Assert.That(receivedData).IsEqualTo(originalTracorData);
    }

    [Test]
    public async Task DisabledTracorServiceSink_ShouldHandleReferenceCountObjects() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracorServiceSink>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var refCountObject = new TestReferenceCountObject();
        var validatorPath = validator.Add(new MatchExpression());

        // Act
        tracor.TracePublic("test", LogLevel.Information, "test", refCountObject);

        // Assert
        await Assert.That(refCountObject.ReferenceCount).IsGreaterThan(0);
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
    /// Test helper class for testing reference counting behavior.
    /// </summary>
    private class TestReferenceCountObject : IReferenceCountObject {
        public int ReferenceCount { get; private set; }

        public void IncrementReferenceCount() {
            this.ReferenceCount++;
        }

        public void DecrementReferenceCount() {
            this.ReferenceCount--;
        }

        public bool PrepareRent() {
            throw new NotImplementedException();
        }

        public long CanBeReturned() {
            throw new NotImplementedException();
        }

        public void Dispose() {
            throw new NotImplementedException();
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
                currentContext.SetStateSuccessful(this, state);
                return TracorValidatorOnTraceResult.Successful;
            }

            return TracorValidatorOnTraceResult.None;
        }

        private class TestMatchState : ValidatorExpressionState {
            protected internal override ValidatorExpressionState Copy()
                => new TestMatchState();
        }
    }
}
