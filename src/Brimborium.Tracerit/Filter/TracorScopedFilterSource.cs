
namespace Brimborium.Tracerit.Filter;

/// <summary>
/// SourceName
/// inherited class must end with TracorScopedFilterSource or give the sourceName
/// </summary>
public class TracorScopedFilterSource : ITracorScopedFilterSource {
    public const string TypeNameSuffix = "TracorScopedFilterSource";
    public static string GetSourceNameFromType(Type type) {
        string typeName = type.Name;
        if (typeName.EndsWith(TypeNameSuffix)) {
            string name;
            name = typeName[..^TypeNameSuffix.Length];
            return name;
        }

        {
            int pos = typeName.IndexOf("Tracor");
            if (0 < pos) {
                return typeName[..pos];
            }
        }

        {
            return typeName;
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

public sealed class PublicTracorScopedFilterSource
    : ITracorScopedFilterSource {
    public string GetSourceName() => TracorConstants.SourceProviderTracorPublic;
}

public sealed class PrivateTracorScopedFilterSource
    : ITracorScopedFilterSource {

    public string GetSourceName() => TracorConstants.SourceProviderTracorPrivate;
}