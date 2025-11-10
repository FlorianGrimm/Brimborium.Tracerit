namespace Brimborium.Tracerit.Test
{
    public class TraceritInstrumentationTests
    {
        [Test]
        public async Task TraceritInstrumentationTests_Constructor_Dispose() {
            var sut = new TraceritInstrumentation();
            await Assert.That(sut.ActivitySource).IsNotNull();
            await Assert.That(sut.GetActivitySource()).IsNotNull();
            sut.Dispose();
            await Assert.That(sut.ActivitySource).IsNull();
            Assert.Throws<Exception>(() => sut.GetActivitySource());
        }
    }
}