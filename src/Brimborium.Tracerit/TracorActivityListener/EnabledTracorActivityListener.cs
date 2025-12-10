#pragma warning disable IDE0018 // Inline variable declaration

namespace Brimborium.Tracerit.TracorActivityListener;

internal sealed class EnabledTracorActivityListener
    : BaseTracorActivityListener
    , ITracorActivityListener
    , IDisposable {
    private ActivityListener? _Listener;
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private readonly ITracorCollectiveSink _Sink;
    private readonly ITracorDataConvertService _TracorDataConvertService;
    private readonly TracorEmergencyLogging _TracorEmergencyLogging;

    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor()]
    public EnabledTracorActivityListener(
        IServiceProvider serviceProvider,
        TracorDataRecordPool tracorDataRecordPool,
        ITracorCollectiveSink sink,
        ITracorDataConvertService tracorDataConvertService,
        IOptionsMonitor<TracorActivityListenerOptions> options,
        ILogger<EnabledTracorActivityListener> logger,
        TracorEmergencyLogging tracorEmergencyLogging) : base(
            serviceProvider,
            options,
            logger) {
        this._TracorDataRecordPool = tracorDataRecordPool;
        this._Sink = sink;
        this._TracorDataConvertService = tracorDataConvertService;
        this._TracorEmergencyLogging = tracorEmergencyLogging;
    }

    protected override void OnChangeOptions(TracorActivityListenerOptions options, string? name) {
        if (this._Listener is null) {
            this._LastOptions = options;
        } else {
            base.OnChangeOptions(options, name);
        }
    }

    /// <summary>
    /// add the listener.
    /// </summary>
    public void Start() {
        this.ThrowIfDisposed();

        using (this._Lock.EnterScope()) {
            if (this._Listener is { }) { return; }

            var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
            this.SetOptionState(nextOptionState);
            this.Restart();
        }
    }

    private void Restart() {
        using (var oldListener = this._Listener) {
            this._Listener = null;
        }

        ActivityListener listener = new ActivityListener() {
            ShouldListenTo = this.OnShouldListenTo,
            ActivityStarted = this.OnActivityStarted,
            ActivityStopped = this.OnActivityStopped,
            ExceptionRecorder = this.OnExceptionRecorder,
            Sample = this.OnSample,
            SampleUsingParentId = this.OnSampleUsingParentId
        };
        ActivitySource.AddActivityListener(listener);
        this._Listener = listener;
    }

    /// <summary>
    /// remove the listener
    /// </summary>
    public void Stop() {
        using (this._Lock.EnterScope()) {
            if (this._Listener is { } listener) {
                listener.Dispose();
                this._Listener = null;
            }
        }
    }

    private bool OnShouldListenTo(ActivitySource activitySource) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activitySource);
        if (string.IsNullOrEmpty(activitySourceIdentifier.Name)) { return false; }
        bool result;
        if (this._OptionState.AllowAllActivitySource) {
            result = true;
        } else {
            result = this.OnShouldListenTo(activitySourceIdentifier);
        }
        this._Logger.OnShouldListenToReturns(activitySourceIdentifier, result);
        if (this._TracorEmergencyLogging.IsEnabled) {
            this._TracorEmergencyLogging.Log($"OnShouldListenTo({activitySource.Name}):{result}");
        }
        return result;
    }

    private bool OnShouldListenTo(ActivitySourceIdentifier activitySourceIdentifier) {
        var currentOptionState = this._OptionState;
        if (currentOptionState.HashSetActivitySourceName.Contains(activitySourceIdentifier.Name)) {
            return true;
        }
        if (activitySourceIdentifier.Version is { Length: > 0 }
            && currentOptionState.HashSetActivitySourceIdentifier.Contains(activitySourceIdentifier)) {
            return true;
        }
        return false;
    }

    private void OnActivityStarted(Activity activity) {
        if (this._Listener is null || this.IsDisposed) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        if (!currentOptionState.ActivitySourceStartEventEnabled) { return; }

        if (!this._Sink.IsGeneralEnabled()) { return; }
        if (!this._Sink.IsEnabled()) { return; }

        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activity.Source);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        TracorIdentifier tracorIdentifier = new(TracorConstants.SourceProviderActivity, activitySourceIdentifier.Name, "Start");
        using (var tracorDataRecord = this._TracorDataRecordPool.Rent()) {
            this.CopyListProperty(activity, tracorDataRecord.ListProperty);
            tracorDataRecord.TracorIdentifier = tracorIdentifier;
            tracorDataRecord.Timestamp = activity.StartTimeUtc;
            this._Sink.OnTrace(true, tracorDataRecord);
        }
    }

    private void OnActivityStopped(Activity activity) {
        if (this._Listener is null || this.IsDisposed) { return; }

        // no locking needed since this._OptionState does not mutate
        var currentOptionState = this._OptionState;

        if (!currentOptionState.ActivitySourceStopEventEnabled) { return; }

        if (!this._Sink.IsGeneralEnabled()) { return; }
        if (!this._Sink.IsEnabled()) { return; }

        var activitySourceIdentifier = ActivitySourceIdentifier.Create(activity.Source);
        if (currentOptionState.AllowAllActivitySource) {
            // no check needed
        } else if (!this.OnShouldListenTo(activitySourceIdentifier)) {
            return;
        }

        TracorIdentifier tracorIdentifier = new(TracorConstants.SourceProviderActivity, activitySourceIdentifier.Name, "Stop");
        using (var tracorDataRecord = this._TracorDataRecordPool.Rent()) {
            this.CopyListProperty(activity, tracorDataRecord.ListProperty);
            tracorDataRecord.TracorIdentifier = tracorIdentifier;
            tracorDataRecord.Timestamp = activity.StartTimeUtc.Add(activity.Duration);
            this._Sink.OnTrace(true, tracorDataRecord);
        }
    }

    private const string _PrefixTag = "tag.";
    private const string _PrefixLink = "link.";
    private void CopyListProperty(Activity activity, List<TracorDataProperty> listProperty) {
        {
            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivitySpanId,
                    activity.Id ?? string.Empty));

            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameActivityTraceId,
                    activity.TraceId.ToString()));

            listProperty.Add(
                TracorDataProperty.CreateStringValue(
                    TracorConstants.TracorDataPropertyNameOperationName,
                    activity.OperationName));

            if (!ReferenceEquals(activity.OperationName, activity.DisplayName)) {
                listProperty.Add(
                    TracorDataProperty.CreateStringValue(
                        TracorConstants.TracorDataPropertyNameDisplayName,
                        activity.DisplayName));
            }

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStartTimeUtc,
                    activity.StartTimeUtc));

            listProperty.Add(
                TracorDataProperty.CreateDateTimeValue(
                    TracorConstants.TracorDataPropertyNameStopTimeUtc,
                    activity.StartTimeUtc.Add(activity.Duration)));
        }
        {
            var enumeratorTagObjects = activity.EnumerateTagObjects();

            while (enumeratorTagObjects.MoveNext()) {
                ref readonly var tag = ref enumeratorTagObjects.Current;
                if (tag.Value is { } tagValue) {
                    if (TracorDataUtility.IsBasicType(tagValue.GetType())) {
                        var tdp = TracorDataProperty.Create(_PrefixTag + tag.Key, tagValue);
                        if (tdp.TypeValue is TracorDataPropertyTypeValue.Null or TracorDataPropertyTypeValue.Any) {
                            //
                        } else {
                            listProperty.Add(tdp);
                        }
                    } else {
                        this._TracorDataConvertService.ConvertObjectToListProperty(
                            true, 1, _PrefixTag + tag.Key, tagValue,
                            listProperty);
                    }
                }
            }
        }
        {
            var enumeratorLinkObjects = activity.EnumerateLinks();
            int index = 0;
            while (enumeratorLinkObjects.MoveNext()) {
                ref readonly var link = ref enumeratorLinkObjects.Current;
                if (link.Context is { } linkContext) {
                    listProperty.Add(TracorDataProperty.CreateStringValue(
                        $"{_PrefixLink}Span[{index}]",
                        linkContext.SpanId.ToString()
                        ));
                    listProperty.Add(TracorDataProperty.CreateStringValue(
                        $"{_PrefixLink}Trace[{index}]",
                        linkContext.TraceId.ToString()
                        ));
                }
                index++;
            }
        }
    }


    private void OnExceptionRecorder(Activity activity, Exception exception, ref TagList tags) { }

    private readonly Dictionary<ActivitySourceIdentifier, ActivitySamplingResult> _OnSampleActivitySamplingResult = new();

    private ActivitySamplingResult OnSample(ref ActivityCreationOptions<ActivityContext> options) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(options.Source);
        ActivitySamplingResult result;
        // TODO: Configure named SamplingResult
        if (this._OnSampleActivitySamplingResult.TryGetValue(activitySourceIdentifier, out result)) {
            return result;
        } else {
            // TODO: Configure default SamplingResult
            return ActivitySamplingResult.AllDataAndRecorded;
        }
    }

    private readonly Dictionary<ActivitySourceIdentifier, ActivitySamplingResult> _OnSampleUsingParentIdActivitySamplingResult = new();

    private ActivitySamplingResult OnSampleUsingParentId(ref ActivityCreationOptions<string> options) {
        var activitySourceIdentifier = ActivitySourceIdentifier.Create(options.Source);
        ActivitySamplingResult result;
        // TODO: Configure named SamplingResult
        if (this._OnSampleUsingParentIdActivitySamplingResult.TryGetValue(activitySourceIdentifier, out result)) {
            return result;
        } else {
            // TODO: Configure default SamplingResult
            return ActivitySamplingResult.AllDataAndRecorded;
        }
    }

    protected override void Dispose(bool disposing) {
        using (var listener = this._Listener) {
            using (var optionsDispose = this._OptionsDispose) {
                if (disposing) {
                    this._OptionsDispose = null;
                    this._Listener = null;
                }
            }
        }
    }

    ~EnabledTracorActivityListener() {
        this.PrepareDispose(disposing: false);
    }

    public List<ActivitySourceIdentifier> GetListActivitySourceIdentifier() {
        return this._OptionState.HashSetActivitySourceIdentifier.ToList();
    }

    // ITracorActivityListener

    public void AddActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceName.Add(name);
            if (this._Listener is null) {
                //
            } else {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
                this.Restart();
            }
        }
    }

    public void RemoveActivitySourceName(string name) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceName.Remove(name)) {
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                    this.Restart();
                }
            }
        }
    }

    public void AddActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            this._DirectModifications.ListActivitySourceIdenifier.Add(activitySourceIdentifier);
            if (this._Listener is null) {
                //
            } else {
                var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                this.SetOptionState(nextOptionState);
                this.Restart();
            }
        }
    }

    public void RemoveActivitySourceIdentifier(ActivitySourceIdentifier activitySourceIdentifier) {
        using (this._Lock.EnterScope()) {
            if (this._DirectModifications.ListActivitySourceIdenifier.Remove(activitySourceIdentifier)) {
                if (this._Listener is null) {
                    //
                } else {
                    var nextOptionState = OptionState.Create(this._LastOptions, this._DirectModifications);
                    this.SetOptionState(nextOptionState);
                    this.Restart();
                }
            }
        }
    }
}
