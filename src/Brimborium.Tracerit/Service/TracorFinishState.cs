namespace Brimborium.Tracerit.Service;

public record class TracorRunningState(
    ImmutableArray<TracorReportStateItem> ListReportState,
    ImmutableDictionary<string, TracorDataProperty> DictGlobalState
    );

public record class TracorFinishState(
    TracorValidatorOnTraceResult Result,
    ImmutableArray<TracorReportStateItem> ListReportState,
    ImmutableDictionary<string, TracorDataProperty> DictGlobalState
    );
