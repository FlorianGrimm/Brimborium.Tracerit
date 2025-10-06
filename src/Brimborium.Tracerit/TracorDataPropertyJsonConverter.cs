using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

public static class TracorDataPropertyNames {
    public static readonly byte[] PropertyName_name = ("name"u8).ToArray();
    public static readonly byte[] PropertyName_text_Value = ("text_Value"u8).ToArray();
    public static readonly byte[] PropertyName_int_Value = ("int_Value"u8).ToArray();
    public static readonly byte[] PropertyName_logLevel_Value = ("logLevel_Value"u8).ToArray();
    public static readonly byte[] PropertyName_enum_Value = ("enum_Value"u8).ToArray();
    public static readonly byte[] PropertyName_dt_Value = ("dt_Value"u8).ToArray();
    public static readonly byte[] PropertyName_dto_Value = ("dto_Value"u8).ToArray();
    public static readonly byte[] PropertyName_bool_Value = ("bool_Value"u8).ToArray();
    public static readonly byte[] PropertyName_long_Value = ("long_Value"u8).ToArray();
    public static readonly byte[] PropertyName_float_Value = ("float_Value"u8).ToArray();
    public static readonly byte[] PropertyName_uuid_Value = ("uuid_Value"u8).ToArray();
}

public class TracorDataPropertyJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataProperty> {

    public override TracorDataProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        // { "name":"<propertyName>" }
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }
        if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }
        if (!reader.ValueTextEquals(TracorDataPropertyNames.PropertyName_name)) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }
        string? propertyName = reader.GetString();
        if (propertyName is not { Length: > 0 }) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }

        // { "name":"<propertyName>", "text_Value": "<value>" }
        if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyName_text_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return TracorDataProperty.CreateString(propertyName, argValue??string.Empty);
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
        return new();
    }

    public override void Write(Utf8JsonWriter writer, TracorDataProperty value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteString(TracorDataPropertyNames.PropertyName_name, value.Name);
        switch (value.TypeValue) {
            case TracorDataPropertyTypeValue.Any:
                break;
            case TracorDataPropertyTypeValue.String:
                writer.WriteString(TracorDataPropertyNames.PropertyName_text_Value, value.TextValue);
                break;
            case TracorDataPropertyTypeValue.Integer:
                writer.WriteString(TracorDataPropertyNames.PropertyName_int_Value, value.TextValue);
                break;
            case TracorDataPropertyTypeValue.LevelValue:
                writer.WriteString(TracorDataPropertyNames.PropertyName_logLevel_Value, value.TextValue);
                break;
            case TracorDataPropertyTypeValue.Enum:
                writer.WriteNumber(TracorDataPropertyNames.PropertyName_enum_Value, value.LongValue);
                writer.WriteString(TracorDataPropertyNames.PropertyName_text_Value, value.TextValue);
                break;
            case TracorDataPropertyTypeValue.DateTime:
                writer.WriteString(TracorDataPropertyNames.PropertyName_dt_Value, value.TextValue);
                writer.WriteNumber(TracorDataPropertyNames.PropertyName_int_Value, value.LongValue);
                break;
            case TracorDataPropertyTypeValue.DateTimeOffset:
                writer.WriteString(TracorDataPropertyNames.PropertyName_dto_Value, value.TextValue);
                writer.WriteNumber(TracorDataPropertyNames.PropertyName_int_Value, value.LongValue);
                break;
            case TracorDataPropertyTypeValue.Boolean:
                writer.WriteBoolean(TracorDataPropertyNames.PropertyName_bool_Value, value.LongValue == 0 ? false : true);
                break;
            case TracorDataPropertyTypeValue.Long:
                writer.WriteNumber(TracorDataPropertyNames.PropertyName_long_Value, value.LongValue);
                break;
            case TracorDataPropertyTypeValue.Float:
                writer.WriteNumber(TracorDataPropertyNames.PropertyName_float_Value, value.FloatValue);
                break;
            case TracorDataPropertyTypeValue.Uuid:
                writer.WriteString(TracorDataPropertyNames.PropertyName_uuid_Value, value.TextValue);
                break;
            default:
                throw new NotSupportedException($"{value.TypeValue}");
        }

        writer.WriteEndObject();
    }
}