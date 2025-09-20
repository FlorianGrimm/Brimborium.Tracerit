
namespace Brimborium.Tracerit;

/*
 https://andrewlock.net/exploring-the-dotnet-8-preview-using-the-new-configuration-binder-source-generator/
https://github.com/martinothamar/Mediator
 */

/// <summary>
/// Represents a unique identifier for a trace point or caller in the tracing system.
/// </summary>
/// <param name="Source">The source identifier, typically representing the component or module.</param>
/// <param name="Scope">The string identifier of the caller or trace point.</param>
public sealed record class TracorIdentitfier(string Source, string Scope) {
    /// <summary>
    /// Creates a TracorIdentitfier with an empty source for matching purposes.
    /// </summary>
    /// <param name="scope">The callee identifier.</param>
    /// <returns>A new TracorIdentitfier with empty source.</returns>
    public static TracorIdentitfier Create(string scope)
        => new(string.Empty, scope);

    /// <summary>
    /// Creates a child TracorIdentitfier by appending the specified callee to the current callee path.
    /// </summary>
    /// <param name="child">The child callee identifier to append.</param>
    /// <returns>A new TracorIdentitfier representing the child path.</returns>
    public TracorIdentitfier Child(string child)
        => new(this.Source, $"{this.Scope}/{child}");

    public TracorDataRecordOperation GetOperation() {
        // TracorDataRecordOperation.Unknown is no a result
        return (this.Source, this.Scope) switch {
            ("operation", "get") => TracorDataRecordOperation.VariableGet,
            ("operation", "filter") => TracorDataRecordOperation.Filter,
            ("operation", "set") => TracorDataRecordOperation.VariableSet,
            _ => TracorDataRecordOperation.Data
        };
    }

    public static TracorIdentitfier? CreateForOperation(TracorDataRecordOperation value) {
        return value switch {
            TracorDataRecordOperation.Unknown => null,
            TracorDataRecordOperation.Data => null,
            TracorDataRecordOperation.Filter => new("operation", "filter"),
            TracorDataRecordOperation.VariableGet => new("operation", "get"),
            TracorDataRecordOperation.VariableSet => new("operation", "set"),
            _ => null
        };
    }

    /// <summary>
    /// Creates a <see cref="CalleeCondition"/> by combining this identifier with an expression condition.
    /// </summary>
    /// <param name="expected">The expected tracor identifier.</param>
    /// <param name="and">The expression condition to combine with.</param>
    /// <returns>A new <see cref="CalleeCondition"/> instance.</returns>
    public static CalleeCondition operator /(TracorIdentitfier expected, IExpressionCondition and)
        => new CalleeCondition(expected, and);
}

/// <summary>
/// Provides case-insensitive equality comparison for TracorIdentitfier instances.
/// Both Source and Callee properties must match exactly (case-insensitive) for equality.
/// </summary>
public sealed class EqualityComparerTracorIdentitfier : EqualityComparer<TracorIdentitfier> {
    private static EqualityComparerTracorIdentitfier? _Default;

    /// <summary>
    /// Gets the default instance of the equality comparer.
    /// </summary>
    public new static EqualityComparerTracorIdentitfier Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentitfier instances are equal using case-insensitive comparison.
    /// </summary>
    /// <param name="x">The first TracorIdentitfier to compare.</param>
    /// <param name="y">The second TracorIdentitfier to compare.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public override bool Equals(TracorIdentitfier? x, TracorIdentitfier? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) { return false; }
        if (!string.Equals(x.Source, y.Source, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (!string.Equals(x.Scope, y.Scope, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for the specified TracorIdentitfier.
    /// </summary>
    /// <param name="obj">The TracorIdentitfier for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentitfier obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}


/// <summary>
/// Provides partial equality comparison for TracorIdentitfier.
/// The current property is only compared if the expected property is not empty.
/// The expected property is always compared case-insensitively.
/// </summary>
public sealed class MatchEqualityComparerTracorIdentitfier : EqualityComparer<TracorIdentitfier> {
    private static MatchEqualityComparerTracorIdentitfier? _Default;

    /// <summary>
    /// Gets the default instance of the match equality comparer.
    /// </summary>
    public new static MatchEqualityComparerTracorIdentitfier Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentitfier instances are partial equal.
    /// The properties are only compare if the y ones are not empty.
    /// </summary>
    /// <param name="x">The first (current) TracorIdentitfier to compare.</param>
    /// <param name="y">The second (expected / partial) TracorIdentitfier to compare.</param>
    /// <returns>True if the instances match; otherwise, false.</returns>
    public override bool Equals(TracorIdentitfier? x, TracorIdentitfier? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) { return false; }
        if (y.Source is { Length: > 0 } ySource) {
            if (!string.Equals(x.Source, ySource, StringComparison.Ordinal)) {
                return false;
            }
        }
        if (y.Scope is { Length: > 0 }) {
            if (!string.Equals(x.Scope, y.Scope, StringComparison.Ordinal)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for the specified TracorIdentitfier.
    /// </summary>
    /// <param name="obj">The TracorIdentitfier for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentitfier obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}

/// <summary>
/// Provides a caching mechanism for TracorIdentitfier child instances to improve performance and reduce memory allocation.
/// This class maintains an immutable dictionary of child identifiers, creating them on-demand and caching them for reuse.
/// </summary>
public sealed class TracorIdentitfierCache {
    private readonly TracorIdentitfier _TracorIdentitfier;
    private ImmutableDictionary<string, TracorIdentitfier> _DictChildByName = ImmutableDictionary<string, TracorIdentitfier>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorIdentitfierCache"/> class.
    /// </summary>
    /// <param name="tracorIdentitfier">The root TracorIdentitfier to cache children for.</param>
    public TracorIdentitfierCache(TracorIdentitfier tracorIdentitfier) {
        this._TracorIdentitfier = tracorIdentitfier;
    }

    /// <summary>
    /// Gets the root TracorIdentitfier associated with this cache.
    /// </summary>
    public TracorIdentitfier TracorIdentitfier => this._TracorIdentitfier;

    /// <summary>
    /// Gets or creates a child TracorIdentitfier with the specified name.
    /// Child identifiers are cached for performance, so subsequent calls with the same name return the same instance.
    /// </summary>
    /// <param name="name">The name of the child identifier. If null or empty, returns the root identifier.</param>
    /// <returns>
    /// A child TracorIdentitfier if name is provided and non-empty; otherwise, the root TracorIdentitfier.
    /// </returns>
    public TracorIdentitfier Child(string? name) {
        if (name is { Length: > 0 } nameValue) {
            while (true) {
                var dictChildByName = this._DictChildByName;
                if (dictChildByName.TryGetValue(nameValue, out var result)) {
                    return result;
                } else {
                    result = this._TracorIdentitfier.Child(nameValue);
                    var dictChildByNameNext = dictChildByName.Add(nameValue, result);
                    var resultCompareExchange = System.Threading.Interlocked.CompareExchange(ref this._DictChildByName, dictChildByNameNext, dictChildByName);
                    if (ReferenceEquals(resultCompareExchange, dictChildByName)) {
                        return result;
                    } else {
                        continue;
                    }
                }
            }
        } else {
            return this._TracorIdentitfier;
        }
    }
}