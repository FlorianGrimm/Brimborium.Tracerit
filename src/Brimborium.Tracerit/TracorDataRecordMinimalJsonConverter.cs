namespace Brimborium.Tracerit;

public sealed class TracorDataRecordMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataRecord> {
    public override TracorDataRecord? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return ITracorDataJsonMinimalConverterUtility.Read(ref reader, typeToConvert, options);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TracorDataRecord value,
        JsonSerializerOptions options) {
        ITracorDataJsonMinimalConverterUtility.Write(writer, value, options);
    }
}

public sealed class ITracorDataMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<ITracorData> {
    public override ITracorData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var converterTracorDataProperty = options.GetConverter(typeof(TracorDataProperty));

        return ITracorDataJsonMinimalConverterUtility.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, ITracorData value, JsonSerializerOptions options) {
        ITracorDataJsonMinimalConverterUtility.Write(writer, value, options);
    }
}

public static class ITracorDataJsonMinimalConverterUtility {
    public static TracorDataRecord? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        if (reader.TokenType != JsonTokenType.StartArray) { throw new JsonException($"StartArray expected, but {reader.TokenType} found."); }
        var depth = reader.CurrentDepth;
        var typeTracorDataProperty = typeof(TracorDataProperty);
        var converterTracorDataProperty = (System.Text.Json.Serialization.JsonConverter<TracorDataProperty>)options.GetConverter(typeTracorDataProperty);
        TracorDataRecord result = new();
        TracorIdentitfier tracorIdentitfier = new TracorIdentitfier();
        int state = 0;
        while (reader.Read()) {
            if (JsonTokenType.EndArray == reader.TokenType) {
                break;
            }

            var tracorDataProperty = converterTracorDataProperty.Read(ref reader, typeTracorDataProperty, options);
            if (state < 4) {
                if (state == 0) {
                    if ("timestamp" == tracorDataProperty.Name) {
                        state = 1;
                        if (tracorDataProperty.TryGetDateTimeValue(out var timestampValue)) {
                            result.Timestamp = timestampValue;
                            continue;
                        }
                    }
                }
                if (state is 0 or 1) {
                    if ("source" == tracorDataProperty.Name) {
                        state = 2;
                        if (tracorDataProperty.TryGetStringValue(out var sourceValue)) {
                            tracorIdentitfier.Source = sourceValue ?? string.Empty;
                            continue;
                        }
                    }
                }

                if (state is 0 or 1 or 2) {
                    if ("scope" == tracorDataProperty.Name) {
                        state = 3;
                        if (tracorDataProperty.TryGetStringValue(out var scopeValue)) {
                            tracorIdentitfier.Scope = scopeValue ?? string.Empty;
                            continue;
                        }
                    }
                }

                if (state is 0 or 1 or 2 or 3) {
                    if ("message" == tracorDataProperty.Name) {
                        state = 4;
                        if (tracorDataProperty.TryGetStringValue(out var messageValue)) {
                            tracorIdentitfier.Message = messageValue ?? string.Empty;
                            continue;
                        }
                    }
                }
                result.ListProperty.Add(tracorDataProperty);
                continue;
            } else {
                result.ListProperty.Add(tracorDataProperty);
            }
        }

        if (reader.TokenType != JsonTokenType.EndArray) { throw new JsonException("EndArray expected"); }
        // if (!reader.Read()) { throw new JsonException("Content expected"); }

        result.TracorIdentitfier = tracorIdentitfier;
        return result;
    }

    [ThreadStatic]
    private static List<TracorDataProperty>? _CacheListTracorDataProperty;

    public static void Write(Utf8JsonWriter writer, ITracorData value, JsonSerializerOptions options) {
        var converterTracorDataProperty = (JsonConverter<TracorDataProperty>)options.GetConverter(typeof(TracorDataProperty));
        if (value is TracorDataRecord tracorDataRecord) {
            List<TracorDataProperty> listTracorDataProperty =
                System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, null)
                ?? new(128);

            writer.WriteStartArray();

            ITracorDataExtension.ConvertPropertiesBase<TracorDataRecord>(value, listTracorDataProperty);
            foreach (var property in listTracorDataProperty) {
                converterTracorDataProperty.Write(writer, property, options);
            }

            foreach (var property in tracorDataRecord.ListProperty) {
                converterTracorDataProperty.Write(writer, property, options);
            }
            writer.WriteEndArray();

            listTracorDataProperty.Clear();
            System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, listTracorDataProperty);
        } else {
            List<TracorDataProperty> listTracorDataProperty =
                System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, null)
                ?? new(128);

            writer.WriteStartArray();
            ITracorDataExtension.ConvertPropertiesBase<TracorDataRecord>(value, listTracorDataProperty);
            value.ConvertProperties(listTracorDataProperty);
            foreach (var property in listTracorDataProperty) {
                converterTracorDataProperty.Write(writer, property, options);
            }
            writer.WriteEndArray();

            listTracorDataProperty.Clear();
            System.Threading.Interlocked.Exchange(ref _CacheListTracorDataProperty, listTracorDataProperty);
        }
    }
}

public sealed class TracorDataJsonMinimalConverterFactory : JsonConverterFactory {
    private TracorDataRecordMinimalJsonConverter? _TracorDataRecordJsonConverter;
    private ITracorDataMinimalJsonConverter? _ITracorDataJsonConverter;

    public override bool CanConvert(Type typeToConvert) {
        if (typeof(ITracorData) == typeToConvert) { return true; }
        if (typeof(TracorDataRecord) == typeToConvert) { return true; }
        if (typeof(ITracorData).IsAssignableFrom(typeToConvert)) { return true; }
        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        if (typeof(ITracorData) == typeToConvert) {
            var result = (this._ITracorDataJsonConverter ??= new ITracorDataMinimalJsonConverter());
            return result;
        }
        if (typeof(TracorDataRecord) == typeToConvert) {
            var result = (this._TracorDataRecordJsonConverter ??= new TracorDataRecordMinimalJsonConverter());
            return result;
        }
        if (typeof(ITracorData).IsAssignableFrom(typeToConvert)) {
            var result = (this._ITracorDataJsonConverter ??= new ITracorDataMinimalJsonConverter());
            return result;
        }
        return default;
    }
}
