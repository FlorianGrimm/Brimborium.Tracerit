namespace Brimborium.Tracerit.Service;

public sealed class TracorDataConvertService : ITracorDataConvertService {
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private readonly LoggerTracorDataPool _LoggerTracorDataPool;
    private bool _AllowReflection;

    public static TracorDataConvertService Create(IServiceProvider serviceProvider) {
        var options = serviceProvider.GetRequiredService<IOptions<TracorDataConvertOptions>>();
        return new TracorDataConvertService(
            serviceProvider,
            options);
    }

    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeNoSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateNoScopeSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivateScopeSource { get; set; } = ImmutableDictionary<TracorIdentifierType, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessorPublic { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;

    public ImmutableDictionary<Type, ITracorConvertObjectToListProperty> TracorConvertToListPropertyByType { get; set; } = ImmutableDictionary<Type, ITracorConvertObjectToListProperty>.Empty;
    private ImmutableDictionary<Type, ITracorConvertObjectToListProperty?> _CacheTracorConvertSelfToListProperty = ImmutableDictionary<Type, ITracorConvertObjectToListProperty?>.Empty;

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
        this.AddTracorConvertToListProperty(serviceProvider.GetServices<ITracorConvertObjectToListProperty>());
    }

    internal void AddOptions(TracorDataConvertOptions value) {
        this._AllowReflection = value.AllowReflection;
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
        if (value.ListTracorConvertToListProperty is { Count: > 0 } listTracorConvertToListProperty) {
            this.AddTracorConvertToListProperty(listTracorConvertToListProperty);
        }
    }

    public void AddTracorConvertToListProperty(IEnumerable<ITracorConvertObjectToListProperty> listTracorConvertToListProperty) {
        if (!listTracorConvertToListProperty.Any()) { return; }

        var builder = this.TracorConvertToListPropertyByType.ToBuilder();
        foreach (var tracorConvertToListProperty in listTracorConvertToListProperty) {
            //var valueType = tracorConvertToListProperty
            //    .GetType()
            //    .GetInterfaces()
            //    .FirstOrDefault(typeInterface => typeInterface.IsGenericType
            //            && typeInterface.GetGenericTypeDefinition() == typeof(ITracorConvertValueToListProperty<>))
            //    ?.GetGenericArguments()
            //    .First();
            var valueType = tracorConvertToListProperty.GetValueType();
            if (valueType is { }) {
                builder[valueType] = tracorConvertToListProperty;
            }
        }
        this.TracorConvertToListPropertyByType = builder.ToImmutable();
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
            if (TracorDataPropertyTypeValue.Any != tracorDataProperty.TypeValue) {
                var tracorData = this._TracorDataRecordPool.Rent();
                tracorData.ListProperty.Add(tracorDataProperty);
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            if (this.GetConverterValueListProperty<T>() is { } converter) {
                var tracorData = this._TracorDataRecordPool.Rent();
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                converter.ConvertValueToListProperty(false, 1, string.Empty, value, tracorData.ListProperty);
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
            if (TracorDataPropertyTypeValue.Any != tracorDataProperty.TypeValue) {
                var tracorData = this._TracorDataRecordPool.Rent();
                tracorData.ListProperty.Add(tracorDataProperty);
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            if (this.GetConverterValueListProperty<T>() is { } converter) {
                var tracorData = this._TracorDataRecordPool.Rent();
                TracorDataUtility.SetActivity(tracorData.ListProperty);
                converter.ConvertValueToListProperty(true, 1, string.Empty, value, tracorData.ListProperty);
                return tracorData;
            }
        }
        {
            var tracorData = this._TracorDataRecordPool.Rent();
            TracorDataUtility.SetActivity(tracorData.ListProperty);
            if (this._AllowReflection) {
                ValueAccessorFactoryUtility.Convert<T>(value!, tracorData.ListProperty);
            }
            return tracorData;
        }
    }

    public ITracorConvertObjectToListProperty? GetTracorConvertObjectToListProperty(Type typeValue) {
        {
            if (this.TracorConvertToListPropertyByType.TryGetValue(
                typeValue,
                out var result)
                ) {
                return result;
            }
        }
        {
            if (this._CacheTracorConvertSelfToListProperty.TryGetValue(typeValue, out var result)) {
                return result;
            }
            bool isITracorConvertSelfToListProperty = typeValue.IsAssignableTo(typeof(ITracorConvertSelfToListProperty));
            if (isITracorConvertSelfToListProperty) {
                var resultT = (ITracorConvertObjectToListProperty)
                    typeof(TracorConvertSelfToListPropertyAdapter<>)
                    .MakeGenericType(typeValue)
                    .GetConstructor(Type.EmptyTypes)!
                    .Invoke(null);
                this._CacheTracorConvertSelfToListProperty = this._CacheTracorConvertSelfToListProperty.SetItem(typeValue, resultT);
                this.TracorConvertToListPropertyByType = this.TracorConvertToListPropertyByType.SetItem(typeValue, resultT);
                return resultT;
            } else {
                this._CacheTracorConvertSelfToListProperty = this._CacheTracorConvertSelfToListProperty.SetItem(typeValue, null);
            }
        }
        return null;
    }

    public ITracorConvertValueToListProperty<T>? GetConverterValueListProperty<T>() {
        {
            if (this.TracorConvertToListPropertyByType.TryGetValue(
                typeof(T),
                out var result)
                && (result is ITracorConvertValueToListProperty<T> resultT)) {
                return resultT;
            }
        }
        {
            if (this._CacheTracorConvertSelfToListProperty.TryGetValue(typeof(T), out var result)) {
                if (result is null) { return null; /* normally */ }
                return result as ITracorConvertValueToListProperty<T>; /* might happen - race */
            }
            bool isITracorConvertSelfToListProperty = typeof(T).IsAssignableTo(typeof(ITracorConvertSelfToListProperty));
            if (isITracorConvertSelfToListProperty) {
                var resultT = (ITracorConvertValueToListProperty<T>)
                    typeof(TracorConvertSelfToListPropertyAdapter<>)
                    .MakeGenericType(typeof(T))
                    .GetConstructor(Type.EmptyTypes)!
                    .Invoke(null);
                this._CacheTracorConvertSelfToListProperty = this._CacheTracorConvertSelfToListProperty.SetItem(typeof(T), resultT);
                this.TracorConvertToListPropertyByType = this.TracorConvertToListPropertyByType.SetItem(typeof(T), resultT);
                return resultT;
            } else {
                this._CacheTracorConvertSelfToListProperty = this._CacheTracorConvertSelfToListProperty.SetItem(typeof(T), null);
            }
        }
        return null;
    }

    public void ConvertObjectToListProperty(
           bool isPublic,
           int levelWatchDog,
           string name,
           object? value,
           List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (this.GetTracorConvertObjectToListProperty(value.GetType()) is { } converter) {
            converter.ConvertObjectToListProperty(isPublic, levelWatchDog, name, value, listProperty);
        }
    }

    public void ConvertValueToListProperty<T>(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value,
        List<TracorDataProperty> listProperty) {
        if (this.GetConverterValueListProperty<T>() is { } converter) {
            converter.ConvertValueToListProperty(isPublic, levelWatchDog, name, value, listProperty);
        }
    }
}

internal sealed class TracorConvertSelfToListPropertyAdapter<T>
    : ITracorConvertValueToListProperty<T>
    where T : ITracorConvertSelfToListProperty {
    public TracorConvertSelfToListPropertyAdapter() { }

    public Type GetValueType() => typeof(T);

    public void ConvertObjectToListProperty(bool isPublic, int levelWatchDog, string name, object? value, List<TracorDataProperty> listProperty) {
        if (value is null) { return; }
        if (value is T valueT) {
            valueT.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
        }
    }

    public void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, T value, List<TracorDataProperty> listProperty) {
        value.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
    }
}