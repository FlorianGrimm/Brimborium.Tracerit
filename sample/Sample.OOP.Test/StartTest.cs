// MIT - Florian Grimm

using Brimborium.Tracerit;
using Brimborium.Tracerit.Condition;
using Brimborium.Tracerit.Expression;
using static Brimborium.Tracerit.TracorExtension;

using Microsoft.Extensions.DependencyInjection;

[assembly: NotInParallel]
namespace Sample.WebApp;
public class StartTest {
    [ClassDataSource<TestingServersService>(Shared = SharedType.PerTestSession)]
    public required TestingServersService TestingServers { get; init; }

    [Test]
    public async Task StartWebTest() {
        await Assert.That(this.TestingServers.IsRunning()).IsTrue();
        
        var expression = new SequenceExpression()
            .Add(
                Predicate((/*ITracorData*/ tracorData, tracorGlobalState) =>
                    string.Equals(tracorData.TracorIdentifier.Scope, "Sample.WebApp.Program.PingResult", StringComparison.Ordinal)
                    && tracorGlobalState.TryCopyDateTimeOffsetValue("now", tracorData, "now")
                ).AsMatch()
            );
        using (var validatorPath = this.TestingServers.GetTracorValidator().Add(
            expression
            )) {
            { 
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:8081");
                var response=await client.GetStringAsync("/ping");
                await Assert.That(response).StartsWith("pong ");
            }

            var finishState = await validatorPath.GetFinishedAsync().ConfigureAwait(false);
            await Assert.That(finishState).IsNotNull();
        }
    }
}
