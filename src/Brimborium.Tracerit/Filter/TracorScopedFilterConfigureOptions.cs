namespace Brimborium.Tracerit.Filter;

internal sealed class TracorScopedFilterConfigureOptions : IConfigureOptions<TracorScopedFilterOptions> {
    private const string LogLevelKey = "LogLevel";
    private const string DefaultCategory = "Default";
    private readonly IConfiguration _Configuration;

    public TracorScopedFilterConfigureOptions(IConfiguration configuration) {
        this._Configuration = configuration;
    }

    public void Configure(TracorScopedFilterOptions options) {
        this.LoadDefaultConfigValues(options);
    }

    private void LoadDefaultConfigValues(TracorScopedFilterOptions options) {
        if (this._Configuration == null) {
            return;
        }

        foreach (IConfigurationSection configurationSection in this._Configuration.GetChildren()) {
            if (configurationSection.Key.Equals(LogLevelKey, StringComparison.OrdinalIgnoreCase)) {
                // Load global category defaults
                LoadRules(options, configurationSection, null);
            } else {
                IConfigurationSection logLevelSection = configurationSection.GetSection(LogLevelKey);
                if (logLevelSection != null) {
                    // Load tracor specific rules
                    string tracor = configurationSection.Key;
                    LoadRules(options, logLevelSection, tracor);
                }
            }
        }
    }

    private static void LoadRules(TracorScopedFilterOptions options, IConfigurationSection configurationSection, string? tracor) {
        foreach (System.Collections.Generic.KeyValuePair<string, string?> section in configurationSection.AsEnumerable(true)) {
            if (TryGetSwitch(section.Value, out LogLevel level)) {
                string? category = section.Key;
                if (category.Equals(DefaultCategory, StringComparison.OrdinalIgnoreCase)) {
                    category = null;
                }
                var newRule = new TracorScopedFilterRule(tracor, category, level, null);
                options.Rules.Add(newRule);
            }
        }
    }

    private static bool TryGetSwitch(string? value, out LogLevel level) {
        if (string.IsNullOrEmpty(value)) {
            level = LogLevel.None;
            return false;
        } else if (Enum.TryParse(value, true, out level)) {
            return true;
        } else {
            throw new InvalidOperationException($"Value not supported {value}.");
        }
    }
}
