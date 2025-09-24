[assembly: NotInParallel]

namespace Brimborium.Tracerit.Collector.Test.TestUtility;

public partial class TestPrepares {
    [Test, Explicit]
    public async Task ExplicitInstallPlaywright() {
        Microsoft.Playwright.Program.Main(["install"]);
        await Task.CompletedTask;
    }

    [Test]
    public Task TestVerifyChecksRun() => VerifyChecks.Run();
}