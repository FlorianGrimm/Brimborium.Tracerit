// MIT - Florian Grimm

namespace Brimborium.Tracerit.Server;

public sealed class TracorServerCollectorToPublisherService : ITracorServerCollectorWrite {
    private readonly ITracorCollectivePublisher _TracorCollectivePublisher;

    public TracorServerCollectorToPublisherService(
        ITracorCollectivePublisher tracorCollectivePublisher
        ) {
        this._TracorCollectivePublisher = tracorCollectivePublisher;
    }

    public void Push(TracorDataRecord tracorDataRecord, string? resourceName) {
        this._TracorCollectivePublisher.OnTrace(
            isPublic: true,
            tracorData: tracorDataRecord);
    }
}
