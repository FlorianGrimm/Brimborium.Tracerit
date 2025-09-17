#if false
namespace Brimborium.Tracerit.Test.Condition;

/// <summary>
/// Unit tests for condition classes including AlwaysCondition, NeverCondition, AndCondition, OrCondition, and predicate conditions.
/// </summary>
public class ConditionTests {

    [Test]
    public async Task AlwaysCondition_ShouldAlwaysReturnTrue() {
        // Arrange
        var condition = AlwaysCondition.Instance;
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = condition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task AlwaysCondition_ShouldBeSingleton() {
        // Arrange & Act
        var instance1 = AlwaysCondition.Instance;
        var instance2 = AlwaysCondition.Instance;

        // Assert
        await Assert.That(instance1).IsSameReferenceAs(instance2);
    }

    [Test]
    public async Task AlwaysConditionTyped_ShouldSetGlobalState() {
        // Arrange
        var condition = new AlwaysCondition<string, string>(
            fnGetProperty: value => value.ToUpper(),
            setGlobalState: "UpperValue");

        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = condition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsTrue();
        await Assert.That(context.GlobalState.TryGetValue("UpperValue", out var value)).IsTrue();
        await Assert.That(value).IsEqualTo("TEST");
    }

    [Test]
    public async Task NeverCondition_ShouldAlwaysReturnFalse() {
        // Arrange
        var condition = NeverCondition.Instance;
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = condition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task AndCondition_ShouldReturnTrueWhenAllConditionsMatch() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value.Length > 0);
        var condition2 = new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value.StartsWith("t"));

        var andCondition = new AndCondition(condition1, condition2);
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = andCondition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task AndCondition_ShouldReturnFalseWhenAnyConditionFails() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data => true);
        var condition2 = new PredicateTracorDataCondition(data => false);

        var andCondition = new AndCondition(condition1, condition2);
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = andCondition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task OrCondition_ShouldReturnTrueWhenAnyConditionMatches() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data => false);
        var condition2 = new PredicateTracorDataCondition(data => true);

        var orCondition = new OrCondition(condition1, condition2);
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = orCondition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task OrCondition_ShouldReturnFalseWhenAllConditionsFail() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data => false);
        var condition2 = new PredicateTracorDataCondition(data => false);

        var orCondition = new OrCondition(condition1, condition2);
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act
        var result = orCondition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PredicateTracorDataCondition_ShouldEvaluatePredicate() {
        // Arrange
        var condition = new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "expected");

        var callee = new TracorIdentitfier("Test", "Method");
        var context = CreateTestContext();

        // Act & Assert
        var result1 = condition.DoesMatch(callee, new ValueTracorData<string>("expected"), context);
        await Assert.That(result1).IsTrue();

        var result2 = condition.DoesMatch(callee, new ValueTracorData<string>("unexpected"), context);
        await Assert.That(result2).IsFalse();
    }

    [Test]
    public async Task PredicateValueCondition_ShouldEvaluateTypedValue() {
        // Arrange
        var condition = new PredicateValueCondition<string>(value => value.Length > 3);
        var callee = new TracorIdentitfier("Test", "Method");
        var context = CreateTestContext();

        // Act & Assert
        var result1 = condition.DoesMatch(callee, new ValueTracorData<string>("test"), context);
        await Assert.That(result1).IsTrue();

        var result2 = condition.DoesMatch(callee, new ValueTracorData<string>("no"), context);
        await Assert.That(result2).IsFalse();
    }

    [Test]
    public async Task PredicateValueCondition_ShouldReturnFalseForWrongType() {
        // Arrange
        var condition = new PredicateValueCondition<string>(value => true);
        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<int>(42);
        var context = CreateTestContext();

        // Act
        var result = condition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task PredicateValueGlobalStateCondition_ShouldAccessGlobalState() {
        // Arrange
        var condition = new PredicateValueGlobalStateCondition<string>((value, globalState) =>
            globalState.TryGetValue("ExpectedValue", out var expected) &&
            expected is string expectedStr && value == expectedStr);

        var callee = new TracorIdentitfier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();
        context.GlobalState["ExpectedValue"] = "test";

        // Act
        var result = condition.DoesMatch(callee, tracorData, context);

        // Assert
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task ConditionOperators_ShouldCreateAndCondition() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data => true);
        var condition2 = new PredicateTracorDataCondition(data => true);

        // Act
        var andCondition = condition1 * condition2;

        // Assert
        await Assert.That(andCondition).IsInstanceOf<AndCondition>();
        await Assert.That(andCondition.ExpressionConditions).HasCount().EqualTo(2);
    }

    [Test]
    public async Task ConditionOperators_ShouldCreateOrCondition() {
        // Arrange
        var condition1 = new PredicateTracorDataCondition(data => false);
        var condition2 = new PredicateTracorDataCondition(data => true);

        // Act
        var orCondition = condition1 + condition2;

        // Assert
        await Assert.That(orCondition).IsInstanceOf<OrCondition>();
        await Assert.That(orCondition.ExpressionConditions).HasCount().EqualTo(2);
    }

    [Test]
    public async Task CalleeCondition_ShouldMatchCallerIdentifier() {
        // Arrange
        var condition = new CalleeCondition("Test", "Method");
        var matchingCallee = new TracorIdentitfier("Test", "Method");
        var nonMatchingCallee = new TracorIdentitfier("Other", "Method");
        var tracorData = new ValueTracorData<string>("test");
        var context = CreateTestContext();

        // Act & Assert
        var result1 = condition.DoesMatch(matchingCallee, tracorData, context);
        await Assert.That(result1).IsTrue();

        var result2 = condition.DoesMatch(nonMatchingCallee, tracorData, context);
        await Assert.That(result2).IsFalse();
    }

    /// <summary>
    /// Creates a test context for condition testing.
    /// </summary>
    private static OnTraceStepCurrentContext CreateTestContext() {
        var identifier = new ValidatorStepIdentifier(0, 0);
        var globalState = new TracorGlobalState();
        var executionState = new OnTraceStepExecutionState(globalState);
        var modifications = new TracorValidatorPathModifications();
        var loggerUtility = new LoggerExtension(NullLogger.Instance);
      here

        return new OnTraceStepCurrentContext(identifier, executionState, modifications, loggerUtility);
    }
}
#endif