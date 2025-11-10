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
        if (TracorConstants.TypeNameNull == typeName) {
            if (reader.TokenType != JsonTokenType.Null) { throw new JsonException(); }
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateNull(propertyName);
        }

        // [ "<propertyName>", "str", "<value>"
        if (TracorConstants.TypeNameString == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateStringValue(propertyName, argValue ?? string.Empty);
        }

        // [ "<propertyName>", "int", "<value>"
        if (TracorConstants.TypeNameInteger == typeName) {
            if (reader.TokenType != JsonTokenType.Number) { throw new JsonException("Number expected"); }
            var longValue = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateIntegerValue(propertyName, longValue);
        }

        // [ "<propertyName>", "lvl", "<value>"
        if (TracorConstants.TypeNameLevelValue == typeName) {
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
        if (TracorConstants.TypeNameEnum == typeName) {
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
        if (TracorConstants.TypeNameDateTime == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            if (DateTime.TryParseExact(
                s: argValue,
                format: "O",
                provider: TracorConstants.TracorCulture.DateTimeFormat,
                style: DateTimeStyles.AdjustToUniversal,
                out var dtValue)) {
                return TracorDataProperty.CreateDateTimeValue(propertyName, dtValue);
            } else {
                throw new JsonException();
            }
        }

        // [ "<propertyName>", "dt", "<value>"
        if (TracorConstants.TypeNameDateTimeOffset == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            if (DateTimeOffset.TryParseExact(
                input: argValue,
                format: "O",
                formatProvider: TracorConstants.TracorCulture.DateTimeFormat,
                styles: DateTimeStyles.AssumeUniversal,
                out var dtoValue)) {
                return TracorDataProperty.CreateDateTimeOffsetValue(propertyName, dtoValue);
            } else {
                throw new JsonException();
            }
        }

        // [ "<propertyName>", "bool", "<value>"
        if (TracorConstants.TypeNameBoolean == typeName) {
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

        // [ "<propertyName>", "dbl", "<value>"
        if (TracorConstants.TypeNameDouble == typeName) {
            if (reader.TokenType != JsonTokenType.Number) { throw new JsonException("Number expected"); }
            var floatValue = reader.GetDouble();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateDoubleValue(propertyName, floatValue);
        }


        // [ "<propertyName>", "dur", "<value>"
        if (TracorConstants.TypeNameDuration == typeName) {
            if (reader.TokenType != JsonTokenType.Number) { throw new JsonException("Number expected"); }
            var durationValue = reader.GetInt64();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            return TracorDataProperty.CreateDurationValue(propertyName, TracorDataUtility.DurationNanosecondsToTimeSpan(durationValue));
        }

        // [ "<propertyName>", "uuid", "<value>"
        if (TracorConstants.TypeNameUuid == typeName) {
            if (reader.TokenType != JsonTokenType.String) { throw new JsonException($"String expected, but {reader.TokenType} found."); }
            var argValue = reader.GetString();
            if (!reader.Read()) { throw new JsonException("Content expected"); }
            if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }

            if (Guid.TryParse(argValue, out var guidValue)) {
                return TracorDataProperty.CreateUuidValue(propertyName, guidValue);
            } else {
                throw new JsonException($"Invalid GUID value: {argValue}");
            }
        }

        throw new JsonException($"Unknown property type in JSON");
    }

    public static void WriteRef(Utf8JsonWriter writer, ref readonly TracorDataProperty value, JsonSerializerOptions options) {
        writer.WriteStartArray();
        writer.WriteStringValue(value.Name);
        writer.WriteStringValue(value.TypeName);
        switch (value.TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                break;
            case TracorDataPropertyTypeValue.String:
                value.TryGetStringValue(out var stringValue);
                writer.WriteStringValue(stringValue);
                break;
            case TracorDataPropertyTypeValue.Level:
            case TracorDataPropertyTypeValue.Enum:
                if (value.InnerObjectValue is string levelValue) {
                    writer.WriteStringValue(levelValue);
                } else {
                    writer.WriteStringValue(string.Empty);
                }
                break;
            case TracorDataPropertyTypeValue.DateTime:
                value.TryGetDateTimeValue(out var dateTimeValue);
                writer.WriteStringValue(dateTimeValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat));
                break;
            case TracorDataPropertyTypeValue.DateTimeOffset:
                value.TryGetDateTimeValue(out var dateTimeOffsetValue);
                writer.WriteStringValue(dateTimeOffsetValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat));
                break;
            case TracorDataPropertyTypeValue.Duration:
                value.TryGetDurationValue(out var durationValue);
                writer.WriteNumberValue(TracorDataUtility.TimeSpanToDurationNanoseconds(durationValue));
                break;
            case TracorDataPropertyTypeValue.Boolean:
                value.TryGetBooleanValue(out var boolValue);
                writer.WriteBooleanValue(boolValue);
                break;
            case TracorDataPropertyTypeValue.Integer:
                value.TryGetIntegerValue(out var integerValue);
                writer.WriteNumberValue(integerValue);
                break;
            case TracorDataPropertyTypeValue.Double:
                value.TryGetDoubleValue(out var floatValue);
                writer.WriteNumberValue(floatValue);
                break;
            case TracorDataPropertyTypeValue.Uuid:
                value.TryGetUuidValue(out var uuidValue);
                writer.WriteStringValue(uuidValue.ToString("d"));
                break;
            case TracorDataPropertyTypeValue.Any:
                writer.WriteStringValue(string.Empty);
                break;
            default:
                throw new NotSupportedException($"{value.TypeValue}");
        }

        writer.WriteEndArray();
    }

    public override void Write(Utf8JsonWriter writer, TracorDataProperty value, JsonSerializerOptions options) {
        WriteRef(writer, in value, options);
        /*
        writer.WriteStartArray();
        writer.WriteStringValue(value.Name);
        writer.WriteStringValue(value.TypeName);
        switch (value.TypeValue) {
            case TracorDataPropertyTypeValue.Null:
                break;
            case TracorDataPropertyTypeValue.String:
                value.TryGetStringValue(out var stringValue);
                writer.WriteStringValue(stringValue);
                break;
            case TracorDataPropertyTypeValue.Level:
            case TracorDataPropertyTypeValue.Enum:
                if (value.InnerObjectValue is string levelValue) {
                    writer.WriteStringValue(levelValue);
                } else {
                    writer.WriteStringValue(string.Empty);
                }
                break;
            case TracorDataPropertyTypeValue.DateTime:
                value.TryGetDateTimeValue(out var dateTimeValue);
                writer.WriteStringValue(dateTimeValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat));
                break;
            case TracorDataPropertyTypeValue.DateTimeOffset:
                value.TryGetDateTimeValue(out var dateTimeOffsetValue);
                writer.WriteStringValue(dateTimeOffsetValue.ToString("o", TracorConstants.TracorCulture.DateTimeFormat));
                break;
            case TracorDataPropertyTypeValue.Duration:
                value.TryGetDurationValue(out var durationValue);
                writer.WriteNumberValue(TracorDataUtility.TimeSpanToDurationNanoseconds(durationValue));
                break;
            case TracorDataPropertyTypeValue.Boolean:
                value.TryGetBooleanValue(out var boolValue);
                writer.WriteBooleanValue(boolValue);
                break;
            case TracorDataPropertyTypeValue.Integer:
                value.TryGetIntegerValue(out var integerValue);
                writer.WriteNumberValue(integerValue);
                break;
            case TracorDataPropertyTypeValue.Double:
                value.TryGetDoubleValue(out var floatValue);
                writer.WriteNumberValue(floatValue);
                break;
            case TracorDataPropertyTypeValue.Uuid:
                value.TryGetUuidValue(out var uuidValue);
                writer.WriteStringValue(uuidValue.ToString("d"));
                break;
            case TracorDataPropertyTypeValue.Any:
                writer.WriteStringValue(string.Empty);
                break;
            default:
                throw new NotSupportedException($"{value.TypeValue}");
        }

        writer.WriteEndArray();
        */
    }
}