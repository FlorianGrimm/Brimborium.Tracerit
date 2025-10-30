#pragma warning disable IDE0037

namespace Brimborium.Tracerit.Test;

public class TracorDataPropertyTests {
    [Test]
    public async Task CreateTests() {
        var d = DateTime.Now;
        {
            string expectedValue = d.ToString("O");
            {
                var sut = TracorDataProperty.CreateStringValue("Test", expectedValue);
                {
                    bool success = sut.TryGetStringValue(out var actualValue);
                    await Assert.That(success).IsTrue();
                    await Assert.That(actualValue).IsEqualTo(expectedValue);
                    await Assert.That(sut.Name).IsEqualTo("Test");
                    await Assert.That(sut.TypeValue).IsEqualTo(TracorDataPropertyTypeValue.String);
                }
                {
                    await Assert.That(sut.TryGetIntegerValue(out _)).IsFalse();
                }
            }
            {
                var sut = TracorDataProperty.Create("Test", expectedValue);
                {
                    bool success = sut.TryGetStringValue(out var actualValue);
                    await Assert.That(success).IsTrue();
                    await Assert.That(actualValue).IsEqualTo(expectedValue);
                    await Assert.That(sut.Name).IsEqualTo("Test");
                    await Assert.That(sut.TypeValue).IsEqualTo(TracorDataPropertyTypeValue.String);
                }
                {
                    await Assert.That(sut.TryGetNullValue(out _)).IsFalse();
                    // await Assert.That(sut.TryGetStringValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetIntegerValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetBooleanValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetEnumValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetLevelValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetDoubleValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetDateTimeValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetDateTimeOffsetValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetUuidValue(out _)).IsFalse();
                    await Assert.That(sut.TryGetAnyValue(out _)).IsFalse();
                }
            }
        }
        {
            long expectedValue = d.Ticks;
            {
                var sut = TracorDataProperty.CreateIntegerValue("Test", expectedValue);
                {
                    bool success = sut.TryGetIntegerValue(out var actualValue);
                    await Assert.That(success).IsTrue();
                    await Assert.That(actualValue).IsEqualTo(expectedValue);
                    await Assert.That(sut.Name).IsEqualTo("Test");
                    await Assert.That(sut.TypeValue).IsEqualTo(TracorDataPropertyTypeValue.Integer);
                }
                {
                    await Assert.That(sut.TryGetStringValue(out _)).IsFalse();
                }
            }
            {
                var sut = TracorDataProperty.Create("Test", expectedValue);
                {
                    bool success = sut.TryGetIntegerValue(out var actualValue);
                    await Assert.That(success).IsTrue();
                    await Assert.That(actualValue).IsEqualTo(expectedValue);
                    await Assert.That(sut.Name).IsEqualTo("Test");
                    await Assert.That(sut.TypeValue).IsEqualTo(TracorDataPropertyTypeValue.Integer);
                }
                {
                    await Assert.That(sut.TryGetStringValue(out _)).IsFalse();
                }
            }
        }

        {
            long expectedValue = d.Ticks;
            Nullable<long> inputValue = d.Ticks;
            {
                var sut = TracorDataProperty.Create("Test", inputValue);
                {
                    bool success = sut.TryGetIntegerValue(out var actualValue);
                    await Assert.That(success).IsTrue();
                    await Assert.That(actualValue).IsEqualTo(expectedValue);
                    await Assert.That(sut.Name).IsEqualTo("Test");
                    await Assert.That(sut.TypeValue).IsEqualTo(TracorDataPropertyTypeValue.Integer);
                }
                {
                    await Assert.That(sut.TryGetStringValue(out _)).IsFalse();
                }
            }
        }
        //{
        //    var value = d.Ticks;
        //    var sut=new TracorDataProperty();
        //    sut.InnerLongValue = value;
        //    await Assert.That(sut.InnerLongValue).IsEqualTo(value);
        //}
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "def")]
    [Arguments(TracorDataPropertyTypeValue.String, "", "def")]
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "")]
    public async Task CreateStringTest(TracorDataPropertyTypeValue typeValue, string argName, string argValue) {
        {
            var act = TracorDataProperty.CreateStringValue(argName, argValue);
            await Assert.That(act.TypeValue).IsEqualTo(typeValue);

            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo(argValue);

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1L).IsEqualTo(-1L);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Integer, "abc", 123)]
    //[Arguments(TracorDataPropertyTypeValue.Integer, "", 123)]
    public async Task CreateIntegerTest(TracorDataPropertyTypeValue typeValue, string argName, int argValue) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateIntegerValue(argName, argValue);
            await Assert.That(act.TypeValue).IsEqualTo(typeValue);

            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(argValue);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        sb.Clear();
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Level, "abc", LogLevel.Warning)]
    [Arguments(TracorDataPropertyTypeValue.Level, "", LogLevel.Information)]
    public async Task CreateLevelValueTest(TracorDataPropertyTypeValue typeValue, string argName, LogLevel argValue) {
        {
            var act = TracorDataProperty.CreateLevelValue(argName, argValue);
            await Assert.That(act.TypeValue).IsEqualTo(typeValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
    }

    [Test]
    [Arguments("abc", "2000-01-02T03:04:05.0000000Z", 946782245000000000)]
    [Arguments("", "2000-01-02T03:04:05.0000000Z", 946782245000000000)]
    public async Task CreateDateTimeTest(string argName, string txtArgValue, long ns) {
        DateTime argValue = DateTime.ParseExact(
            txtArgValue,
            "O",
            TracorConstants.TracorCulture.DateTimeFormat,
            System.Globalization.DateTimeStyles.AdjustToUniversal);
        {
            var act = TracorDataProperty.CreateDateTimeValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        {
            await Assert.That(TracorDataUtility.DateTimeToUnixTimeNanoseconds(argValue)).IsEqualTo(ns);
        }
    }


    [Test]
    [Arguments("abc", "2000-01-02T03:04:05.0000000+02:00", 946775045000000000)]
    [Arguments("", "2000-01-02T03:04:05.0000000+02:00", 946775045000000000)]
    public async Task CreateDateTimeOffsetTest(string argName, string txtArgValue, long ns) {
        DateTimeOffset argValue = DateTimeOffset.ParseExact(txtArgValue, "O", null);
        DateTimeOffset argValueUtc = new DateTimeOffset(new DateTime(argValue.UtcTicks), TimeSpan.Zero);

        await Assert.That(argValue.UtcTicks).IsEqualTo(argValueUtc.UtcTicks);
        {
            var act = TracorDataProperty.CreateDateTimeOffsetValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            
            await Assert.That((act.TryGetDateTimeOffsetValue(out var dtoValue1) ? dtoValue1 : DateTimeOffset.FromUnixTimeMilliseconds(0)).UtcTicks).IsEqualTo(argValue.UtcTicks);
            await Assert.That((act.TryGetDateTimeOffsetValue(out var dtoValue2) ? dtoValue2 : DateTimeOffset.FromUnixTimeMilliseconds(0)).UtcTicks).IsEqualTo(argValueUtc.UtcTicks);

            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        {
            await Assert.That(TracorDataUtility.DateTimeOffsetToUnixTimeNanoseconds(argValue)).IsEqualTo(ns);
            await Assert.That(TracorDataUtility.DateTimeOffsetToUnixTimeNanosecondsAndOffset(argValue).longVaule).IsEqualTo(ns);
        }
    }

    [Test]
    [Arguments("abc", true, "1")]
    [Arguments("abc", false, "0")]
    [Arguments("", false, "0")]
    public async Task CreateBooleanTest(string argName, bool argValue, string textValue) {
        {
            var act = TracorDataProperty.CreateBoolean(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsEqualTo(argValue);
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        {
            await Assert.That((result: TracorDataUtility.TryConvertObjectToBooleanValue(textValue, out var boolValue), boolValue: boolValue))
                .IsEquivalentTo((result: true, boolValue: argValue));
        }
    }

    [Test]
    [Arguments("abc", 1.5d)]
    [Arguments("abc", 100.5d)]
    [Arguments("abc", -4.2)]
    [Arguments("", 0d)]
    public async Task CreateFloatTest(string argName, double argValue) {
        {
            var act = TracorDataProperty.CreateDoubleValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(argValue);
        }
    }

    /*
    [Test, Explicit]
    public Task UsingTracorDataProperty() {
        TracorDataRecordPool pool = new(0);
        for (long idxRepeat = 0; idxRepeat < 10000; idxRepeat++) {
            using (TracorDataRecordCollection collection = new TracorDataRecordCollection()) {
                for (long idxCalls = 0; idxCalls < 1000; idxCalls++) {
                    var tdr = pool.Rent();
                    List<KeyValuePair<string, object>> list = new();
                    for (long idxProp = 0; idxProp < 10; idxProp++) {
                        tdr.ListProperty.Add(TracorDataProperty.CreateIntegerValue("Test", idxProp));
                    }
                    collection.ListData.Add(tdr);
                }
            }
        }
        return Task.CompletedTask;
    }
    */
}
