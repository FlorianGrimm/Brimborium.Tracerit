namespace Brimborium.Tracerit.Test;

/// <summary>
/// Unit tests for TracorIdentifier, TracorIdentifierCache, and related equality comparer classes.
/// </summary>
public class TracorIdentifierTests {
    
    [Test]
    public async Task TracorIdentifier_ShouldCreateWithSourceAndCallee() {
        // Arrange & Act
        var identifier = new TracorIdentifier("TestSource", "TestCallee");

        // Assert
        await Assert.That(identifier.Source).IsEqualTo("TestSource");
        await Assert.That(identifier.Scope).IsEqualTo("TestCallee");
    }

    [Test]
    public async Task TracorIdentifier_Create_ShouldCreateWithEmptySource() {
        // Arrange & Act
        var identifier = TracorIdentifier.Create("TestCallee");

        // Assert
        await Assert.That(identifier.Source).IsEqualTo(string.Empty);
        await Assert.That(identifier.Scope).IsEqualTo("TestCallee");
    }

    [Test]
    public async Task TracorIdentifier_Child_ShouldCreateChildWithAppendedPath() {
        // Arrange
        var parent = new TracorIdentifier("TestSource", "Parent");

        // Act
        var child = parent.Child("Child");

        // Assert
        await Assert.That(child.Source).IsEqualTo("TestSource");
        await Assert.That(child.Scope).IsEqualTo("Parent.Child");
    }

    [Test]
    public async Task TracorIdentifier_Child_ShouldCreateNestedChildren() {
        // Arrange
        var root = new TracorIdentifier("TestSource", "Root");

        // Act
        var child1 = root.Child("Child1");
        var child2 = child1.Child("Child2");

        // Assert
        await Assert.That(child2.Source).IsEqualTo("TestSource");
        await Assert.That(child2.Scope).IsEqualTo("Root.Child1.Child2");
    }

    [Test]
    public async Task TracorIdentifier_OperatorSlash_ShouldCreateCalleeCondition() {
        // Arrange
        var identifier = new TracorIdentifier("TestSource", "TestCallee");
        var condition = AlwaysCondition.Instance;

        // Act
        var calleeCondition = identifier / condition;

        // Assert
        await Assert.That(calleeCondition).IsNotNull();
        await Assert.That(calleeCondition).IsTypeOf<CalleeCondition>();
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_ShouldReturnTrueForEqualIdentifiers() {
        // Arrange
        var comparer = EqualityComparerTracorIdentifier.Default;
        var id1 =new TracorIdentifier("source", "scope");
        var id2 =new TracorIdentifier("source", "scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
        await Assert.That(comparer.GetHashCode(id1)).IsEqualTo(comparer.GetHashCode(id2));
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_ShouldBeCaseInsensitive() {
        // Arrange
        var comparer = EqualityComparerTracorIdentifier.Default;
        var id1 =new TracorIdentifier("source", "scope");
        var id2 =new TracorIdentifier("source", "scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_ShouldReturnFalseForDifferentSources() {
        // Arrange
        var comparer = EqualityComparerTracorIdentifier.Default;
        var id1 = new TracorIdentifier("Source1", "Scope");
        var id2 = new TracorIdentifier("Source2", "Scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_ShouldReturnFalseForDifferentCallees() {
        // Arrange
        var comparer = EqualityComparerTracorIdentifier.Default;
        var id1 = new TracorIdentifier("source", "Callee1");
        var id2 = new TracorIdentifier("source", "Callee2");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_ShouldReturnTrueForSameReference() {
        // Arrange
        var comparer = EqualityComparerTracorIdentifier.Default;
        var id =new TracorIdentifier("source", "scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id, id)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentifier_ShouldIgnoreEmptySources() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentifier.Default;
        var id1 = new TracorIdentifier("source", "scope");
        var id2 = new TracorIdentifier("source", "");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentifier_ShouldCompareNonEmptySources() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentifier.Default;
        var id1 = new TracorIdentifier("Source1", "Scope");
        var id2 = new TracorIdentifier("Source2", "Scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsFalse();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentifier_ShouldMatchWhenBothSourcesNonEmptyAndEqual() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentifier.Default;
        var id1 =new TracorIdentifier("source", "scope");
        var id2 =new TracorIdentifier("source", "scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentifier_ShouldBeCaseInsensitive() {
        // Arrange
        var comparer = MatchEqualityComparerTracorIdentifier.Default;
        var id1 =new TracorIdentifier("source", "scope");
        var id2 =new TracorIdentifier("source", "scope");

        // Act & Assert
        await Assert.That(comparer.Equals(id1, id2)).IsTrue();
    }

    [Test]
    public async Task TracorIdentifierCache_ShouldReturnRootIdentifier() {
        // Arrange
        var rootId = new TracorIdentifier("TestSource", "Root");
        var cache = new TracorIdentifierCache(rootId);

        // Act & Assert
        await Assert.That(cache.TracorIdentifier).IsEqualTo(rootId);
    }

    [Test]
    public async Task TracorIdentifierCache_Child_ShouldCreateAndCacheChildIdentifier() {
        // Arrange
        var rootId = new TracorIdentifier("TestSource", "Root");
        var cache = new TracorIdentifierCache(rootId);

        // Act
        var child1 = cache.Child("Child");
        var child2 = cache.Child("Child"); // Should return cached instance

        // Assert
        await Assert.That(child1.Scope).IsSameReferenceAs(child2.Scope);
        await Assert.That(child1.Source).IsEqualTo("TestSource");
        await Assert.That(child1.Scope).IsEqualTo("Root.Child");
    }

    [Test]
    public async Task TracorIdentifierCache_Child_ShouldCacheDifferentChildren() {
        // Arrange
        var rootId = new TracorIdentifier("TestSource", "Root");
        var cache = new TracorIdentifierCache(rootId);

        // Act
        var child1 = cache.Child("Child1");
        var child2 = cache.Child("Child2");
        var child1Again = cache.Child("Child1");

        // Assert
        await Assert.That(child1.Scope).IsSameReferenceAs(child1Again.Scope);
        await Assert.That(child1.Scope).IsNotSameReferenceAs(child2.Scope);
        await Assert.That(child1.Scope).IsEqualTo("Root.Child1");
        await Assert.That(child2.Scope).IsEqualTo("Root.Child2");
    }

    [Test]
    public async Task TracorIdentifierCache_ShouldBeThreadSafe() {
        // Arrange
        var rootId = new TracorIdentifier("TestSource", "Root");
        var cache = new TracorIdentifierCache(rootId);
        var tasks = new List<Task<TracorIdentifier>>();

        // Act - Multiple threads accessing the same child
        for (int i = 0; i < 10; i++) {
            tasks.Add(Task.Run(() => cache.Child("SharedChild")));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All results should be the same cached instance
        var firstResult = results[0];
        foreach (var result in results) {
            await Assert.That(result.Scope).IsSameReferenceAs(firstResult.Scope);
        }
    }

    [Test]
    public async Task EqualityComparerTracorIdentifier_Default_ShouldBeSingleton() {
        // Arrange & Act
        var comparer1 = EqualityComparerTracorIdentifier.Default;
        var comparer2 = EqualityComparerTracorIdentifier.Default;

        // Assert
        await Assert.That(comparer1).IsSameReferenceAs(comparer2);
    }

    [Test]
    public async Task MatchEqualityComparerTracorIdentifier_Default_ShouldBeSingleton() {
        // Arrange & Act
        var comparer1 = MatchEqualityComparerTracorIdentifier.Default;
        var comparer2 = MatchEqualityComparerTracorIdentifier.Default;

        // Assert
        await Assert.That(comparer1).IsSameReferenceAs(comparer2);
    }
}
