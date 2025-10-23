using Brimborium.Tracerit.Utility;
using System.Collections.Concurrent;

namespace Brimborium.Tracerit.Test.Utility;

/// <summary>
/// Unit tests for ReferenceCountPool and related reference counting functionality.
/// </summary>
public class ReferenceCountPoolTests {

    [Test]
    public async Task ReferenceCountObject_ShouldStartWithReferenceCountOne() {
        // Arrange
        var obj = new TestReferenceCountObject(null);

        // Act & Assert
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(1);
    }

    [Test]
    public async Task ReferenceCountObject_ShouldIncrementReferenceCount() {
        // Arrange
        var obj = new TestReferenceCountObject(null);

        // Act
        obj.IncrementReferenceCount();

        // Assert
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(2);
    }

    [Test]
    public async Task ReferenceCountObject_ShouldDecrementOnDispose() {
        // Arrange
        var obj = new TestReferenceCountObject(null);
        obj.IncrementReferenceCount(); // Count = 2

        // Act
        obj.Dispose(); // Count = 1

        // Assert
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(1);
        await Assert.That(obj.IsResetStateCalled).IsFalse();
    }

    [Test]
    public async Task ReferenceCountObject_ShouldResetStateWhenCountReachesZero() {
        // Arrange
        var obj = new TestReferenceCountObject(null);

        // Act
        obj.Dispose(); // Count = 0

        // Assert
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(0);
        await Assert.That(obj.IsResetStateCalled).IsTrue();
    }

    [Test]
    public async Task ReferenceCountPool_ShouldCreateNewObjectWhenEmpty() {
        // Arrange
        var pool = new TestReferenceCountPool(10);

        // Act
        var obj = pool.Rent();

        // Assert
        await Assert.That(obj).IsNotNull();
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(1);
    }

    [Test]
    public async Task ReferenceCountPool_ShouldReuseReturnedObjects() {
        // Arrange
        var pool = new TestReferenceCountPool(10);
        var obj1 = pool.Rent();
        obj1.Dispose(); // Return to pool

        // Act
        var obj2 = pool.Rent();

        // Assert
        await Assert.That(obj2).IsSameReferenceAs(obj1);
        await Assert.That(((IReferenceCountObject)obj2).CanBeReturned()).IsEqualTo(1);
    }

    [Test]
    public async Task ReferenceCountPool_ShouldTrackCount() {
        // Arrange
        var pool = new TestReferenceCountPool(10);

        // Act
        var obj1 = pool.Rent();
        var obj2 = pool.Rent();
        await Assert.That(pool.Count).IsEqualTo(0); // No objects returned yet

        obj1.Dispose(); // Return one object
        await Assert.That(pool.Count).IsEqualTo(1);

        obj2.Dispose(); // Return second object
        await Assert.That(pool.Count).IsEqualTo(2);
    }

    [Test]
    public async Task ReferenceCountPool_ShouldHandleCapacityLimit() {
        // Arrange
        var pool = new TestReferenceCountPool(2); // Small capacity
        var objects = new List<TestReferenceCountObject>();

        // Act - Fill the pool beyond capacity
        for (int i = 0; i < 5; i++) {
            var obj = pool.Rent();
            objects.Add(obj);
        }

        // Return all objects
        foreach (var obj in objects) {
            obj.Dispose();
        }

        // Assert - Pool should not exceed capacity
        await Assert.That(pool.Count).IsLessThanOrEqualTo(2);
    }

    [Test]
    public async Task ReferenceCountPool_ShouldBeThreadSafe() {
        // Arrange
        var pool = new TestReferenceCountPool(100);
        var tasks = new List<Task>();
        var rentedObjects = new ConcurrentBag<TestReferenceCountObject>();

        // Act - Multiple threads renting and returning objects
        for (int i = 0; i < 10; i++) {
            tasks.Add(Task.Run(() => {
                for (int j = 0; j < 100; j++) {
                    var obj = pool.Rent();
                    rentedObjects.Add(obj);
                    obj.IncrementReferenceCount();
                    obj.Dispose(); // Decrement back to 1
                    obj.Dispose(); // Return to pool
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - All operations should complete without exceptions
        await Assert.That(rentedObjects).HasCount().EqualTo(1000);
    }

    [Test]
    public async Task ReferenceCountObject_PrepareRent_ShouldOnlySucceedWhenReady() {
        // Arrange
        var obj = new TestReferenceCountObject(null);
        obj.Dispose(); // Count = 0, state reset

        // Act & Assert
        await Assert.That(((IReferenceCountObject)obj).PrepareRent()).IsTrue();
        await Assert.That(((IReferenceCountObject)obj).CanBeReturned()).IsEqualTo(1);

        // Should not be able to prepare rent again
        await Assert.That(((IReferenceCountObject)obj).PrepareRent()).IsFalse();
    }


    /// <summary>
    /// Test implementation of ReferenceCountObject for testing purposes.
    /// </summary>
    private class TestReferenceCountObject : ReferenceCountObject {
        public bool IsResetStateCalled { get; private set; }
        private bool _IsStateReset = true;

        public TestReferenceCountObject(IReferenceCountPool? owner) : base(owner) {
        }

        protected override void ResetState() {
            this.IsResetStateCalled = true;
            this._IsStateReset = true;
        }

        protected override bool IsStateReset() => this._IsStateReset;

        public void SetStateNotReset() => this._IsStateReset = false;
    }

    /// <summary>
    /// Test implementation of ReferenceCountPool for testing purposes.
    /// </summary>
    private class TestReferenceCountPool : ReferenceCountPool<TestReferenceCountObject> {
        public TestReferenceCountPool(int capacity) : base(capacity) {
        }

        protected override TestReferenceCountObject Create() {
            return new TestReferenceCountObject(this);
        }
    }
}
