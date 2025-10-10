namespace Brimborium.Tracerit;

public static partial class ITracorSinkExtension {
    public static OptionalTracorPrivate GetPrivateTracor(
        this ITracorSink tracorSink,
        LogLevel logLevel,
        string message,
        [CallerMemberName]string scope=""
        ) {
        if (tracorSink.IsPrivateEnabled(scope, logLevel)) {
            return new(true, scope, logLevel, message, tracorSink);
        } else {
            return new(false, scope, logLevel, message, tracorSink);
        }
    }

    public static OptionalTracorPublic GetPublicTracor(
        this ITracorSink tracorSink,
        LogLevel logLevel,
        string message,
        [CallerMemberName] string scope = ""
        ) {
        if (tracorSink.IsPublicEnabled(scope, logLevel)) {
            return new(true, scope, logLevel, message, tracorSink);
        } else {
            return new(false, scope, logLevel, message, tracorSink);
        }
    }
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly record struct OptionalTracorPrivate {
    private readonly bool _Enabled;
    private readonly string _Scope;
    private readonly LogLevel _Level;
    private readonly string _Message;
    private readonly ITracorSink _TracorSink;

    public OptionalTracorPrivate(
        bool enabled,
        string scope,
        LogLevel level,
        string message,
        ITracorSink tracorSink) {
        this._Enabled = enabled;
        this._Scope = scope;
        this._Level = level;
        this._Message = message;
        this._TracorSink = tracorSink;
    }

    public bool Enabled => this._Enabled;

    public void TracePrivate<T>(T value) {
        if (this._Enabled) {
            this._TracorSink.TracePrivate<T>(this._Scope, this._Level, this._Message, value);
        }
    }

    private string GetDebuggerDisplay() {
        return $"{this._Enabled}, Private:{this._Scope}, {this._Level}, {this._Message}";
    }
}

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly record struct OptionalTracorPublic {
    private readonly bool _Enabled;
    private readonly string _Scope;
    private readonly LogLevel _Level;
    private readonly string _Message;
    private readonly ITracorSink _TracorSink;

    public OptionalTracorPublic(
        bool enabled,
        string scope,
        LogLevel level,
        string message,
        ITracorSink tracorSink) {
        this._Enabled = enabled;
        this._Scope = scope;
        this._Level = level;
        this._Message = message;
        this._TracorSink = tracorSink;
    }

    public bool Enabled => this._Enabled;

    public void TracePrivate<T>(T value) {
        if (this._Enabled) {
            this._TracorSink.TracePublic<T>(this._Scope, this._Level, this._Message, value);
        }
    }

    private string GetDebuggerDisplay() {
        return $"{this._Enabled}, Public:{this._Scope}, {this._Level}, {this._Message}";
    }
}
