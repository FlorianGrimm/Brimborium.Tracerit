namespace Brimborium.Tracerit.Filter;

/// <summary>
/// ITracorFactory extension methods for common scenarios.
/// </summary>
public static class TracorScopedFilterFactoryExtensions {
    /// <summary>
    /// Creates a new <see cref="ITracorScopedFilter"/> instance using the full name of the given type.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The <see cref="ITracorScopedFilter"/> that was created.</returns>
    public static ITracorScopedFilter<T> CreateTracorScopedFilter<T>(this ITracorScopedFilterFactory factory) {
        ArgumentNullException.ThrowIfNull(factory);

        return new TracorScopedFilter<T>(factory);
    }
    /// <summary>
    /// Creates a new <see cref="ITracorScopedFilter"/> instance using the full name of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <param name="type">The type.</param>
    /// <returns>The <see cref="ITracorScopedFilter"/> that was created.</returns>
    public static ITracorScopedFilter CreateTracorScopedFilter(this ITracorScopedFilterFactory factory, Type type) {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(type);

        return factory.CreateTracorScopedFilter(
            TypeNameHelper.GetTypeDisplayName(
                type, 
                includeGenericParameters: 
                false, nestedTypeDelimiter: '.'));
    }
}
