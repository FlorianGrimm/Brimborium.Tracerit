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

    private static TracorIdentifier _TidMessageActivityStart = new TracorIdentifier(string.Empty, TracorConstants.SourceActivity, string.Empty, TracorConstants.MessageActivityStart);
    private static TracorIdentifier _TidMessageActivityStop = new TracorIdentifier(string.Empty, TracorConstants.SourceActivity, string.Empty, TracorConstants.MessageActivityStop);

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<GroupByActivityExpressionState>();
        if (state.Result.IsComplete()) {
            return TracorValidatorOnTraceResult.None;
        }
        var currentContext_Identifier = currentContext.Identifier.ToString();

        if (!(tracorData.TryGetDataProperty(
                TracorConstants.TracorDataPropertyNameActivityTraceId,
                out var tdpTraceId)
            && tdpTraceId.TryGetStringValue(out var traceId)
            && (traceId is { Length: > 0 })

            && tracorData.TryGetDataProperty(
                TracorConstants.TracorDataPropertyNameActivitySpanId,
                out var tdpSpanId)
            && tdpSpanId.TryGetStringValue(out var spanId)
            && (spanId is { Length: > 0 })
            )) {
            return TracorValidatorOnTraceResult.None;
        } else if (state.TraceId is null) {
            if (tracorData.TracorIdentifier.DoesMatch(_TidMessageActivityStart)) {

                var tpdFork = TracorDataProperty.CreateStringValue(currentContext_Identifier, spanId);
                if (currentContext.TryGetFork(tpdFork) is not null) {
                    return TracorValidatorOnTraceResult.None;
                }

                if (this.OnStart is { } onStart) {
                    var childResult = onStart.OnTrace(tracorData, currentContext.GetChildContext(0));
                    if (TracorValidatorOnTraceResult.Failed == childResult) {
                        return TracorValidatorOnTraceResult.None;
                    }
                }
                currentContext.CreateFork(tpdFork);
                state.TraceId = traceId;
                state.SpanId = spanId;
                state.Scope = tracorData.TracorIdentifier.Scope;
                return TracorValidatorOnTraceResult.None;
            }
            return TracorValidatorOnTraceResult.None;
        } else {
            bool isCurrentTrace = string.Equals(traceId, state.TraceId, StringComparison.Ordinal);
            if (isCurrentTrace) {
                if (string.Equals(spanId, state.SpanId, StringComparison.Ordinal)) {
                    isCurrentTrace = true;
                } else {
                    isCurrentTrace = false;
                }
            }
            if (isCurrentTrace) {
                if (!tracorData.TracorIdentifier.DoesMatch(_TidMessageActivityStop)) {
                    if (this.OnItem is { } onItem) {
                        var traceResult = onItem.OnTrace(tracorData, currentContext.GetChildContext(1));
                        if (traceResult.IsComplete()) {
                            currentContext.SetStateComplete(this, state, traceResult);
                            return traceResult;
                        }
                    }
                    return TracorValidatorOnTraceResult.None;
                } else {
                    TracorValidatorOnTraceResult traceResult;
                    if (this.OnStop is { } onStop) {
                        traceResult = onStop.OnTrace(tracorData, currentContext.GetChildContext(2));
                    } else {
                        traceResult = TracorValidatorOnTraceResult.Successful;
                    }
                    currentContext.SetStateComplete(this, state, traceResult);
                    return traceResult;
                }
            }
        }
        return TracorValidatorOnTraceResult.None;

#if false
        if (string.Equals(TracorConstants.SourceActivity, tracorData.TracorIdentifier.Source, StringComparison.Ordinal)) {
            if (string.Equals(TracorConstants.MessageActivityStart, tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
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

            } else if (string.Equals(TracorConstants.MessageActivityStop, tracorData.TracorIdentifier.Message, StringComparison.Ordinal)) {
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
            }
        }

        {
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
#endif
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
        TracorDataProperty tdpTraceId) {
        return currentContext.TryGetFork(
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
    internal sealed class GroupByActivityExpressionState : ValidatorExpressionState {
        public string? TraceId;
        public string? SpanId;
        public string? Scope;

        public GroupByActivityExpressionState() {
        }

        private GroupByActivityExpressionState(
                TracorValidatorOnTraceResult result,
                string? traceId,
                string? spanId,
                string? scope
            ) : base(result) {
            this.TraceId = traceId;
            this.SpanId = spanId;
            this.Scope = scope;
        }

        protected internal override ValidatorExpressionState Copy()
            => new GroupByActivityExpressionState(
                this.Result,
                this.TraceId,
                this.SpanId,
                this.Scope
                );
    }
}