namespace Brimborium.Tracerit.Condition;

public sealed class EqualsCondition<TValue, TProperty> : IExpressionCondition {
    private readonly Func<TValue, TProperty> _FnGetProperty;
    private readonly TProperty _ExpectedValue;
    private readonly IEqualityComparer<TProperty> _EqualityComparer;

    public EqualsCondition(
        Func<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        IEqualityComparer<TProperty>? equalityComparer = default) {
        this._FnGetProperty = fnGetProperty;
        this._ExpectedValue = expectedValue;
        this._EqualityComparer = equalityComparer ?? EqualityComparer<TProperty>.Default;
    }
    public bool DoesMatch(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (tracorData is ITracorData<TValue> tracorDataTyped
            && tracorDataTyped.TryGetOriginalValue(out var value)) {
            var propertyValue = this._FnGetProperty(value);
            var result = this._EqualityComparer.Equals(propertyValue, this._ExpectedValue);
            return result;
        }
        return false;
    }
}
public static class EqualsConditionExtension {
    public static EqualsCondition<TValue, TProperty> EqualsCondition<TValue, TProperty>(
        this WrapFunc<TValue, TProperty> fnGetProperty,
        TProperty expectedValue,
        IEqualityComparer<TProperty>? equalityComparer = default) 
        => new EqualsCondition<TValue, TProperty>(
            fnGetProperty.Value, expectedValue, equalityComparer);
}
