namespace Brimborium.Tracerit.Test.Utility;

public class TracorDataUtilityTests {
    [Test]
    public async Task DateTimeTest() {
        var cultureInfo = System.Globalization.CultureInfo.GetCultureInfo("de-DE");
        {
            var dt0 = DateTime.UtcNow;
            var ns0 = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dt0);
            var dt1 = TracorDataUtility.UnixTimeNanosecondsToDateTime(ns0);
            var ns1 = TracorDataUtility.DateTimeToUnixTimeNanoseconds(dt1);
            await Assert.That(ns0).IsEqualTo(ns1);
            await Assert.That(dt0).IsEqualTo(dt1);
        }
        {
            await Assert.That(DateTime.TryParse("2001-01-02T03:04:05Z", cultureInfo, out var dt)).IsTrue();
            await Assert.That(dt.Ticks).IsEqualTo(631140050450000000L);
            await Assert.That(dt.Kind).IsEqualTo(DateTimeKind.Local);
            var (ns, kind) = TracorDataUtility.DateTimeToUnixTimeNanosecondsAndKind(dt);
            await Assert.That(ns).IsEqualTo(978404645000000000L);
            await Assert.That(kind).IsEqualTo(2.0d);
        
            var dtAct = TracorDataUtility.UnixTimeNanosecondsAndKindToDateTime(ns, kind);
            await Assert.That(dtAct.Ticks).IsEqualTo(dt.Ticks);
            await Assert.That(dtAct.Kind).IsEqualTo(dt.Kind);
        }
        {
            await Assert.That(DateTime.TryParse("2001-01-02T03:04:05", cultureInfo, out var dt)).IsTrue();
            await Assert.That(dt.Ticks).IsEqualTo(631140014450000000);
            await Assert.That(dt.Kind).IsEqualTo(DateTimeKind.Unspecified);
            var (ns, kind) = TracorDataUtility.DateTimeToUnixTimeNanosecondsAndKind(dt);
            await Assert.That(ns).IsEqualTo(978401045000000000L);
            await Assert.That(kind).IsEqualTo(0.0d);

            var dtAct = TracorDataUtility.UnixTimeNanosecondsAndKindToDateTime(ns, kind);
            await Assert.That(dtAct.Ticks).IsEqualTo(dt.Ticks);
            await Assert.That(dtAct.Kind).IsEqualTo(dt.Kind);
        }
    }

    [Test]
    public async Task TryCastObjectToIntegerTest() {
        object?[] inputValueUnsuccess = [null, false, true, "", DateTime.MaxValue, DateTimeOffset.MinValue];
        object?[] inputValue1 = [(long)1, (long?)1, (int)1, (byte)1];
        object?[] inputValue42 = [(long)42, (long?)42, (int)42, (int?)42, (byte)42];
        foreach (var inputValue in inputValueUnsuccess) {
            var success = TracorDataUtility.TryCastObjectToInteger(inputValue, out long result);
            await Assert.That(success).IsFalse();
            await Assert.That(result).IsEqualTo(0);
        }
        foreach (var inputValue in inputValue1) {
            var success = TracorDataUtility.TryCastObjectToInteger(inputValue, out long result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsEqualTo(1);
        }
        foreach (var inputValue in inputValue42) {
            var success = TracorDataUtility.TryCastObjectToInteger(inputValue, out long result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsEqualTo(42);
        }
    }

    [Test]
    public async Task TryConvertObjectToIntegerTest() {
        object?[] inputValueUnsuccess = [null, false, true, "", DateTime.MaxValue, DateTimeOffset.MinValue];
        object?[] inputValue1 = [(long)1, (long?)1, (int)1, (byte)1, "1"];
        object?[] inputValue42 = [(long)42, (long?)42, (int)42, (int?)42, (byte)42];
        foreach (var inputValue in inputValueUnsuccess) {
            var success = TracorDataUtility.TryConvertObjectToIntegerValue(inputValue, out long result);
            await Assert.That(success).IsFalse();
            await Assert.That(result).IsEqualTo(0);
        }
        foreach (var inputValue in inputValue1) {
            var success = TracorDataUtility.TryConvertObjectToIntegerValue(inputValue, out long result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsEqualTo(1);
        }
        foreach (var inputValue in inputValue42) {
            var success = TracorDataUtility.TryConvertObjectToIntegerValue(inputValue, out long result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsEqualTo(42);
        }
    }

    [Test]
    public async Task TryCastObjectToBooleanTest() {
        object?[] inputValueUnsuccess = [null, 1.0, 1, "", DateTime.MaxValue, DateTimeOffset.MinValue];
        object?[] inputValueFalse = [false, (bool?)false];
        object?[] inputValueTrue = [true, (bool?)true];
        foreach (var inputValue in inputValueUnsuccess) {
            var success = TracorDataUtility.TryCastObjectToBoolean(inputValue, out bool result);
            await Assert.That(success).IsFalse();
            await Assert.That(result).IsFalse();
        }
        foreach (var inputValue in inputValueFalse) {
            var success = TracorDataUtility.TryCastObjectToBoolean(inputValue, out bool result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsFalse();
        }
        foreach (var inputValue in inputValueTrue) {
            var success = TracorDataUtility.TryCastObjectToBoolean(inputValue, out bool result);
            await Assert.That(success).IsTrue();
            await Assert.That(result).IsTrue();
        }
    }

    [Test]
    public async Task TimeSpanTest() {
        await Assert.That(TracorDataUtility.TimeSpanToDurationNanoseconds(TimeSpan.FromSeconds(1))).IsEqualTo(1_000_000);
        await Assert.That(TracorDataUtility.DurationNanosecondsToTimeSpan(1_000_000)).IsEqualTo(TimeSpan.FromSeconds(1));
    }
}
