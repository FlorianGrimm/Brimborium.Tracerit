namespace Brimborium.Tracerit.Expression;

public sealed class GroupByRootActivityExpression : ValidatorExpression {
    public GroupByRootActivityExpression(
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

    public GroupByRootActivityExpression Add(IValidatorExpression expression) {
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

    private static TracorIdentifier _TidMessageActivityStart = new TracorIdentifier(string.Empty, TracorConstants.SourceProviderActivity, string.Empty, TracorConstants.MessageActivityStart);
    private static TracorIdentifier _TidMessageActivityStop = new TracorIdentifier(string.Empty, TracorConstants.SourceProviderActivity, string.Empty, TracorConstants.MessageActivityStop);

    public override TracorValidatorOnTraceResult OnTrace(
        ITracorData tracorData,
        OnTraceStepCurrentContext currentContext) {
        var state = currentContext.GetState<GroupByRootActivityExpressionState>();
        if (state.Result.IsComplete()) {
            return state.Result;
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
        }

        if (state.TraceId is null) {
            if (tracorData.TracorIdentifier.DoesMatch(_TidMessageActivityStart)) {

                var tpdFork = TracorDataProperty.CreateStringValue(currentContext_Identifier, traceId);
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
        }

        {
            bool isCurrentTrace = string.Equals(traceId, state.TraceId, StringComparison.Ordinal);
            if (isCurrentTrace) {
                bool isCurrentTraceSpan = isCurrentTrace
                    && string.Equals(spanId, state.SpanId, StringComparison.Ordinal);
                bool isStop = tracorData.TracorIdentifier.DoesMatch(
                    new TracorIdentifier(string.Empty, TracorConstants.SourceProviderActivity, state.Scope ?? string.Empty, TracorConstants.MessageActivityStop));
                if (!(isCurrentTraceSpan && isStop)) {
                    if (this.OnItem is { } onItem) {
                        var traceResult = onItem.OnTrace(tracorData, currentContext.GetChildContext(1));
                        if (traceResult.IsComplete()) {
                            currentContext.SetStateComplete(this, state, traceResult, tracorData.Timestamp);
                            return traceResult;
                        }
                    }
                    return TracorValidatorOnTraceResult.None;
                }

                if (isStop && isCurrentTraceSpan) {
                    {
                        TracorValidatorOnTraceResult traceResult;
                        if (this.OnStop is { } onStop) {
                            traceResult = onStop.OnTrace(tracorData, currentContext.GetChildContext(2));
                        } else {
                            traceResult = TracorValidatorOnTraceResult.Successful;
                        }
                        currentContext.SetStateComplete(this, state, traceResult, tracorData.Timestamp);
                        return traceResult;
                    }
                }
            }
        }
        return TracorValidatorOnTraceResult.None;
    }

    internal static bool IsCurrentFork(
        string nameScopeTrace,
        ref readonly TracorDataProperty tdpTraceId,
        OnTraceStepCurrentContext currentContext) {
        if (tdpTraceId.TryGetStringValue(out var traceIdValue)
            && currentContext.DictForkState.TryGetValue(nameScopeTrace, out var tdpCurrent)
            && tdpCurrent.TryGetStringValue(out var currentValue)) {
            if (string.Equals(traceIdValue, currentValue, StringComparison.Ordinal)) {
                return true;
            }
        }
        return false;
    }

    internal sealed class GroupByRootActivityExpressionState : ValidatorExpressionState {
        public string? TraceId;
        public string? SpanId;
        public string? Scope;

        public GroupByRootActivityExpressionState() {
        }

        private GroupByRootActivityExpressionState(
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
            => new GroupByRootActivityExpressionState(
                this.Result,
                this.TraceId,
                this.SpanId,
                this.Scope
                );
    }
}
