
namespace Brimborium.Tracerit.Service;

public sealed class TracorCollectivePublisher : ITracorCollectivePublisher {
    private readonly Lock _LockWrite = new();
    // is treated as immutable - no need to Lock when reading
    private List<ITracorCollectiveSink> _ListSubscribedSinks = [];

    public TracorCollectivePublisher(
        IEnumerable<ITracorCollectiveSink> listSinks) {
        foreach (var sink in listSinks) {
            if (sink.IsGeneralEnabled()) {
                this._ListSubscribedSinks.Add(sink);
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
            if (System.Threading.Interlocked.Exchange(ref this._Sink, null) is { } sink) {
                this._TracorCollectivePublisher.UnsubscribeCollectiveSink(sink);
            }

        }
    }
    public bool IsGeneralEnabled() => true;

    public bool IsEnabled() {
        return 0 < this._ListSubscribedSinks.Count;
    }

    public void OnTrace(bool isPublic, ITracorData tracorData) {
        var listSinks = this._ListSubscribedSinks;
        foreach (var sink in listSinks) {
            sink.OnTrace(isPublic, tracorData);
        }
    }
}
