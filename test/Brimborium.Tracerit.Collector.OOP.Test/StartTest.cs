// MIT - Florian Grimm

using Brimborium.Tracerit;
using Microsoft.Extensions.DependencyInjection;

using Brimborium.Tracerit.Condition;
using Brimborium.Tracerit.Expression;

using Brimborium.Tracerit.Collector.OOP.Test.TestUtility;

using static Brimborium.Tracerit.TracorExtension;

[assembly: NotInParallel]
namespace Brimborium.Tracerit.Collector.OOP.Test;

public class StartTest {
    [ClassDataSource<WebAppIntegration>(Shared = SharedType.PerTestSession)]
    public required WebAppIntegration WebAppIntegration { get; init; }

    [Test]
    public async Task StartWebTest() {
        await Assert.That(this.WebAppIntegration.IsRunning()).IsTrue();
        
        var expression = new SequenceExpression()
            .Add(
                Predicate((/*ITracorData*/ tracorData, tracorGlobalState) =>
                    string.Equals(tracorData.TracorIdentifier.Scope, "Sample.WebApp.Program.PingResult", StringComparison.Ordinal)
                    && tracorGlobalState.TryCopyDateTimeOffsetValue("now", tracorData, "now")
                ).AsMatch()
            );
        using (var validatorPath = this.WebAppIntegration.GetTracorValidator().Add(
            expression
            )) {
            { 
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:8080");
                var response=await client.GetStringAsync("/ping");
                await Assert.That(response).StartsWith("pong ");
            }

            var finishState = await validatorPath.GetFinishedAsync().ConfigureAwait(false);
            await Assert.That(finishState).IsNotNull();
        }
    }
}
