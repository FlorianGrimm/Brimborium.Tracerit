
namespace Brimborium.Tracerit;

public class TracorDataRecordJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataRecord> {
    public override TracorDataRecord? Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options) {
        throw new NotImplementedException();
    }

    [ThreadStatic]
    private static List<TracorDataProperty>? _CacheListTracorDataProperty;

    public override void Write(
        Utf8JsonWriter writer, 
        TracorDataRecord value,
        JsonSerializerOptions options) {
        writer.WriteStartArray();

        List<TracorDataProperty> listTracorDataProperty = 
            System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, null)
            ?? new(128);
        value.ConvertProperties(listTracorDataProperty);
        foreach (var tracorDataProperty in listTracorDataProperty) {
            writer.WriteStartObject();
            writer.WriteString("name", tracorDataProperty.Name);
            writer.WriteString("type", tracorDataProperty.TypeName);
            writer.WriteString("text_Value", tracorDataProperty.TextValue);
            writer.WriteEndObject();
        }
        listTracorDataProperty.Clear();
        System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, listTracorDataProperty);

        writer.WriteEndArray();
    }
}