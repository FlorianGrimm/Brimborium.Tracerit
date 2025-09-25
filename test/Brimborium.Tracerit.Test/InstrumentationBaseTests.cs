namespace Brimborium.Tracerit.Test; 
public class InstrumentationBaseTests {
    public async Task EmptyConstructorUsesNamespace() {
        var sut = new SampleTest1Instrumentation();
        await Assert.That(sut.ActivitySource?.Name).IsEquivalentTo("Brimborium.Tracerit.Test");
    }

    public async Task PassdownNameAndVersion() {
        var sut = new SampleTest2Instrumentation();
        await Assert.That(sut.ActivitySource?.Name).IsEquivalentTo(SampleTest2Instrumentation.ActivitySourceName);
        await Assert.That(sut.ActivitySource?.Version).IsEquivalentTo(SampleTest2Instrumentation.ActivitySourceVersion);
    }

    public async Task EmptyConstructorUsesDisplayNameAttribute() {
        var sut = new SampleTest3Instrumentation();
        await Assert.That(sut.ActivitySource?.Name).IsEquivalentTo(SampleTest3Instrumentation.ActivitySourceName);
    }
}
