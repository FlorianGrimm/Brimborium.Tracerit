namespace Brimborium.Tracerit.Filter;

public interface ITracorScopedFilter {
    /// <summary>
    /// Checks if the given <paramref name="logLevel"/> is enabled.
    /// </summary>
    /// <param name="sourceName">sourceName</param>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns><see langword="true" /> if enabled.</returns>
    bool IsEnabled(
        string sourceName,
        LogLevel logLevel);

    bool IncludingAllSubScope();
}

public interface ITracorScopedFilter<out TCategoryName> : ITracorScopedFilter {
}


public interface ITracorScopedFilterBuilder {
    IServiceCollection Services { get; }
}


public interface ITracorScopedFilterFactory : IDisposable {
    /// <summary>
    /// Creates a new <see cref="ITracorScopedFilter"/> instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the tracor.</param>
    /// <returns>The <see cref="ITracorScopedFilter"/>.</returns>
    ITracorScopedFilter CreateTracorScopedFilter(string categoryName);

    /// <summary>
    /// Adds an <see cref="ITracorScopedFilterSource"/> to the TracorScopedFilter system.
    /// </summary>
    /// <param name="register">The <see cref="ITracorScopedFilterSource"/> to register.</param>
    void AddTracorScopedFilterSourceRegister(ITracorScopedFilterSource register);
}

public interface ITracorScopedFilterSource {
    /// <summary>
    /// Get the name of source.
    /// </summary>
    /// <returns>The name</returns>
    string GetSourceName();
}

public interface ITracorScopedFilterSourceConfiguration {
    IConfiguration Configuration { get; }
}

public interface ITracorScopedFilterSourceConfiguration<out T>
    : ITracorScopedFilterSourceConfiguration {
}

public interface ITracorScopedFilterConfigurationFactory {
    IConfiguration GetConfiguration(Type providerType);
}