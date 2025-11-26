namespace Brimborium.Tracerit.Diagnostics;

/// <summary>
/// Resolves an ActivitySource from a service provider.
/// </summary>
public interface IActivitySourceResolver {
    /// <summary>
    /// Resolves the ActivitySource using the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use for resolution.</param>
    /// <returns>The resolved ActivitySource, or null if not available.</returns>
    ActivitySource? Resolve(IServiceProvider serviceProvider);
}

public class InstrumentationBaseResolver<T>
    : IActivitySourceResolver
    where T : InstrumentationBase {
    private ActivitySource? _ActivitySource;
    private bool _IsResolved;

    public InstrumentationBaseResolver() {
    }

    public ActivitySource? Resolve(IServiceProvider serviceProvider) {
        if (!this._IsResolved) {
            lock (this) {
                if (!this._IsResolved) {
                    this._IsResolved = true;
                    var instrumentationBase = serviceProvider.GetService<T>();
                    this._ActivitySource = instrumentationBase?.ActivitySource;
                }
            }
        }
        return this._ActivitySource;
    }
}