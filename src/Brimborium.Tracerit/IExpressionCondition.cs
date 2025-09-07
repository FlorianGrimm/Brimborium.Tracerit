namespace Brimborium.Tracerit; 

public interface IExpressionCondition {
    bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext);
}
public interface IExpressionCondition<T>: IExpressionCondition {
}