namespace Brimborium.Tracerit.DataAccessor;

public sealed class JsonDocumentTracorDataPool
    : ReferenceCountPool<JsonDocumentTracorData> {
    public JsonDocumentTracorDataPool(int capacity) : base(capacity) { }
    protected override JsonDocumentTracorData Create() => new JsonDocumentTracorData(this);
}
