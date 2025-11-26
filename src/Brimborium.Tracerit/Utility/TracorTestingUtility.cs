// MIT - Florian Grimm

namespace Brimborium.Tracerit.Utility;

public static class TracorTestingUtility {
    public static void WireParentTestingProcessForTesting(IConfigurationRoot configuration, Action? action = null) {
        if (configuration.GetSection("ParentTestingProcess").Value is { Length: > 0 } strParentTestingProcess
            && int.TryParse(strParentTestingProcess, out var parentTestingProcess)) {
            var parentProcess = System.Diagnostics.Process.GetProcessById(parentTestingProcess);
            if (parentProcess != null) {
                parentProcess.WaitForExitAsync().ContinueWith(t => {
                    if (action is { }) { action(); }
                    System.Environment.Exit(0);
                });
            }
        }
    }
}
