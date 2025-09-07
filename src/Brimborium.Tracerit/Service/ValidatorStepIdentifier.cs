namespace Brimborium.Tracerit.Service;

public sealed class ValidatorStepIdentifier {
    private readonly ValidatorStepIdentifier? _Parent;
    private readonly int _Level;
    private readonly int _Index;
    private readonly List<ValidatorStepIdentifier?> _ListChild = [];

    public ValidatorStepIdentifier(int level, int index) {
        this._Parent = null;
        this._Level = level;
        this._Index = index;
    }

    public ValidatorStepIdentifier(ValidatorStepIdentifier parent, int index) {
        this._Level = parent._Level + 1;
        this._Parent = parent;
        this._Index = index;
    }

    private string? _ToStringCache;

    public bool IsLevel(int level) => level == this._Level;

    public override string ToString() {
        if (this._Parent is { } parent) {
            return this._ToStringCache ??= $"{this._Level}:{this._Index};{parent}";
        } else {
            return this._ToStringCache ??= $"{this._Level}:{this._Index}";
        }
    }

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
