using System.Runtime.InteropServices;

namespace Brimborium.Tracerit;

public sealed class TracorDataRecordMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorDataRecord> {
    private readonly TracorDataRecordPool? _TracorDataRecordPool;

    public TracorDataRecordMinimalJsonConverter(
        TracorDataRecordPool? tracorDataRecordPool
        ) {
        this._TracorDataRecordPool = tracorDataRecordPool;
    }

    public override TracorDataRecord? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        return ITracorDataJsonMinimalConverterUtility.Read(ref reader, typeToConvert, options, this._TracorDataRecordPool);
    }

    public override void Write(
        Utf8JsonWriter writer,
        TracorDataRecord value,
        JsonSerializerOptions options) {
        ITracorDataJsonMinimalConverterUtility.Write(writer, value, options);
    }
}

public sealed class TracorPropertySinkTargetMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<TracorPropertySinkTarget> {
    public override TracorPropertySinkTarget? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        throw new JsonException();
    }
    public override void Write(
        Utf8JsonWriter writer,
        TracorPropertySinkTarget value,
        JsonSerializerOptions options) {
        writer.WriteStartArray();
        if (value.ListHeader is { Count:>0 } listHeader) {
            var listTracorDataProperty = listHeader.ListTracorDataProperty;
            for (int index = 0; index < listHeader.Count; index++) {
                ref readonly var item = ref listTracorDataProperty[index];
                TracorDataPropertyMinimalJsonConverter.WriteRef(writer, in item, options);
            }
        }

        if (value.ListProperty is { Count: > 0 } listProperty) {
            var spanProperty = CollectionsMarshal.AsSpan(listProperty);
            for (int index = 0, cnt = spanProperty.Length; index < cnt; index++) { 
                ref readonly var item = ref spanProperty[index];
                TracorDataPropertyMinimalJsonConverter.WriteRef(writer, in item, options);
            }
        }

        if (value.ListPropertyFromTracorData is { Count: > 0 } listPropertyFromTracorData) {
            var spanProperty = CollectionsMarshal.AsSpan(listPropertyFromTracorData);
            for (int index = 0, cnt = spanProperty.Length; index < cnt; index++) {
                ref readonly var item = ref spanProperty[index];
                TracorDataPropertyMinimalJsonConverter.WriteRef(writer, in item, options);
            }
        }

        writer.WriteEndArray();
    }
}
    

public sealed class ITracorDataMinimalJsonConverter
    : System.Text.Json.Serialization.JsonConverter<ITracorData> {
    public override ITracorData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        //var converterTracorDataProperty = options.GetConverter(typeof(TracorDataProperty));
        return ITracorDataJsonMinimalConverterUtility.Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, ITracorData value, JsonSerializerOptions options) {
        ITracorDataJsonMinimalConverterUtility.Write(writer, value, options);
    }
}

public static class ITracorDataJsonMinimalConverterUtility {
#pragma warning disable IDE0060 // Remove unused parameter
    public static TracorDataRecord? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options,
        TracorDataRecordPool? tracorDataRecordPool = default) {
#pragma warning restore IDE0060 // Remove unused parameter
        if (reader.TokenType != JsonTokenType.StartArray) {
            throw new JsonException($"StartArray expected, but {reader.TokenType} found.");
        }

        //var depth = reader.CurrentDepth;
        var typeTracorDataProperty = typeof(TracorDataProperty);
        var converterTracorDataProperty = (System.Text.Json.Serialization.JsonConverter<TracorDataProperty>)options.GetConverter(typeTracorDataProperty);

        TracorDataRecord result;
        if (tracorDataRecordPool is { } pool) {
            result = pool.Rent();
        } else {
            result = new();
        }
        TracorIdentifier tracorIdentifier = new TracorIdentifier();
        int state = 0;
        while (reader.Read()) {
            if (JsonTokenType.EndArray == reader.TokenType) {
                break;
            }

            var tracorDataProperty = converterTracorDataProperty.Read(ref reader, typeTracorDataProperty, options);
            if (state < 4) {
                if (state == 0) {
                    if (TracorConstants.TracorDataPropertyNameTimestamp == tracorDataProperty.Name) {
                        state = 1;
                        if (tracorDataProperty.TryGetDateTimeValue(out var timestampValue)) {
                            result.Timestamp = timestampValue;
                            continue;
                        }
                    }
                }
                if (state is 0 or 1) {
                    if (TracorConstants.TracorDataPropertyNameSource == tracorDataProperty.Name) {
                        state = 2;
                        if (tracorDataProperty.TryGetStringValue(out var sourceValue)) {
                            tracorIdentifier.SourceProvider = sourceValue ?? string.Empty;
                            continue;
                        }
                    }
                }

                if (state is 0 or 1 or 2) {
                    if (TracorConstants.TracorDataPropertyNameScope == tracorDataProperty.Name) {
                        state = 3;
                        if (tracorDataProperty.TryGetStringValue(out var scopeValue)) {
                            tracorIdentifier.Scope = scopeValue ?? string.Empty;
                            continue;
                        }
                    }
                }

                if (state is 0 or 1 or 2 or 3) {
                    if (TracorConstants.TracorDataPropertyNameMessage == tracorDataProperty.Name) {
                        state = 4;
                        if (tracorDataProperty.TryGetStringValue(out var messageValue)) {
                            tracorIdentifier.Message = messageValue ?? string.Empty;
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

        result.TracorIdentifier = tracorIdentifier;
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
    private readonly TracorDataRecordPool? _TracorDataRecordPool;

    private TracorDataRecordMinimalJsonConverter? _TracorDataRecordJsonConverter;
    private ITracorDataMinimalJsonConverter? _ITracorDataJsonConverter;
    private TracorPropertySinkTargetMinimalJsonConverter? _TracorPropertySinkTargetMinimalJsonConverter;

    public TracorDataJsonMinimalConverterFactory(
        TracorDataRecordPool? tracorDataRecordPool
        ) {
        this._TracorDataRecordPool = tracorDataRecordPool;
    }

    public override bool CanConvert(Type typeToConvert) {
        if (typeof(TracorPropertySinkTarget) == typeToConvert) { return true; }
        if (typeof(TracorDataRecord) == typeToConvert) { return true; }
        if (typeof(ITracorData) == typeToConvert) { return true; }
        if (typeof(ITracorData).IsAssignableFrom(typeToConvert)) { return true; }
        return false;
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) {
        if (typeof(TracorDataRecord) == typeToConvert) {
            var result = (this._TracorDataRecordJsonConverter ??= new TracorDataRecordMinimalJsonConverter(this._TracorDataRecordPool));
            return result;
        }
        if (typeof(TracorPropertySinkTarget) == typeToConvert) {
            var result = (this._TracorPropertySinkTargetMinimalJsonConverter ??= new TracorPropertySinkTargetMinimalJsonConverter());
            return result;
        }
        if (typeof(ITracorData) == typeToConvert) {
            var result = (this._ITracorDataJsonConverter ??= new ITracorDataMinimalJsonConverter());
            return result;
        }
        if (typeof(ITracorData).IsAssignableFrom(typeToConvert)) {
            var result = (this._ITracorDataJsonConverter ??= new ITracorDataMinimalJsonConverter());
            return result;
        }
        return default;
    }
}

public static class JsonSerializerOptionsExtensions {
    public static JsonSerializerOptions AddTracorDataMinimalJsonConverter(
        this JsonSerializerOptions? that,
        TracorDataRecordPool? tracorDataRecordPool) {
        that ??= new();
        that.Converters.Add(new TracorDataRecordMinimalJsonConverter(tracorDataRecordPool));
        that.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        return that;

    }
}