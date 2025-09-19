namespace Brimborium.Tracerit.Test;

/// <summary>
/// Unit tests for TracorIdentitfier, TracorIdentitfierCache, and related equality comparer classes.
/// </summary>
public class TracorIdentitfierTests {
    
    [Test]
    public async Task TracorIdentitfier_ShouldCreateWithSourceAndCallee() {
        // Arrange & Act
        var identifier = new TracorIdentitfier("TestSource", "TestCallee");

        // Assert
        await Assert.That(identifier.Source).IsEqualTo("TestSource");
        await Assert.That(identifier.Scope).IsEqualTo("TestCallee");
    }

    [Test]
    public async Task TracorIdentitfier_Create_ShouldCreateWithEmptySource() {
        // Arrange & Act
        var identifier = TracorIdentitfier.Create("TestCallee");

        // Assert
        await Assert.That(identifier.Source).IsEqualTo(string.Empty);
        await Assert.That(identifier.Scope).IsEqualTo("TestCallee");
    }

    [Test]
    public async Task TracorIdentitfier_Child_ShouldCreateChildWithAppendedPath() {
        // Arrange
        var parent = new TracorIdentitfier("TestSource", "Parent");

        // Act
        var child = parent.Child("Child");

        // Assert
        await Assert.That(child.Source).IsEqualTo("TestSource");
        await Assert.That(child.Scope).IsEqualTo("Parent/Child");
    }

    [Test]
    public async Task TracorIdentitfier_Child_ShouldCreateNestedChildren() {
        // Arrange
        var root = new TracorIdentitfier("TestSource", "Root");

        // Act
        var child1 = root.Child("Child1");
        var child2 = child1.Child("Child2");

        // Assert
        await Assert.That(child2.Source).IsEqualTo("TestSource");
        await Assert.That(child2.Scope).IsEqualTo("Root/Child1/Child2");
    }

    [Test]
    public async Task TracorIdentitfier_OperatorSlash_ShouldCreateCalleeCondition() {
        // Arrange
        var identifier = new TracorIdentitfier("TestSource", "TestCallee");
        var condition = AlwaysCondition.Instance;

        // Act
        var calleeCondition = identifier / condition;

        // Assert
        await Assert.That(calleeCondition).IsNotNull();
        await Assert.That(calleeCondition).IsTypeOf<CalleeCondition>();
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldReturnTrueForEqualIdentifiers() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("Source", "Callee");
        var id2 = new TracorIdentitfier("Source", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
        await Assert.That(comparer.GetHashCode(id1)).IsEqualTo(comparer.GetHashCode(id2));
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldBeCaseInsensitive() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("SOURCE", "CALLEE");
        var id2 = new TracorIdentitfier("source", "callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldReturnFalseForDifferentSources() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("Source1", "Callee");
        var id2 = new TracorIdentitfier("Source2", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldReturnFalseForDifferentCallees() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("Source", "Callee1");
        var id2 = new TracorIdentitfier("Source", "Callee2");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldHandleNullValues() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id = new TracorIdentitfier("Source", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(null, null)).IsTrue();
        await Assert.That(comparer.Equals(id, null)).IsFalse();
        await Assert.That(comparer.Equals(null, id)).IsFalse();
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_ShouldReturnTrueForSameReference() {
        // Arrange
        var comparer = EqualityComparerTracorIdentitfier.Default;
        var id = new TracorIdentitfier("Source", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id, id)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentitfier_ShouldIgnoreEmptySources() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("", "Callee");
        var id2 = new TracorIdentitfier("Source", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentitfier_ShouldCompareNonEmptySources() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("Source1", "Callee");
        var id2 = new TracorIdentitfier("Source2", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentitfier_ShouldMatchWhenBothSourcesNonEmptyAndEqual() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("Source", "Callee");
        var id2 = new TracorIdentitfier("Source", "Callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentitfier_ShouldBeCaseInsensitive() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentitfier.Default;
        var id1 = new TracorIdentitfier("SOURCE", "CALLEE");
        var id2 = new TracorIdentitfier("source", "callee");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task TracorIdentitfierCache_ShouldReturnRootIdentifier() {
        // Arrange
        var rootId = new TracorIdentitfier("TestSource", "Root");
        var cache = new TracorIdentitfierCache(rootId);

        // Act & Assert
        await Assert.That(cache.TracorIdentitfier).IsEqualTo(rootId);
    }

    [Test]
    public async Task TracorIdentitfierCache_Child_ShouldCreateAndCacheChildIdentifier() {
        // Arrange
        var rootId = new TracorIdentitfier("TestSource", "Root");
        var cache = new TracorIdentitfierCache(rootId);

        // Act
        var child1 = cache.Child("Child");
        var child2 = cache.Child("Child"); // Should return cached instance

        // Assert
        await Assert.That(child1).IsSameReferenceAs(child2);
        await Assert.That(child1.Source).IsEqualTo("TestSource");
        await Assert.That(child1.Scope).IsEqualTo("Root/Child");
    }

    [Test]
    public async Task TracorIdentitfierCache_Child_ShouldCacheDifferentChildren() {
        // Arrange
        var rootId = new TracorIdentitfier("TestSource", "Root");
        var cache = new TracorIdentitfierCache(rootId);

        // Act
        var child1 = cache.Child("Child1");
        var child2 = cache.Child("Child2");
        var child1Again = cache.Child("Child1");

        // Assert
        await Assert.That(child1).IsSameReferenceAs(child1Again);
        await Assert.That(child1).IsNotSameReferenceAs(child2);
        await Assert.That(child1.Scope).IsEqualTo("Root/Child1");
        await Assert.That(child2.Scope).IsEqualTo("Root/Child2");
    }

    [Test]
    public async Task TracorIdentitfierCache_ShouldBeThreadSafe() {
        // Arrange
        var rootId = new TracorIdentitfier("TestSource", "Root");
        var cache = new TracorIdentitfierCache(rootId);
        var tasks = new List<Task<TracorIdentitfier>>();

        // Act - Multiple threads accessing the same child
        for (int i = 0; i < 10; i++) {
            tasks.Add(Task.Run(() => cache.Child("SharedChild")));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be the same cached instance
        var firstResult = results[0];
        foreach (var result in results) {
            await Assert.That(result).IsSameReferenceAs(firstResult);
        }
    }

    [Test]
    public async Task EqualityComparerTracorIdentitfier_Default_ShouldBeSingleton() {
        // Arrange & Act
        var comparer1 = EqualityComparerTracorIdentitfier.Default;
        var comparer2 = EqualityComparerTracorIdentitfier.Default;

        // Assert
        await Assert.That(comparer1).IsSameReferenceAs(comparer2);
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentitfier_Default_ShouldBeSingleton() {
        // Arrange & Act
        var comparer1 = MatchEqualityComparerTracorIdentitfier.Default;
        var comparer2 = MatchEqualityComparerTracorIdentitfier.Default;

        // Assert
        await Assert.That(comparer1).IsSameReferenceAs(comparer2);
    }
}
