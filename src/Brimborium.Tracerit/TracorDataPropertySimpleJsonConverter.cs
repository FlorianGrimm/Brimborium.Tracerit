#if later
namespace Brimborium.Tracerit;

public sealed class TracorDataPropertySimpleJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataProperty> {

    public override TracorDataProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        // { "name":"<propertyName>" }
        if (reader.TokenType != JsonTokenType.StartObject) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }
        if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }

        if (!reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_name)) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }
        string? propertyName = reader.GetString();
        if (propertyName is not { Length: > 0 }) { throw new JsonException(); }
        if (!reader.Read()) { throw new JsonException(); }

        if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }

        // { "name":"<propertyName>", "text_Value": "<value>" }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_text_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return TracorDataProperty.CreateString(propertyName, argValue ?? string.Empty);
        }


        // { "name":"<propertyName>", "int_Value": "<value>" }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_int_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            if (long.TryParse(argValue, System.Globalization.CultureInfo.InvariantCulture, out var intValue)) {
                return TracorDataProperty.CreateInteger(propertyName, intValue);
            } else {
                throw new JsonException($"Invalid integer value: {argValue}");
            }
        }

        // { "name":"<propertyName>", "logLevel_Value": "<value>" }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_logLevel_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            if (Enum.TryParse<LogLevel>(argValue, out var logLevelValue)) {
                return TracorDataProperty.CreateLevelValue(propertyName, logLevelValue);
            } else {
                throw new JsonException($"Invalid LogLevel value: {argValue}");
            }
        }

        // { "name":"<propertyName>", "enum_Value": <number>, "text_Value": "<value>" }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_enum_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var enumLongValue = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }
            if (!reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_text_Value)) { throw new JsonException(); }
            if (!reader.Read()) { throw new JsonException(); }
            var textValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return new TracorDataProperty(
                name: propertyName,
                typeValue: TracorDataPropertyTypeValue.Enum,
                textValue: textValue ?? string.Empty
            ) {
                InnerLongValue = enumLongValue
            };
        }

        // { "name":"<propertyName>", "dt_Value": "<value>", "int_Value": <nanoseconds> }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_dt_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var textValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }
            if (!reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_int_Value)) { throw new JsonException(); }
            if (!reader.Read()) { throw new JsonException(); }
            var nanoseconds = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }

            if (DateTime.TryParseExact(
                s: textValue,
                format: "O",
                provider: System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                style: DateTimeStyles.None,
                out var dtValue)) {
                return TracorDataProperty.CreateDateTime(propertyName, dtValue);
            } else {
                var dateTime = TracorDataUtility.UnixTimeNanosecondsToDateTime(nanoseconds);
                return TracorDataProperty.CreateDateTime(propertyName, dateTime);
            }
        }

        // { "name":"<propertyName>", "dto_Value": "<value>", "int_Value": <nanoseconds> }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_dto_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var textValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.PropertyName) { throw new JsonException(); }
            if (!reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_int_Value)) { throw new JsonException(); }
            if (!reader.Read()) { throw new JsonException(); }
            var nanoseconds = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }

            if (DateTimeOffset.TryParseExact(
                input: textValue,
                format: "O",
                formatProvider: System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat,
                styles: DateTimeStyles.AssumeUniversal,
                out var dtoValue)) {
                return TracorDataProperty.CreateDateTimeOffset(propertyName, dtoValue);
            } else {
                var dateTimeOffset = TracorDataUtility.UnixTimeNanosecondsToDateTimeOffset(nanoseconds);
                return TracorDataProperty.CreateDateTimeOffset(propertyName, dateTimeOffset);
            }
        }

        // { "name":"<propertyName>", "bool_Value": true/false }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_bool_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var boolValue = reader.GetBoolean();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return TracorDataProperty.CreateBoolean(propertyName, boolValue);
        }

        // { "name":"<propertyName>", "long_Value": <number> }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_int_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var longValue = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return TracorDataProperty.CreateInteger(propertyName, longValue);
        }

        // { "name":"<propertyName>", "float_Value": <number> }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_float_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var floatValue = reader.GetDouble();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            return TracorDataProperty.CreateFloat(propertyName, floatValue);
        }

        // { "name":"<propertyName>", "uuid_Value": "<guid>" }
        if (reader.ValueTextEquals(TracorDataPropertyNames.PropertyNameu8_uuid_Value)) {
            if (!reader.Read()) { throw new JsonException(); }
            var guidString = reader.GetString();
            if (!reader.Read()) { throw new JsonException(); }
            if (reader.TokenType != JsonTokenType.EndObject) { throw new JsonException(); }
            if (Guid.TryParse(guidString, out var guidValue)) {
                return TracorDataProperty.CreateGuid(propertyName, guidValue);
            } else {
                throw new JsonException($"Invalid GUID value: {guidString}");
            }
        }

        throw new JsonException($"Unknown property type in JSON");
    }

    public override void Write(Utf8JsonWriter writer, TracorDataProperty value, JsonSerializerOptions options) {
        writer.WriteStartObject();
        writer.WriteString(TracorDataPropertyNames.PropertyNameu8_name, value.Name);
        switch (value.TypeValue) {
            case TracorDataPropertyTypeValue.Any:
                break;
            case TracorDataPropertyTypeValue.String:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_text_Value, value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.LevelValue:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_logLevel_Value, value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.Enum:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_text_Value, value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.DateTime:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_dt_Value, value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.DateTimeOffset:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_dto_Value, value.InnerTextValue);
                break;
            case TracorDataPropertyTypeValue.Boolean:
                writer.WriteBoolean(TracorDataPropertyNames.PropertyNameu8_bool_Value, value.InnerLongValue != 0);
                break;
            case TracorDataPropertyTypeValue.Integer:
                writer.WriteNumber(TracorDataPropertyNames.PropertyNameu8_int_Value, value.InnerLongValue);
                break;
            case TracorDataPropertyTypeValue.Float:
                writer.WriteNumber(TracorDataPropertyNames.PropertyNameu8_float_Value, value.InnerFloatValue);
                break;
            case TracorDataPropertyTypeValue.Uuid:
                writer.WriteString(TracorDataPropertyNames.PropertyNameu8_uuid_Value, value.InnerTextValue);
                break;
            default:
                throw new NotSupportedException($"{value.TypeValue}");
        }

        writer.WriteEndObject();
    }
}
#endif