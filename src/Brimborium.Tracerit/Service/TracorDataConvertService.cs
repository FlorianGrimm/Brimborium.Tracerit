namespace Brimborium.Tracerit.Service;

public sealed class TracorDataConvertService : ITracorDataConvertService {
    private readonly TracorDataRecordPool _TracorDataRecordPool;
    private bool _AllowReflection;

    public static TracorDataConvertService Create(IServiceProvider serviceProvider) {
        var options = serviceProvider.GetRequiredService<IOptions<TracorDataConvertOptions>>();
        return new TracorDataConvertService(
            serviceProvider,
            options);
    }

    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePrivate { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableDictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByTypePublic { get; set; } = ImmutableDictionary<Type, ITracorDataAccessorFactory>.Empty;
    public ImmutableArray<ITracorDataAccessorFactory> ListTracorDataAccessorPublic { get; set; } = ImmutableArray<ITracorDataAccessorFactory>.Empty;

    public ImmutableDictionary<Type, ITracorConvertObjectToListProperty> TracorConvertToListPropertyByType { get; set; } = ImmutableDictionary<Type, ITracorConvertObjectToListProperty>.Empty;
    private ImmutableDictionary<Type, ITracorConvertObjectToListProperty?> _CacheTracorConvertSelfToListProperty = ImmutableDictionary<Type, ITracorConvertObjectToListProperty?>.Empty;

    public TracorDataConvertService(
        TracorDataRecordPool tracorDataRecordPool
        ) {
        this._TracorDataRecordPool = tracorDataRecordPool;

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
        this.AddTracorConvertObjectToListProperty([
            new TracorConvertBoolValueToListProperty(),
            new TracorConvertByteValueToListProperty(),
            new TracorConvertDateOnlyValueToListProperty(),
            new TracorConvertDateTimeValueToListProperty(),
            new TracorConvertDateTimeOffsetValueToListProperty(),
            new TracorConvertDecimalValueToListProperty(),
            new TracorConvertDoubleValueToListProperty(),
            new TracorConvertFloatValueToListProperty(),
            new TracorConvertGuidValueToListProperty(),
            new TracorConvertIntValueToListProperty(),
            new TracorConvertLogLevelValueToListProperty(),
            new TracorConvertLongValueToListProperty(),
            new TracorConvertNintValueToListProperty(),
            new TracorConvertNuintValueToListProperty(),
            new TracorConvertSbyteValueToListProperty(),
            new TracorConvertShortValueToListProperty(),
            new TracorConvertStringValueToListProperty(),
            new TracorConvertTimeSpanValueToListProperty(),
            new TracorConvertUintValueToListProperty(),
            new TracorConvertUlongValueToListProperty(),
            new TracorConvertUshortValueToListProperty(),
            ]);
    }

    public TracorDataConvertService(
        IServiceProvider serviceProvider
        ) : this(
            tracorDataRecordPool: serviceProvider.GetService<TracorDataRecordPool>() ?? new(0)
        ) {
    }

    public TracorDataConvertService(
        IServiceProvider serviceProvider,
        Microsoft.Extensions.Options.IOptions<TracorDataConvertOptions> options
        ) : this(serviceProvider) {
        this.AddOptions(options.Value);
        this.AddTracorConvertObjectToListProperty(serviceProvider.GetServices<ITracorConvertObjectToListProperty>());
    }

    internal void AddOptions(TracorDataConvertOptions value) {
        this._AllowReflection = value.AllowReflection;
        if (0 < value.TracorDataAccessorByTypePrivate.Count) {
            foreach (var (type, tracorDataAccessorFactory) in value.TracorDataAccessorByTypePrivate) {
                this.AddTracorDataAccessorByTypePrivate(type, tracorDataAccessorFactory);
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
        if (value.ListTracorConvertObjectToListProperty is { Count: > 0 } listTracorConvertToListProperty) {
            this.AddTracorConvertObjectToListProperty(listTracorConvertToListProperty);
        }
    }

    public void AddTracorConvertObjectToListProperty(IEnumerable<ITracorConvertObjectToListProperty> listTracorConvertToListProperty) {
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

    public TracorDataConvertService AddTracorDataAccessorByTypePrivate<T>(ITracorDataAccessorFactory<T> factory) {
        this.TracorDataAccessorByTypePrivate = this.TracorDataAccessorByTypePrivate
            .SetItem(typeof(T), factory);
        return this;
    }
    public TracorDataConvertService AddTracorDataAccessorByTypePrivate(Type type, ITracorDataAccessorFactory factory) {
        this.TracorDataAccessorByTypePrivate = this.TracorDataAccessorByTypePrivate
            .SetItem(type, factory);
        return this;
    }

    public TracorDataConvertService AddTracorDataAccessorByTypePublic<T>(ITracorDataAccessorFactory<T> factory) {
        this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic
            .SetItem(typeof(T), factory);
        return this;
    }

    public TracorDataConvertService AddTracorDataAccessorByTypePublic(Type type, ITracorDataAccessorFactory factory) {
        this.TracorDataAccessorByTypePublic = this.TracorDataAccessorByTypePublic
            .SetItem(type, factory);
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
                return this.CreateTracorDataRecord();
            }
        }
        if (this.TryGetPrivateStrategy<T>(out var strategy)) {
            return strategy.Convert(false, callee, value);
        }
        if (0 < this.TracorDataAccessorByTypePrivate.Count) {
            if (this.TracorDataAccessorByTypePrivate.TryGetValue(type, out var tracorDataAccessorFactory)) {
                return this.SetStrategy<T>(false, true, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this))
                    .Convert(false, callee, value);
            }
        }
        if (0 < this.TracorDataAccessorByTypePublic.Count) {
            if (this.TracorDataAccessorByTypePublic.TryGetValue(type, out var tracorDataAccessorFactory)) {
                return this.SetStrategy<T>(false, true,
                    new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this))
                    .Convert(false, callee, value);
            }
        }
        if (0 < this.ListTracorDataAccessorPublic.Length) {
            foreach (var tracorDataAccessorFactory in this.ListTracorDataAccessorPublic) {
                if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped) {
                    if (tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                        this.SetStrategy<T>(false, true, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this));
                        return tracorDataTyped;
                    }
                }
                if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                    this.SetStrategy<T>(false, true, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this));
                    return tracorData;
                }
            }
        }

        {
            return this.SetStrategy<T>(true, true, new StrategyConvertValueToListProperty<T>(this))
                .Convert(false, callee, value);
        }

    }

    public ITracorData ConvertPublic<T>(TracorIdentifier callee, T value) {
        var type = typeof(T);
        if (type.IsClass) {
            if (value is null) {
                return this.CreateTracorDataRecord();
            }
        }
        if (this.TryGetPublicStrategy<T>(out var strategy)) {
            return strategy.Convert(true, callee, value);
        }
        if (0 < this.TracorDataAccessorByTypePublic.Count) {
            if (this.TracorDataAccessorByTypePublic.TryGetValue(type, out var tracorDataAccessorFactory)) {
                return this.SetStrategy<T>(true, false, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this))
                    .Convert(true, callee, value);
            }
        }
        if (0 < this.ListTracorDataAccessorPublic.Length) {
            foreach (var tracorDataAccessorFactory in this.ListTracorDataAccessorPublic) {
                if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped) {
                    if (tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                        this.SetStrategy<T>(true, false, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this));
                        return tracorDataTyped;
                    }
                } else if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                    this.SetStrategy<T>(true, false, new StrategyTracorDataAccessorFactory<T>(tracorDataAccessorFactory, this));
                    return tracorData;
                }
            }
        }

        {
            return this.SetStrategy<T>(true, false, new StrategyConvertValueToListProperty<T>(this))
                .Convert(true, callee, value);
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
        if (value is ITracorConvertSelfToListProperty tracorConvertSelfToListProperty) {
            tracorConvertSelfToListProperty.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
            return;
        }
        if (this.GetTracorConvertObjectToListProperty(value.GetType()) is { } converter) {
            converter.ConvertObjectToListProperty(isPublic, levelWatchDog, name, value, listProperty);
            return;
        }
    }

    public void ConvertValueToListProperty<T>(
        bool isPublic,
        int levelWatchDog,
        string name,
        T value,
        List<TracorDataProperty> listProperty) {
        if (value is ITracorConvertSelfToListProperty tracorConvertSelfToListProperty) {
            tracorConvertSelfToListProperty.ConvertSelfToListProperty(isPublic, levelWatchDog, name, listProperty);
            return;
        }
        if (this.GetConverterValueListProperty<T>() is { } converter) {
            converter.ConvertValueToListProperty(isPublic, levelWatchDog, name, value, listProperty);
            return;
        }
    }

    internal TracorDataRecord CreateTracorDataRecord() {
        var tracorData = this._TracorDataRecordPool.Rent();
        TracorDataUtility.SetActivity(tracorData.ListProperty);
        return tracorData;
    }

    private System.Collections.Immutable.ImmutableDictionary<Type, object> _PrivateStrategy = System.Collections.Immutable.ImmutableDictionary<Type, object>.Empty;
    private System.Collections.Immutable.ImmutableDictionary<Type, object> _PublicStrategy = System.Collections.Immutable.ImmutableDictionary<Type, object>.Empty;

    private bool TryGetPrivateStrategy<T>([MaybeNullWhen(false)] out Strategy<T> strategy) {
        if (this._PrivateStrategy.TryGetValue(typeof(T), out var strategyObj)
            && strategyObj is Strategy<T> strategyT) {
            strategy = strategyT;
            return true;
        }
        strategy = null;
        return false;
    }

    private bool TryGetPublicStrategy<T>([MaybeNullWhen(false)] out Strategy<T> strategy) {
        if (this._PublicStrategy.TryGetValue(typeof(T), out var strategyObj)
            && strategyObj is Strategy<T> strategyT) {
            strategy = strategyT;
            return true;
        }
        strategy = null;
        return false;
    }
    private Strategy<T> SetStrategy<T>(bool isPublic, bool isPrivate, Strategy<T> strategy) {
        if (isPublic) {
            this._PublicStrategy = this._PublicStrategy.SetItem(typeof(T), strategy);
        }
        if (isPrivate) {
            this._PrivateStrategy = this._PrivateStrategy.SetItem(typeof(T), strategy);
        }
        return strategy;
    }

    internal abstract class Strategy<T> {
        internal abstract ITracorData Convert(
            bool isPublic,
            TracorIdentifier callee,
            T value);
    }

    internal class StrategyTracorDataAccessorFactory<T>(
        ITracorDataAccessorFactory tracorDataAccessorFactory,
        TracorDataConvertService tracorDataConvertService
        ) : Strategy<T> {
        internal override ITracorData Convert(
            bool isPublic,
            TracorIdentifier callee,
            T value) {
            if (tracorDataAccessorFactory is ITracorDataAccessorFactory<T> tracorDataAccessorFactoryTyped) {
                if (tracorDataAccessorFactoryTyped.TryGetDataTyped(value, out var tracorDataTyped)) {
                    return tracorDataTyped;
                }
            }
            if (tracorDataAccessorFactory.TryGetData(value!, out var tracorData)) {
                return tracorData;
            }

            return tracorDataConvertService.CreateTracorDataRecord();
        }
    }

    internal class StrategyConvertValueToListProperty<T>(
        TracorDataConvertService tracorDataConvertService
        ) : Strategy<T> {

        internal override ITracorData Convert(
            bool isPublic,
            TracorIdentifier callee,
            T value) {
            var tracorData = tracorDataConvertService._TracorDataRecordPool.Rent();
            TracorDataUtility.SetActivity(tracorData.ListProperty);
            tracorDataConvertService.ConvertValueToListProperty(
                isPublic,
                1,
                "value",
                value,
                tracorData.ListProperty);
            return tracorData;
        }
    }
}
