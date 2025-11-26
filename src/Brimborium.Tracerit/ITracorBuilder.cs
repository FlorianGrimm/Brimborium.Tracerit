namespace Brimborium.Tracerit;

/// <summary>
/// Builder interface for configuring Tracor services.
/// </summary>
public interface ITracorBuilder {
    /// <summary>
    /// Gets the service collection for registering Tracor services.
    /// </summary>
    public IServiceCollection Services { get; }
}

/// <summary>
/// Builder for configuring Tracor services.
/// </summary>
/// <param name="services">the service builder.</param>
public class TracorBuilder(IServiceCollection services)
    : ITracorBuilder {
    public IServiceCollection Services { get; } = services;
}
