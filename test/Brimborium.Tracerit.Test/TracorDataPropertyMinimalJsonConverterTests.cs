namespace Brimborium.Tracerit.Test;

public class TracorDataPropertyMinimalJsonConverterTests {
    private static TracorDataRecord getTestData() {
        TracorDataRecord given = new TracorDataRecord();
        given.Timestamp = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        given.TracorIdentitfier = new TracorIdentitfier("test", "test");
        given.ListProperty.Add(TracorDataProperty.CreateString("stringProp", "test value"));
        given.ListProperty.Add(TracorDataProperty.CreateInteger("intProp", -42));
        given.ListProperty.Add(TracorDataProperty.CreateLevelValue("levelProp", LogLevel.Critical));
        given.ListProperty.Add(TracorDataProperty.CreateEnum("enumProp", LogLevel.Debug));
        given.ListProperty.Add(TracorDataProperty.CreateDateTime("dtProp", new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc)));
        given.ListProperty.Add(TracorDataProperty.CreateDateTimeOffset("dtoProp", new DateTimeOffset((new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc)).Ticks, TimeSpan.Zero)));
        given.ListProperty.Add(TracorDataProperty.CreateBoolean("boolProp", true));
        given.ListProperty.Add(TracorDataProperty.CreateInteger("longProp", -9223372036854775808L));
        given.ListProperty.Add(TracorDataProperty.CreateFloat("floatProp", -123.456));
        given.ListProperty.Add(TracorDataProperty.CreateGuid("guidProp", Guid.Empty));
        return given;
    }

    [Test]
    public async Task SerializeTracorDataRecord() {
        TracorDataRecord given = getTestData();
        var options = new JsonSerializerOptions(
            TracorDataSerialization.GetMinimalJsonSerializerOptions()
            ) {
            WriteIndented = true
        };
        var act = TracorDataSerialization.SerializeSimple([given, given], options);
        await Verify(act);
    }

  
    [Test]
    public async Task DeSerializeTracorDataRecord() {
        TracorDataRecord givenData = getTestData();
        var options = new JsonSerializerOptions(
            TracorDataSerialization.GetMinimalJsonSerializerOptions()
            ) {
            WriteIndented = true
        };
        var givenJson = TracorDataSerialization.SerializeSimple([givenData, givenData], options);

        var act = TracorDataSerialization.DeserializeSimple(givenJson);
        await Assert.That(act.ListData.Count).IsEqualTo(2);
        await Assert.That(act.ListData[0].Timestamp.Year).IsEqualTo(2001);
        await Assert.That(act.ListData[0].TracorIdentitfier.Source).IsEqualTo("test");
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task SerializeDeserializeBoolean(bool testValue) {
        TracorDataProperty given = TracorDataProperty.CreateBoolean("testBool", testValue);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeInteger() {
        TracorDataProperty given = TracorDataProperty.CreateInteger("testLong", 9223372036854775807L);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }


    [Test]
    public async Task SerializeDeserializeFloat() {
        TracorDataProperty given = TracorDataProperty.CreateFloat("testFloat", 3.14159);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerFloatValue).IsEqualTo(given.InnerFloatValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeGuid() {
        var testGuid = Guid.NewGuid();
        TracorDataProperty given = TracorDataProperty.CreateGuid("testGuid", testGuid);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerUuidValue).IsEqualTo(given.InnerUuidValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeAllTypes() {
        var properties = new List<TracorDataProperty> {
            TracorDataProperty.CreateString("stringProp", "test value"),
            TracorDataProperty.CreateInteger("intProp", -42),
            TracorDataProperty.CreateLevelValue("levelProp", LogLevel.Critical),
            TracorDataProperty.CreateEnum("enumProp", LogLevel.Debug),
            TracorDataProperty.CreateDateTime("dtProp", DateTime.UtcNow),
            TracorDataProperty.CreateDateTimeOffset("dtoProp", DateTimeOffset.UtcNow),
            TracorDataProperty.CreateBoolean("boolProp", true),
            TracorDataProperty.CreateInteger("longProp", -9223372036854775808L),
            TracorDataProperty.CreateFloat("floatProp", -123.456),
            TracorDataProperty.CreateGuid("guidProp", Guid.Empty)
        };

        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        foreach (var given in properties) {
            var json = System.Text.Json.JsonSerializer.Serialize(given, options);
            var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);

            await Assert.That(act.Name).IsEqualTo(given.Name);
            await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
            await Assert.That(act.Value).IsEqualTo(given.Value);

            //act.Value

            //await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);

            // Type-specific value checks
            //switch (given.TypeValue) {
            //    case TracorDataPropertyTypeValue.Integer:
            //    case TracorDataPropertyTypeValue.LevelValue:
            //    case TracorDataPropertyTypeValue.Enum:
            //    case TracorDataPropertyTypeValue.DateTime:
            //    case TracorDataPropertyTypeValue.DateTimeOffset:
            //    case TracorDataPropertyTypeValue.Boolean:
            //        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
            //        break;
            //    case TracorDataPropertyTypeValue.Float:
            //        await Assert.That(act.InnerFloatValue).IsEqualTo(given.InnerFloatValue);
            //        break;
            //    case TracorDataPropertyTypeValue.Uuid:
            //        await Assert.That(act.InnerUuidValue).IsEqualTo(given.InnerUuidValue);
            //        break;
            //}
        }
    }

    [Test]
    public async Task DeserializeValidInteger() {
        var json = """["testInt","int",1234]""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        var p = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        var succ = p.TryGetIntegerValue(out var intValue);
        await Assert.That(succ).IsTrue();
        await Assert.That(intValue).IsEqualTo(1234);
    }

    [Test]
    public async Task DeserializeInvalidInteger_ThrowsJsonException() {
        var json = """["testInt","int","not_a_number"]""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Number expected");
    }

    [Test]
    public async Task DeserializeInvalidLogLevel_ThrowsJsonException() {
        var json = """["testLevel","lvl","InvalidLevel"]""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Invalid LogLevel value");
    }

    [Test]
    public async Task DeserializeInvalidGuid_ThrowsJsonException() {
        var json = """["testGuid", "uuid", "not-a-guid"]""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyMinimalJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Invalid GUID value");
    }
}
