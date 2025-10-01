namespace Brimborium.Tracerit.Service;

public sealed class TesttimeTracorValidator : ITracorValidator {

    public static TesttimeTracorValidator Create(IServiceProvider serviceProvider) {
        var options = serviceProvider.GetRequiredService<IOptions<TracorValidatorOptions>>();
        var activityTracorDataPool = serviceProvider.GetRequiredService<ActivityTracorDataPool>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        return new TesttimeTracorValidator(options, activityTracorDataPool, loggerFactory);
    }

    private ImmutableArray<ITracorValidatorPath> _ListValidatorPath = ImmutableArray<ITracorValidatorPath>.Empty;
    private readonly Lock _LockListTracorStepPath = new();
    private readonly ILoggerFactory _LoggerFactory;
    private readonly ActivityTracorDataPool _ActivityTracorDataPool;
    private ILogger? _LoggerCondition;
    private LoggerUtility? _LoggerUtility;

    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeNoSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateNoScopeSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;

    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;

    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessorPublic { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;

    public TesttimeTracorValidator(ActivityTracorDataPool activityTracorDataPool, ILoggerFactory loggerFactory) {
        this._ActivityTracorDataPool = activityTracorDataPool;
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<string>());
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<int>());
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<bool>());
        this.AddTracorDataAccessorByTypePublic(new BoundAccessorTracorDataFactory<Uri>(new SystemUriTracorDataAccessor()));
        this.AddTracorDataAccessorByTypePublic(new JsonDocumentTracorDataFactory());
        this.AddTracorDataAccessorByTypePublic(new ActivityTracorDataFactory(activityTracorDataPool));
        this.AddTracorDataAccessorByTypePublic(new LoggerTracorDataFactory());
        this._LoggerFactory = loggerFactory;
    }

    public TesttimeTracorValidator(
        Microsoft.Extensions.Options.IOptions<TracorValidatorOptions> options,
        ActivityTracorDataPool activityTracorDataPool,
        ILoggerFactory loggerFactory
        ) : this(activityTracorDataPool, loggerFactory) {
        this.AddOptions(options.Value);
    }

    public TesttimeTracorValidator AddTracorDataAccessorByTypePrivate(TracorIdentitfierType tracorIdentitfierType, ITracorDataAccessorFactory tracorDataAccessorFactory) {
        bool isSourceEmpty = string.IsNullOrEmpty(tracorIdentitfierType.Source);
        bool isScopeEmpty = string.IsNullOrEmpty(tracorIdentitfierType.Scope);
        if (!isScopeEmpty && isSourceEmpty) {
            this.TracorDataAccessorByTypePrivateScopeNoSource = this.TracorDataAccessorByTypePrivateScopeNoSource.SetItem(tracorIdentitfierType, tracorDataAccessorFactory);

        } else if (isScopeEmpty && !isScopeEmpty) {
            this.TracorDataAccessorByTypePrivateNoScopeSource = this.TracorDataAccessorByTypePrivateNoScopeSource.SetItem(tracorIdentitfierType, tracorDataAccessorFactory);

        } else if (!isSourceEmpty && !isScopeEmpty) {
            this.TracorDataAccessorByTypePrivateScopeSource = this.TracorDataAccessorByTypePrivateScopeSource.SetItem(tracorIdentitfierType, tracorDataAccessorFactory);

        } else {
            // seam like a leak
            // this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic.SetItem(tracorIdentitfierType.TypeParameter, tracorDataAccessorFactory);
            throw new ArgumentException("Source and Scope is empty.", nameof(tracorIdentitfierType));
        }
        return this;
    }

    public TesttimeTracorValidator AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> factory) {
        this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic.SetItem(typeof(T), factory);
        return this;
    }

    public TesttimeTracorValidator AddListTracorDataAccessor(ITracorDataAccessorFactory factory) {
        this.ListTracorDataAccessorPublic = this.ListTracorDataAccessorPublic.Add(factory);
        return this;
    }

    public void AddOptions(TracorValidatorOptions value) {
        if (0 < value.TracorDataAccessorByTypePrivate.Count) {
            foreach (var (tracorIdentitfierType, tracorDataAccessorFactory) in value.TracorDataAccessorByTypePrivate) {
                this.AddTracorDataAccessorByTypePrivate(tracorIdentitfierType, tracorDataAccessorFactory);
            }
        }

        if (0 < value.TracorDataAccessorByTypePublic.Count) {
            this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic
                .SetItems(value.TracorDataAccessorByTypePublic);
        }

        if (0 < value.ListTracorDataAccessor.Count) {
            this.ListTracorDataAccessorPublic = this.ListTracorDataAccessorPublic
                .AddRange(value.ListTracorDataAccessor);
        }
    }

    public bool IsEnabled() {
        return 0 < this._ListValidatorPath.Length;
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

    public ITracorData ConvertPrivate<T>(TracorIdentitfier callee, T value) {
        var type = typeof(T);
        if (type.IsClass) {
            if (value is null) {
                return new NullTypeData();
            }
        }
        if (0 < this.TracorDataAccessorByTypePrivateScopeSource.Count) {
            TracorIdentitfierType tracorIdentitfierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateScopeSource.TryGetValue(tracorIdentitfierType, out var tracorDataAccessorFactory)) {
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
        if (0 < this.TracorDataAccessorByTypePrivateScopeNoSource.Count) {
            TracorIdentitfierType tracorIdentitfierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateScopeNoSource.TryGetValue(tracorIdentitfierType, out var tracorDataAccessorFactory)) {
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
        if (0 < this.TracorDataAccessorByTypePrivateNoScopeSource.Count) {
            TracorIdentitfierType tracorIdentitfierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateNoScopeSource.TryGetValue(tracorIdentitfierType, out var tracorDataAccessorFactory)) {
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
        if (0 < this.TracorDataAccessorByTypePublic.Count) {
            if (this.TracorDataAccessorByTypePublic.TryGetValue(type, out var tracorDataAccessorFactory)) {
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
        if (0 < this.ListTracorDataAccessorPublic.Length) {
            foreach (var tracorDataAccessorFactory in this.ListTracorDataAccessorPublic) {
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

    public ITracorData ConvertPublic<T>(T value) {
        var type = typeof(T);
        if (type.IsClass) {
            if (value is null) {
                return new NullTypeData();
            }
        }
        if (0 < this.TracorDataAccessorByTypePublic.Count) {
            if (this.TracorDataAccessorByTypePublic.TryGetValue(type, out var tracorDataAccessorFactory)) {
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
        if (0 < this.ListTracorDataAccessorPublic.Length) {
            foreach (var tracorDataAccessorFactory in this.ListTracorDataAccessorPublic) {
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
