
namespace Brimborium.Tracerit;

/*
 https://andrewlock.net/exploring-the-dotnet-8-preview-using-the-new-configuration-binder-source-generator/
https://github.com/martinothamar/Mediator
 */

/// <summary>
/// Represents a unique identifier for a trace point or caller in the tracing system.
/// </summary>
/// <param name="Callee">The string identifier of the caller or trace point.</param>
public sealed record class TracorIdentitfier(string Source, string Callee) {
    // for matching
    public static TracorIdentitfier Create(string callee)
        => new(string.Empty, callee);

    public TracorIdentitfier Child(string callee)
        => new(this.Source, $"{this.Callee}/{callee}");

    /// <summary>
    /// Creates a <see cref="CalleeCondition"/> by combining this identifier with an expression condition.
    /// </summary>
    /// <param name="expected">The expected tracor identifier.</param>
    /// <param name="and">The expression condition to combine with.</param>
    /// <returns>A new <see cref="CalleeCondition"/> instance.</returns>
    public static CalleeCondition operator /(TracorIdentitfier expected, IExpressionCondition and)
        => new CalleeCondition(expected, and);
}

public sealed class EqualityComparerTracorIdentitfier : EqualityComparer<TracorIdentitfier> {
    private static EqualityComparerTracorIdentitfier? _Default;
    public new static EqualityComparerTracorIdentitfier Default => (_Default ??= new());

    public override bool Equals(TracorIdentitfier? x, TracorIdentitfier? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) { return false; }
        if (!string.Equals(x.Source, y.Source, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        if (!string.Equals(x.Callee, y.Callee, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return true;
    }
    public override int GetHashCode([DisallowNull] TracorIdentitfier obj)
        => HashCode.Combine(obj.Source, obj.Callee);
}


public sealed class MatchEqualityComparerTracorIdentitfier : EqualityComparer<TracorIdentitfier> {
    private static MatchEqualityComparerTracorIdentitfier? _Default;
    public new static MatchEqualityComparerTracorIdentitfier Default => (_Default ??= new());

    public override bool Equals(TracorIdentitfier? x, TracorIdentitfier? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) { return false; }
        if (x.Source is { Length: > 0 } xSource && y.Source is { Length: > 0 } ySource) {
            if (!string.Equals(xSource, ySource, StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
        }
        if (!string.Equals(x.Callee, y.Callee, StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        return true;
    }
    public override int GetHashCode([DisallowNull] TracorIdentitfier obj)
        => HashCode.Combine(obj.Source, obj.Callee);
}

public sealed class TracorIdentitfierCache {
    private readonly TracorIdentitfier _TracorIdentitfier;
    private ImmutableDictionary<string, TracorIdentitfier> _DictChildByName = ImmutableDictionary<string, TracorIdentitfier>.Empty;

    public TracorIdentitfierCache(TracorIdentitfier tracorIdentitfier) {
        this._TracorIdentitfier = tracorIdentitfier;
    }

    public TracorIdentitfier TracorIdentitfier => this._TracorIdentitfier;

    public TracorIdentitfier Child(string? name) {
        if (name is { Length: > 0 } nameValue) {
            if (this._DictChildByName.TryGetValue(nameValue, out var result)) {
                return result;
            } else {
                result = this._TracorIdentitfier.Child(nameValue);
                this._DictChildByName = this._DictChildByName.Add(nameValue, result);
                return result;
            }
        } else {
            return this._TracorIdentitfier;
        }
    }
}