namespace Brimborium.Tracerit.Utility;

public interface IReferenceCountObject : IDisposable {
    void IncrementReferenceCount();
    bool PrepareRent();
    long CanBeReturned();
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

    public void IncrementReferenceCount() {
        Interlocked.Increment(ref this._ReferenceCount);
    }

    public void Dispose() {
        var result = Interlocked.Decrement(ref this._ReferenceCount);
        if (0 == result) {
            this.ResetState();
            this._Owner?.Return(this);
        }
    }

    protected abstract void ResetState();

    bool IReferenceCountObject.PrepareRent() {
        if (!this.IsStateReseted()) { return false; }
        if (0 != this._ReferenceCount) { return false; }
        this._ReferenceCount = 1;
        return true;
    }
    protected abstract bool IsStateReseted();

    long IReferenceCountObject.CanBeReturned() {
        var result = this._ReferenceCount;
        return result switch {
            0 => this.IsStateReseted() switch {
                true => this._ReferenceCount,
                false => long.MinValue
            },
            _ => result
        };
    }
}
public abstract class ReferenceCountPool<T>
    : IReferenceCountPool<T>
    where T : class, IReferenceCountObject {
    public const int DefaultMaxPoolSize = 2048;
    //public static LoggerTracorDataSharedPool Current = new(DefaultMaxPoolSize);

    public readonly int Capacity;
    private readonly T?[] pool;
    private long rentIndex;
    private long returnIndex;

    public ReferenceCountPool(int capacity) {
        this.Capacity = capacity;
        this.pool = new T?[capacity];
    }

    public int Count => (int)(Volatile.Read(ref this.returnIndex) - Volatile.Read(ref this.rentIndex));

    public T Rent() {
        while (true) {
            var rentSnapshot = Volatile.Read(ref this.rentIndex);
            var returnSnapshot = Volatile.Read(ref this.returnIndex);

            if (rentSnapshot >= returnSnapshot) {
                break; // buffer is empty
            }

            if (Interlocked.CompareExchange(ref this.rentIndex, rentSnapshot + 1, rentSnapshot) == rentSnapshot) {
                {
                    var result = Interlocked.Exchange(ref this.pool[rentSnapshot % this.Capacity], null);
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
            return;
        }

        while (true) {
            var rentSnapshot = Volatile.Read(ref this.rentIndex);
            var returnSnapshot = Volatile.Read(ref this.returnIndex);

            if (returnSnapshot - rentSnapshot >= this.Capacity) {
                return; // buffer is full
            }

            if (Interlocked.CompareExchange(ref this.returnIndex, returnSnapshot + 1, returnSnapshot) == returnSnapshot) {
                this.pool[returnSnapshot % this.Capacity] = valueT;
                return;
            }
        }
    }

    private bool TryRentCoreRare(long rentSnapshot, [NotNullWhen(true)] out T? logRecord) {
        SpinWait wait = default;
        while (true) {
            if (wait.NextSpinWillYield) {
                // Super rare case. If many threads are hammering
                // rent/return it is possible a read was issued an index and
                // then yielded while other threads caused the pointers to
                // wrap around. When the yielded thread wakes up its read
                // index could have been stolen by another thread. To
                // prevent deadlock, bail out of read after spinning. This
                // will cause either a successful rent from another index,
                // or a new record to be created
                logRecord = null;
                return false;
            }

            wait.SpinOnce();

            logRecord = Interlocked.Exchange(ref this.pool[rentSnapshot % this.Capacity], null);
            if (logRecord != null) {
                // Rare case where the write was still working when the read came in
                return true;
            }
        }
    }
}
