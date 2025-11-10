using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Tracerit.Test.JSONLines;
public class JSONLinesFileTests {
    [Test]
    public async Task TestJSONLinesFile() {
        var jsonSerializerOptions = new JsonSerializerOptions()
            .AddTracorDataMinimalJsonConverter(null);
        jsonSerializerOptions.Converters.Add(new TracorDataPropertyMinimalJsonConverter());
        var logfile = GetFileFQN("log-tracor-sample-1.jsonl");
        using (var fileStream = System.IO.File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
            var listTracorDataRecord = await JsonLinesSerializer.DeserializeAsync<TracorDataRecord>(
                utf8Json: fileStream,
                options: jsonSerializerOptions,
                leaveOpen: true,
                cancellationToken: CancellationToken.None);
            await Assert.That(listTracorDataRecord.Count).IsGreaterThan(0);
        }
    }

    private static string GetFileFQN(
        string fileName,
        [CallerFilePath] string callerFilePath=""
        ) {
        return System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(callerFilePath)??string.Empty,
            fileName
            );
    }
}
