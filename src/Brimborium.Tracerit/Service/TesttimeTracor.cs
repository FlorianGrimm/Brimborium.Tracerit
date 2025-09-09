namespace Brimborium.Tracerit.Service;

internal sealed class TesttimeTracor : ITracor {
    private readonly TesttimeTracorValidator _Validator;
    private readonly ILogger _Logger;

    public TesttimeTracor(
        TesttimeTracorValidator tracorTestor,
        LazyCreatedLogger<TesttimeTracor> logger) {
        this._Validator = tracorTestor;
        this._Logger = logger;
    }
    public bool IsGeneralEnabled() => true;

    public bool IsCurrentlyEnabled() => this._Validator.IsEnabled();

    public void Trace<T>(TracorIdentitfier callee, T value) {
        try {
            if (value is not ITracorData tracorData) {
                tracorData = this._Validator.Convert(callee, value);
            }
            this._Validator.OnTrace(callee, tracorData);
        } catch (Exception error){
            this._Logger.LogError(exception: error, message: "Trace Failed");
        }
    }
}
