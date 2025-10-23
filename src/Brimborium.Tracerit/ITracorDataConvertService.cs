namespace Brimborium.Tracerit;

public interface ITracorDataConvertService {
    ITracorData ConvertPrivate<T>(TracorIdentifier callee, T value);
    ITracorData ConvertPublic<T>(T value);
}