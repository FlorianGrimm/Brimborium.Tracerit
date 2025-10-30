namespace Brimborium.Tracerit.Service;

public record struct TracorReportStateItem(
    string Label,
    TracorValidatorOnTraceResult Result,
    DateTime Timestamp);
