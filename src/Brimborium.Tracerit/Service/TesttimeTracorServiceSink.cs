namespace Brimborium.Tracerit.Service;

/// <summary>
/// Test-time implementation of <see cref="ITracorServiceSink"/> that enables full tracing functionality for testing scenarios.
/// This implementation processes trace data through validators and handles reference counting for objects.
/// </summary>
internal sealed class TesttimeTracorServiceSink : ITracorServiceSink {
    private readonly ITracorCollectivePublisher _Publisher;
    private readonly ITracorDataConvertService _TracorDataConvertService;
    private readonly ILogger _Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TesttimeTracorServiceSink"/> class.
    /// </summary>
    /// <param name="tracorValidator">The validator used to process trace data.</param>
    /// <param name="logger">The logger for error reporting.</param>
    public TesttimeTracorServiceSink(
        ITracorCollectivePublisher publisher,
        ITracorDataConvertService tracorDataConvertService,
        LazyCreatedLogger<TesttimeTracorServiceSink> logger) {
        this._Publisher = publisher;
        this._TracorDataConvertService = tracorDataConvertService;
        this._Logger = logger;
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
#warning message
            TracorIdentitfier callee = new(TracorConsts.SourceTracorPrivate, scope);
            ITracorData tracorData;
            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
            } else {
                tracorData = this._TracorDataConvertService.ConvertPrivate(callee, value);
            }

            if (value is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
                this._Publisher.OnTrace(false, callee, tracorData);
                referenceCountObject.Dispose();
            } else {
                this._Publisher.OnTrace(false, callee, tracorData);
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
            TracorIdentitfier callee = new(TracorConsts.SourceTracorPublic, scope);
            if (value is ITracorData valueTracorData) {
                tracorData = valueTracorData;
            } else {
                tracorData = this._TracorDataConvertService.ConvertPublic(/*callee,*/ value);
            }

            if (value is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
                this._Publisher.OnTrace(true, callee, tracorData);
                referenceCountObject.Dispose();
            } else {
                this._Publisher.OnTrace(true, callee, tracorData);
                if (tracorData is IDisposable disposable) {
                    disposable.Dispose();
                }
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
}
