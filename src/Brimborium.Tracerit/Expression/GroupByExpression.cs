namespace Brimborium.Tracerit.Expression;

public sealed class GroupByExpression<T> : ValidatorExpression {
    public GroupByExpression(string? label = default) : base(label) {
    }

    public EqualityComparer<T> EqualityComparer { get; set; } = EqualityComparer<T>.Default;

    public string PropertyName { get; set; } = string.Empty;

    public IValidatorExpression? Expression { get; set; }

    public GroupByExpression<T> Add(IValidatorExpression expression) { 
        this.Expression = expression;
        return this;
    }

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (string.IsNullOrEmpty(this.PropertyName)) {
            return TracorValidatorOnTraceResult.None;
        }
        if (!tracorData.TryGetPropertyValue(this.PropertyName, out var propertyValue)) {
            return TracorValidatorOnTraceResult.None;
        }
        if (propertyValue is null) {
            return TracorValidatorOnTraceResult.None;
        }
        if (propertyValue is not T propertyValueTyped) {
            return TracorValidatorOnTraceResult.None;
        }
        if (currentContext.ForkState.TryGetValue(this.PropertyName, out var globalStateValue)) {
            if (globalStateValue is T globalStateValueTyped) {
                var isEqual = this.EqualityComparer.Equals(globalStateValueTyped, propertyValueTyped);
                if (isEqual) {
                    // continue this is a bound fork
                } else {
                    return TracorValidatorOnTraceResult.None;
                }
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        } else {
            {
                // is their a fork that handels this?
                var fork = currentContext.TryGetFork(this.PropertyName, propertyValueTyped);
                if (fork is not null) {
                    return TracorValidatorOnTraceResult.None;
                }
            }
            currentContext.CreateFork(this.PropertyName, propertyValueTyped, this.EqualityComparer);
            // continue
        }
        var state = currentContext.GetState<GroupByExpressionState>();
        if (this.Expression is { } expression) {
            var childResult = expression.OnTrace(tracorData, currentContext.GetChildContext(0));
            if (childResult.IsComplete()) {
                return currentContext.SetStateComplete(this, state, childResult);
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        } else {
            // the unbound groupby never terminates
            // the bound groupby terminates
            currentContext.SetStateSuccessfull(this, state);
            return TracorValidatorOnTraceResult.Successfull;
        }
    }

    internal sealed class GroupByExpressionState : ValidatorExpressionState { }
}
