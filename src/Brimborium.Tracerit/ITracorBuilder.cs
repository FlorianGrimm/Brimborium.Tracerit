namespace Brimborium.Tracerit;

public interface ITracorBuilder { 
    public IServiceCollection Services { get; }
}

public class TracorBuilder(IServiceCollection services)
    : ITracorBuilder {
    public IServiceCollection Services { get; } = services;
}
