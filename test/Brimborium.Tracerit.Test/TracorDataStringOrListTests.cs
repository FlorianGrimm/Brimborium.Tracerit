using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Brimborium.Tracerit.Test;
public class TracorDataStringOrListTests {
    [Test]
    public async Task CreateStringOrListTest() {
        {
            var act = new TracorDataStringOrList("abc");
            MemoryStream ms = new();
            System.Text.Json.Utf8JsonWriter utf8JsonWriter = new(ms);
            var options=new JsonSerializerOptions();
            options.Converters.Add(new TracorDataStringOrListJsonConverter());
            System.Text.Json.JsonSerializer.Serialize(utf8JsonWriter, act, options);
            utf8JsonWriter.Flush();
            var json = Encoding.UTF8.GetString(ms.ToArray());
            await Assert.That(json).IsEqualTo(@"""abc""");
        }
        {
            var act = new TracorDataStringOrList() {
                ListValue = new List<TracorDataStringOrList>() {
                    new TracorDataStringOrList("abc"),
                    new TracorDataStringOrList("def"),
                }
            };
            MemoryStream ms = new();
            System.Text.Json.Utf8JsonWriter utf8JsonWriter = new(ms);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new TracorDataStringOrListJsonConverter());
            System.Text.Json.JsonSerializer.Serialize(utf8JsonWriter, act, options);
            utf8JsonWriter.Flush();
            var json = Encoding.UTF8.GetString(ms.ToArray());
            await Assert.That(json).IsEqualTo(@"[""abc"",""def""]");
        }
    }
}
