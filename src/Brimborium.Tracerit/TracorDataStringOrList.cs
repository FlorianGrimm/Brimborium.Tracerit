using System.Text.Json;
using System.Text.Json.Serialization;

namespace Brimborium.Tracerit;

/// <summary>
/// HACK
/// </summary>
public class TracorDataStringOrList {
    private string? _StringValue;
    private List<TracorDataStringOrList>? _ListValue;

    public TracorDataStringOrList() {
    }
    public TracorDataStringOrList(string value) {
        this._StringValue= value;
        this._ListValue= null;
    }

    public TracorDataStringOrList(List<TracorDataStringOrList> value) {
        this._StringValue= null;
        this._ListValue= value;
    }

    public object? Value {
        get {
            return this.ListValue is not null
                ? this.ListValue
                : this.StringValue;
        }

        set {
            if (value is List<TracorDataStringOrList> lv) {
                this.ListValue = lv;
                return;
            } else if (value is string sv) {
                this.StringValue = sv;
                return;
            } else if (value is null) {
                this._StringValue = null;
                this._ListValue = null;
                return;

            }
        }
    }
    public string? StringValue {
        get => this._StringValue;
        set {
            this._StringValue = value;
            this._ListValue = null;
        }
    }
    public List<TracorDataStringOrList>? ListValue {
        get => this._ListValue;
        set {
            this._ListValue = value;
            this._StringValue = null;
        }
    }


}

public class TracorDataStringOrListJsonConverter : JsonConverter<TracorDataStringOrList> {
    public override TracorDataStringOrList? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) {
        TracorDataStringOrList? result=null;
        if (reader.TokenType == JsonTokenType.String) {
            var sv = reader.GetString();
            result = new TracorDataStringOrList() { StringValue = sv };
        } else if (reader.TokenType == JsonTokenType.StartArray) {
            var list = new List<TracorDataStringOrList>();
            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndArray) {
                    break;
                }
                var item = this.Read(ref reader, typeToConvert, options);
                if (item is not null) {
                    list.Add(item);
                }
            }
            result = new TracorDataStringOrList() { ListValue = list };
        }
        return result;
    }

    public override void Write(
        Utf8JsonWriter writer,
        TracorDataStringOrList value,
        JsonSerializerOptions options) {
        if (value.StringValue is string sv) {
            writer.WriteStringValue(sv);
            return;
        } else if (value.ListValue is List<TracorDataStringOrList> lv) {
            writer.WriteStartArray();
            foreach (var item in lv) {
                this.Write(writer, item, options);
            }
            writer.WriteEndArray();
            return;
        }
    }
}