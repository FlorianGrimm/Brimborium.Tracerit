namespace Brimborium.Tracerit.Utility;

public interface IReferenceCountObject : IDisposable {
    void IncrementReferenceCount();
    bool PrepareRent();
    long CanBeReturned();
}

public interface IReferenceCountObject<T> {
    T GetValue();
    void SetValue(T value);
}

public interface IReferenceCountPool {
    void Return(IReferenceCountObject value);
}
public interface IReferenceCountPool<T> : IReferenceCountPool {
    T Rent();
}

public abstract class ReferenceCountObject
    : IReferenceCountObject {
    private long _ReferenceCount;
    private readonly IReferenceCountPool? _Owner;

    protected ReferenceCountObject(IReferenceCountPool? owner) {
        this._ReferenceCount = 1;
        this._Owner = owner;
    }

    protected long GetReferenceCount() => this._ReferenceCount;

    public void IncrementReferenceCount() {
        Interlocked.Increment(ref this._ReferenceCount);
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose() {
        var result = Interlocked.Decrement(ref this._ReferenceCount);
        if (0 == result) {
            this.ResetState();
            this._Owner?.Return(this);
        }
    }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize

    protected abstract void ResetState();

    bool IReferenceCountObject.PrepareRent() {
        if (!this.IsStateReset()) { return false; }
        if (0 < this._ReferenceCount) { return false; }
        this._ReferenceCount = 1;
        return true;
    }
    protected abstract bool IsStateReset();

    long IReferenceCountObject.CanBeReturned() {
        var result = this._ReferenceCount;
        return result switch {
            0 => this.IsStateReset() switch {
                true => 0,
                false => long.MaxValue
            },
            _ => result
        };
    }
}

public abstract class ReferenceCountObject<T>
    : ReferenceCountObject, IReferenceCountObject<T>
    where T : class {
    protected T? _Value;

    protected ReferenceCountObject(IReferenceCountPool? owner) : base(owner) {
    }

    public T GetValue() => this._Value ?? throw new ObjectDisposedException(this.GetType().Name);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void SetValue(T value) {
        if (!ReferenceEquals(this._Value, null)) {
            throw new ObjectDisposedException(this.GetType().Name);
        }

        this._Value = value;
    }

    protected override bool IsStateReset() {
        return this._Value is null;
    }

    protected override void ResetState() {
        this._Value = null;
    }
}
public abstract class ReferenceCountPool<T>
    : IReferenceCountPool<T>
    where T : class, IReferenceCountObject {
    public const int DefaultMaxPoolSize = 2048;
    //public static LoggerTracorDataSharedPool Current = new(DefaultMaxPoolSize);

    public readonly int Capacity;
    private readonly T?[] _Pool;
    private long _RentIndex;
    private long _ReturnIndex;

    public ReferenceCountPool(int capacity = 0) {
        this.Capacity = 0 < capacity ? capacity : DefaultMaxPoolSize;
        this._Pool = new T?[this.Capacity];
    }

    public int Count => (int)(Volatile.Read(ref this._ReturnIndex) - Volatile.Read(ref this._RentIndex));

    public T Rent() {
        while (true) {
            var rentSnapshot = Volatile.Read(ref this._RentIndex);
            var returnSnapshot = Volatile.Read(ref this._ReturnIndex);

            if (rentSnapshot >= returnSnapshot) {
                break; // buffer is empty
            }

            if (Interlocked.CompareExchange(ref this._RentIndex, rentSnapshot + 1, rentSnapshot) == rentSnapshot) {
                {
                    var result = Interlocked.Exchange(ref this._Pool[rentSnapshot % this._Pool.Length], null);
                    if (result is { }) {
                        if (result.PrepareRent()) {
                            return result;
                        }
                    }
                }
                /*
                {
                    if (this.TryRentCoreRare(rentSnapshot, out var result)) {
                        if (result.CanBeRent()) {
                            return result;
                        }
                    } else {
                        continue;
                    }
                }
                */
            }
        }
        return this.Create();
    }

    protected abstract T Create();

    public void Return(IReferenceCountObject value) {
        if (value is not T valueT) {
            return;
        }

        if (0 != valueT.CanBeReturned()) {
            // 0 < valueT.CanBeReturned() -> buggy?
            // 0 > valueT.CanBeReturned() -> not ready
            return;
        }

        while (true) {
            var rentSnapshot = Volatile.Read(ref this._RentIndex);
            var returnSnapshot = Volatile.Read(ref this._ReturnIndex);

            if (returnSnapshot - rentSnapshot >= this.Capacity) {
                return; // buffer is full
            }

            if (Interlocked.CompareExchange(ref this._ReturnIndex, returnSnapshot + 1, returnSnapshot) == returnSnapshot) {
                this._Pool[returnSnapshot % this._Pool.Length] = valueT;
                return;
            }
        }
    }
}
