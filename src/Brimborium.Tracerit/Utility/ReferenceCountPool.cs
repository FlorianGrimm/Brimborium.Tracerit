#pragma warning disable IDE0041 // Use 'is null' check

namespace Brimborium.Tracerit.Utility;

/// <summary>
/// Interface for reference-counted poolable objects. Implements IDisposable for automatic pool return.
/// </summary>
public interface IReferenceCountObject : IDisposable {
    /// <summary>
    /// Increments the reference count (for sharing ownership).
    /// </summary>
    void IncrementReferenceCount();

    /// <summary>
    /// Prepares the object for rent by resetting count to 1. Returns false if not ready.
    /// </summary>
    bool PrepareRent();

    /// <summary>
    /// Returns current reference count, or 0 if ready to return to pool, or MaxValue if state not reset.
    /// </summary>
    long CanBeReturned();
}

/// <summary>
/// Generic interface for reference-counted objects wrapping a value of type T.
/// </summary>
/// <typeparam name="T">The type of wrapped value.</typeparam>
public interface IReferenceCountObject<T> {
    /// <summary>
    /// Gets the wrapped value. Throws ObjectDisposedException if disposed.
    /// </summary>
    T GetValue();

    /// <summary>
    /// Sets the wrapped value. Can only be set once after rent.
    /// </summary>
    void SetValue(T value);
}

/// <summary>
/// Base interface for object pools that accept reference-counted objects.
/// </summary>
public interface IReferenceCountPool {
    /// <summary>
    /// Returns an object to the pool (called automatically when reference count reaches 0).
    /// </summary>
    void Return(IReferenceCountObject value);
}

/// <summary>
/// Generic pool interface for renting reference-counted objects.
/// </summary>
/// <typeparam name="T">The type of poolable object.</typeparam>
public interface IReferenceCountPool<T> : IReferenceCountPool {
    /// <summary>
    /// Rents an object from the pool. Creates new if pool is empty.
    /// </summary>
    T Rent();
}

/// <summary>
/// Abstract base class for reference-counted poolable objects. Starts with count=1, returns to pool when count=0.
/// </summary>
public abstract class ReferenceCountObject
    : IReferenceCountObject {
    private long _ReferenceCount;
    private readonly IReferenceCountPool? _Owner;

    /// <summary>
    /// Initializes with reference count 1 and optional owning pool.
    /// </summary>
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

/// <summary>
/// Generic reference-counted object that wraps a value of type T.
/// </summary>
/// <typeparam name="T">The type of wrapped value (must be a class).</typeparam>
public abstract class ReferenceCountObject<T>
    : ReferenceCountObject, IReferenceCountObject<T>
    where T : class {
    protected T? _Value;

    /// <summary>
    /// Initializes with optional owning pool.
    /// </summary>
    protected ReferenceCountObject(IReferenceCountPool? owner) : base(owner) {
    }

    /// <summary>
    /// Gets the wrapped value. Throws if disposed.
    /// </summary>
    public T GetValue() => this._Value ?? throw new ObjectDisposedException(this.GetType().Name);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public void SetValue(T value) {
        ObjectDisposedException.ThrowIf(!ReferenceEquals(this._Value, null), this);

        this._Value = value;
    }

    protected override bool IsStateReset() {
        return this._Value is null;
    }

    protected override void ResetState() {
        this._Value = null;
    }
}

/// <summary>
/// Thread-safe object pool using lock-free operations. Uses a ring buffer with quick slot for fast rent/return.
/// </summary>
/// <typeparam name="T">The poolable object type (must implement IReferenceCountObject).</typeparam>
public abstract class ReferenceCountPool<T>
    : IReferenceCountPool<T>
    where T : class, IReferenceCountObject {
    public const int DefaultMaxPoolSize = 2048;

    public readonly int Capacity;
    private readonly T?[] _Pool;
    private long _RentIndex;
    private long _ReturnIndex;
    private T? _Quick;

    /// <summary>
    /// Creates a pool with specified capacity (defaults to 2048).
    /// </summary>
    public ReferenceCountPool(int capacity = 0) {
        this.Capacity = 0 < capacity ? capacity : DefaultMaxPoolSize;
        this._Pool = new T?[this.Capacity];
    }

    /// <summary>
    /// Gets the approximate count of objects in the pool.
    /// </summary>
    public int Count => (int)(Volatile.Read(ref this._ReturnIndex) - Volatile.Read(ref this._RentIndex));

    /// <summary>
    /// Rents an object from pool. Returns from quick slot or ring buffer, creates new if empty.
    /// </summary>
    public T Rent() {
        var quick = Interlocked.Exchange(ref this._Quick, null);
        if (quick is not null) {
            quick.IncrementReferenceCount();
            return quick;
        }
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
        {
            var result = this.Create();
            return result;
        }
    }

    /// <summary>
    /// Factory method to create new poolable objects. Must pass 'this' as owner.
    /// </summary>
    protected abstract T Create();

    /// <summary>
    /// Returns an object to pool. Validates readiness, tries quick slot first, then ring buffer.
    /// </summary>
    public void Return(IReferenceCountObject value) {
        if (value is not T valueT) {
            return;
        }

        if (0 != valueT.CanBeReturned()) {
            // 0 < valueT.CanBeReturned() -> buggy?
            // 0 > valueT.CanBeReturned() -> not ready
            return;
        }

        if (ReferenceEquals(
            Interlocked.CompareExchange(ref this._Quick, valueT, null),
            null)) {
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
