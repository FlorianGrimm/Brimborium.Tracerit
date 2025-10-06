namespace Brimborium.Tracerit.Filter.Internal;

public sealed class NullTracorScopedFilter : ITracorScopedFilter {
    private static NullTracorScopedFilter? _Instance;
    public static NullTracorScopedFilter Instance
        => (_Instance ??= new NullTracorScopedFilter());

    private NullTracorScopedFilter() { }

    /// <inheritdoc />
    public bool IsEnabled(string sourceName, LogLevel logLevel) => false;
}

public class NullTracorScopedFilter<T> : ITracorScopedFilter<T> {
    private static NullTracorScopedFilter<T>? _Instance;
    public static NullTracorScopedFilter<T> Instance
        => (_Instance ??= new());

    /// <inheritdoc />
    public bool IsEnabled(string sourceName, LogLevel logLevel) => false;
}

/// <summary>
/// An <see cref="ITracorScopedFilterFactory"/> used to create an instance of
/// <see cref="NullTracorScopedFilter"/> that logs nothing.
/// </summary>
public sealed class NullTracorScopedFilterFactory : ITracorScopedFilterFactory {
    private static NullTracorScopedFilterFactory? _Instance;
    public static NullTracorScopedFilterFactory Instance
        => (_Instance ??= new NullTracorScopedFilterFactory());

    /// <summary>
    /// Creates a new <see cref="NullTracorScopedFilterFactory"/> instance.
    /// </summary>
    public NullTracorScopedFilterFactory() { }


    /// <inheritdoc />
    /// <remarks>
    /// This returns a <see cref="NullTracorScopedFilter"/> instance that logs nothing.
    /// </remarks>
    public ITracorScopedFilter CreateTracor(string name) {
        return NullTracorScopedFilter.Instance;
    }

    /// <inheritdoc />
    /// <remarks>
    /// This method ignores the parameter and does nothing.
    /// </remarks>
    public void AddTracorScopedFilterSourceRegister(ITracorScopedFilterSource register) {
    }

    /// <inheritdoc />
    public void Dispose() {
    }
}

/// <summary>
/// Provider for the <see cref="NullTracorScopedFilter"/>.
/// </summary>
public sealed class NullTracorScopedFilterSource : ITracorScopedFilterSource {
    private static NullTracorScopedFilterSource? _Instance;
    public static NullTracorScopedFilterSource Instance 
        => (_Instance ??= new NullTracorScopedFilterSource());

    private NullTracorScopedFilterSource() {
    }

    /// <inheritdoc />
    public ITracorScopedFilter CreateTracorScopedFilter(string categoryName) {
        return NullTracorScopedFilter.Instance;
    }

    /// <inheritdoc />
    public void Dispose() {
    }

    public string GetSourceName() => "Null";
}
