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

    public override OnTraceResult OnTrace(TracorIdentitfier callee, ITracorData tracorData, OnTraceStepCurrentContext currentContext) {
        if (string.IsNullOrEmpty(this.PropertyName)) {
            return OnTraceResult.None;
        }
        if (!tracorData.TryGetPropertyValue(this.PropertyName, out var propertyValue)) {
            return OnTraceResult.None;
        }
        if (propertyValue is null) {
            return OnTraceResult.None;
        }
        if (propertyValue is not T propertyValueTyped) {
            return OnTraceResult.None;
        }
        if (currentContext.ForkState.TryGetValue(this.PropertyName, out var globalStateValue)) {
            if (globalStateValue is T globalStateValueTyped) {
                var isEqual = this.EqualityComparer.Equals(globalStateValueTyped, propertyValueTyped);
                if (isEqual) {
                    // continue this is a bound fork
                } else {
                    return OnTraceResult.None;
                }
            } else {
                return OnTraceResult.None;
            }
        } else {
            {
                // is their a fork that handels this?
                var fork = currentContext.TryGetFork(this.PropertyName, propertyValueTyped);
                if (fork is not null) {
                    return OnTraceResult.None;
                }
            }
            currentContext.CreateFork(this.PropertyName, propertyValueTyped, this.EqualityComparer);
            // continue
        }
        var state = currentContext.GetState<GroupByExpressionState>();
        if (this.Expression is { } expression) {
            var childResult = expression.OnTrace(callee, tracorData, currentContext.GetChildContext(0));
            if (OnTraceResult.Successfull == childResult) {
                currentContext.SetStateSuccessfull(this, state);
                return OnTraceResult.Successfull;
            } else {
                return OnTraceResult.None;
            }
        } else {
            // the unbound groupby never terminates
            // the bound groupby terminates
            currentContext.SetStateSuccessfull(this, state);
            return OnTraceResult.Successfull;
        }
    }

    internal sealed class GroupByExpressionState : ValidatorExpressionState { }
}
