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
}
