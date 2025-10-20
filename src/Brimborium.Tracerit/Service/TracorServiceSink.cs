
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
            return true;
        } else {
            return false;
        }
    }

    public bool IsPublicEnabled(string scope, LogLevel logLevel) {
        if (this._Publisher.IsEnabled()) {
            return true;
        } else {
            return false;
        }
    }

    public void TracePrivate<T>(string scope, LogLevel level, string message, T value) {
        try {
#warning message tracorData.TracorIdentitfier
            TracorIdentitfier callee = new(TracorConsts.SourceTracorPrivate, scope, message);
            ITracorData tracorData;
            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
                tracorData.TracorIdentitfier = callee;
            } else {
                tracorData = this._TracorDataConvertService.ConvertPrivate(callee, value);
            }
            tracorData.Timestamp = DateTime.UtcNow;

            if (value is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
                this._Publisher.OnTrace(false, tracorData);
                referenceCountObject.Dispose();
            } else {
                this._Publisher.OnTrace(false, tracorData);
                if (tracorData is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
     
    public void TracePublic<T>(string scope, LogLevel level, string message, T value) {
        try {
            ITracorData tracorData;
#warning message
           
            TracorIdentitfier callee = new(TracorConsts.SourceTracorPublic, scope, message);

            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
                tracorData.TracorIdentitfier = callee;
            } else {
                tracorData = this._TracorDataConvertService.ConvertPublic(/*callee,*/ value);
            }
            tracorData.Timestamp = DateTime.UtcNow;

            if (value is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
                this._Publisher.OnTrace(true, tracorData);
                referenceCountObject.Dispose();
            } else {
                this._Publisher.OnTrace(true, tracorData);
                if (tracorData is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
}
