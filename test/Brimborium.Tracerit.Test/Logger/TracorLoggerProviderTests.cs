#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium_Tracerit_Logger;

public class TracorLoggerProviderTests {
    
    [Test]
    public async Task Constructor_WithValidTracor_ShouldInitializeCorrectly() {
        // Arrange
        var mockTracor = new MockTracor();
        
        // Act
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Assert
        await Assert.That(provider).IsNotNull();
    }

    [Test]
    public async Task CreateLogger_WithValidName_ShouldReturnTracorLogger() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        const string loggerName = "TestLogger";
        
        // Act
        var logger = provider.CreateLogger(loggerName);
        
        // Assert
        await Assert.That(logger).IsNotNull();
        await Assert.That(logger).IsTypeOf<TracorLogger>();
    }

    [Test]
    public async Task CreateLogger_WithNullName_ShouldReturnTracorLogger() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act
        var logger = provider.CreateLogger(null!);
        
        // Assert
        await Assert.That(logger).IsNotNull();
        await Assert.That(logger).IsTypeOf<TracorLogger>();
    }

    [Test]
    public async Task CreateLogger_WithEmptyName_ShouldReturnTracorLogger() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act
        var logger = provider.CreateLogger(string.Empty);
        
        // Assert
        await Assert.That(logger).IsNotNull();
        await Assert.That(logger).IsTypeOf<TracorLogger>();
    }

    [Test]
    public async Task CreateLogger_MultipleCalls_ShouldReturnDifferentInstances() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act
        var logger1 = provider.CreateLogger("Logger1");
        var logger2 = provider.CreateLogger("Logger2");
        
        // Assert
        await Assert.That(logger1).IsNotNull();
        await Assert.That(logger2).IsNotNull();
        await Assert.That(logger1).IsNotEqualTo(logger2);
    }

    [Test]
    public async Task SetScopeProvider_WithValidProvider_ShouldSetExternalScopeProvider() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        var mockScopeProvider = new MockExternalScopeProvider();
        
        // Act
        provider.SetScopeProvider(mockScopeProvider);
        var logger = provider.CreateLogger("TestLogger") as TracorLogger;
        
        // Assert
        await Assert.That(logger).IsNotNull();
        // Note: We can't directly test the internal _ExternalScopeProvider field,
        // but we can test that BeginScope works correctly after setting the scope provider
        var scope = logger!.BeginScope("test scope");
        await Assert.That(scope).IsNotNull();
        await Assert.That(mockScopeProvider.PushedScopes).HasCount().EqualTo(1);
        await Assert.That(mockScopeProvider.PushedScopes[0]).IsEqualTo("test scope");
    }

    [Test]
    public async Task SetScopeProvider_WithNullProvider_ShouldHandleGracefully() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act & Assert - Should not throw
        provider.SetScopeProvider(null!);
        var logger = provider.CreateLogger("TestLogger");
        await Assert.That(logger).IsNotNull();
    }

    [Test]
    public async Task Dispose_ShouldNotThrow()
    {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);

        // Act & Assert - Should not throw
        provider.Dispose();
        
        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task Dispose_MultipleCalls_ShouldNotThrow()
    {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);

        // Act & Assert - Should not throw
        provider.Dispose();
        provider.Dispose(); // Second call should also not throw

        await ValueTask.CompletedTask;
    }

    [Test]
    public async Task CreateLogger_AfterDispose_ShouldStillWork() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act
        provider.Dispose();
        var logger = provider.CreateLogger("TestLogger");
        
        // Assert
        await Assert.That(logger).IsNotNull();
        await Assert.That(logger).IsTypeOf<TracorLogger>();
    }

    [Test]
    public async Task ProviderAlias_ShouldBeTracor() {
        // Arrange & Act
        var providerAliasAttribute = typeof(TracorLoggerProvider)
            .GetCustomAttributes(typeof(ProviderAliasAttribute), false)
            .FirstOrDefault() as ProviderAliasAttribute;
        
        // Assert
        await Assert.That(providerAliasAttribute).IsNotNull();
        await Assert.That(providerAliasAttribute!.Alias).IsEqualTo("Tracor");
    }

    [Test]
    public async Task Implements_ILoggerProvider() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Assert
        await Assert.That(provider).IsAssignableTo<ILoggerProvider>();
    }

    [Test]
    public async Task Implements_ISupportExternalScope() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Assert
        await Assert.That(provider).IsAssignableTo<ISupportExternalScope>();
    }

    [Test]
    public async Task CreateLogger_WithScopeProvider_LoggerShouldUseScopeProvider() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        var mockScopeProvider = new MockExternalScopeProvider();
        
        // Act
        provider.SetScopeProvider(mockScopeProvider);
        var logger = provider.CreateLogger("TestLogger") as TracorLogger;
        
        // Test that the logger can use the scope provider
        var testScope = "test scope data";
        var scope = logger!.BeginScope(testScope);
        
        // Assert
        await Assert.That(scope).IsNotNull();
        await Assert.That(mockScopeProvider.PushedScopes).Contains((object)testScope);
        
        // Cleanup
        scope?.Dispose();
        await Assert.That(mockScopeProvider.PushedScopes).DoesNotContain((object)testScope);
    }

    [Test]
    public async Task CreateLogger_WithoutScopeProvider_BeginScopeShouldReturnNull() {
        // Arrange
        var mockTracor = new MockTracor();
        var provider = new TracorLoggerProvider(mockTracor);
        
        // Act
        var logger = provider.CreateLogger("TestLogger") as TracorLogger;
        var scope = logger!.BeginScope("test scope");
        
        // Assert
        await Assert.That(scope).IsNull();
    }
}
