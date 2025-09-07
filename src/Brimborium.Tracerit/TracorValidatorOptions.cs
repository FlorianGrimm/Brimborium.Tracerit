namespace Brimborium.Tracerit;

public sealed class TracorValidatorOptions {
    public TracorValidatorOptions() { }
    public Dictionary<Type, ITracorDataAccessorFactory> TracorDataAccessorByType { get; } = new ();
    public List<ITracorDataAccessorFactory> ListTracorDataAccessor { get; } = new ();
    public TracorValidatorOptions AddTracorDataAccessorByType<T>(ITracorDataAccessorFactory<T> tracorDataAccessorFactory) {
        this.TracorDataAccessorByType.Add(typeof(T), tracorDataAccessorFactory);
        return this;
    }
}