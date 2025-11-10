// MIT - Florian Grimm

using Microsoft.IO;

namespace Brimborium.Tracerit.Service;

public sealed class TracorMemoryPoolManager {
    private RecyclableMemoryStreamManager? _RecyclableMemoryStreamManager;

    public RecyclableMemoryStreamManager RecyclableMemoryStreamManager {
        get => this._RecyclableMemoryStreamManager ??= new RecyclableMemoryStreamManager();
        set => this._RecyclableMemoryStreamManager = value;
    }
}
