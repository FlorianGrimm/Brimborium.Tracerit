namespace Brimborium.Tracerit.Service;

/// <summary>
/// Represents a hierarchical identifier for validation steps, providing a unique path through the validation tree.
/// This class manages parent-child relationships and provides efficient string representation caching.
/// </summary>
public sealed class ValidatorStepIdentifier {
    private readonly ValidatorStepIdentifier? _Parent;
    private readonly int _Level;
    private readonly int _Index;
    private readonly List<ValidatorStepIdentifier?> _ListChild = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorStepIdentifier"/> class as a root identifier.
    /// </summary>
    /// <param name="level">The level in the validation hierarchy.</param>
    /// <param name="index">The index at this level.</param>
    public ValidatorStepIdentifier(int level, int index) {
        this._Parent = null;
        this._Level = level;
        this._Index = index;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatorStepIdentifier"/> class as a child identifier.
    /// </summary>
    /// <param name="parent">The parent identifier.</param>
    /// <param name="index">The index at this level.</param>
    public ValidatorStepIdentifier(ValidatorStepIdentifier parent, int index) {
        this._Level = parent._Level + 1;
        this._Parent = parent;
        this._Index = index;
    }

    private string? _ToStringCache;

    /// <summary>
    /// Determines whether this identifier is at the specified level.
    /// </summary>
    /// <param name="level">The level to check.</param>
    /// <returns>True if this identifier is at the specified level; otherwise, false.</returns>
    public bool IsLevel(int level) => level == this._Level;

    /// <summary>
    /// Returns a string representation of this identifier, including the full hierarchical path.
    /// The result is cached for performance.
    /// </summary>
    /// <returns>A string representation in the format "level:index;parent" or "level:index" for root identifiers.</returns>
    public override string ToString() {
        if (this._Parent is { } parent) {
            return this._ToStringCache ??= $"{this._Level}:{this._Index};{parent}";
        } else {
            return this._ToStringCache ??= $"{this._Level}:{this._Index}";
        }
    }

    /// <summary>
    /// Gets or creates a child identifier at the specified index.
    /// Child identifiers are cached for efficient reuse.
    /// </summary>
    /// <param name="childIndex">The index of the child identifier to retrieve or create.</param>
    /// <returns>The child identifier at the specified index.</returns>
    public ValidatorStepIdentifier GetChildIdentifier(int childIndex) {
        if (childIndex < this._ListChild.Count
            && this._ListChild[childIndex] is { } result) {
            return result;
        }
        result = new ValidatorStepIdentifier(this, childIndex);
        if (childIndex < this._ListChild.Count) {
            this._ListChild[childIndex] = result;
        } else {
            while (this._ListChild.Count < childIndex) {
                this._ListChild.Add(null);
            }
            this._ListChild.Add(result);
        }
        return result;
    }
}
