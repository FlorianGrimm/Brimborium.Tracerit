namespace Brimborium.Tracerit.Filter;

[DebuggerDisplay("{DebuggerToString(),nq}")]
internal sealed class TracorScopedFilterOptionsBySourceName {
    public static TracorScopedFilterOptionsBySourceName Create(
        LogLevel minLevel,
        List<TracorScopedFilterRule> listRules,
        string[] listSourceName
        ) {
        TracorScopedFilterOptionsBySourceName result = new(
            minLevel, listRules, listSourceName);
        foreach (var sourceName in listSourceName) {
            var listRulesBySourceName = GetListRulesBySourceName(listRules, sourceName);
            result.RulesBySourceName[sourceName] = listRulesBySourceName;
        }
        result.RulesBySourceName[string.Empty] = GetListRulesBySourceName(listRules, string.Empty);
        return result;

        static TracorScopedFilterRule[] GetListRulesBySourceName(
            List<TracorScopedFilterRule> listRules,
            string sourceName) {
            List<TracorScopedFilterRule> listRulesBySourceName = new();
            foreach (var rule in listRules) {
                if (string.IsNullOrEmpty(rule.SourceName)
                    || string.Equals(rule.SourceName, sourceName, StringComparison.OrdinalIgnoreCase)) {
                    listRulesBySourceName.Add(rule);
                }
            }

            return listRulesBySourceName
                .OrderBy(a => a.Filter is null ? 0 : 1)
                .ThenBy(a => a.SourceName ?? string.Empty)
                .ThenBy(a => a.CategoryName ?? string.Empty)
                .ToArray()
                ;
        }
    }

    public readonly LogLevel MinLevel;
    public readonly List<TracorScopedFilterRule> Rules;
    public readonly string[] ListSourceName;
    public readonly Dictionary<string, TracorScopedFilterRule[]> RulesBySourceName;

    public TracorScopedFilterOptionsBySourceName(
        LogLevel minLevel,
        List<TracorScopedFilterRule> rules,
        string[] listSourceName) {
        this.MinLevel = minLevel;
        this.Rules = rules;
        this.ListSourceName = listSourceName;
        this.RulesBySourceName = new(StringComparer.OrdinalIgnoreCase);
    }

    internal string DebuggerToString() {
        string debugText;
        if (this.MinLevel != LogLevel.None) {
            debugText = $"MinLevel = {this.MinLevel}";
        } else {
            // Display "Enabled = false". This makes it clear that the entire ITracor
            // is disabled and nothing is written.
            //
            // If "MinLevel = None" was displayed then someone could think that the
            // min level is disabled and everything is written.
            debugText = $"Enabled = false";
        }

        if (this.Rules.Count > 0) {
            debugText += $", Rules = {this.Rules.Count}";
        }

        return debugText;
    }
}