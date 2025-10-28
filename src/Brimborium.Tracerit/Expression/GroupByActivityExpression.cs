
namespace Brimborium.Tracerit.Expression;

public sealed class GroupByActivityExpression : ValidatorExpression {
    public GroupByActivityExpression(
        string? label = default,
        IValidatorExpression? onStart = default,
        IValidatorExpression? onItem = default,
        IValidatorExpression? onStop = default
        ) : base(label) {
        this.OnStart = onStart;
        this.OnItem = onItem;
        this.OnStop = onStop;
    }

    /// <summary>
    /// Side effect free
    /// </summary>
    public IValidatorExpression? OnStart { get; set; }
    public IValidatorExpression? OnItem { get; set; }
    public IValidatorExpression? OnStop { get; set; }

    public GroupByActivityExpression Add(IValidatorExpression expression) {
        if (this.OnStart is null) {
            this.OnStart = expression;
            return this;
        }
        if (this.OnItem is null) {
            this.OnItem = expression;
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
        var state = currentContext.GetState<GroupByActivityExpressionState>();
        if (state.Result.IsComplete()) {
            return TracorValidatorOnTraceResult.None;
        }
        var currentContext_Identifier = currentContext.Identifier.ToString();
        var nameScopeTrace = $"{currentContext_Identifier}.Trace";
        bool hasTraceId = tracorData.TryGetDataProperty(
            TracorConstants.TracorDataPropertyNameActivityTraceId,
            out var tdpTraceId);

        if (!hasTraceId) { return TracorValidatorOnTraceResult.None; }

        if (!string.Equals("Activity", tracorData.TracorIdentifier.Source, StringComparison.Ordinal)) {
            return TracorValidatorOnTraceResult.None;
        }

        {
            if (string.Equals("Start", tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
                OnTraceStepExecutionState? fork = TryGetFork(currentContext, nameScopeTrace, tdpTraceId);
                if (fork is not null) {
                    return TracorValidatorOnTraceResult.None;
                }
                if (this.OnStart is { } onStart) {
                    var childResult = onStart.OnTrace(tracorData, currentContext.GetChildContext(0));
                    if (TracorValidatorOnTraceResult.Failed == childResult) {
                        return TracorValidatorOnTraceResult.None;
                    }
                }
                currentContext.CreateFork(nameScopeTrace, tdpTraceId);
                return TracorValidatorOnTraceResult.None;
            } else if (string.Equals("Stop", tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
                if (IsCurrentFork(nameScopeTrace, ref tdpTraceId, currentContext)) {
                    TracorValidatorOnTraceResult traceResult;
                    if (this.OnStop is { } onStop) {
                        traceResult = onStop.OnTrace(tracorData, currentContext.GetChildContext(2));
                    } else {
                        traceResult = TracorValidatorOnTraceResult.Successful;
                    }
                    currentContext.SetStateComplete(this, state, traceResult);
                    return traceResult;
                }
                return TracorValidatorOnTraceResult.None;
            } else {
                if (IsCurrentFork(nameScopeTrace, ref tdpTraceId, currentContext)) {
                    //
                    if (this.OnItem is { } onItem) {
                        var traceResult = onItem.OnTrace(tracorData, currentContext.GetChildContext(1));
                        if (traceResult.IsComplete()) {
                            currentContext.SetStateComplete(this, state, traceResult);
                            return traceResult;
                        }
                    }
                }
                return TracorValidatorOnTraceResult.None;
            }
        }
    }

    internal static bool IsCurrentFork(
        string nameScopeTrace,
        ref readonly TracorDataProperty tdpTraceId,
        OnTraceStepCurrentContext currentContext) {
        if (tdpTraceId.TryGetStringValue(out var traceIdValue)
            && currentContext.ForkState.TryGetValue(nameScopeTrace, out var tdpCurrent)
            && tdpCurrent.TryGetStringValue(out var currentValue)) {
            if (string.Equals(traceIdValue, currentValue, StringComparison.Ordinal)) {
                return true;
            }
        }
        return false;
    }

    private static OnTraceStepExecutionState? TryGetFork(
        OnTraceStepCurrentContext currentContext,
        string nameScopeTrace,
        TracorDataProperty tdpTraceId) {
        return currentContext.TryGetFork(
            nameScopeTrace,
            tdpTraceId,
            static (tdpTraceId, tdpCurrent) => {
                if (tdpTraceId.TryGetStringValue(out var traceIdValue)
                    && tdpCurrent.TryGetStringValue(out var currentValue)) {
                    if (string.Equals(traceIdValue, currentValue, StringComparison.Ordinal)) {
                        return true;
                    }
                }
                return false;
            });
    }
    internal class GroupByActivityExpressionState : ValidatorExpressionState {
    }
}