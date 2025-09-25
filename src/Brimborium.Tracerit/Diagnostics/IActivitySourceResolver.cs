namespace Brimborium.Tracerit.Diagnostics;

public interface IActivitySourceResolver {
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