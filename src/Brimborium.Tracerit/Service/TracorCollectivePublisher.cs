
namespace Brimborium.Tracerit.Service;

public sealed class TracorCollectivePublisher : ITracorCollectivePublisher {
    private readonly Lock _LockWrite = new();
    // is treated as immutable - no need to Lock when reading
    private List<ITracorCollectiveSink> _ListSubscribedSinks = [];

    public TracorCollectivePublisher(
        IEnumerable<ITracorCollectiveSink> listSinks) {
        foreach (var sink in listSinks) {
            if (sink.IsEnabled()) {
                _ListSubscribedSinks.Add(sink);
            }
        }
    }

    public IDisposable SubscribeCollectiveSink(ITracorCollectiveSink sink) {
        using (this._LockWrite.EnterScope()) {
            this._ListSubscribedSinks = new(
                this._ListSubscribedSinks.Concat([sink])
            );
            System.Threading.Interlocked.MemoryBarrier();
        }
        return new RemoveSubscribedSink(this, sink);
    }
    internal void UnsubscribeCollectiveSink(ITracorCollectiveSink sink) {
        using (this._LockWrite.EnterScope()) {
            this._ListSubscribedSinks = new(
                this._ListSubscribedSinks.Where(s => s != sink)
            );
            System.Threading.Interlocked.MemoryBarrier();
        }
    }

    internal class RemoveSubscribedSink : IDisposable {
        private TracorCollectivePublisher _TracorCollectivePublisher;
        private ITracorCollectiveSink? _Sink;

        public RemoveSubscribedSink(TracorCollectivePublisher tracorCollectivePublisher, ITracorCollectiveSink sink) {
            this._TracorCollectivePublisher = tracorCollectivePublisher;
            this._Sink = sink;
        }

        public void Dispose() {
            if (System.Threading.Interlocked.Exchange(ref _Sink, null) is { } sink) {
                this._TracorCollectivePublisher.UnsubscribeCollectiveSink(sink);
            }

        }
    }

    public bool IsEnabled() {
        //    var listSinks = this._ListSubscribedSinks;
        //    if (0 == listSinks.Count) { return false; }
        //    foreach (var sink in listSinks) {
        //        if (sink.IsEnabled()) {
        //            return true;
        //        }
        //    }
        //    return false;
        return 0 < this._ListSubscribedSinks.Count;
    }

    public void OnTrace(bool isPublic, TracorIdentitfier callee, ITracorData tracorData) {
        var listSinks = this._ListSubscribedSinks;
        foreach (var sink in listSinks) {
            sink.OnTrace(isPublic, callee, tracorData);
        }
    }
}
