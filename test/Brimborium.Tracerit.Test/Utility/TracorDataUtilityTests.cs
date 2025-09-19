namespace Brimborium.Tracerit.Test.Utility;

public class TracorDataUtilityTests {
    [Test]
    public async Task DateTimeTest() {
        {
            var dt0 = DateTime.UtcNow;
            var ns0 = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dt0);
            var dt1 = TracorDataUtility.UnixTimeNanosecondsToDateTime(ns0);
            var ns1 = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dt1);
            await Assert.That(ns0).IsEqualTo(ns1);
            await Assert.That(dt0).IsEqualTo(dt1);
        }
    }
}
