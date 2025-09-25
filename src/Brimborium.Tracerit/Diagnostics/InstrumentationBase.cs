using System.Reflection;

namespace Brimborium.Tracerit.Diagnostics;

public class InstrumentationBase : IInstrumentation, IDisposable {
    private ActivitySource? _ActivitySource;
    private readonly bool _IsShared;

    protected InstrumentationBase() {
        string? name = null;
        var lstDisplayNameAttribute = this.GetType().GetCustomAttributes<System.ComponentModel.DisplayNameAttribute>();
        if (lstDisplayNameAttribute is { }) {
            foreach (var attribute in lstDisplayNameAttribute) {
                var displayName = attribute.DisplayName;
                if (string.IsNullOrWhiteSpace(displayName)) {
                    continue;
                } else {
                    name = displayName;
                    break;
                }
            }
        }
        var type = this.GetType();
        if (name is null) {
            name = type.Namespace ?? type.Name ?? throw new Exception("anonymous class");
        }
        var version = type.Assembly.GetName().Version?.ToString();
        this._ActivitySource = new ActivitySource(name, version);
        this._IsShared = false;
    }

    protected InstrumentationBase(string name, string? version = "")
        : this(new ActivitySource(name, version), false) {
    }

    protected InstrumentationBase(ActivitySourceIdentifier activitySourceIdentifier)
        : this(new ActivitySource(activitySourceIdentifier.Name, activitySourceIdentifier.Version), false) {
    }

    protected InstrumentationBase(ActivitySource activitySource, bool isShared) {
        this._ActivitySource = activitySource;
        this._IsShared = isShared;
    }

    public ActivitySource? ActivitySource => this._ActivitySource;

    public ActivitySource GetActivitySource() => this._ActivitySource ?? throw new ObjectDisposedException(this.GetType().Name ?? nameof(InstrumentationBase));

    public void Dispose() {
        if (this._IsShared) {
            //
        } else {
            using (var a = this._ActivitySource) {
                this._ActivitySource = null;
            }
        }
    }
}
