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
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "def", true)]
    [Arguments(TracorDataPropertyTypeValue.String, "", "def", false)]
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "", true)]
    public async Task CreateStringTest(TracorDataPropertyTypeValue typeValue, string argName, string argValue, bool successfull) {
        {
            var act = TracorDataProperty.CreateStringValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo(argValue);

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1L).IsEqualTo(-1L);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        /*
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateString(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:str:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameString);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(argValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            } 
        }
        sb.Clear();
        */
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Integer, "abc", 123, true)]
    //[Arguments(TracorDataPropertyTypeValue.Integer, "", 123, false)]
    public async Task CreateIntegerTest(TracorDataPropertyTypeValue typeValue, string argName, int argValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateIntegerValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(argValue);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        /*
        {
            var act = TracorDataProperty.CreateInteger(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:int:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameInteger);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        */
        sb.Clear();
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Level, "abc", LogLevel.Warning, true)]
    [Arguments(TracorDataPropertyTypeValue.Level, "", LogLevel.Information, false)]
    public async Task CreateLevelValueTest(TracorDataPropertyTypeValue typeValue, string argName, LogLevel argValue, bool successfull) {
        {
            var act = TracorDataProperty.CreateLevelValue(argName, argValue);

            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);

            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        /*
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateLevelValue(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:lvl:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameLevelValue);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(argValue.ToString());
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
        */
    }

    /*
       
   94678224500 000 0000
   94677864500 000 0000
     */

    [Test]
    [Arguments(TracorDataPropertyTypeValue.DateTime, "abc", "2000-01-02T03:04:05.0000000Z", 946782245000000000, true)]
    [Arguments(TracorDataPropertyTypeValue.DateTime, "", "2000-01-02T03:04:05.0000000Z", 946782245000000000, false)]
    public async Task CreateDateTimeTest(TracorDataPropertyTypeValue typeValue, string argName, string txtArgValue, long ns, bool successfull) {
        DateTime argValue = DateTime.ParseExact(
            txtArgValue,
            "O",
            System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
            System.Globalization.DateTimeStyles.AdjustToUniversal);
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateDateTimeValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        /*
        {
            var act = TracorDataProperty.CreateDateTime(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:dt:{txtArgValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameDateTime);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        */
        sb.Clear();
    }


    [Test]
    [Arguments(TracorDataPropertyTypeValue.DateTimeOffset, "abc", "2000-01-02T03:04:05.0000000+02:00", 946782245000000000, true)]
    [Arguments(TracorDataPropertyTypeValue.DateTimeOffset, "", "2000-01-02T03:04:05.0000000+02:00", 946782245000000000, false)]
    public async Task CreateDateTimeOffsetTest(TracorDataPropertyTypeValue typeValue, string argName, string txtArgValue, long ns, bool successfull) {
        DateTimeOffset argValue = DateTimeOffset.ParseExact(txtArgValue, "O", null);
        DateTimeOffset argValueUtc = new DateTimeOffset(new DateTime(argValue.Ticks), TimeSpan.Zero);

        {
            var act = TracorDataProperty.CreateDateTimeOffsetValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);

            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(argValue);

            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }

        /*
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateDateTimeOffset(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:dto:{txtArgValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameDateTimeOffset);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValueUtc);
            }
        }
        sb.Clear();
        */
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Boolean, "abc", true, "1", true)]
    [Arguments(TracorDataPropertyTypeValue.Boolean, "abc", false, "0", true)]
    [Arguments(TracorDataPropertyTypeValue.Boolean, "", false, "0", false)]
    public async Task CreateBooleanTest(TracorDataPropertyTypeValue typeValue, string argName, bool argValue, string textValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.CreateBoolean(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsEqualTo(argValue);
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
        }
        /*
        {
            var act = TracorDataProperty.CreateBoolean(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:bool:{textValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameBoolean);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(textValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        */
        sb.Clear();
    }

    [Test]
    [Arguments(TracorDataPropertyTypeValue.Double, "abc", 1.5d, true)]
    [Arguments(TracorDataPropertyTypeValue.Double, "abc", 100.5d, true)]
    [Arguments(TracorDataPropertyTypeValue.Double, "abc", -4.2, true)]
    [Arguments(TracorDataPropertyTypeValue.Double, "", 0d, false)]
    public async Task CreateFloatTest(TracorDataPropertyTypeValue typeValue, string argName, double argValue, bool successfull) {
        var sb = new StringBuilder();
        var textValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        {
            var act = TracorDataProperty.CreateDoubleValue(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetDoubleValue(out var floatValue) ? floatValue : -1).IsEqualTo(argValue);
        }
        /*
        {
            var act = TracorDataProperty.CreateFloat(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:flt:{textValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataSerialization.ParseTracorDataPropertyMinimizeString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeValue).IsEquivalentTo(typeValue);
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameFloat);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.InnerTextValue).IsEqualTo(textValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        */
        sb.Clear();
    }

    [Test]
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
}
