
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
        var currentContext_Identifier = currentContext.Identifier.ToString();
        var nameScopeTrace = $"{currentContext_Identifier}.Trace";
        bool hasTraceId = tracorData.TryGetDataProperty(
            TracorConstants.TracorDataPropertyNameActivityTraceId,
            out var tdpTraceId);
        
        if (!hasTraceId) { return TracorValidatorOnTraceResult.None; }

        if (string.Equals("Activity", tracorData.TracorIdentifier.Source, StringComparison.Ordinal)) {
            if (string.Equals("Start", tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
                OnTraceStepExecutionState? fork = TryGetFork(currentContext, nameScopeTrace, tdpTraceId);
                if (fork is not null) {
                    return TracorValidatorOnTraceResult.None;
                }
                currentContext.CreateFork(nameScopeTrace, tdpTraceId);
                if (this.OnStart is { } onStart) {
                    // TODO
                }
            } else if (string.Equals("Stop", tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
                if (this.IsCurrentFork(nameScopeTrace, ref tdpTraceId, currentContext)) { 
                    //
                } else {
                    //
                }
                if (currentContext.ForkState.TryGetValue(nameScopeTrace, out var tdpForkTraceId)) {
                    if (tdpTraceId.Equals(tdpForkTraceId)) {
                        // TODO: here
                        if (this.OnStop is { } onStop) {
                        }
                        return TracorValidatorOnTraceResult.Successful;
                    }
                }
            } else {
                if (this.IsCurrentFork(nameScopeTrace, ref tdpTraceId, currentContext)) {
                    //
                } else {
                    //
                }
                //OnTraceStepExecutionState? fork = TryGetFork(currentContext, nameScopeTrace, tdpTraceId);
                //currentContext.ForkState
                //if (fork is not null) {
                //    return TracorValidatorOnTraceResult.None;
                //}
            }
        }

        if (this.OnItem is { } onItem) {
        }

        return TracorValidatorOnTraceResult.None;
    }

    private bool IsCurrentFork(
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
}