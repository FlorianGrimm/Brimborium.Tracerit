
namespace Brimborium.Tracerit.Filter;

/// <summary>
/// inherited class must end with TracorScopedFilterSource
/// </summary>
public class TracorScopedFilterSource : ITracorScopedFilterSource {
    public const string TypeNameSuffix = "TracorScopedFilterSource";
    public static string GetSourceNameFromType(Type type) {
        string typename = type.Name;
        if (typename.EndsWith(TypeNameSuffix)) {
            string name;
            name = typename[..^TypeNameSuffix.Length];
            return name;
        }

        {
            int pos = typename.IndexOf("Tracor");
            if (0 < pos) {
                return typename[..pos];
            }
        }

        {
            return typename;
        }
    }

    private readonly string _SourceName;

    protected TracorScopedFilterSource() {
        this._SourceName = GetSourceNameFromType(this.GetType());
    }

    public TracorScopedFilterSource(
        string sourceName
        ) {
        this._SourceName = sourceName;
    }

    public string GetSourceName() => this._SourceName;
}