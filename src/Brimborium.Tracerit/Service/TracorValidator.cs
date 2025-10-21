﻿namespace Brimborium.Tracerit.Service;

public sealed class TracorValidator : ITracorValidator {

    public static TracorValidator Create(IServiceProvider serviceProvider) {
        var activityTracorDataPool = serviceProvider.GetRequiredService<ActivityTracorDataPool>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return new TracorValidator(
            activityTracorDataPool,
            loggerFactory);
    }

    private ImmutableArray<ITracorValidatorPath> _ListValidatorPath = ImmutableArray<ITracorValidatorPath>.Empty;
    private readonly Lock _LockListTracorStepPath = new();
    private readonly ILoggerFactory _LoggerFactory;
    private readonly ActivityTracorDataPool _ActivityTracorDataPool;
    private ILogger? _LoggerCondition;
    private LoggerUtility? _LoggerUtility;


    public TracorValidator(
        ActivityTracorDataPool activityTracorDataPool,
        ILoggerFactory loggerFactory) {
        this._ActivityTracorDataPool = activityTracorDataPool;
        this._LoggerFactory = loggerFactory;
    }

    public bool IsGeneralEnabled() => true;

    public bool IsEnabled() {
        return 0 < this._ListValidatorPath.Length;
    }

    public ITracorValidatorPath? GetExisting(IValidatorExpression step) {
        foreach (var validatorPath in this._ListValidatorPath) {
            if (ReferenceEquals(step, validatorPath.Step)) {
                return validatorPath;
            }
        }
        return default;
    }

    public ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState) {
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

    public void OnTrace(bool isPublic, ITracorData tracorData) {
        // no need for checking isPublic
        foreach (var validatorPath in this._ListValidatorPath) {
            validatorPath.OnTrace(tracorData);
        }
    }
}
