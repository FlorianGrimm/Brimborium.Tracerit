namespace Brimborium.Tracerit.DataAccessor;

public sealed class SystemUriTracorDataAccessor : ITracorDataAccessor<Uri> {
    public List<string> GetListPropertyNameTyped(Uri value) {
        return ["Value", "ToString", "Host", "PathAndQuery"];
    }

    public bool TryGetPropertyValueTyped(Uri value, string propertyName, out object? propertyValue) {
        if ("Value" == propertyName) {
            propertyValue = value;
            return true;
        }
        if ("ToString" == propertyName) {
            propertyValue = value.ToString();
            return true;
        }
        if ("Host" == propertyName) {
            propertyValue = value.Host;
            return true;
        }
        if ("PathAndQuery" == propertyName) {
            propertyValue = value.PathAndQuery;
            return true;
        }
        propertyValue = default;
        return false;
    }
}
