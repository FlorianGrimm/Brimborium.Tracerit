namespace Brimborium.Tracerit.Collector.Test.TestUtility;

public class TestsIntegration {
    [ClassDataSource<WebApplicationFactoryIntegration>(Shared = SharedType.PerTestSession)]
    public required WebApplicationFactoryIntegration WebApplicationFactory { get; init; }

    [Test]
    public async Task PingTest() {
        var client = this.WebApplicationFactory.CreateClient();

        var response = await client.GetAsync("/_test/ping");

        var stringContent = await response.Content.ReadAsStringAsync();

        await Assert.That(stringContent).StartsWith("pong ");
    }
}
