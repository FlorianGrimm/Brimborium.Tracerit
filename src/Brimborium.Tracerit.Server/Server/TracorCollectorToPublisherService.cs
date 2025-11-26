// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

public sealed class TracorCollectorToPublisherService : ITracorCollector {
    private readonly ITracorCollectivePublisher _TracorCollectivePublisher;

    public TracorCollectorToPublisherService(
        ITracorCollectivePublisher tracorCollectivePublisher
        ) {
        this._TracorCollectivePublisher = tracorCollectivePublisher;
    }

    public TracorDataCollection GetListTracorDataRecord(string? name) {
        throw new NotSupportedException();
    }

    public void Push(TracorDataRecord tracorDataRecord) {
        this._TracorCollectivePublisher.OnTrace(
            isPublic: true,
            tracorData: tracorDataRecord);
    }
}
