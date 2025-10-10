namespace Brimborium.Tracerit;

public sealed class TracorDataPropertyMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataProperty> {

    public override TracorDataProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        // [ 
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException($"StartArray expected, but {reader.TokenType} found."); }
        if (!reader.Read()) { throw new JsonException("Content expected"); }

        // [ "<propertyName>"
        if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
        string? propertyName = reader.GetString();
        if (propertyName is not { Length: > 0 }) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException("Content expected"); }

        // [ "<propertyName>", "<typeName>"
        if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
        string? typeName = reader.GetString();
        if (typeName is not { Length: > 0 }) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException("Content expected"); }

        // [ "<propertyName>", "null", "<value>"
        if (TracorDataProperty.TypeNameNull == typeName) {
            if (reader.TokenType != JsonTokenType.Null) { throw new JsonException(); }
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateNull(propertyName);
        }

        // [ "<propertyName>", "str", "<value>"
        if (TracorDataProperty.TypeNameString == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateString(propertyName, argValue ?? string.Empty);
        }

        // [ "<propertyName>", "int", "<value>"
        if (TracorDataProperty.TypeNameInteger == typeName) {
            if (reader.TokenType != JsonTokenType.Number) { throw new JsonException("Number expected"); }
            var longValue = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateInteger(propertyName, longValue);
        }

        // [ "<propertyName>", "lvl", "<value>"
        if (TracorDataProperty.TypeNameLevelValue == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            if (Enum.TryParse<LogLevel>(argValue, out var logLevelValue)) {
                return TracorDataProperty.CreateLevelValue(propertyName, logLevelValue);
            } else {
                throw new JsonException($"Invalid LogLevel value: {argValue}");
            }
        }

        // [ "<propertyName>", "enum", "<value>"
        if (TracorDataProperty.TypeNameEnum == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return new TracorDataProperty(
                name: propertyName,
                typeValue: TracorDataPropertyTypeValue.Enum,
                textValue: argValue ?? string.Empty
            );
        }

        // [ "<propertyName>", "dt", "<value>"
        if (TracorDataProperty.TypeNameDateTime == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
            
            if (DateTime.TryParseExact(
                s: argValue,
                format: "O",
                provider: System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                style: DateTimeStyles.AdjustToUniversal,
                out var dtValue)) {
                return TracorDataProperty.CreateDateTime(propertyName, dtValue);
            } else {
                throw new JsonException();
            }
        }

        // [ "<propertyName>", "dt", "<value>"
        if (TracorDataProperty.TypeNameDateTimeOffset == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
            
            if (DateTimeOffset.TryParseExact(
                input: argValue,
                format: "O",
                formatProvider: System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                styles: DateTimeStyles.AssumeUniversal,
                out var dtoValue)) {
                return TracorDataProperty.CreateDateTimeOffset(propertyName, dtoValue);
            } else {
                throw new JsonException();
            }
        }

        // [ "<propertyName>", "bool", "<value>"
        if (TracorDataProperty.TypeNameBoolean == typeName) {
            bool boolValue;
            if (reader.TokenType is JsonTokenType.False or JsonTokenType.True) {
                boolValue = reader.GetBoolean();
            } else if (reader.TokenType == JsonTokenType.Number) {
                boolValue = reader.GetInt64() != 0;
            } else {
                throw new JsonException();
            }
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
            
            return TracorDataProperty.CreateBoolean(propertyName, boolValue);
        }

        // [ "<propertyName>", "flt", "<value>"
        if (TracorDataProperty.TypeNameFloat == typeName) {
            if (reader.TokenType != JsonTokenType.Number) { throw new JsonException("Number expected"); }
            var floatValue = reader.GetDouble();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
            
            return TracorDataProperty.CreateFloat(propertyName, floatValue);
        }

        // [ "<propertyName>", "uuid", "<value>"
        if (TracorDataProperty.TypeNameUuid == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
            
            if (Guid.TryParse(argValue, out var guidValue)) {
                return TracorDataProperty.CreateGuid(propertyName, guidValue);
            } else {
                throw new JsonException($"Invalid GUID value: {argValue}");
            }
        }

        throw new JsonException($"Unknown property type in JSON");
    }

    public override void Write(Utf8JsonWriter writer, TracorDataProperty value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Name);
        writer.WriteStringValue(value.TypeName);
        switch (value.TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                break;
            case TracorDataPropertyTypeValue.String:
            case TracorDataPropertyTypeValue.LevelValue:
            case TracorDataPropertyTypeValue.Enum:
            case TracorDataPropertyTypeValue.DateTime:
            case TracorDataPropertyTypeValue.DateTimeOffset:
                writer.WriteStringValue(value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.Boolean:
                writer.WriteBooleanValue(value.InnerLongValue != 0);
                break;
            case TracorDataPropertyTypeValue.Integer:
                writer.WriteNumberValue(value.InnerLongValue);
                break;
            case TracorDataPropertyTypeValue.Float:
                writer.WriteNumberValue(value.InnerFloatValue);
                break;
            case TracorDataPropertyTypeValue.Uuid:
                writer.WriteStringValue(value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.Any:
                break;
            default:
                throw new NotSupportedException($"{value.TypeValue}");
        }

        writer.WriteEndArray();
    }
}