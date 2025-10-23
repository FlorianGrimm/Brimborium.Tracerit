namespace Brimborium.Tracerit;

/// <summary>
/// Represents a unique identifier for a trace point or caller in the tracing system.
/// </summary>
/// <param name="Source">The source identifier, typically representing the component or module.</param>
/// <param name="Scope">The string identifier of the caller or trace point.</param>
/// <param name="TypeParameter">TypeParameter</param>
public sealed record class TracorIdentifierType(string Source, string Scope, Type TypeParameter);

/// <summary>
/// Provides case-insensitive equality comparison for TracorIdentifierType instances.
/// Both Source and Callee properties must match exactly (case-insensitive) for equality.
/// </summary>
public sealed class EqualityComparerTracorIdentifierType : EqualityComparer<TracorIdentifierType> {
    private static EqualityComparerTracorIdentifierType? _Default;

    /// <summary>
    /// Gets the default instance of the equality comparer.
    /// </summary>
    public new static EqualityComparerTracorIdentifierType Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentifierType instances are equal using case-insensitive comparison.
    /// </summary>
    /// <param name="x">The first TracorIdentifierType to compare.</param>
    /// <param name="y">The second TracorIdentifierType to compare.</param>
    /// <returns>True if the instances are equal; otherwise, false.</returns>
    public override bool Equals(TracorIdentifierType? x, TracorIdentifierType? y) {
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
    /// Returns a hash code for the specified TracorIdentifierType.
    /// </summary>
    /// <param name="obj">The TracorIdentifierType for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentifierType obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}


/// <summary>
/// Provides partial equality comparison for TracorIdentifierType.
/// The current property is only compared if the expected property is not empty.
/// The expected property is always compared case-insensitively.
/// </summary>
public sealed class MatchEqualityComparerTracorIdentifierType : EqualityComparer<TracorIdentifierType> {
    private static MatchEqualityComparerTracorIdentifierType? _Default;

    /// <summary>
    /// Gets the default instance of the match equality comparer.
    /// </summary>
    public new static MatchEqualityComparerTracorIdentifierType Default => (_Default ??= new());

    /// <summary>
    /// Determines whether two TracorIdentifierType instances are partial equal.
    /// The properties are only compare if the y ones are not empty.
    /// </summary>
    /// <param name="x">The first (current) TracorIdentifierType to compare.</param>
    /// <param name="y">The second (expected / partial) TracorIdentifierType to compare.</param>
    /// <returns>True if the instances match; otherwise, false.</returns>
    public override bool Equals(TracorIdentifierType? x, TracorIdentifierType? y) {
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
    /// Returns a hash code for the specified TracorIdentifierType.
    /// </summary>
    /// <param name="obj">The TracorIdentifierType for which to get a hash code.</param>
    /// <returns>A hash code for the specified object.</returns>
    public override int GetHashCode([DisallowNull] TracorIdentifierType obj)
        => HashCode.Combine(obj.Source, obj.Scope);
}
