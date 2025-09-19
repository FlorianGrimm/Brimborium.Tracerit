
namespace Brimborium.Tracerit.TracorActivityListener;

/// <summary>
/// do nothing
/// </summary>
internal sealed class RuntimeTracorActivityListener
    : BaseTracorActivityListener
    , ITracorActivityListener {

    public RuntimeTracorActivityListener(
        IServiceProvider serviceProvider,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<RuntimeTracorActivityListener> logger) : base(
            serviceProvider,
            options,
            logger) {
    }

    public void Start() {
        // do nothing
    }

    public void Stop() {
        // do nothing
    }

    // ITracorActivityListener

    public void AddActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceName.Add(name);
            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
        }
    }


    public void RemoveActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceName.Remove(name)) {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
            }
        }
    }

    public void AddActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceIdenifier.Add(activitySourceIdentifier);
            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
        }
    }

    public void RemoveActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceIdenifier.Remove(activitySourceIdentifier)) {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
            }
        }
    }
}
