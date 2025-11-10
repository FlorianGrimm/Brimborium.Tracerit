#pragma warning disable IDE0057 // Use range operator

namespace Brimborium.Tracerit;

public sealed class TracorPropertySinkTarget : ReferenceCountObject {
    /// <summary>
    /// Use for Timestamp or TracorIdentifier
    /// </summary>
    /// <code>
    /// ITracorDataExtension.CopyPropertiesToSinkBase<TracorDataRecord>(this, target);
    /// </code>
    public readonly ArrayTracorDataProperty ListHeader;
    public readonly List<TracorDataProperty> ListProperty;
    public List<TracorDataProperty>? ListPropertyFromTracorData;

    public TracorPropertySinkTarget(IReferenceCountPool? owner) : base(owner) {
        this.ListHeader = new ArrayTracorDataProperty();
        this.ListProperty = new List<TracorDataProperty>(100);
        this.ListPropertyFromTracorData = null;
    }

    protected override bool IsStateReset() {
        return (0 == this.ListHeader.Count)
            && (0 == this.ListProperty.Count)
            && (this.ListPropertyFromTracorData is null);
    }

    protected override void ResetState() {
        this.ListHeader.Clear();
        this.ListProperty.Clear();
        this.ListPropertyFromTracorData = null;
    }
}

public sealed class ArrayTracorDataProperty : ICollection<TracorDataProperty> {
    private TracorDataProperty[] _ListTracorDataProperty;
    public ReadOnlySpan<TracorDataProperty> ListTracorDataProperty => this._ListTracorDataProperty.AsSpan(0, this.Count);

    public ArrayTracorDataProperty(int capacity = 16) {
        this._Capacity = capacity < 16 ? 16 : capacity;
        this._ListTracorDataProperty = new TracorDataProperty[this._Capacity];
    }

    private int _Capacity;

    public int Count { get; set; }

    public bool IsReadOnly => false;

    public void Add(TracorDataProperty item) {
        var index = this.Count++;
        if (this._Capacity <= index) {
            this._Capacity += 16;
            Array.Resize<TracorDataProperty>(ref this._ListTracorDataProperty, this._Capacity);
        }
        this._ListTracorDataProperty[index] = item;
    }

    public ref TracorDataProperty GetNext() {
        var index = this.Count++;
        if (this._Capacity <= index) {
            this._Capacity += 16;
            Array.Resize<TracorDataProperty>(ref this._ListTracorDataProperty, this._Capacity);
        }
        return ref this._ListTracorDataProperty[index];
    }

    public void Clear() {
        this.Count = 0;
    }

    public bool Contains(TracorDataProperty item) {
        return this.ListTracorDataProperty.Contains(item);
    }

    public void CopyTo(TracorDataProperty[] array, int arrayIndex) {
        this.ListTracorDataProperty.Slice(arrayIndex).CopyTo(array.AsSpan());
    }

    public IEnumerator<TracorDataProperty> GetEnumerator() {
        var count = this.Count;
        for (int index = 0; index < count; index++) {
            yield return this.ListTracorDataProperty[index];
        }
    }

    public bool Remove(TracorDataProperty item) {
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return this._ListTracorDataProperty.GetEnumerator();
    }
}

public sealed class TracorPropertySinkTargetPool
    : ReferenceCountPool<TracorPropertySinkTarget> {
    public TracorPropertySinkTargetPool() : base(16) { }

    protected override TracorPropertySinkTarget Create()
        => new TracorPropertySinkTarget(this);
}