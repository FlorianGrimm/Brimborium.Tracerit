namespace Brimborium.Tracerit;

public interface ITracorData {
    List<string> GetListPropertyName();
    bool TryGetPropertyValue(string propertyName, out object? propertyValue);
}

public interface ITracorData<TValue> : ITracorData {
    bool TryGetOriginalValue([MaybeNullWhen(false)] out TValue value);
}

public interface ITracorDataAccessorFactory {
    bool TryGetData(object value, [MaybeNullWhen(false)] out ITracorData tracorData);
}

public interface ITracorDataAccessorFactory<T> : ITracorDataAccessorFactory {
    bool TryGetDataTyped(T value, [MaybeNullWhen(false)] out ITracorData tracorData);
}

public interface ITracorDataAccessor {
    List<string> GetListPropertyName(object value);
    bool TryGetPropertyValueTyped(object value, string propertyName, out object? propertyValue);
}

public interface ITracorDataAccessor<T> {
    List<string> GetListPropertyNameTyped(T value);
    bool TryGetPropertyValueTyped(T value, string propertyName, out object? propertyValue);
}
