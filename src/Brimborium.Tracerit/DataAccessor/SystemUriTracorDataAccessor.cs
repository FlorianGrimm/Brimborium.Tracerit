namespace Brimborium.Tracerit.DataAccessor;

public sealed class SystemUriTracorDataAccessor : ITracorDataAccessor<Uri> {
    public void ConvertProperties(Uri value, List<TracorDataProperty> listProperty) {
        listProperty.Add(TracorDataProperty.CreateStringValue(nameof(System.Uri.AbsoluteUri), value.AbsoluteUri));
        listProperty.Add(TracorDataProperty.CreateStringValue(nameof(System.Uri.Host), value.Host));
        listProperty.Add(TracorDataProperty.CreateStringValue(nameof(System.Uri.LocalPath), value.LocalPath));
        listProperty.Add(TracorDataProperty.CreateStringValue(nameof(System.Uri.Query), value.Query));
        listProperty.Add(TracorDataProperty.CreateStringValue(nameof(System.Uri.PathAndQuery), value.PathAndQuery));
    }

    public List<string> GetListPropertyNameTyped(Uri value) {
        return [TracorConstants.TracorDataPropertyNameValue, "ToString", "Host", "PathAndQuery"];
    }

    public bool TryGetPropertyValueTyped(Uri value, string propertyName, out object? propertyValue) {
        switch (propertyName) {
            case TracorConstants.TracorDataPropertyNameValue:
                propertyValue = value;
                return true;
            case nameof(System.Uri.ToString):
                propertyValue = value.ToString();
                return true;

            case nameof(System.Uri.AbsoluteUri):
                propertyValue = value.AbsoluteUri;
                return true;
            case nameof(System.Uri.Host):
                propertyValue = value.Host;
                return true;
            case nameof(System.Uri.LocalPath):
                propertyValue = value.LocalPath;
                return true;
            case nameof(System.Uri.PathAndQuery):
                propertyValue = value.PathAndQuery;
                return true;

            default:
                propertyValue = default;
                return false;
        }
    }
}
