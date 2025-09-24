#pragma warning disable IDE0130 // Namespace does not match folder structure

namespace Brimborium.Tracerit.DataAccessor;

public sealed class ActivityTracorDataPool : ReferenceCountPool<ActivityTracorData> {
    public static ActivityTracorDataPool Create(IServiceProvider provider)
        => new ActivityTracorDataPool();

    public ActivityTracorDataPool() : base() {
    }

    public ActivityTracorDataPool(int capacity) : base(capacity) {
    }


    protected override ActivityTracorData Create()
        => new ActivityTracorData();
}