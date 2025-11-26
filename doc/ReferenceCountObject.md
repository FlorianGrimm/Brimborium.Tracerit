# Reference Count Pool

A thread-safe object pooling system with reference counting for reusable objects.

## Location

`src\Brimborium.Tracerit\Utility\ReferenceCountPool.cs`

## Overview

This module provides a lock-free object pool that uses reference counting to manage object lifetimes. Objects are rented from the pool, used (potentially by multiple consumers via reference counting), and automatically returned when the reference count drops to zero.

## Components

### Interfaces

- **`IReferenceCountObject`**: Base interface for poolable objects. Extends `IDisposable` and provides reference counting operations.
- **`IReferenceCountObject<T>`**: Generic interface for objects that wrap a value of type `T`.
- **`IReferenceCountPool`**: Base pool interface with `Return()` method.
- **`IReferenceCountPool<T>`**: Generic pool interface with `Rent()` method.

### Classes

- **`ReferenceCountObject`**: Abstract base class implementing reference counting logic.
- **`ReferenceCountObject<T>`**: Generic version that wraps a value.
- **`ReferenceCountPool<T>`**: Abstract pool implementation with lock-free rent/return operations.

## Lifecycle

1. **Rent**: Call `pool.Rent()` to get an object (creates new if pool empty, reuses if available)
2. **Share**: Call `IncrementReferenceCount()` to share ownership
3. **Release**: Call `Dispose()` to decrement count; object returns to pool when count reaches 0

## Key Features

- **Thread-safe**: Uses `Interlocked` operations for lock-free concurrency
- **Quick slot**: Fast path for single recently-returned object
- **Circular buffer**: Ring buffer with configurable capacity (default 2048)
- **State validation**: Objects must pass `IsStateReset()` check before reuse

## Usage Example

```csharp
// Custom poolable object
public class MyData : ReferenceCountObject {
    public string Value;
    public MyData(IReferenceCountPool? owner) : base(owner) { }
    protected override void ResetState() => Value = null;
    protected override bool IsStateReset() => Value is null;
}

// Custom pool
public class MyDataPool : ReferenceCountPool<MyData> {
    public MyDataPool(int capacity = 0) : base(capacity) { }
    protected override MyData Create() => new MyData(this);
}

// Usage
var pool = new MyDataPool();
var obj = pool.Rent();        // Get from pool or create new
obj.Value = "hello";
obj.IncrementReferenceCount(); // Share with another consumer
obj.Dispose();                 // First consumer done (count=1)
obj.Dispose();                 // Last consumer done, returns to pool
```

## Implementations

- `TracorDataRecordPool` - Pool for trace data records
- `TracorPropertySinkTargetPool` - Pool for property sink targets
- `ActivityTracorDataPool` - Pool for activity trace data
- `JsonDocumentTracorDataPool` - Pool for JSON document trace data
