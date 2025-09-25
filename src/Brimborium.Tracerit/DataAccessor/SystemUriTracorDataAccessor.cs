namespace Brimborium.Tracerit.DataAccessor;

public sealed class SystemUriTracorDataAccessor : ITracorDataAccessor<Uri> {
    public void ConvertProperties(Uri value, List<TracorDataProperty> listProperty) {
        listProperty.Add(TracorDataProperty.CreateString(nameof(System.Uri.AbsoluteUri), value.AbsoluteUri));
        listProperty.Add(TracorDataProperty.CreateString(nameof(System.Uri.Host), value.Host));
        listProperty.Add(TracorDataProperty.CreateString(nameof(System.Uri.LocalPath), value.LocalPath));
        listProperty.Add(TracorDataProperty.CreateString(nameof(System.Uri.Query), value.Query));
        listProperty.Add(TracorDataProperty.CreateString(nameof(System.Uri.PathAndQuery), value.PathAndQuery));
    }

    public List<string> GetListPropertyNameTyped(Uri value) {
        return ["Value", "ToString", "Host", "PathAndQuery"];
    }

    public bool TryGetPropertyValueTyped(Uri value, string propertyName, out object? propertyValue) {
        switch (propertyName) {
            case "Value":
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
