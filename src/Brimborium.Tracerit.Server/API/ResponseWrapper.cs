namespace Brimborium.Tracerit.API;

public class ResponseWrapper { }

/// <summary>
/// Marker interface for API response wrappers.
/// </summary>
public interface IResponseWrapper { }

/// <summary>
/// Interface for failed API responses containing an error message.
/// </summary>
public interface IResponseFailed : IResponseWrapper {
    /// <summary>
    /// Gets or sets the error message describing the failure.
    /// </summary>
    string Error { get; set; }
}
public class ResponseWrapper<T> : ResponseWrapper { }
public class ResponseSuccessful<T> : ResponseWrapper<T> {
    public required T Result { get; set; }
}
public class ResponseFailed<T> : ResponseWrapper<T>, IResponseFailed {
    public required string Error { get; set; }
}