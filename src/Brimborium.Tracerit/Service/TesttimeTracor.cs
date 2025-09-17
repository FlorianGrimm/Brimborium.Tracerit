using Brimborium.Tracerit.Utility;

namespace Brimborium.Tracerit.Service;

/// <summary>
/// Test-time implementation of <see cref="ITracor"/> that enables full tracing functionality for testing scenarios.
/// This implementation processes trace data through validators and handles reference counting for objects.
/// </summary>
internal sealed class TesttimeTracor : ITracor {
    private readonly TesttimeTracorValidator _Validator;
    private readonly ILogger _Logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TesttimeTracor"/> class.
    /// </summary>
    /// <param name="tracorTestor">The validator used to process trace data.</param>
    /// <param name="logger">The logger for error reporting.</param>
    public TesttimeTracor(
        TesttimeTracorValidator tracorTestor,
        LazyCreatedLogger<TesttimeTracor> logger) {
        this._Validator = tracorTestor;
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
    public bool IsCurrentlyEnabled() => this._Validator.IsEnabled();

    /// <summary>
    /// Traces a value with the specified caller identifier, converting it to trace data and processing it through validators.
    /// </summary>
    /// <typeparam name="T">The type of the value being traced.</typeparam>
    /// <param name="callee">The identifier of the caller or trace point.</param>
    /// <param name="value">The value to be traced.</param>
    public void Trace<T>(TracorIdentitfier callee, T value) {
        try {
            if (value is IReferenceCountObject referenceCountObject) {
                referenceCountObject.IncrementReferenceCount();
            }
            if (value is not ITracorData tracorData) {
                tracorData = this._Validator.Convert(callee, value);
            }
            this._Validator.OnTrace(callee, tracorData);
            if (tracorData is IDisposable tracorDataDisposable) {
                tracorDataDisposable.Dispose();
            }
        } catch (Exception error) {
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
}
