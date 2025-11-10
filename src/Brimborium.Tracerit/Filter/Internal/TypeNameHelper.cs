
namespace Brimborium.Tracerit.Filter;

public class TypeNameHelper
{
    internal static string GetTypeDisplayName(Type type, bool includeGenericParameters, char nestedTypeDelimiter)
    {
        return type.FullName ?? type.Name;
    }

}

public class ProviderAliasUtilities
{ 
    public static string GetProviderAlias(Type type)
    {
        return type.Name;
    }

    internal static string? GetAlias(Type providerType)
    {
        return providerType.Name;
    }

}