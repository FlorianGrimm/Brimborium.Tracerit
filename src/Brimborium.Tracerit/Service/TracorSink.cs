#pragma warning disable IDE0037

namespace Brimborium.Tracerit.Service;

/// <summary>
/// ITracorSink bound to baseScope
/// </summary>
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
        
        // use current
        if (tsf is { }) { return tsf.IsEnabled(TracorConstants.SourceProviderTracorPrivate, level); }

        if (fqScope is { Length: > 0 }) {
            var subTracorScopedFilter = this._TracorServiceSink.GetTracorScopedFilterFactory().CreateTracorScopedFilter(fqScope);
            return subTracorScopedFilter.IsEnabled(TracorConstants.SourceProviderTracorPublic, level);
        }

        //if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPrivateEnabled(fqScope, level); }
        return false;
    }

    public bool IsPublicEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);
        
        // use current
        if (tsf is { }) { return tsf.IsEnabled(TracorConstants.SourceProviderTracorPublic, level); }

        if (fqScope is { Length: > 0 }) {
            var subTracorScopedFilter = this._TracorServiceSink.GetTracorScopedFilterFactory().CreateTracorScopedFilter(fqScope);
            return subTracorScopedFilter.IsEnabled(TracorConstants.SourceProviderTracorPublic, level);
        }

        //if (fqScope is { Length: > 0 }) { return this._TracorServiceSink.IsPublicEnabled(fqScope, level); }
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

            if (this._TracorScopedFilter.IncludingAllSubScope()) {
                return (fqScope: fqScope, tracorScopedFilter: this._TracorScopedFilter);
            } else {
                return (fqScope: fqScope, tracorScopedFilter: null);
            }
        }
        return (fqScope: this._BaseScope, tracorScopedFilter: this._TracorScopedFilter);
    }
}

/// <summary>
/// ITracorSink bound to baseScope
/// </summary>
/// <typeparam name="TCategoryName">the source for the baseScope</typeparam>
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

        // use current
        if (tsf is { }) { return tsf.IsEnabled(TracorConstants.SourceProviderTracorPrivate, level); }

        // use the sub-scope
        if (fqScope is { Length: > 0 }) {
            var subTracorScopedFilter = this._TracorServiceSink.GetTracorScopedFilterFactory().CreateTracorScopedFilter(fqScope);
            return subTracorScopedFilter.IsEnabled(TracorConstants.SourceProviderTracorPublic, level);
        }

        return false;
    }

    public bool IsPublicEnabled(string? scope, LogLevel level) {
        if (!this._TracorServiceSink.IsCurrentlyEnabled()) { return false; }
        var (fqScope, tsf) = this.GetScopeAndTracorScopedFilter(scope);

        // use current
        if (tsf is { }) { return tsf.IsEnabled(TracorConstants.SourceProviderTracorPublic, level); }

        // use the sub-scope
        if (fqScope is { Length: > 0 }) {
            var subTracorScopedFilter = this._TracorServiceSink.GetTracorScopedFilterFactory().CreateTracorScopedFilter(fqScope);
            return subTracorScopedFilter.IsEnabled(TracorConstants.SourceProviderTracorPublic, level);
        }

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
            if (this._TracorScopedFilter.IncludingAllSubScope()) {
                return (fqScope: fqScope, tracorScopedFilter: this._TracorScopedFilter);
            } else { 
                return (fqScope: fqScope, tracorScopedFilter: null);
            }
        }
        return (fqScope: this._BaseScope, tracorScopedFilter: this._TracorScopedFilter);
    }
}