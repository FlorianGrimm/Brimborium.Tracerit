namespace Brimborium.Tracerit.Service;

public class TracorDataConvertService : ITracorDataConvertService {
    public static TracorDataConvertService Create(IServiceProvider serviceProvider) {
        var options = serviceProvider.GetRequiredService<IOptions<TracorDataConvertOptions>>();
        var activityTracorDataPool = serviceProvider.GetRequiredService<ActivityTracorDataPool>();
        
        return new TracorDataConvertService(
            activityTracorDataPool,
            options);
    }


    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeNoSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateNoScopeSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeSource { get; set; } = ImmutableDictionary<TracorIdentitfierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessorPublic { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;


    public TracorDataConvertService(
        ActivityTracorDataPool activityTracorDataPool
        ) {
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<string>());
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<int>());
        this.AddTracorDataAccessorByTypePublic(new ValueAccessorFactory<bool>());
        this.AddTracorDataAccessorByTypePublic(new BoundAccessorTracorDataFactory<Uri>(new SystemUriTracorDataAccessor()));
        this.AddTracorDataAccessorByTypePublic(new JsonDocumentTracorDataFactory());
        this.AddTracorDataAccessorByTypePublic(new ActivityTracorDataFactory(activityTracorDataPool));
        this.AddTracorDataAccessorByTypePublic(new LoggerTracorDataFactory());
    }

    public TracorDataConvertService(
        ActivityTracorDataPool activityTracorDataPool,
        Microsoft.Extensions.Options.IOptions<TracorDataConvertOptions> options
        ) : this(activityTracorDataPool) {
        this.AddOptions(options.Value);
    }

    internal void AddOptions(TracorDataConvertOptions value) {
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

    public TracorDataConvertService AddTracorDataAccessorByTypePrivate(TracorIdentitfierType tracorIdentitfierType, ITracorDataAccessorFactory tracorDataAccessorFactory) {
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



    public TracorDataConvertService AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> factory) {
        this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic.SetItem(typeof(T), factory);
        return this;
    }

    public TracorDataConvertService AddListTracorDataAccessor(ITracorDataAccessorFactory factory) {
        this.ListTracorDataAccessorPublic = this.ListTracorDataAccessorPublic.Add(factory);
        return this;
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


}