namespace Brimborium.Tracerit.Filter;

internal static class TracorScopedFilterRuleSelector {
    public static void Select(
        TracorScopedFilterOptionsBySourceName options,
        string? sourceName,
        string category,
        out LogLevel? minLevel,
        out Func<string?, string?, LogLevel, bool>? filter) {
        filter = null;
        minLevel = options.MinLevel;

        // Filter rule selection:
        // 1. Select rules for current tracor type, if there is none, select ones without tracor type specified
        // 2. Select rules with longest matching categories
        // 3. If there nothing matched by category take all rules without category
        // 3. If there is only one rule use it's level and filter
        // 4. If there are multiple rules use last
        // 5. If there are no applicable rules use global minimal level
        TracorScopedFilterRule[]? listRule = null;
        if (
            ((sourceName is { Length: > 0 } key)
                && options.RulesBySourceName.TryGetValue(key, out listRule))
            || (options.RulesBySourceName.TryGetValue(string.Empty, out listRule))
            ) {

            RulePrio bestRulePrio = new(null, 0, 0);

            foreach (TracorScopedFilterRule currentRule in listRule) {
                var resultPrio = IsBetter(
                    currentRule,
                    bestRulePrio,
                    sourceName ?? string.Empty, category);
                if (resultPrio.rule is { }) {
                    bestRulePrio = resultPrio;
                }
            }

            if (bestRulePrio.rule is { } bestRule) {
                filter = bestRule.Filter;
                minLevel = bestRule.LogLevel;
            }
        }
    }

    private record struct RulePrio(
        TracorScopedFilterRule? rule,
        int prioSourceName,
        int prioCategoryName
        );

    private static RulePrio IsBetter(
        TracorScopedFilterRule currentRule,
        RulePrio betterRulePrio,
        string sourceName,
        string category) {

        int currentPrioSourceName = prioSourceName(sourceName, currentRule.SourceName);
        if (currentPrioSourceName == 0) { return betterRulePrio; }

        int currentPrioCategoryName = prioCategoryName(category, currentRule.CategoryName);
        if (currentPrioCategoryName == 0) { return betterRulePrio; }

        // currentRule matches

        RulePrio currentRulePrio = new(currentRule, currentPrioSourceName, currentPrioCategoryName);

        if (betterRulePrio.rule is null) {
            // and their is no better one.
            return currentRulePrio;
        }

        {
            var currentRuleHasFiler = currentRule.Filter is not null;
            var bestRuleHasFiler = betterRulePrio.rule.Filter is not null;

            if (currentRuleHasFiler && !bestRuleHasFiler) {
                return currentRulePrio;

            } else if (!currentRuleHasFiler && bestRuleHasFiler) {
                return betterRulePrio;

            }

            if (currentPrioSourceName < betterRulePrio.prioSourceName) {
                return betterRulePrio;

            } else if (betterRulePrio.prioSourceName < currentPrioSourceName) {
                return currentRulePrio;

            }

            if (currentPrioCategoryName < betterRulePrio.prioCategoryName) {
                return betterRulePrio;

            } else if (betterRulePrio.prioCategoryName < currentPrioCategoryName) {
                return currentRulePrio;

            }
        }

        return currentRulePrio;
    }


    private static int prioSourceName(string sourceName, string? sourceNameRule) {
        if (string.IsNullOrEmpty(sourceNameRule)) {
            // OK
            return 1;
        } else {
            if (string.Equals(sourceName, sourceNameRule, StringComparison.OrdinalIgnoreCase)) {
                // OK
                return 2;
            } else {
                // does not match
                return 0;
            }
        }
    }

    private static int prioCategoryName(string categoryName, string? categoryNameRule) {
        if (string.IsNullOrEmpty(categoryNameRule)) {
            // OK
            return 1;
        } else {
            if (string.Equals(categoryName, categoryNameRule, StringComparison.OrdinalIgnoreCase)) {
                // OK
                return 2 + categoryName.Length;
            } else {
                // is not equal, but might match
                const char WildcardChar = '*';

                int wildcardIndex = categoryNameRule.IndexOf(WildcardChar);
                if ((0 < wildcardIndex)
                    && (0 < categoryNameRule.IndexOf(WildcardChar, wildcardIndex + 1))
                    ) {
                    throw new InvalidOperationException($"More than one wildcard {categoryNameRule}.");
                }

                ReadOnlySpan<char> prefix, suffix;
                if (wildcardIndex < 0) {
                    prefix = categoryNameRule.AsSpan();
                    suffix = default;
                } else {
                    prefix = categoryNameRule.AsSpan(0, wildcardIndex);
                    suffix = categoryNameRule.AsSpan(wildcardIndex + 1);
                }

                if (!categoryName.AsSpan().StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    || !categoryName.AsSpan().EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) {
                    return 0;
                }
                return 2 + prefix.Length + ((wildcardIndex < 0) ? 0 : 2);
            }
        }
    }
}
