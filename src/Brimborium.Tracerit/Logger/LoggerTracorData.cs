namespace Brimborium.Tracerit.Logger;

public sealed class LoggerTracorData : ITracorData {
    private readonly KeyValuePair<string, object?>[] _Arguments;

    public LoggerTracorData(KeyValuePair<string, object?>[] arguments) {
        this._Arguments = arguments;
    }

    public KeyValuePair<string, object?>[] Arguments => this._Arguments;

    public List<string> GetListPropertyName() {
        return this._Arguments.Select(i => i.Key).ToList();
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        foreach (var arg in this._Arguments) {
            if (arg.Key == propertyName) {
                propertyValue = arg.Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }
}