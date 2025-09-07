namespace Brimborium.Tracerit;

/*
 https://andrewlock.net/exploring-the-dotnet-8-preview-using-the-new-configuration-binder-source-generator/
https://github.com/martinothamar/Mediator
 */
public sealed record class TracorIdentitfier(string Callee) {
    public TracorIdentitfier(TracorIdentitfier? parent, string callee)
        : this(parent is { } ? $"{parent.Callee}/{callee}" : callee) {
    }
}
