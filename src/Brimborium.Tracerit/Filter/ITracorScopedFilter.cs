namespace Brimborium.Tracerit.Filter;

/// <summary>
/// Filter interface for checking if a specific log level is enabled for a source.
/// </summary>
public interface ITracorScopedFilter {
    /// <summary>
    /// Checks if the given <paramref name="logLevel"/> is enabled.
    /// </summary>
    /// <param name="sourceName">The name of the source to check.</param>
    /// <param name="logLevel">The log level to be checked.</param>
    /// <returns><see langword="true" /> if enabled.</returns>
    bool IsEnabled(
        string sourceName,
        LogLevel logLevel);

    /// <summary>
    /// Determines whether this filter includes all sub-scopes.
    /// </summary>
    /// <returns>True if all sub-scopes are included; otherwise, false.</returns>
    bool IncludingAllSubScope();
}

/// <summary>
/// Generic scoped filter bound to a specific category type.
/// </summary>
/// <typeparam name="TCategoryName">The category type.</typeparam>
public interface ITracorScopedFilter<out TCategoryName> : ITracorScopedFilter {
}


/// <summary>
/// Builder interface for configuring scoped filters with service collection access.
/// </summary>
public interface ITracorScopedFilterBuilder {
    /// <summary>
    /// Gets the service collection for registering filter services.
    /// </summary>
    IServiceCollection Services { get; }
}


/// <summary>
/// Factory interface for creating scoped filters by category name.
/// </summary>
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

/// <summary>
/// Interface representing a source for scoped filter configuration.
/// </summary>
public interface ITracorScopedFilterSource {
    /// <summary>
    /// Gets the name of the filter source.
    /// </summary>
    /// <returns>The source name.</returns>
    string GetSourceName();
}

/// <summary>
/// Interface for accessing the configuration of a filter source.
/// </summary>
public interface ITracorScopedFilterSourceConfiguration {
    /// <summary>
    /// Gets the configuration for this filter source.
    /// </summary>
    IConfiguration Configuration { get; }
}

/// <summary>
/// Generic configuration interface bound to a specific type.
/// </summary>
/// <typeparam name="T">The type associated with this configuration.</typeparam>
public interface ITracorScopedFilterSourceConfiguration<out T>
    : ITracorScopedFilterSourceConfiguration {
}

/// <summary>
/// Factory interface for retrieving configuration by provider type.
/// </summary>
public interface ITracorScopedFilterConfigurationFactory {
    /// <summary>
    /// Gets the configuration for the specified provider type.
    /// </summary>
    /// <param name="providerType">The type of the provider.</param>
    /// <returns>The configuration for the provider.</returns>
    IConfiguration GetConfiguration(Type providerType);
}