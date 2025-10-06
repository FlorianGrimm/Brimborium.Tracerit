namespace Brimborium.Tracerit;

public interface ITracorDataConvertService {
    ITracorData ConvertPrivate<T>(TracorIdentitfier callee, T value);
    ITracorData ConvertPublic<T>(T value);
}