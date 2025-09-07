namespace Brimborium.Tracerit.Service;

public sealed class ValueTracorData<TValue> : ITracorData<TValue> {
    private readonly TValue _Value;

    public ValueTracorData(TValue value) {
        this._Value = value;
    }
    public List<string> GetListPropertyName() {
        return new List<string> { "Value" };
    }

    public bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value) {
        value = this._Value;
        return true;
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        if (typeof(TValue).GetProperty(propertyName) is { } propertyInfo) {
            propertyValue=propertyInfo.GetValue(this._Value);
            return true;
        }

        if ("Value" == propertyName) {
            propertyValue = this._Value;
            return true;
        }
        propertyValue = null;
        return false;
    }
}
