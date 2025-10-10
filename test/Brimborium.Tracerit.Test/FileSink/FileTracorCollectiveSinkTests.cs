using Brimborium.Tracerit.FileSink;

namespace Brimborium.Tracerit.Test.FileSink;

public class FileTracorCollectiveSinkTests {
    [Test]
    public async Task GetDirectory_baseDirectory_Test() {
        var root = GetLogRootDirectory();
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: root,
            getBaseDirectory: null,
            directory: null);
        await Assert.That(act).IsEqualTo(root);
    }

    [Test]
    public async Task GetDirectory_getBaseDirectory_Test() {
        var root = GetLogRootDirectory();
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: null,
            getBaseDirectory: () => root,
            directory: null);
        await Assert.That(act).IsEqualTo(root);
    }

    [Test]
    public async Task GetDirectory_baseDirectory_directory_Test() {
        var root = GetLogRootDirectory();
        var rootLog2 = System.IO.Path.Combine(root, "Log2");
        System.IO.Directory.CreateDirectory(rootLog2);
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: root,
            getBaseDirectory: null,
            directory: "Log2");
        await Assert.That(act).IsEqualTo(rootLog2);
    }

    [Test]
    public async Task GetDirectory_getBaseDirectory_directory_Test() {
        var root = GetLogRootDirectory();
        var rootLog2 = System.IO.Path.Combine(root, "Log2");
        System.IO.Directory.CreateDirectory(rootLog2);
        var act = FileTracorCollectiveSink.GetDirectory(
            baseDirectory: null,
            getBaseDirectory: () => root,
            directory: "Log2");
        await Assert.That(act).IsEqualTo(rootLog2);
    }

    [Test]
    public async Task OnTraceHandlesReferenceCount() {
        var start = System.DateTime.UtcNow;
        var root = GetLogRootDirectory();
        var rootLog2 = System.IO.Path.Combine(root, "Log2");
        System.IO.Directory.CreateDirectory(rootLog2);
        TracorIdentitfier callee = new("test", "a", "b");
        TracorDataRecordPool tracorDataRecordPool = new TracorDataRecordPool(0);
        List<TracorDataRecord> listTracorDataRecord = new();
        var ctsApplicationStopping = new CancellationTokenSource();

        using (var sutFileTracorCollectiveSink = new FileTracorCollectiveSink(
            new TracorOptions() {
                ApplicationName = "test"
            },
            new FileTracorOptions() {
                BaseDirectory = root,
                Directory = "Log2",
                Period = TimeSpan.FromMinutes(30),
                FlushPeriod = TimeSpan.FromSeconds(10),
                GetApplicationStopping = (_) => ctsApplicationStopping.Token
            })) {
            for (int idx = 0; idx < 1000; idx++) {
                using (TracorDataRecord tracorDataRecord = tracorDataRecordPool.Rent()) {
                    tracorDataRecord.TracorIdentitfier = callee;
                    tracorDataRecord.Timestamp = DateTime.UtcNow;
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateInteger("idx", idx));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("c", "d"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e1", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e2", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e3", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e4", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e5", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e6", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e7", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e8", "f"));
                    tracorDataRecord.ListProperty.Add(TracorDataProperty.CreateString("e9", "f"));
                    sutFileTracorCollectiveSink.OnTrace(true, tracorDataRecord);
                    listTracorDataRecord.Add(tracorDataRecord);
                }
            }
            await Assert.That(listTracorDataRecord[0]).IsNotSameReferenceAs(listTracorDataRecord[1]);
            await sutFileTracorCollectiveSink.FlushAsync();
            var elapsed = System.DateTime.UtcNow - start;
            System.Console.Out.WriteLine(elapsed.TotalMilliseconds);
            ctsApplicationStopping.Cancel();

        }
    }

    private static string GetLogRootDirectory() {
        return System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(GetFile())!,
            "Log");

        static string GetFile([CallerFilePath] string file = "") => file;
    }
}
