namespace Brimborium.Tracerit.API;

public class ResponseWrapper { }
public interface IResponseWrapper { }
public interface IResponseFailed : IResponseWrapper {
    string Error { get; set; }
}
public class ResponseWrapper<T> : ResponseWrapper { }
public class ResponseSuccessful<T> : ResponseWrapper<T> {
    public required T Result { get; set; }
}
public class ResponseFailed<T> : ResponseWrapper<T>, IResponseFailed {
    public required string Error { get; set; }
}