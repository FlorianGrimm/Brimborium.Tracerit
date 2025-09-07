namespace Brimborium.Tracerit.Test.Logger;

public class TracorLoggerTests {
    [Test]
    public async Task MagicConstantOwnNamespaceLength() {
#pragma warning disable TUnitAssertions0005 // Assert.That(...) should not be used with a constant value
        await Assert.That(this.GetType().Namespace).StartsWith(TracorLogger.OwnNamespace);
        await Assert.That(TracorLogger.OwnNamespace.Length).IsEqualTo(TracorLogger.OwnNamespaceLength);
#pragma warning restore TUnitAssertions0005 // Assert.That(...) should not be used with a constant value
    }
}
