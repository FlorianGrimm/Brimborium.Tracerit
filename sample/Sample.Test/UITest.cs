namespace Sample.WebApp;

public class UITest : AppPageTest {
    [Test]
    public async Task I1000Test() {
        List<LoggerTracorData> loggerTracorDatas = new List<LoggerTracorData>();
        var baseUrl = this.WebApplicationFactory.GetBaseUrl();
        var serviceProvider = this.WebApplicationFactory.GetServices();

        using (var validatorPath = this.WebApplicationFactory.GetTracorValidator().Add(
            new FilterExpression(
                "",
                Wrap((LoggerTracorData loggerTracorData) => {
                    loggerTracorData.IncrementReferenceCount();
                    loggerTracorDatas.Add(loggerTracorData);
                    return true;
                }).PredicateTracorData(),
                [
                    new SequenceExpression()
                    + Wrap(static (LoggerTracorData loggerTracorData) => {
                        return loggerTracorData.DoesMatch("Sample.WebApp.UITest", nameof(LogExtension.PingReceived))
                            && (loggerTracorData.TryGetPropertyValue<int>("i", out var i))
                            && (0 == i);
                    }).PredicateTracorData().AsMatch()
                    + Wrap(static (LoggerTracorData loggerTracorData) => {
                        return loggerTracorData.DoesMatch("Sample.WebApp.UITest", nameof(LogExtension.PingReceived))
                            && (loggerTracorData.TryGetPropertyValue<int>("i", out var i))
                            && (9 == i);
                    }).PredicateTracorData().AsMatch()
                ]
            ))) {
            var logger = this.WebApplicationFactory.GetServices().GetRequiredService<ILogger<UITest>>();
            for (int i = 0; i < 10; i++) {
                using var client = this.WebApplicationFactory.CreateClient();
                var response = await client.GetAsync("/ping");
                var stringContent = await response.Content.ReadAsStringAsync();
                await Assert.That(stringContent).StartsWith("pong ");
                logger.PingReceived(i);
            }
            {
                await this.Page.GotoAsync(baseUrl.ToString());
                await Task.Delay(100);
                await validatorPath.GetFinishedAsync(null, TimeSpan.FromSeconds(10));
                await Assert.That(loggerTracorDatas.Count).IsGreaterThan(0);
               
                // await Verify(loggerTracorDatas);
            }
        }
    }
}
