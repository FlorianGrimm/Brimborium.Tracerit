namespace Brimborium.Tracerit;

public interface ITracorCollectivePublisher: ITracorCollectiveSink {
    /// <summary>
    /// Subscribe to the tracor stream.
    /// </summary>
    /// <param name="sink">the sink to add</param>
    /// <returns>A <see cref="System.IDisposable"/> to remove the sink.</returns>
    IDisposable SubscribeCollectiveSink(ITracorCollectiveSink sink);
}
