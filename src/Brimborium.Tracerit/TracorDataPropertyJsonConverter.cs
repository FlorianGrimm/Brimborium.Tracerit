#if LATER
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

public partial class TracorDataPropertyJsonConverter : JsonConverter<TracorDataProperty> {
    public override TracorDataProperty Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    public override void Write(
        Utf8JsonWriter writer, 
        TracorDataProperty value, 
        JsonSerializerOptions options) {
        writer.WriteStartArray();
        var sb = new System.Text.StringBuilder();
        value.ToMinimizeString(sb);
        writer.WriteStringValue(sb.ToString());
        writer.WriteEndArray();
    }
}
#endif