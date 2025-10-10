namespace Brimborium.Tracerit.Test;

public class TracorDataPropertyTests {
    [Test]
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "def", true)]
    [Arguments(TracorDataPropertyTypeValue.String, "", "def", false)]
    [Arguments(TracorDataPropertyTypeValue.String, "abc", "", true)]
    public async Task CreateStringTest(TracorDataPropertyTypeValue typeValue, string argName, string argValue, bool successfull) {
        {
            var act = TracorDataProperty.CreateString(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo(argValue);

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1L).IsEqualTo(-1L);

            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
            var act = TracorDataProperty.CreateInteger(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");

            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(argValue);

            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
    [Arguments(TracorDataPropertyTypeValue.LevelValue, "abc", LogLevel.Warning, true)]
    [Arguments(TracorDataPropertyTypeValue.LevelValue, "", LogLevel.Information, false)]
    public async Task CreateLevelValueTest(TracorDataPropertyTypeValue typeValue, string argName, LogLevel argValue, bool successfull) {
        {
            var act = TracorDataProperty.CreateLevelValue(argName, argValue);
            
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);

            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
            var act = TracorDataProperty.CreateDateTime(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);

            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(argValue);

            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
            var act = TracorDataProperty.CreateDateTimeOffset(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);

            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtoValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(argValue);

            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsEqualTo(argValue);
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(-1);
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
    [Arguments(TracorDataPropertyTypeValue.Float, "abc", 1.5d, true)]
    [Arguments(TracorDataPropertyTypeValue.Float, "abc", 100.5d, true)]
    [Arguments(TracorDataPropertyTypeValue.Float, "abc", -4.2, true)]
    [Arguments(TracorDataPropertyTypeValue.Float, "", 0d, false)]
    public async Task CreateFloatTest(TracorDataPropertyTypeValue typeValue, string argName, double argValue, bool successfull) {
        var sb = new StringBuilder();
        var textValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        {
            var act = TracorDataProperty.CreateFloat(argName, argValue);
            await Assert.That(act.TryGetStringValue(out var txtValue) ? txtValue : "no").IsEqualTo("no");
            await Assert.That(act.TryGetIntegerValue(out var intValue) ? intValue : -1).IsEqualTo(-1);
            await Assert.That(act.TryGetLevelValueValue(out var lvlValue) ? lvlValue : LogLevel.None).IsEqualTo(LogLevel.None);
            await Assert.That(act.TryGetDateTimeValue(out var dtValue) ? dtValue : DateTime.MinValue).IsEqualTo(DateTime.MinValue);
            await Assert.That(act.TryGetDateTimeOffsetValue(out var dtoValue) ? dtValue : DateTimeOffset.FromUnixTimeMilliseconds(0)).IsEqualTo(DateTimeOffset.FromUnixTimeMilliseconds(0));
            await Assert.That(act.TryGetBooleanValue(out var boolValue) ? boolValue : default(bool?)).IsNull();
            await Assert.That(act.TryGetFloatValue(out var floatValue) ? floatValue : -1).IsEqualTo(argValue);
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
}
