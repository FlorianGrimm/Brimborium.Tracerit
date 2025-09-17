
using System.Reflection.Metadata;

namespace Brimborium.Tracerit;

public interface ITracorActivityListener {
    void Start();
    void Stop();

    void AddActivitySourceName(string name);
    void RemoveActivitySourceName(string name);
    void AddListInstrumentation(ActivitySourceIdenifier instrumentationRegistration);
    void RemoveListInstrumentation(ActivitySourceIdenifier instrumentationRegistration);
}

public class TracorActivityListenerOptions {
    public bool AllowAllActivitySource { get; set; }
    public List<string> ListActivitySourceName { get; set; } = new();
    public List<ActivitySourceIdenifier> ListActivitySourceIdenifier { get; set; } = new();
}

public record struct ActivitySourceIdenifier(
    string Name,
    string Version = ""
    ) {
    public static ActivitySourceIdenifier Create(string name, string? version) {
        if (version is null || version is { Length: 0 }) {
            return new ActivitySourceIdenifier(name, string.Empty);
        } else {
            return new ActivitySourceIdenifier(name, version);
        }
    }
}
