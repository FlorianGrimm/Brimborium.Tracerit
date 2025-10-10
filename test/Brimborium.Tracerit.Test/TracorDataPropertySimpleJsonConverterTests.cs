#if later
namespace Brimborium.Tracerit.Test; 

public class TracorDataPropertySimpleJsonConverterTests {
    [Test]
    public async Task SerializeDeserializeString() {
        TracorDataProperty given = TracorDataProperty.CreateString("abc", "def");
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act).IsEquivalentTo(given);
    }

    [Test]
    public async Task SerializeDeserializeStringList() {
        TracorDataProperty given0 = TracorDataProperty.CreateString("abc0", "def1");
        TracorDataProperty given1 = TracorDataProperty.CreateString("abc0", "def1");
        List<TracorDataProperty> given = [given0, given1];
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<List<TracorDataProperty>>(json, options);
        await Assert.That(act?.Count).IsNotNull().And.IsEqualTo(2);
        await Assert.That(act).IsEquivalentTo(given);
    }

    [Test]
    public async Task SerializeDeserializeInteger() {
        TracorDataProperty given = TracorDataProperty.CreateInteger("testInt", 42);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeLevelValue() {
        TracorDataProperty given = TracorDataProperty.CreateLevelValue("testLevel", LogLevel.Warning);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeEnum() {
        TracorDataProperty given = TracorDataProperty.CreateEnum("testEnum", LogLevel.Error);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

#warning TODO
    /*
    [Test, Explicit]
    public async Task SerializeDeserializeDateTime() {
        var testDateTime = new DateTime(2023, 12, 25, 10, 30, 45, DateTimeKind.Utc);
        TracorDataProperty given = TracorDataProperty.CreateDateTime("testDateTime", testDateTime);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.LongValue).IsEqualTo(given.LongValue);
        // Verify the deserialized DateTime matches the original
        var deserializedDateTime = TracorDataUtility.UnixTimeNanosecondsToDateTime(act.LongValue);
        await Assert.That(deserializedDateTime).IsEqualTo(testDateTime);
    }
    */
#warning TODO
    /*
    [Test]
    public async Task SerializeDeserializeDateTimeOffset() {
        var testDateTimeOffset = new DateTimeOffset(2023, 12, 25, 10, 30, 45, TimeSpan.FromHours(2));
        TracorDataProperty given = TracorDataProperty.CreateDateTimeOffset("testDateTimeOffset", testDateTimeOffset);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.LongValue).IsEqualTo(given.LongValue);
        // Verify the deserialized DateTimeOffset matches the original
        var deserializedDateTimeOffset = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(act.LongValue);
        await Assert.That(deserializedDateTimeOffset.Ticks).IsEqualTo(testDateTimeOffset.Ticks);
    }
    */

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task SerializeDeserializeBoolean(bool testValue) {
        TracorDataProperty given = TracorDataProperty.CreateBoolean("testBool", testValue);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);
        await Assert.That(act.Name).IsEqualTo(given.Name);
        await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
        await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
        await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
    }

    [Test]
    public async Task SerializeDeserializeSimpleLong() {
        TracorDataProperty given = TracorDataProperty.CreateInteger("testLong", 9223372036854775807L);
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
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
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
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
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());
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
            // TracorDataProperty.CreateDateTime("dtProp", DateTime.UtcNow),
            // TracorDataProperty.CreateDateTimeOffset("dtoProp", DateTimeOffset.UtcNow),
            TracorDataProperty.CreateBoolean("boolProp", true),
            TracorDataProperty.CreateInteger("longProp", -9223372036854775808L),
            TracorDataProperty.CreateFloat("floatProp", -123.456),
            TracorDataProperty.CreateGuid("guidProp", Guid.Empty)
        };

        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());

        foreach (var given in properties) {
            var json = System.Text.Json.JsonSerializer.Serialize(given, options);
            var act = System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options);

            await Assert.That(act.Name).IsEqualTo(given.Name);
            await Assert.That(act.TypeValue).IsEqualTo(given.TypeValue);
            if (act.TypeValue == TracorDataPropertyTypeValue.DateTime) {
                // TODO:
                await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);

            } else {
                await Assert.That(act.InnerTextValue).IsEqualTo(given.InnerTextValue);
            }

            // Type-specific value checks
            switch (given.TypeValue) {
                case TracorDataPropertyTypeValue.Integer:
                case TracorDataPropertyTypeValue.LevelValue:
                case TracorDataPropertyTypeValue.Enum:
                case TracorDataPropertyTypeValue.DateTime:
                case TracorDataPropertyTypeValue.DateTimeOffset:
                case TracorDataPropertyTypeValue.Boolean:
                    await Assert.That(act.InnerLongValue).IsEqualTo(given.InnerLongValue);
                    break;
                case TracorDataPropertyTypeValue.Float:
                    await Assert.That(act.InnerFloatValue).IsEqualTo(given.InnerFloatValue);
                    break;
                case TracorDataPropertyTypeValue.Uuid:
                    await Assert.That(act.InnerUuidValue).IsEqualTo(given.InnerUuidValue);
                    break;
            }
        }
    }

    [Test]
    public async Task DeserializeInvalidInteger_ThrowsJsonException() {
        var json = """{"name":"testInt","int_Value":"not_a_number"}""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Invalid integer value");
    }

    [Test]
    public async Task DeserializeInvalidLogLevel_ThrowsJsonException() {
        var json = """{"name":"testLevel","logLevel_Value":"InvalidLevel"}""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Invalid LogLevel value");
    }

    [Test]
    public async Task DeserializeInvalidGuid_ThrowsJsonException() {
        var json = """{"name":"testGuid","uuid_Value":"not-a-guid"}""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Invalid GUID value");
    }

    [Test]
    public async Task DeserializeUnknownPropertyType_ThrowsJsonException() {
        var json = """{"name":"testProp","unknown_Value":"value"}""";
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertySimpleJsonConverter());

        var exception = Assert.Throws<JsonException>(() =>
            System.Text.Json.JsonSerializer.Deserialize<TracorDataProperty>(json, options));
        await Assert.That(exception.Message).Contains("Unknown property type");
    }
}
#endif