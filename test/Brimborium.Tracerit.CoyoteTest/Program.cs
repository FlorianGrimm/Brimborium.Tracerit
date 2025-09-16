#pragma warning disable CA1515 // Consider making public types internal

namespace Brimborium.Tracerit.CoyoteTest;

public static class Program {

    public static void Main(string[] args) {
        //Microsoft.Coyote.SystematicTesting.TestingEngine.Run();
        // Brimborium.Tracerit.CoyoteTest.Utility.ReferenceCountPoolTests test = new();
        // test.SingleThreadedRent();
        // await test.MultiThreadedRent().ConfigureAwait(false);
        var configuration = Configuration.Create().WithTestingIterations(10);
        using var engine = TestingEngine.Create(configuration, TestTask);
        engine.Run();
    }

    public static async Task TestTask() {
        Brimborium.Tracerit.CoyoteTest.Utility.ReferenceCountPoolTests test = new();
        //test.SingleThreadedRent();
        await test.MultiThreadedRent().ConfigureAwait(false);
    }
}
