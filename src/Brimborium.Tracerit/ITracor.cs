namespace Brimborium.Tracerit;

public interface ITracor {
    bool IsGeneralEnabled();
    bool IsCurrentlyEnabled();

    void Trace<T>(TracorIdentitfier callee, T Value);
}
