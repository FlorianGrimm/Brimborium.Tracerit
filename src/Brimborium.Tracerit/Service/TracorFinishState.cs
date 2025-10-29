namespace Brimborium.Tracerit.Service;

public record class TracorRunningState(
    TracorForkState ForkState,
    TracorGlobalState GlobalState
    );

public record class TracorFinishState(
    TracorValidatorOnTraceResult Result,
    TracorForkState ForkState,
    TracorGlobalState GlobalState
    );
