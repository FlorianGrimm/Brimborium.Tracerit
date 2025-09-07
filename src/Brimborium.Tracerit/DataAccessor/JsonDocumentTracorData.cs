namespace Brimborium.Tracerit.DataAccessor;

public sealed class JsonDocumentTracorDataFactor
    : ITracorDataAccessorFactory<System.Text.Json.JsonDocument> {
    public bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        if (value is System.Text.Json.JsonDocument document) {
            tracorData = new JsonDocumentTracorData(document);
            return true;
        }
        tracorData = default;
        return false;
    }

    public bool TryGetDataTyped(System.Text.Json.JsonDocument value, [MaybeNullWhen(false)] out ITracorData tracorData) {
        tracorData = new JsonDocumentTracorData(value);
        return true;
    }
}

public sealed class JsonDocumentTracorData
    : ITracorData<System.Text.Json.JsonDocument> {
    private readonly System.Text.Json.JsonDocument _Value;

    public string? this[string name] {
        get {
            if (this._Value.RootElement.TryGetProperty(name, out var result)) {
                return result.ToString();
            } else {
                return null;
            }
        }
    }

    public JsonDocumentTracorData(System.Text.Json.JsonDocument value) {
        this._Value = value;
    }

    public List<string> GetListPropertyName() {
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
            return this._Value.RootElement.EnumerateObject().Select(item => item.Name).ToList();
        }
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
            var len = this._Value.RootElement.GetArrayLength();
            var result = new List<string>(len);
            for (var i = 0; i < len; i++) {
                result.Add(i.ToString());
            }
            return result;
        }
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.String) {
            return ["Value"];
        }
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.False) {
            return ["Value"];
        }
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.True) {
            return ["Value"];
        }
        return [];
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out System.Text.Json.JsonDocument value) {
        value = this._Value;
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object) {
            if (this._Value.RootElement.TryGetProperty(Encoding.UTF8.GetBytes(propertyName), out var jsonElement)) {
                propertyValue = jsonElement.ToString();
                return true;
            }
        } else if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array) {
            if (int.TryParse(propertyName, out var idx) && 0 <= idx) {
                if (this._Value.RootElement.EnumerateArray().Skip(idx).FirstOrDefault().ToString() is { } result) {
                    propertyValue = result;
                    return true;
                }
            }
        } else if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.String
            && this._Value.RootElement.GetString() is { } stringValue) {
            propertyValue = stringValue;
            return true;
        } else if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.False) {
            propertyValue = false;
            return true;
        } else if (this._Value.RootElement.ValueKind == System.Text.Json.JsonValueKind.True) {
            propertyValue = true;
            return true;
        }

        propertyValue = null;
        return false;
    }
}
