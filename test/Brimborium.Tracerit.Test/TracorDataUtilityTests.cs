namespace Brimborium.Tracerit.Test;

public class TracorDataUtilityTests {
    [Test]
    public async Task RoundTripTracorDataPropertyConvertStringToTypeName() {
        var listValue = Enum.GetValues<TracorDataPropertyTypeValue>();
        foreach (var givenValue in listValue) {
            var typeName1 = TracorDataUtility.TracorDataPropertyConvertTypeValueToString(givenValue, null);
            var (actualValue, typeName2) = TracorDataUtility.TracorDataPropertyConvertStringToTypeName(typeName1);
            await Assert.That(actualValue).IsEqualTo(givenValue);
        }
    }

    [Test]
    public async Task DateTimeOffsetToUnixTimeNanosecondsAndOffsetTest() {
        {
            var dto = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var (ns, off) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(dto);
            await Assert.That(ns).IsEqualTo(0);
            await Assert.That(off).IsEqualTo(0);
        }
        {
            var dto = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(0, 0);
            await Assert.That(dto.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)).IsEqualTo("1970-01-01T00:00:00.0000000+00:00");
        }
        {
            var dto = new DateTimeOffset(1970, 1, 1, 2, 0, 0, TimeSpan.FromHours(2));
            var (ns, off) = TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(dto);
            await Assert.That(ns).IsEqualTo(0);
            await Assert.That(off).IsEqualTo(120);
        }

        {
            var dto = TracorDataUtility.UnixTimeNanosecondsAndOffsetToDateTimeOffset(0, 120);
            await Assert.That(dto.ToString("o", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat)).IsEqualTo("1970-01-01T02:00:00.0000000+02:00");
        }
    }
}
