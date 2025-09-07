namespace Brimborium.Tracerit.Condition;

internal sealed class NeverCondition : IExpressionCondition {
    private static NeverCondition? _Instance;
    public static IExpressionCondition Instance => _Instance ??= new NeverCondition();

    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) => false;
}
