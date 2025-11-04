namespace Brimborium.Tracerit.Service;

public sealed class LateTracorDataConvertService {
    public static LateTracorDataConvertService Create(ITracorDataConvertService tracorDataConvertService) 
        =>new (tracorDataConvertService);
    private ITracorDataConvertService? _TracorDataConvertService;
    private IServiceProvider? _ServiceProvider;
    private LateTracorDataConvertService(ITracorDataConvertService tracorDataConvertService) {
        this._TracorDataConvertService = tracorDataConvertService;
    }

    public LateTracorDataConvertService(IServiceProvider serviceProvider) {
        this._ServiceProvider = serviceProvider;
    }

    public ITracorDataConvertService GetTracorDataConvertService() {
        if (this._TracorDataConvertService is { } result) {
            return result;
        }
        if (this._ServiceProvider is { } serviceProvider) { 
            return this._TracorDataConvertService ??= serviceProvider.GetRequiredService<ITracorDataConvertService>();
        }
        throw new NotSupportedException();
    }
}
