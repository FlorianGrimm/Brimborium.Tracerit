namespace Brimborium.Tracerit.Test.Expression;

/// <summary>
/// Unit tests for validator expression classes including MatchExpression, SequenceExpression, and FilterExpression.
/// </summary>
public class ExpressionTests {

    [Test]
    public async Task MatchExpression_ShouldSucceedWhenConditionMatches() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "test"));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("test"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task MatchExpression_ShouldNotSucceedWhenConditionDoesNotMatch() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "expected"));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("actual"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunnging()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task MatchExpression_WithChildren_ShouldProcessChildrenAfterMatch() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var childExpression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "child"));

        var expression = new MatchExpression(
            condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "parent"),
            listChild: childExpression);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("parent"));
        validatorPath.OnTrace(callee, new ValueTracorData<string>("child"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task SequenceExpression_ShouldRequireChildrenInOrder() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression()
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "first")))
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "second")));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("first"));
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunnging()).HasCount().EqualTo(1);

        validatorPath.OnTrace(callee, new ValueTracorData<string>("second"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task SequenceExpression_ShouldNotSucceedWithWrongOrder() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression()
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "first")))
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "second")));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("second"));
        validatorPath.OnTrace(callee, new ValueTracorData<string>("first"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
    }

    [Test]
    public async Task SequenceExpression_OperatorPlus_ShouldAddExpression() {
        // Arrange
        var firstExpression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "first"));
        var secondExpression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "second"));

        var sequence = new SequenceExpression().Add(firstExpression);

        // Act
        var newSequence = sequence + secondExpression;

        // Assert
        await Assert.That(newSequence.ListChild).HasCount().EqualTo(2);
    }

    [Test]
    public async Task FilterExpression_ShouldProcessAllChildrenSimultaneously() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var child1 = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "test"));
        var child2 = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "test"));

        var expression = new FilterExpression(
            label: "FilterExpression",
            condition: new PredicateTracorDataCondition(data => true),
            listChild: [child1, child2]);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("test"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task FilterExpression_ShouldNotSucceedUntilAllChildrenSucceed() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var child1 = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "first"));
        var child2 = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "second"));

        var expression = new FilterExpression(
            label: "FilterExpression",
            condition: new PredicateTracorDataCondition(data => true),
            listChild: [child1, child2]);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("first"));
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();

        validatorPath.OnTrace(callee, new ValueTracorData<string>("second"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task FilterExpression_ShouldNotProcessChildrenWhenConditionFails() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddTesttimeTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var child = new MatchExpression(condition: new PredicateTracorDataCondition(data => true));
        var expression = new FilterExpression(
            condition: new PredicateTracorDataCondition(data => false),
            listChild: child);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentitfier("Test", "Method");

        // Act
        validatorPath.OnTrace(callee, new ValueTracorData<string>("test"));

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunnging()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task ValidatorExpression_ShouldHaveUniqueInstanceIndex() {
        // Arrange
        var expression1 = new MatchExpression();
        var expression2 = new MatchExpression();

        // Act & Assert
        await Assert.That(((IValidatorExpression)expression1).GetInstanceIndex()).IsNotEqualTo(((IValidatorExpression)expression2).GetInstanceIndex());
    }

    [Test]
    public async Task ValidatorExpression_ShouldReturnLabel() {
        // Arrange
        var label = "TestLabel";
        var expression = new MatchExpression(label: label);

        // Act & Assert
        await Assert.That(expression.Label).IsEqualTo(label);
    }
}
