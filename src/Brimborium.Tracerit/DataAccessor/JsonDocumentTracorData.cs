namespace Brimborium.Tracerit.DataAccessor;
public sealed class JsonDocumentTracorData
    : ReferenceCountObject<System.Text.Json.JsonDocument>
    , ITracorData<System.Text.Json.JsonDocument> {

    public JsonDocumentTracorData(IReferenceCountPool? owner) : base(owner) {
    }

    public JsonDocumentTracorData(System.Text.Json.JsonDocument value) : base(default) {
        this._Value = value;
    }

    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var propertyValue)) {
                return propertyValue;
            } else {
                return null;
            }
        }
    }

    public List<string> GetListPropertyName() {
        var value = this.GetValue();

        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
            return value.RootElement.EnumerateObject().Select(item => item.Name).ToList();
        }
        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var len = value.RootElement.GetArrayLength();
            var result = new List<string>(len);
            for (var i = 0; i < len; i++) {
                result.Add(i.ToString());
            }
            return result;
        }
        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.String) {
            return ["Value"];
        }
        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.False) {
            return ["Value"];
        }
        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.True) {
            return ["Value"];
        }
        return [];
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out System.Text.Json.JsonDocument value) {
        value = this.GetValue();
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        var value = this.GetValue();

        if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
            if (value.RootElement.TryGetProperty(Encoding.UTF8.GetBytes(propertyName), out var jsonElement)) {
                propertyValue = jsonElement.ToString();
                return true;
            }
        } else if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
            if (int.TryParse(propertyName, out var idx) && 0 <= idx) {
                if (value.RootElement.EnumerateArray().Skip(idx).FirstOrDefault().ToString() is { } result) {
                    propertyValue = result;
                    return true;
                }
            }
        } else if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.String
            && value.RootElement.GetString() is { } stringValue) {
            propertyValue = stringValue;
            return true;
        } else if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.False) {
            propertyValue = false;
            return true;
        } else if (value.RootElement.ValueKind == System.Text.Json.JsonValueKind.True) {
            propertyValue = true;
            return true;
        }

        propertyValue = null;
        return false;
    }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
        // TODO: if needed
    }
}
