using Brimborium.Tracerit.Utility;

namespace Brimborium.Tracerit.Test.Service;

/// <summary>
/// Unit tests for ITracor implementations including RuntimeTracor and TesttimeTracor.
/// </summary>
public class TracorTests {
    
    [Test]
    public async Task RuntimeTracor_ShouldAlwaysReturnFalseForEnabled() {
        // Arrange
        var tracor = new RuntimeTracor();

        // Act & Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsFalse();
        await Assert.That(tracor.IsCurrentlyEnabled()).IsFalse();
    }

    [Test]
    public async Task RuntimeTracor_ShouldDisposeDisposableValues() {
        // Arrange
        var tracor = new RuntimeTracor();
        var disposableValue = new TestDisposable();
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        tracor.Trace(callee, disposableValue);

        // Assert
        await Assert.That(disposableValue.IsDisposed).IsTrue();
    }

    [Test]
    public async Task RuntimeTracor_ShouldNotThrowForNonDisposableValues() {
        // Arrange
        var tracor = new RuntimeTracor();
        var callee = new TracorIdentitfier("Test", "Method");

        // Act & Assert - Should not throw
        tracor.Trace(callee, "test string");
        tracor.Trace(callee, 42);
        tracor.Trace(callee, new object());

        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task TesttimeTracor_ShouldReturnTrueForGeneralEnabled() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var tracor = serviceProvider.GetRequiredService<ITracor>();

        // Act & Assert
        await Assert.That(tracor.IsGeneralEnabled()).IsTrue();
    }

    [Test]
    public async Task TesttimeTracor_ShouldProcessTraceData() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentitfier("Test", "Method");

        // Create a simple validation path
        var validatorPath = validator.Add(new MatchExpression());

        // Act
        tracor.Trace(callee, "test value");

        // Assert - Should not throw and validator should be processing
        await Assert.That(tracor.IsCurrentlyEnabled()).IsTrue();
    }

    [Test]
    public async Task TesttimeTracor_ShouldConvertValuesToTracorData() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentitfier("Test", "Method");

        bool tracorDataReceived = false;
        ITracorData? receivedData = null;

        // Create a custom validator to capture the trace data
        var validatorPath = validator.Add(new TestMatchExpression((c, data, ctx) => {
            tracorDataReceived = true;
            receivedData = data;
            return true;
        }));

        // Act
        tracor.Trace(callee, "test value");

        // Assert
        await Assert.That(tracorDataReceived).IsTrue();
        await Assert.That(receivedData).IsNotNull();
        await Assert.That(receivedData!.TryGetPropertyValue("Value", out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("test value");
    }

    [Test]
    public async Task TesttimeTracor_ShouldHandleITracorDataDirectly() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentitfier("Test", "Method");

        var originalTracorData = new ValueTracorData<string>("direct trace data");
        ITracorData? receivedData = null;

        var validatorPath = validator.Add(new TestMatchExpression((c, data, ctx) => {
            receivedData = data;
            return true;
        }));

        // Act
        tracor.Trace(callee, originalTracorData);

        // Assert
        await Assert.That(receivedData).IsEqualTo(originalTracorData);
    }

    [Test]
    public async Task TesttimeTracor_ShouldHandleReferenceCountObjects() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var tracor = serviceProvider.GetRequiredService<ITracor>();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();
        var callee = new TracorIdentitfier("Test", "Method");

        var refCountObject = new TestReferenceCountObject();
        var validatorPath = validator.Add(new MatchExpression());

        // Act
        tracor.Trace(callee, refCountObject);

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
        private readonly Func<TracorIdentitfier, ITracorData, OnTraceStepCurrentContext, bool> _condition;

        public TestMatchExpression(Func<TracorIdentitfier, ITracorData, OnTraceStepCurrentContext, bool> condition) 
            : base(null) {
            this._condition = condition;
        }

        public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
            var state = currentContext.GetState<TestMatchState>();
            if (state.Successfull) {
                return OnTraceResult.Successfull;
            }

            if (this._condition(callee, tracorData, currentContext)) {
                currentContext.SetStateSuccessfull(this, state);
                return OnTraceResult.Successfull;
            }

            return OnTraceResult.None;
        }

        private class TestMatchState : ValidatorExpressionState {
        }
    }
}
