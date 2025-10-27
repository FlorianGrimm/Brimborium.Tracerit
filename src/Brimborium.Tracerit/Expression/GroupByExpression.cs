namespace Brimborium.Tracerit.Expression;

public sealed class GroupByExpression : ValidatorExpression {
    public GroupByExpression(
        string? label = default,
        string propertyName = "",
        IValidatorExpression? expression = default,
        IValidatorExpression? onStop = default
        ) : base(label) {
        this.PropertyName = propertyName;
        this.Expression = expression;
        this.OnStop = onStop;
    }

    public string PropertyName { get; set; } = string.Empty;

    public IValidatorExpression? Expression { get; set; }
    public IValidatorExpression? OnStart { get; set; }
    public IValidatorExpression? OnStop { get; set; }

    public GroupByExpression Add(IValidatorExpression expression) {
        if (this.Expression is null) {
            this.Expression = expression;
            return this;
        }
        if (this.OnStop is null) {
            this.OnStop = expression;
            return this;
        }
        throw new NotSupportedException();
    }

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        if (string.IsNullOrEmpty(this.PropertyName)) {
            return TracorValidatorOnTraceResult.None;
        }

        if (!tracorData.TryGetDataProperty(propertyName: this.PropertyName, out var tracorDataProperty)) {
            return TracorValidatorOnTraceResult.None;
        }

        if (currentContext.ForkState.TryGetValue(this.PropertyName, out var forkStateValue)) {
            var isEqual = TracorDataPropertyValueEqualityComparer.Default.Equals(forkStateValue, forkStateValue);
            if (isEqual) {
                // continue this is a bound fork
            } else {
                return TracorValidatorOnTraceResult.None;
            }
        } else {
            {
                // is their a fork that handles this?
                var fork = currentContext.TryGetFork(this.PropertyName, forkStateValue);
                if (fork is not null) {
                    return TracorValidatorOnTraceResult.None;
                }
            }
            currentContext.CreateFork(this.PropertyName, forkStateValue);
            // continue
        }
        var state = currentContext.GetState<GroupByExpressionState>();
        if (this.Expression is { } expression) {
            var childResult = expression.OnTrace(tracorData, currentContext.GetChildContext(0));
            if (childResult.IsComplete()) {
                return currentContext.SetStateComplete(this, state, childResult);
            } else {
                if (this.OnStop is { } endOf) {
                    var endOfResult = endOf.OnTrace(tracorData, currentContext.GetChildContext(1));
                    if (endOfResult.IsComplete()) {
                        return currentContext.SetStateComplete(this, state, endOfResult);
                    }
                }
                return TracorValidatorOnTraceResult.None;
            }
        }

        {
            // the unbound group by never terminates
            // the bound group by terminates
            currentContext.SetStateSuccessful(this, state);
            return TracorValidatorOnTraceResult.Successful;
        }
    }

    internal sealed class GroupByExpressionState : ValidatorExpressionState { }
}
