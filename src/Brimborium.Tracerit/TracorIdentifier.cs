
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
/// <param name="Message">The Message</param>
public record struct TracorIdentifier(string Source = "", string Scope = "", string Message = "") {
    /// <summary>
    /// Creates a <see cref="TracorIdentifier"/> with an empty source for matching purposes.
    /// </summary>
    /// <param name="scope">The callee identifier.</param>
    /// <returns>A new <see cref="TracorIdentifier"/> with empty source.</returns>
    public static TracorIdentifier Create(string scope)
        => new(string.Empty, scope, string.Empty);

    /// <summary>
    /// Creates a child <see cref="TracorIdentifier"/> by appending the specified callee to the current callee path.
    /// </summary>
    /// <param name="child">The child callee identifier to append.</param>
    /// <returns>A new <see cref="TracorIdentifier"/> representing the child path.</returns>
    public TracorIdentifier Child(string child)
        => new(this.Source, $"{this.Scope}.{child}", string.Empty);

    public void ConvertProperties(List<TracorDataProperty> listTracorDataProperties) {
        listTracorDataProperties.Add(TracorDataProperty.CreateStringValue("Source", this.Source));
        listTracorDataProperties.Add(TracorDataProperty.CreateStringValue("Scope", this.Scope));
        listTracorDataProperties.Add(TracorDataProperty.CreateStringValue("Message", this.Message));
    }

    public bool IsEmpty() 
        => string.IsNullOrEmpty(this.Source) 
        && string.IsNullOrEmpty(this.Scope) 
        && string.IsNullOrEmpty(this.Message);

    /// <summary>
    /// Creates a <see cref="CalleeCondition"/> by combining this identifier with an expression condition.
    /// </summary>
    /// <param name="expected">The expected tracor identifier.</param>
    /// <param name="and">The expression condition to combine with.</param>
    /// <returns>A new <see cref="CalleeCondition"/> instance.</returns>
    public static CalleeCondition operator /(TracorIdentifier expected, IExpressionCondition and)
        => new CalleeCondition(expected, and);
}

/// <summary>
/// Provides case-insensitive equality comparison for <see cref="TracorIdentifier"/> instances.
/// Both Source and Callee properties must match exactly (case-insensitive) for equality.
/// </summary>
public sealed class EqualityComparerTracorIdentifier : EqualityComparer<TracorIdentifier> {
    private static EqualityComparerTracorIdentifier? _Default;

    /// <summary>
    /// Gets the default instance of the equality comparer.
    /// </summary>
    public new static EqualityComparerTracorIdentifier Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentifier instances are equal using case-insensitive comparison.
    /// </summary>
    /// <param name="x">The first TracorIdentifier to compare.</param>
    /// <param name="y">The second TracorIdentifier to compare.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public override bool Equals(TracorIdentifier x, TracorIdentifier y) {
        if (!string.Equals(x.Source, y.Source, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (!string.Equals(x.Scope, y.Scope, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (!string.Equals(x.Message, y.Message, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for the specified TracorIdentifier.
    /// </summary>
    /// <param name="obj">The TracorIdentifier for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentifier obj)
        => HashCode.Combine(obj.Source, obj.Scope, obj.Message);
}


/// <summary>
/// Provides partial equality comparison for TracorIdentifier.
/// The current property is only compared if the expected property is not empty.
/// The expected property is always compared case-insensitively.
/// </summary>
public sealed class MatchEqualityComparerTracorIdentifier : EqualityComparer<TracorIdentifier> {
    private static MatchEqualityComparerTracorIdentifier? _Default;

    /// <summary>
    /// Gets the default instance of the match equality comparer.
    /// </summary>
    public new static MatchEqualityComparerTracorIdentifier Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentifier instances are partial equal.
    /// The properties are only compare if the y ones are not empty.
    /// </summary>
    /// <param name="x">The first (current) TracorIdentifier to compare.</param>
    /// <param name="y">The second (expected / partial) TracorIdentifier to compare.</param>
    /// <returns>True if the instances match; otherwise, false.</returns>
    public override bool Equals(TracorIdentifier x, TracorIdentifier y) {
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
        if (y.Message is { Length: > 0 }) {
            if (!string.Equals(x.Message, y.Message, StringComparison.Ordinal)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Returns a hash code for the specified TracorIdentifier.
    /// </summary>
    /// <param name="obj">The TracorIdentifier for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentifier obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}

/// <summary>
/// Provides a caching mechanism for TracorIdentifier child instances to improve performance and reduce memory allocation.
/// This class maintains an immutable dictionary of child identifiers, creating them on-demand and caching them for reuse.
/// </summary>
public sealed class TracorIdentifierCache {
    private readonly TracorIdentifier _TracorIdentifier;
    private ImmutableDictionary<string, TracorIdentifier> _DictChildByName = ImmutableDictionary<string, TracorIdentifier>.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorIdentifierCache"/> class.
    /// </summary>
    /// <param name="tracorIdentifier">The root TracorIdentifier to cache children for.</param>
    public TracorIdentifierCache(TracorIdentifier tracorIdentifier) {
        this._TracorIdentifier = tracorIdentifier;
    }

    /// <summary>
    /// Gets the root TracorIdentifier associated with this cache.
    /// </summary>
    public TracorIdentifier TracorIdentifier => this._TracorIdentifier;

    /// <summary>
    /// Gets or creates a child TracorIdentifier with the specified name.
    /// Child identifiers are cached for performance, so subsequent calls with the same name return the same instance.
    /// </summary>
    /// <param name="name">The name of the child identifier. If null or empty, returns the root identifier.</param>
    /// <returns>
    /// A child TracorIdentifier if name is provided and non-empty; otherwise, the root TracorIdentifier.
    /// </returns>
    public TracorIdentifier Child(string? name) {
        if (name is { Length: > 0 } nameValue) {
            while (true) {
                var dictChildByName = this._DictChildByName;
                if (dictChildByName.TryGetValue(nameValue, out var result)) {
                    return result;
                } else {
                    result = this._TracorIdentifier.Child(nameValue);
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
            return this._TracorIdentifier;
        }
    }
}