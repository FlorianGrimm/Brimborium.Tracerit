namespace Brimborium.Tracerit.Collector;

public class AppConfig  {
    public string? ServiceName { get; set; }

    public string? LogDirectory { get; set; }

    public bool LimitLocalhost { get; set; } = true;
}