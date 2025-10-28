namespace Brimborium.Tracerit.Service;

public class TracorDataConvertService : ITracorDataConvertService {
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private readonly LoggerTracorDataPool _LoggerTracorDataPool;

    public static TracorDataConvertService Create(IServiceProvider serviceProvider) {
        var options = serviceProvider.GetRequiredService<IOptions<TracorDataConvertOptions>>();
        //var activityTracorDataPool = serviceProvider.GetRequiredService<ActivityTracorDataPool>();

        return new TracorDataConvertService(
            serviceProvider,
            options);
    }

    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeNoSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateNoScopeSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessorPublic { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;

    public TracorDataConvertService(
        TracorDataRecordPool tracorDataRecordPool,
        LoggerTracorDataPool loggerTracorDataPool
        ) {
        this._TracorDataRecordPool = tracorDataRecordPool;
        this._LoggerTracorDataPool = loggerTracorDataPool;

        this.AddTracorDataAccessorByTypePublic(new BoundAccessorTracorDataFactory<Uri>(new SystemUriTracorDataAccessor(), tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new JsonDocumentTracorDataFactory());
        //this.AddTracorDataAccessorByTypePublic(new LoggerTracorDataFactory());

        //this.AddTracorDataAccessorByTypePublic(new ActivityTracorDataFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataStringValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataBoolValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataIntValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataLongValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataFloatValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataDoubleValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataDateTimeValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataDateTimeOffsetValueAccessorFactory(tracorDataRecordPool));
        //this.AddTracorDataAccessorByTypePublic(new TracorDataUuidValueAccessorFactory(tracorDataRecordPool));
    }

    public TracorDataConvertService(
        IServiceProvider serviceProvider
        ) : this(
            tracorDataRecordPool: serviceProvider.GetService<TracorDataRecordPool>() ?? new(0),
            loggerTracorDataPool: serviceProvider.GetService<LoggerTracorDataPool>() ?? new(0)
        ) {
    }

    public TracorDataConvertService(
        IServiceProvider serviceProvider,
        Microsoft.Extensions.Options.IOptions<TracorDataConvertOptions> options
        ) : this(serviceProvider) {
        this.AddOptions(options.Value);
    }

    internal void AddOptions(TracorDataConvertOptions value) {
        if (0 < value.TracorDataAccessorByTypePrivate.Count) {
            foreach (var (tracorIdentifierType, tracorDataAccessorFactory) in value.TracorDataAccessorByTypePrivate) {
                this.AddTracorDataAccessorByTypePrivate(tracorIdentifierType, tracorDataAccessorFactory);
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

    public TracorDataConvertService AddTracorDataAccessorByTypePrivate(TracorIdentifierType tracorIdentifierType, ITracorDataAccessorFactory tracorDataAccessorFactory) {
        bool isSourceEmpty = string.IsNullOrEmpty(tracorIdentifierType.Source);
        bool isScopeEmpty = string.IsNullOrEmpty(tracorIdentifierType.Scope);
        if (!isScopeEmpty && isSourceEmpty) {
            this.TracorDataAccessorByTypePrivateScopeNoSource = this.TracorDataAccessorByTypePrivateScopeNoSource.SetItem(tracorIdentifierType, tracorDataAccessorFactory);

        } else if (isScopeEmpty && !isScopeEmpty) {
            this.TracorDataAccessorByTypePrivateNoScopeSource = this.TracorDataAccessorByTypePrivateNoScopeSource.SetItem(tracorIdentifierType, tracorDataAccessorFactory);

        } else if (!isSourceEmpty && !isScopeEmpty) {
            this.TracorDataAccessorByTypePrivateScopeSource = this.TracorDataAccessorByTypePrivateScopeSource.SetItem(tracorIdentifierType, tracorDataAccessorFactory);

        } else {
            // seam like a leak
            // this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic.SetItem(tracorIdentifierType.TypeParameter, tracorDataAccessorFactory);
            throw new ArgumentException("Source and Scope is empty.", nameof(tracorIdentifierType));
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

    public ITracorData ConvertPrivate<T>(TracorIdentifier callee, T value) {
        var type = typeof(T);
        if (type.IsClass) {
            if (value is null) {
                return new NullTypeData();
            }
        }
        if (0 < this.TracorDataAccessorByTypePrivateScopeSource.Count) {
            TracorIdentifierType tracorIdentifierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateScopeSource.TryGetValue(tracorIdentifierType, out var tracorDataAccessorFactory)) {
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
            TracorIdentifierType tracorIdentifierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateScopeNoSource.TryGetValue(tracorIdentifierType, out var tracorDataAccessorFactory)) {
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
            TracorIdentifierType tracorIdentifierType = new(callee.Source, callee.Scope, type);
            if (this.TracorDataAccessorByTypePrivateNoScopeSource.TryGetValue(tracorIdentifierType, out var tracorDataAccessorFactory)) {
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

        {
            if (value is ITracorDataSelfAccessor tracorDataSelfAccessor) {
                var tracorData = this._TracorDataRecordPool.Rent();
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                tracorDataSelfAccessor.ConvertProperties(tracorData.ListProperty);
                return tracorData;
            }
        }

        {
            var tracorDataProperty = TracorDataProperty.Create("value", value);
            if (TracorDataPropertyTypeValue.Any == tracorDataProperty.TypeValue) {
                var tracorData = this._TracorDataRecordPool.Rent();
                tracorData.ListProperty.Add(tracorDataProperty);
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            var tracorData = this._TracorDataRecordPool.Rent();
            TracorDataUtility.SetActivity(tracorData.ListProperty);
            ValueAccessorFactoryUtility.Convert<T>(value!, tracorData.ListProperty);
            return tracorData;
        }
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
                if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped
                    && tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                    return tracorDataTyped;
                } else if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
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
                } else if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                    return tracorData;
                }
            }
        }
        {
            if (value is ITracorDataSelfAccessor tracorDataSelfAccessor) {
                var tracorData = this._TracorDataRecordPool.Rent();
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                tracorDataSelfAccessor.ConvertProperties(tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            var tracorDataProperty = TracorDataProperty.Create("value", value);
            if (TracorDataPropertyTypeValue.Any == tracorDataProperty.TypeValue) {
                var tracorData = this._TracorDataRecordPool.Rent();
                tracorData.ListProperty.Add(tracorDataProperty);
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            var tracorData = this._TracorDataRecordPool.Rent();
            TracorDataUtility.SetActivity(tracorData.ListProperty);
            ValueAccessorFactoryUtility.Convert<T>(value!, tracorData.ListProperty);
            return tracorData;
        }
    }
}