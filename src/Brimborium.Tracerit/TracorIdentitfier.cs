namespace Brimborium.Tracerit;

/*
 https://andrewlock.net/exploring-the-dotnet-8-preview-using-the-new-configuration-binder-source-generator/
https://github.com/martinothamar/Mediator
 */

/// <summary>
/// Represents a unique identifier for a trace point or caller in the tracing system.
/// </summary>
/// <param name="Callee">The string identifier of the caller or trace point.</param>
public sealed record class TracorIdentitfier(string Callee) {
    /// <summary>
    /// Initializes a new instance of the <see cref="TracorIdentitfier"/> class with a parent identifier and a child callee.
    /// </summary>
    /// <param name="parent">The parent identifier, or null if this is a root identifier.</param>
    /// <param name="callee">The child callee identifier.</param>
    public TracorIdentitfier(TracorIdentitfier? parent, string callee)
        : this(parent is { } ? $"{parent.Callee}/{callee}" : callee) {
    }

    /// <summary>
    /// Creates a <see cref="CalleeCondition"/> by combining this identifier with an expression condition.
    /// </summary>
    /// <param name="expected">The expected tracor identifier.</param>
    /// <param name="and">The expression condition to combine with.</param>
    /// <returns>A new <see cref="CalleeCondition"/> instance.</returns>
    public static CalleeCondition operator /(TracorIdentitfier expected, IExpressionCondition and) {
        return new CalleeCondition(expected, and);
    }
}
