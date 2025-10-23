namespace Brimborium.Tracerit.Test.Service;

/// <summary>
/// Unit tests for ITracorValidator implementations and validation path functionality.
/// </summary>
public class TracorValidatorTests {

    [Test]
    public async Task TracorValidator_ShouldCreateValidationPath() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression();

        // Act
        var validatorPath = validator.Add(expression);

        // Assert
        await Assert.That(validatorPath).IsNotNull();
        await Assert.That(validatorPath).IsAssignableTo<ITracorValidatorPath>();
    }

    [Test]
    public async Task TracorValidator_ShouldCreateValidationPathWithGlobalState() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression();
        var globalState = new TracorGlobalState().SetValue("TestKey", "TestValue");

        // Act
        var validatorPath = validator.Add(expression, globalState);

        // Assert
        await Assert.That(validatorPath).IsNotNull();
    }

    [Test]
    public async Task TracorValidatorPath_ShouldTrackRunningStates() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression()
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "first")))
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data =>
                data.TryGetPropertyValue<string>("Value", out var value) && value == "second")));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new ValueTracorData<string>("first") { TracorIdentifier = callee });
        var runningStates = validatorPath.GetListRunning();

        // Assert
        await Assert.That(runningStates).HasCount().EqualTo(1);
        await Assert.That(validatorPath.GetListFinished()).IsEmpty();
    }

    [Test]
    public async Task TracorValidatorPath_ShouldTrackFinishedStates() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(condition: new PredicateTracorDataCondition(data =>
            data.TryGetPropertyValue<string>("Value", out var value) && value == "test"));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new ValueTracorData<string>("test") { TracorIdentifier = callee });
        var finishedStates = validatorPath.GetListFinished();

        // Assert
        await Assert.That(finishedStates).HasCount().EqualTo(1);
        await Assert.That(validatorPath.GetListRunning()).IsEmpty();
    }

    [Test]
    public async Task TracorValidatorPath_GetFinished_ShouldReturnMatchingState() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression(condition: new AlwaysCondition<string, string>(
            fnGetProperty: value => value,
            setGlobalState: "TestProperty"));

        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new ValueTracorData<string>("test value") { TracorIdentifier = callee });
        var finishedState = validatorPath.GetFinished(state =>
            state.TryGetValue("TestProperty", out var prop) && prop is string str && str == "test value");

        // Assert
        await Assert.That(finishedState).IsNotNull();
        await Assert.That(finishedState!.GetValue<string>("TestProperty")).IsEqualTo("test value");
    }

    [Test]
    public async Task TracorValidatorPath_GetFinished_ShouldReturnNullForNonMatchingPredicate() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression();
        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        validatorPath.OnTrace(new ValueTracorData<string>("test") { TracorIdentifier = callee });
        var finishedState = validatorPath.GetFinished(state =>
            state.TryGetValue("NonExistentKey", out var _));

        // Assert
        await Assert.That(finishedState).IsNull();
    }

    [Test]
    public async Task TracorValidatorPath_ShouldDisposeCorrectly() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression();
        var validatorPath = validator.Add(expression);

        // Act & Assert - Should not throw
        validatorPath.Dispose();

        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task TracorValidator_OnTrace_ShouldProcessAllValidatorPaths() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression1 = new MatchExpression(condition: new PredicateTracorDataCondition(data => true));
        var expression2 = new MatchExpression(condition: new PredicateTracorDataCondition(data => true));

        var validatorPath1 = validator.Add(expression1);
        var validatorPath2 = validator.Add(expression2);
        var callee = new TracorIdentifier("Test", "Method");
        var tracorData = new ValueTracorData<string>("test") { TracorIdentifier = callee };

        // Act
        validator.OnTrace(true, tracorData);

        // Assert
        await Assert.That(validatorPath1.GetListFinished()).HasCount().EqualTo(1);
        await Assert.That(validatorPath2.GetListFinished()).HasCount().EqualTo(1);
    }

    [Test]
    public async Task TracorValidatorPath_GetFinishedAsync_ShouldReturnFinishedState() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new MatchExpression();
        var validatorPath = validator.Add(expression);
        var callee = new TracorIdentifier("Test", "Method");

        // Act
        var traceTask = Task.Run(() => {
            Thread.Sleep(100); // Small delay to simulate async processing
            validatorPath.OnTrace(new ValueTracorData<string>("test") { TracorIdentifier = callee });
        });

        var finishedState = await validatorPath.GetFinishedAsync(null, TimeSpan.FromSeconds(1));

        // Assert
        await Assert.That(finishedState).IsNotNull();
        await traceTask;
    }

    [Test]
    public async Task TracorValidatorPath_GetRunngingAsync_ShouldReturnRunningState() {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddEnabledTracor();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var validator = serviceProvider.GetRequiredService<ITracorValidator>();

        var expression = new SequenceExpression("TestLabel")
            .Add(new MatchExpression(label: "FirstMatch",condition: new PredicateTracorDataCondition(data => true)))
            .Add(new MatchExpression(condition: new PredicateTracorDataCondition(data => false))); // Never matches

        using (var validatorPath = validator.Add(expression)) {
            var callee = new TracorIdentifier("Test", "Method");

            // Act
            validatorPath.OnTrace(new ValueTracorData<string>("test") { TracorIdentifier = callee });

            var runningStateTestLabel = validatorPath.GetRunning("TestLabel");
            var runningStateFirstMatch = validatorPath.GetRunning("FirstMatch");

            // Assert
            await Assert.That(runningStateTestLabel).IsNull();
            await Assert.That(runningStateFirstMatch).IsNotNull();
        }
    }
}
