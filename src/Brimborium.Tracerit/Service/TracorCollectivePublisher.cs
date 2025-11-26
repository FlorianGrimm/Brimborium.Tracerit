
namespace Brimborium.Tracerit.Service;

/// <summary>
/// Publisher service that manages multiple subscribed sinks and distributes trace events to all of them.
/// Implements the pub/sub pattern for trace data distribution.
/// </summary>
public sealed class TracorCollectivePublisher : ITracorCollectivePublisher {
    private readonly Lock _LockWrite = new();
    private readonly TracorEmergencyLogging _TracorEmergencyLogging;

    // is treated as immutable - no need to Lock when reading
    private ITracorCollectiveSink[] _ListSubscribedSinks = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TracorCollectivePublisher"/> class with the specified sinks.
    /// </summary>
    /// <param name="listSinks">The initial collection of sinks to subscribe.</param>
    /// <param name="tracorEmergencyLogging">The emergency logging service for diagnostics.</param>
    public TracorCollectivePublisher(
        IEnumerable<ITracorCollectiveSink> listSinks,
        TracorEmergencyLogging tracorEmergencyLogging
        ) {
        this._TracorEmergencyLogging = tracorEmergencyLogging;
        List<ITracorCollectiveSink> listSubscribedSinks = new(listSinks.Count());
        foreach (var sink in listSinks) {
            if (sink.IsGeneralEnabled()) {
                if (tracorEmergencyLogging.IsEnabled) {
                    tracorEmergencyLogging.Log($"TracorCollectivePublisher add sink {sink.GetType().Name}");
                }
                listSubscribedSinks.Add(sink);
            } else {
                if (tracorEmergencyLogging.IsEnabled) {
                    tracorEmergencyLogging.Log($"TracorCollectivePublisher ignore sink {sink.GetType().Name}");
                }
            }
        }
        this._ListSubscribedSinks = listSubscribedSinks.ToArray();
    }

    public IDisposable SubscribeCollectiveSink(ITracorCollectiveSink sink) {
        using (this._LockWrite.EnterScope()) {
            List<ITracorCollectiveSink> listSubscribedSinks = new(this._ListSubscribedSinks.Length + 1);
            listSubscribedSinks.AddRange(this._ListSubscribedSinks);
            listSubscribedSinks.Add(sink);
            this._ListSubscribedSinks = listSubscribedSinks.ToArray();
            System.Threading.Interlocked.MemoryBarrier();
        }
        return new RemoveSubscribedSink(this, sink);
    }
    internal void UnsubscribeCollectiveSink(ITracorCollectiveSink sink) {
        using (this._LockWrite.EnterScope()) {
            List<ITracorCollectiveSink> listSubscribedSinks = new(this._ListSubscribedSinks.Length);
            listSubscribedSinks.AddRange(this._ListSubscribedSinks.Where(s => s != sink));
            this._ListSubscribedSinks = listSubscribedSinks.ToArray();
            System.Threading.Interlocked.MemoryBarrier();
        }
    }

    internal class RemoveSubscribedSink : IDisposable {
        private readonly TracorCollectivePublisher _TracorCollectivePublisher;
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

    public bool IsEnabled() => (0 < this._ListSubscribedSinks.Length);

    public void OnTrace(bool isPublic, ITracorData tracorData) {
        var listSinks = this._ListSubscribedSinks;
        for (int idx = 0; idx < listSinks.Length; idx++) {
            listSinks[idx].OnTrace(isPublic, tracorData);
        }
    }
}
