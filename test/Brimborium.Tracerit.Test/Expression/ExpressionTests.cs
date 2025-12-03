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
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(
            condition: Predicate(
                data => data.IsEqualString("value", "test")));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("value", "test")]
        });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task MatchExpression_ShouldNotSucceedWhenConditionDoesNotMatch() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(
            condition: Predicate(
                data => data.IsEqualString("Value", "expected")));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("value", "test")]
        });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunning()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task MatchExpression_WithChildren_ShouldProcessChildrenAfterMatch() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();



        var expression = new MatchExpression(
            condition: Predicate(
                data => data.IsEqualString("value", "parent")),
            listChild: [
                new MatchExpression(
                    condition: Predicate(
                        data => data.IsEqualString("value", "child")))]);

        using (var validatorPath = validator.Add(expression)) {
            var callee = new TracorIdentifier("Test", "Method");

            // Act
            validatorPath.OnTrace(new TracorDataRecord() {
                TracorIdentifier = callee,
                ListProperty = [new TracorDataProperty("value", "parent")]
            });
            validatorPath.OnTrace(new TracorDataRecord() {
                TracorIdentifier = callee,
                ListProperty = [new TracorDataProperty("value", "child")]
            });

            // Assert
            await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
        }
    }

    [Test]
    public async Task SequenceExpression_ShouldRequireChildrenInOrder() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression()
            .Add(new MatchExpression(
                condition: Predicate(
                    data => data.IsEqualString("value", "first"))))
            .Add(new MatchExpression(condition:
                new PredicateTracorDataCondition(
                    data => data.IsEqualString("value", "second"))));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(
                new TracorDataRecord() {
                    TracorIdentifier = callee,
                    ListProperty = [new TracorDataProperty("value", "first")]
                });
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunning()).HasCount().EqualTo(1);

        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("value", "second")]
        });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task SequenceExpression_ShouldNotSucceedWithWrongOrder() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression()
            .Add(new MatchExpression(condition: Predicate(
                data => data.IsEqualString("value", "second")))
            .Add(new MatchExpression(condition: Predicate(data =>
                data.IsEqualString("value", "second")))));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("value", "second")]
        });

        validatorPath.OnTrace(
                new TracorDataRecord() {
                    TracorIdentifier = callee,
                    ListProperty = [new TracorDataProperty("value", "first")]
                });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
    }

    [Test]
    public async Task SequenceExpression_OperatorPlus_ShouldAddExpression() {
        // Arrange
        var firstExpression = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "first")));
        var secondExpression = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "second")));

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
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var child1 = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "test")));
        var child2 = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "test")));

        var expression = new FilterExpression(
            label: "FilterExpression",
            condition: Predicate(data => true),
            listChild: [child1, child2]);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("value", "test")]
        });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task FilterExpression_ShouldNotSucceedUntilAllChildrenSucceed() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var child1 = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "first")));
        var child2 = new MatchExpression(condition: Predicate(data =>
            data.IsEqualString("value", "second")));

        var expression = new FilterExpression(
            label: "FilterExpression",
            condition: Predicate(data => true),
            listChild: [child1, child2]);

        using (var validatorPath = validator.Add(expression)) {
            var callee = new TracorIdentifier("Test", "Method");

            // Act
            validatorPath.OnTrace(
                new TracorDataRecord() { 
                    TracorIdentifier = callee ,
                    ListProperty = [new TracorDataProperty("value", "first")]
                });

            await Assert.That(validatorPath.GetListFinished()).IsEmpty();

            validatorPath.OnTrace(new TracorDataRecord() {
                TracorIdentifier = callee,
                ListProperty = [new TracorDataProperty("value", "second")]
            }); 

            // Assert
            await Assert.That(validatorPath.GetListFinished()).HasCount().EqualTo(1);
        }
    }

    [Test]
    public async Task FilterExpression_ShouldNotProcessChildrenWhenConditionFails() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor(
            configureTracor: default,
            configureConvert: default,
            tracorScopedFilterSection: default);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new FilterExpression(
            condition: Predicate(data => false),
            listChild: [
                new MatchExpression(
                    condition: Predicate(data => true))]);

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new TracorDataRecord() {
            TracorIdentifier = callee,
            ListProperty = [new TracorDataProperty("first", "first")]
        });

        // Assert
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
        await Assert.That(validatorPath.GetListRunning()).HasCount().EqualTo(1);
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
