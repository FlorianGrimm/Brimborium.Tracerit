namespace Brimborium.Tracerit.Test;

public class TracorDataPropertyTests {
    [Test]
    [Arguments("abc", "def", true)]
    [Arguments("", "def", false)]
    [Arguments("abc", "", true)]
    public async Task ConvertStringTest(string argName, string argValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertString(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:str:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameString);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(argValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            } 
        }
        sb.Clear();
    }

    [Test]
    [Arguments("abc", 123, true)]
    [Arguments("", 123, false)]
    public async Task ConvertIntegerTest(string argName, int argValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertInteger(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:int:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameInteger);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }

    [Test]
    [Arguments("abc", LogLevel.Warning, true)]
    [Arguments("", LogLevel.Information, false)]
    public async Task ConvertLevelValueTest(string argName, LogLevel argValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertLevelValue(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:lvl:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameLevelValue);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(argValue.ToString());
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }

    /*
       
   94678224500 000 0000
   94677864500 000 0000
     */

    [Test]
    [Arguments("abc", "2000-01-02T03:04:05.0000000", 946782245000000000, true)]
    [Arguments("", "2000-01-02T03:04:05.0000000", 946782245000000000, false)]
    public async Task ConvertDateTimeTest(string argName, string txtArgValue, long ns, bool successfull) {
        DateTime argValue = DateTime.ParseExact(txtArgValue, "O", null);
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertDateTime(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:dt:{ns}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameDateTime);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }


    [Test]
    [Arguments("abc", "2000-01-02T03:04:05.0000000+02:00", 946782245000000000, true)]
    [Arguments("", "2000-01-02T03:04:05.0000000+02:00", 946782245000000000, false)]
    public async Task ConvertDateTimeOffsetTest(string argName, string txtArgValue, long ns, bool successfull) {
        DateTimeOffset argValue = DateTimeOffset.ParseExact(txtArgValue, "O", null);
        DateTimeOffset argValueUtc = new DateTimeOffset(new DateTime(argValue.Ticks), TimeSpan.Zero);

        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertDateTimeOffset(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:dto:{ns}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameDateTimeOffset);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(ns.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValueUtc);
            }
        }
        sb.Clear();
    }

    [Test]
    [Arguments("abc", true, "1", true)]
    [Arguments("abc", false, "0", true)]
    [Arguments("", false, "0", false)]
    public async Task ConvertBooleanTest(string argName, bool argValue, string textValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertBoolean(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:bool:{textValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameBoolean);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(textValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }

    [Test]
    [Arguments("abc", 1, true)]
    [Arguments("abc", 100000, true)]
    [Arguments("", 0, false)]
    public async Task ConvertLongTest(string argName, long argValue, bool successfull) {
        var sb = new StringBuilder();
        {
            var act = TracorDataProperty.ConvertLong(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:long:{argValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameLong);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }

    [Test]
    [Arguments("abc", 1.5, true)]
    [Arguments("abc", 100.5, true)]
    [Arguments("", 0, false)]
    public async Task ConvertDoubleTest(string argName, double argValue, bool successfull) {
        var sb = new StringBuilder();
        var textValue = argValue.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        {
            var act = TracorDataProperty.ConvertDouble(argName, argValue);
            act.ToMinimizeString(sb);
            await Assert.That(sb.ToString()).IsEqualTo($"{argName}:dbl:{textValue}");
        }
        {
            var stringValue = sb.ToString();
            var success = TracorDataProperty.TryParseFromJsonString(stringValue, out var act);
            await Assert.That(success).IsEqualTo(success);
            if (successfull) {
                await Assert.That(act.TypeName).IsEqualTo(TracorDataProperty.TypeNameDouble);
                await Assert.That(act.Name).IsEqualTo(argName);
                await Assert.That(act.TextValue).IsEqualTo(textValue);
                await Assert.That(act.Value).IsEqualTo(argValue);
            }
        }
        sb.Clear();
    }
}
