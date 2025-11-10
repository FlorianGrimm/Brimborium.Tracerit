
namespace Brimborium.Tracerit.Service;

/// <summary>
/// Test-time implementation of <see cref="ITracorServiceSink"/> that enables full tracing functionality for testing scenarios.
/// This implementation processes trace data through validators and handles reference counting for objects.
/// </summary>
internal sealed class TracorServiceSink : ITracorServiceSink {
    private readonly ITracorCollectivePublisher _Publisher;
    private readonly ITracorScopedFilterFactory _TracorScopedFilterFactory;
    private readonly ITracorDataConvertService _TracorDataConvertService;
    private readonly ILogger _Logger;
    private readonly TracorEmergencyLogging _TracorEmergencyLogging;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorServiceSink"/> class.
    /// </summary>
    /// <param name="tracorValidator">The validator used to process trace data.</param>
    /// <param name="logger">The logger for error reporting.</param>
    public TracorServiceSink(
        ITracorCollectivePublisher publisher,
        ITracorScopedFilterFactory tracorScopedFilterFactory,
        ITracorDataConvertService tracorDataConvertService,
        LazyCreatedLogger<TracorServiceSink> logger,
        TracorEmergencyLogging tracorEmergencyLogging) {
        this._Publisher = publisher;
        this._TracorScopedFilterFactory = tracorScopedFilterFactory;
        this._TracorDataConvertService = tracorDataConvertService;
        this._Logger = logger;
        this._TracorEmergencyLogging = tracorEmergencyLogging;
    }

    public ITracorScopedFilterFactory GetTracorScopedFilterFactory() {
        return this._TracorScopedFilterFactory;
    }

    /// <summary>
    /// Determines if tracing is generally enabled at the configuration level.
    /// </summary>
    /// <returns>Always returns true for test-time scenarios.</returns>
    public bool IsGeneralEnabled() => true;

    /// <summary>
    /// Determines if tracing is currently enabled and active for processing.
    /// </summary>
    /// <returns>True if the validator is enabled; otherwise, false.</returns>
    public bool IsCurrentlyEnabled() => this._Publisher.IsEnabled();

    public bool IsPrivateEnabled(string scope, LogLevel logLevel) {
        if (this._Publisher.IsEnabled()) {
            var tracorScopedFilter = this._TracorScopedFilterFactory.CreateTracorScopedFilter(scope);
            if (tracorScopedFilter.IsEnabled(sourceName: TracorConstants.SourceProviderTracorPrivate, logLevel: logLevel)) {
                return true;
            }
        }
        return false;
    }

    public bool IsPublicEnabled(string scope, LogLevel logLevel) {
        if (this._Publisher.IsEnabled()) {
            var tracorScopedFilter = this._TracorScopedFilterFactory.CreateTracorScopedFilter(scope);
            if (tracorScopedFilter.IsEnabled(sourceName: TracorConstants.SourceProviderTracorPublic, logLevel: logLevel)) {
                return true;
            }
        }
        return false;
    }

    public void TracePrivate<T>(string scope, LogLevel level, string message, T value) {
        try {
            var timestamp = DateTime.UtcNow;
            TracorIdentifier callee = new(TracorConstants.SourceProviderTracorPrivate, scope, message);
            ITracorData tracorData;
            bool disposeTracorData;

            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
                disposeTracorData = false;
            } else {
                tracorData = this._TracorDataConvertService.ConvertPrivate(callee, value);
                disposeTracorData = true;
            }
            tracorData.TracorIdentifier = callee;
            tracorData.Timestamp = timestamp;

            if (disposeTracorData && tracorData is IReferenceCountObject referenceCountObject) {
                referenceCountObject.Dispose();
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }

    public void TracePublic<T>(string scope, LogLevel level, string message, T value) {
        try {
            ITracorData tracorData;
            bool disposeTracorData;

            DateTime timestamp = DateTime.UtcNow;
            TracorIdentifier callee = new(TracorConstants.SourceProviderTracorPublic, scope, message);
            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
                disposeTracorData = false;
                if (tracorData is TracorDataRecord tracorDataRecord) {
                    TracorDataUtility.SetActivityIfNeeded(tracorDataRecord.ListProperty);
                }
            } else {
                tracorData = this._TracorDataConvertService.ConvertPublic(callee, value);
                disposeTracorData = true;
            }
            tracorData.TracorIdentifier = callee;
            tracorData.Timestamp = timestamp;

            this._Publisher.OnTrace(true, tracorData);

            if (disposeTracorData && tracorData is IReferenceCountObject referenceCountObject) {
                referenceCountObject.Dispose();
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
}
