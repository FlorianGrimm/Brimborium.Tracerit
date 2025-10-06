using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Tracerit.Test; 
public class TracorDataPropertyJsonConverterTests {
    [Test]
    public async Task SerializeDeserializeString() {
        TracorDataProperty given = TracorDataProperty.CreateString("abc", "def");
        System.Text.Json.JsonSerializerOptions options = new();
        options.Converters.Add(new TracorDataPropertyJsonConverter());
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
        options.Converters.Add(new TracorDataPropertyJsonConverter());
        var json = System.Text.Json.JsonSerializer.Serialize(given, options);
        var act = System.Text.Json.JsonSerializer.Deserialize<List<TracorDataProperty>>(json, options);
        await Assert.That(act?.Count).IsNotNull().And.IsEqualTo(2);
        await Assert.That(act).IsEquivalentTo(given);
    }

    // CreateInteger
    // CreateLevelValue
    // CreateEnum object
    // CreateEnum<T>
    // CreateDateTime
    // CreateDateTimeOffset
    // CreateBoolean
    // CreateLong
    // CreateFloat
    // CreateGuid
}
