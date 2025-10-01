namespace Brimborium.Tracerit;

/// <summary>
/// Represents a unique identifier for a trace point or caller in the tracing system.
/// </summary>
/// <param name="Source">The source identifier, typically representing the component or module.</param>
/// <param name="Scope">The string identifier of the caller or trace point.</param>
/// <param name="TypeParameter">TypeParameter</param>
public sealed record class TracorIdentitfierType(string Source, string Scope, Type TypeParameter);

/// <summary>
/// Provides case-insensitive equality comparison for TracorIdentitfierType instances.
/// Both Source and Callee properties must match exactly (case-insensitive) for equality.
/// </summary>
public sealed class EqualityComparerTracorIdentitfierType : EqualityComparer<TracorIdentitfierType> {
    private static EqualityComparerTracorIdentitfierType? _Default;

    /// <summary>
    /// Gets the default instance of the equality comparer.
    /// </summary>
    public new static EqualityComparerTracorIdentitfierType Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentitfierType instances are equal using case-insensitive comparison.
    /// </summary>
    /// <param name="x">The first TracorIdentitfierType to compare.</param>
    /// <param name="y">The second TracorIdentitfierType to compare.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public override bool Equals(TracorIdentitfierType? x, TracorIdentitfierType? y) {
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
    /// Returns a hash code for the specified TracorIdentitfierType.
    /// </summary>
    /// <param name="obj">The TracorIdentitfierType for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentitfierType obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}


/// <summary>
/// Provides partial equality comparison for TracorIdentitfierType.
/// The current property is only compared if the expected property is not empty.
/// The expected property is always compared case-insensitively.
/// </summary>
public sealed class MatchEqualityComparerTracorIdentitfierType : EqualityComparer<TracorIdentitfierType> {
    private static MatchEqualityComparerTracorIdentitfierType? _Default;

    /// <summary>
    /// Gets the default instance of the match equality comparer.
    /// </summary>
    public new static MatchEqualityComparerTracorIdentitfierType Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentitfierType instances are partial equal.
    /// The properties are only compare if the y ones are not empty.
    /// </summary>
    /// <param name="x">The first (current) TracorIdentitfierType to compare.</param>
    /// <param name="y">The second (expected / partial) TracorIdentitfierType to compare.</param>
    /// <returns>True if the instances match; otherwise, false.</returns>
    public override bool Equals(TracorIdentitfierType? x, TracorIdentitfierType? y) {
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
    /// Returns a hash code for the specified TracorIdentitfierType.
    /// </summary>
    /// <param name="obj">The TracorIdentitfierType for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentitfierType obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}
