namespace Brimborium.Tracerit.Service;

public sealed class TracorSink : ITracorSink {
    private readonly string _BaseScope;
    private readonly ITracorServiceSink _TracorServiceSink;
    private readonly ITracorScopedFilter _TracorScopedFilter;

    public TracorSink(
        string baseScope,
        ITracorServiceSink tracorServiceSink
        ) {
        this._BaseScope = baseScope;
        this._TracorServiceSink = tracorServiceSink;
        this._TracorScopedFilter = tracorServiceSink
            .GetTracorScopedFilterFactory()
            .CreateTracorScopedFilter(baseScope);
    }

    public bool IsPrivateEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);
        if (tsf is { }) { return tsf.IsEnabled(TracorConsts.SourceTracorPrivate, level); }
        if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPrivateEnabled(fqScope, level); }
        return false;
    }

    public bool IsPublicEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);
        if (tsf is { }) { return tsf.IsEnabled(TracorConsts.SourceTracorPublic, level); }
        if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPublicEnabled(fqScope, level); }
        return false;
    }

    public void TracePrivate<T>(string? scope, LogLevel level, string message, T value) {
        var fqScope = this.GetScope(scope);
        this._TracorServiceSink.TracePrivate<T>(fqScope, level, message, value);
    }

    public void TracePublic<T>(string? scope, LogLevel level, string message, T value) {
        var fqScope = this.GetScope(scope);
        this._TracorServiceSink.TracePublic<T>(fqScope, level, message, value);
    }

    private string GetScope(string? scope) {
        if (scope is { Length: > 0 } && this._BaseScope is { Length: > 0 }) {
            return $"{this._BaseScope}.{scope}";
        } else {
            return this._BaseScope;
        }
    }

    private (string? fqScope, ITracorScopedFilter? tracorScopedFilter) GetScopeAndTracorScopedFilter(string? scope) {
        if (scope is { Length: > 0 } && this._BaseScope is { Length: > 0 }) {
            string fqScope = $"{this._BaseScope}.{scope}";

            return (fqScope: fqScope, tracorScopedFilter: null);
        }
        return (fqScope: this._BaseScope, tracorScopedFilter: this._TracorScopedFilter);
    }
}
public sealed class TracorSink<TCategoryName> : ITracorSink<TCategoryName> {
    private readonly string _BaseScope;
    private readonly ITracorServiceSink _TracorServiceSink;
    private readonly ITracorScopedFilter _TracorScopedFilter;

    public TracorSink(
        ITracorServiceSink tracorServiceSink
        ) {
        this._BaseScope = TypeNameHelper.GetTypeDisplayName(
                typeof(TCategoryName),
                includeGenericParameters:
                false, nestedTypeDelimiter: '.');
        this._TracorServiceSink = tracorServiceSink;
        this._TracorScopedFilter = tracorServiceSink
            .GetTracorScopedFilterFactory()
            .CreateTracorScopedFilter(this._BaseScope);
    }

    public bool IsPrivateEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);
        if (tsf is { }) { return tsf.IsEnabled(TracorConsts.SourceTracorPrivate, level); }
        if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPrivateEnabled(fqScope, level); }
        return false;
    }

    public bool IsPublicEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);
        if (tsf is { }) { return tsf.IsEnabled(TracorConsts.SourceTracorPublic, level); }
        if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPublicEnabled(fqScope, level); }
        return false;
    }

    public void TracePrivate<T>(string? scope, LogLevel level, string message, T value) {
        var fqScope = this.GetScope(scope);
        this._TracorServiceSink.TracePrivate<T>(fqScope, level, message, value);
    }

    public void TracePublic<T>(string? scope, LogLevel level, string message, T value) {
        var fqScope = this.GetScope(scope);
        this._TracorServiceSink.TracePublic<T>(fqScope, level, message, value);
    }

    private string GetScope(string? scope) {
        if (scope is { Length: > 0 } && this._BaseScope is { Length: > 0 }) {
            return $"{this._BaseScope}.{scope}";
        } else {
            return this._BaseScope;
        }
    }

    private (string? fqScope, ITracorScopedFilter? tracorScopedFilter) GetScopeAndTracorScopedFilter(string? scope) {
        if (scope is { Length: > 0 } && this._BaseScope is { Length: > 0 }) {
            string fqScope = $"{this._BaseScope}.{scope}";

            return (fqScope: fqScope, tracorScopedFilter: null);
        }
        return (fqScope: this._BaseScope, tracorScopedFilter: this._TracorScopedFilter);
    }
}