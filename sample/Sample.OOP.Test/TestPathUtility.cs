// MIT - Florian Grimm


using System.Runtime.CompilerServices;

namespace Sample.WebApp;

public static class TestPathUtility {
    private static string? _GetProjectSourceCodeFolder;
    public static string GetProjectSourceCodeFolder() {
        return _GetProjectSourceCodeFolder ??= Intern();

        static string Intern([CallerFilePath] string value = "")
            => (System.IO.Path.GetDirectoryName(value) ?? throw new Exception());
    }

    private static string? _GetProjectAssemblyFolder;
    public static string GetProjectAssemblyFolder() {
        return _GetProjectAssemblyFolder ??= Intern();
        static string Intern()
            => (System.IO.Path.GetDirectoryName(typeof(TestPathUtility).Assembly.Location) ?? throw new Exception());
    }
}