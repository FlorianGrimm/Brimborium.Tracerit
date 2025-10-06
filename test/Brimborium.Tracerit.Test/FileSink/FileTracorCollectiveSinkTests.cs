using Brimborium.Tracerit.FileSink;

namespace Brimborium.Tracerit.Test.FileSink;

public class FileTracorCollectiveSinkTests {
    [Test]
    public async Task GetDirectory_baseDirectory_Test() {
        var root = System.IO.Path.Combine(GetDirectory(), "LogDestination");        
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory:root,
            getBaseDirectory: null, 
            directory: null);
        await Assert.That(act).IsEqualTo(root);
    }

    [Test]
    public async Task GetDirectory_getBaseDirectory_Test() {
        var root = System.IO.Path.Combine(GetDirectory(), "LogDestination");
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: null,
            getBaseDirectory: ()=>root,
            directory: null);
        await Assert.That(act).IsEqualTo(root);
    }
    [Test]
    public async Task GetDirectory_baseDirectory_Test() {
        var root = System.IO.Path.Combine(GetDirectory(), "LogDestination");
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: root,
            getBaseDirectory: null,
            directory: "Log");
        await Assert.That(act).IsEqualTo(root);
    }


    private static string GetDirectory() {
        return System.IO.Path.GetDirectoryName(GetFile())!;

        static string GetFile([CallerFilePath] string file = "") => file;
    }
}
