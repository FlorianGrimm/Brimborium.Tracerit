namespace Brimborium.Tracerit.Diagnostics;
#pragma warning disable IDE0009 // Member access should be qualified.
/// <summary>
/// A disposable struct that manages a root activity and automatically restores the previous activity context when disposed.
/// </summary>
/// <remarks>
/// <example>
/// <code>
/// using(var rootActivity = activitySource.StartRootActivity()){
///   /* Work is performed in the root activity context */
///   rootActivity.Activity?.SetTag("operation", "some-operation");
///
/// }
/// </code>
/// </example>
public struct RestoreRootActivity : IDisposable {
    /// <summary>
    /// The activity that was current before this root activity was started.
    /// This will be restored when the struct is disposed.
    /// </summary>
    private readonly Activity? _PreviousActivity;

    /// <summary>
    /// The root activity being managed by this instance.
    /// </summary>
    private Activity? _RootActivity;

    /// <summary>
    /// Gets the root activity managed by this instance.
    /// </summary>
    public readonly Activity? Activity => this._RootActivity;

    /// <summary>
    /// Donot use this constructor. Use <see cref="ActivitySourceBase.StartRootActivity"/> instead.
    /// </summary>
    public RestoreRootActivity(Activity? previous, Activity? rootActivity) {
        _PreviousActivity = previous;
        _RootActivity = rootActivity;
    }

    /// <summary>
    /// Disposes the root activity and restores the previous activity context.
    /// </summary>
    /// <remarks>
    /// The previous activity is only restored if it exists and has not been stopped,
    /// preventing the restoration of invalid activity contexts.
    /// </para>
    /// </remarks>
    public void Dispose() {
        try {
            using (var rootActivity = _RootActivity) {
                _RootActivity = null;
                Activity.Current = null;
                rootActivity?.Stop();
            }
            if (_PreviousActivity is not null && !_PreviousActivity.IsStopped) {
                Activity.Current = _PreviousActivity;
            }
        } catch {
        }
    }
}
#pragma warning restore IDE0009 // Member access should be qualified.