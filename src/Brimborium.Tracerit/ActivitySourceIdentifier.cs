namespace Brimborium.Tracerit;

/// <summary>
/// Represents an identifier for an activity source, containing both name and version information.
/// Used to uniquely identify and filter activity sources for monitoring purposes.
/// </summary>
/// <param name="Name">The name of the activity source.</param>
/// <param name="Version">The version of the activity source. Defaults to an empty string if not specified.</param>
public record struct ActivitySourceIdentifier(
    string Name,
    string? Version = default
    ) {
    /// <summary>
    /// Creates an ActivitySourceIdentifier with the specified name and version.
    /// If the version is null or empty, it defaults to an empty string.
    /// </summary>
    /// <param name="name">The name of the activity source.</param>
    /// <param name="version">The version of the activity source. Can be null or empty.</param>
    /// <returns>A new ActivitySourceIdentifier instance.</returns>
    public static ActivitySourceIdentifier Create(string name, string? version) {
        if (version is null || version is { Length: 0 }) {
            return new ActivitySourceIdentifier(name, string.Empty);
        } else {
            return new ActivitySourceIdentifier(name, version);
        }
    }
}
