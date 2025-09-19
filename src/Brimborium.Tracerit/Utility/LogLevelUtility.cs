namespace Brimborium.Tracerit.Utility;

public static class LogLevelUtility {
    /// <summary>
    /// Static dictionary mapping string representations to LogLevel values for configuration parsing.
    /// </summary>
    private static Dictionary<string, LogLevel>? _LogLevelByName;

    /// <summary>
    /// Gets a dictionary that maps string representations to LogLevel values for configuration parsing.
    /// </summary>
    /// <returns>A case-insensitive dictionary mapping log level names to LogLevel enum values.</returns>
    /// <remarks>
    /// The dictionary includes standard log level names (Trace, Debug, Information, Warning, Error, Critical, None)
    /// as well as convenience aliases (True/False, Enable/Disable).
    /// </remarks>
    public static Dictionary<string, LogLevel> GetDictLogLevelByName()
        => _LogLevelByName ??= new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase) {
            { "Trace", LogLevel.Trace },
            { "Debug", LogLevel.Debug },
            { "Information", LogLevel.Information },
            { "Warning", LogLevel.Warning },
            { "Error", LogLevel.Error },
            { "Critical", LogLevel.Critical },
            { "None", LogLevel.None },
            { "True", LogLevel.Information },
            { "False", LogLevel.None },
            { "Enable", LogLevel.Information },
            { "Disable", LogLevel.None }
        };

    /// <summary>
    /// Try to convert the valur to LogLevel.
    /// </summary>
    /// <param name="value">loglevel text</param>
    /// <returns>The LogLevel or null</returns>
    public static bool TryGetLogLevelByName(string? value, [MaybeNullWhen(false)] out LogLevel logLevel) {
        if (value is not { } txt) { logLevel = LogLevel.None; return false; }

        var dict = GetDictLogLevelByName();
        if (dict.TryGetValue(txt, out logLevel)) {
            return true;
        } else {
            logLevel = LogLevel.None; return false;
        }
    }
}
