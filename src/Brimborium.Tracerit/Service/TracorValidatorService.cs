namespace Brimborium.Tracerit.Service;

public sealed class TracorValidatorService : IDisposable {
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private readonly ITracorValidator _TracorValidator;
    private readonly ILogger<TracorValidatorService> _Logger;
    private Timer? _Timer;
    private ImmutableArray<ITracorValidatorPath> _ListValidatorPath = ImmutableArray<ITracorValidatorPath>.Empty;
    private bool _EnableFinished;
    private TimeSpan _Period = TimeSpan.FromMinutes(1);

    public TracorValidatorService(
        TracorDataRecordPool tracorDataRecordPool,
        ITracorValidator tracorValidator,
        ILogger<TracorValidatorService> logger
        ) {
        this._TracorDataRecordPool = tracorDataRecordPool;
        this._TracorValidator = tracorValidator;
        this._Logger = logger;
    }

    public TracorValidatorService(
        TracorDataRecordPool tracorDataRecordPool,
        ITracorValidator tracorValidator,
        IOptions<TracorValidatorServiceOptions> options,
        ILogger<TracorValidatorService> logger
        ) :this(tracorDataRecordPool, tracorValidator, logger){
        this.SetOptions(options.Value);
    }

    public void SetOptions(TracorValidatorServiceOptions value) {
        if (value.EnableFinished is { } enableFinished) {
            if (this._EnableFinished != enableFinished) {
                this._EnableFinished = enableFinished;
                foreach (var validatorPath in this._ListValidatorPath) {
                    validatorPath.EnableFinished = enableFinished;
                }
            }
        }
        foreach (var step in value.ListValidator) {
            this.AddValidator(step);
        }
    }

    public void Init() { 
        this._Timer = new Timer(TimerLoop, null, this._Period, this._Period);
    }

    private void TimerLoop(object? state) {
        using (var tick = this._TracorDataRecordPool.Rent()) {
            tick.TracorIdentifier = new TracorIdentifier("Timer", "Timer", "Tick");
            tick.Timestamp = DateTime.UtcNow;
            this._TracorValidator.OnTrace(true, tick);
        }
    }

    public ITracorValidatorPath AddValidator(IValidatorExpression step, List<TracorDataProperty>? globalStateValue = default) {
        if (this._TracorValidator.GetExisting(step) is { } result) {
            return result;
        }
        result = this._TracorValidator.Add(step, globalStateValue);
        this._ListValidatorPath = this._ListValidatorPath.Add(result);
        result.AddFinishCallback((validator, state) => {
            //TODO: handle
        });
        result.EnableFinished = this._EnableFinished;
        return result;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken) {

        await Task.CompletedTask;
    }

    public void Dispose() {
        using (var timer = this._Timer) {
            this._Timer = null;
        }
    }
}
