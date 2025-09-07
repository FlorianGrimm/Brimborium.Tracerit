#pragma warning disable IDE0301 // Simplify collection initialization
namespace Brimborium.Tracerit.Service;

public sealed class TesttimeTracorValidator : ITracorValidator {
    private ImmutableArray<ITracorValidatorPath> _ListValidatorPath = ImmutableArray<ITracorValidatorPath>.Empty;
    private readonly Lock _LockListTracorStepPath = new();

    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByType { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessor { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;

    public TesttimeTracorValidator() {
        this.AddTracorDataAccessorByType(new ValueAccessorFactory<string>());
        this.AddTracorDataAccessorByType(new ValueAccessorFactory<int>());
        this.AddTracorDataAccessorByType(new ValueAccessorFactory<bool>());
        this.AddTracorDataAccessorByType(new TracorDataAccessorFactory<Uri>(new SystemUriTracorDataAccessor()));
        this.AddTracorDataAccessorByType(new JsonDocumentTracorDataFactor());

    }

    public TesttimeTracorValidator(
        Microsoft.Extensions.Options.IOptions<TracorValidatorOptions> options
        ) :this() {
        this.AddOptions(options.Value);
    }

    public TesttimeTracorValidator AddTracorDataAccessorByType<T>(ITracorDataAccessorFactory<T> factory) {
        this.TracorDataAccessorByType.SetItem(typeof(T), factory);
        return this;
    }

    public TesttimeTracorValidator AddListTracorDataAccessor(ITracorDataAccessorFactory factory) {
        this.ListTracorDataAccessor.Add(factory);
        return this;
    }

    public void AddOptions(TracorValidatorOptions value) {
        if (0 < value.TracorDataAccessorByType.Count) {
            this.TracorDataAccessorByType = this.TracorDataAccessorByType
                .SetItems(value.TracorDataAccessorByType);
        }
        if (0 < value.ListTracorDataAccessor.Count) {
            this.ListTracorDataAccessor = this.ListTracorDataAccessor
                .AddRange(value.ListTracorDataAccessor);
        }
    }

    public bool IsEnabled() {
        return 0 < this._ListValidatorPath.Length;
    }

    public ITracorValidatorPath Add(IValidatorExpression step, TracorGlobalState? globalState) {
        using (this._LockListTracorStepPath.EnterScope()) {
            TracorValidatorPathRemover remover = new(this);
            var result = new TracorValidatorPath(step, globalState, remover);
            remover.Child = result;
            this._ListValidatorPath = this._ListValidatorPath.Add(result);
            return result;
        }
    }

    private sealed class TracorValidatorPathRemover : IDisposable {
        private readonly TesttimeTracorValidator? _Owner;
        public TracorValidatorPath? Child;

        public TracorValidatorPathRemover(TesttimeTracorValidator owner) {
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

    public ITracorData Convert<T>(TracorIdentitfier callee, T value) {
        var type = typeof(T);
        if (type.IsClass) {
            if (value is null) {
                return new NullTypeData();
            }
        }
        {
            if (this.TracorDataAccessorByType.TryGetValue(type, out var tracorDataAccessorFactory)) {
                if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped) {
                    if (tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                        return tracorDataTyped;
                    }
                }
                if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                    return tracorData;
                }
            }
        }
        {
            foreach (var tracorDataAccessorFactory in this.ListTracorDataAccessor) {
                if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped) {
                    if (tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                        return tracorDataTyped;
                    }
                }
                if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                    return tracorData;
                }
            }
        }
        return new ValueTracorData<T>(value);
    }

    public void OnTrace(TracorIdentitfier callee, ITracorData tracorData) {
        foreach (var validatorPath in this._ListValidatorPath) {
            validatorPath.OnTrace(callee, tracorData);
        }
    }
}
