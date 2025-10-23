#pragma warning disable IDE0009 // Member access should be qualified.

using System.Runtime.InteropServices;

namespace Brimborium.Tracerit;

public partial struct TracorDataProperty {
    private const int ValueBufferLength = 16;

    [System.Runtime.CompilerServices.InlineArray(ValueBufferLength)]
    public struct ValueBuffer {
        private byte _firstElement;
    }

    private Span<byte> GetValueWriteSpan() {
        return MemoryMarshal.CreateSpan(
            ref Unsafe.As<ValueBuffer, byte>(ref _ValueBuffer),
            ValueBufferLength);
    }

    private readonly ReadOnlySpan<byte> GetValueReadSpan() {
        return MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<ValueBuffer, byte>(ref Unsafe.AsRef(in _ValueBuffer)),
            ValueBufferLength);
    }
}
