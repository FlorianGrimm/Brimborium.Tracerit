namespace Brimborium.Tracerit.Service;

/// <summary>
/// Validates trace data against defined expressions and manages validation paths.
/// Used primarily for testing scenarios to verify expected trace sequences.
/// Also implements <see cref="ITracorCollectiveSink"/> to receive trace events.
/// </summary>
public sealed class TracorValidator : ITracorValidator {
    private ImmutableArray<ITracorValidatorPath> _ListValidatorPath = ImmutableArray<ITracorValidatorPath>.Empty;
    private readonly Lock _LockListTracorStepPath = new();
    private readonly ILoggerFactory _LoggerFactory;
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private ILogger? _LoggerCondition;
    private LoggerUtility? _LoggerUtility;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorValidator"/> class.
    /// </summary>
    /// <param name="tracorDataRecordPool">The pool for trace data records.</param>
    /// <param name="loggerFactory">The logger factory for creating condition loggers.</param>
    [Microsoft.Extensions.DependencyInjection.ActivatorUtilitiesConstructor]
    public TracorValidator(
        TracorDataRecordPool tracorDataRecordPool,
        ILoggerFactory loggerFactory) {
        this._TracorDataRecordPool = tracorDataRecordPool;
        this._LoggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    public bool IsGeneralEnabled() => true;

    /// <inheritdoc />
    public bool IsEnabled() {
        return 0 < this._ListValidatorPath.Length;
    }

    /// <inheritdoc />
    public ITracorValidatorPath? GetExisting(IValidatorExpression step) {
        foreach (var validatorPath in this._ListValidatorPath) {
            if (ReferenceEquals(step, validatorPath.Step)) {
                return validatorPath;
            }
        }
        return default;
    }

    /// <inheritdoc />
    public ITracorValidatorPath Add(IValidatorExpression step, List<TracorDataProperty>? globalState = default) {
        if (this._LoggerCondition is null || this._LoggerUtility is null) {
            this._LoggerCondition ??= this._LoggerFactory.CreateLogger(typeof(AlwaysCondition).Namespace!);
            this._LoggerUtility ??= new LoggerUtility(this._LoggerCondition);
        }
        using (this._LockListTracorStepPath.EnterScope()) {
            TracorValidatorPathRemover remover = new(this);
            var result = new TracorValidatorPath(step, globalState, remover, this._LoggerUtility);
            remover.Child = result;
            this._ListValidatorPath = this._ListValidatorPath.Add(result);
            return result;
        }
    }

    private sealed class TracorValidatorPathRemover : IDisposable {
        private readonly TracorValidator? _Owner;
        public TracorValidatorPath? Child;

        public TracorValidatorPathRemover(TracorValidator owner) {
            this._Owner = owner;
        }

        public void Dispose() {
            if (this._Owner is { } owner
                && this.Child is { } child
                ) {
                using (owner._LockListTracorStepPath.EnterScope()) {
                    owner._ListValidatorPath = owner._ListValidatorPath.Remove(child);
                }
            }
        }
    }

    /// <inheritdoc />
    public void OnTrace(bool isPublic, ITracorData tracorData) {
        // no need for checking isPublic
        foreach (var validatorPath in this._ListValidatorPath) {
            validatorPath.OnTrace(tracorData);
        }
    }
}
