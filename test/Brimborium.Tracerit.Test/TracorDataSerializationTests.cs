#if later
namespace Brimborium.Tracerit.Test;

public class TracorDataSerializationTests {
    public const string JsonData = """
        [
            [
                "Source::str:Activity",
                "Scope::str:sample.test1.Stop",
                "operation::str:test2"
            ],
            [
                "Source::str:Activity",
                "Scope::str:sample.test1.Stop",
                "operation::str:test3"
            ],
            [
                "Source::str:Activity",
                "Scope::str:sample.test1.Stop",
                "operation::str:test1"
            ]
        ]
        """;

    public readonly static System.Text.Json.JsonSerializerOptions JsonSerializerOptions
        = new System.Text.Json.JsonSerializerOptions() {
            WriteIndented = true,
            IndentSize = 4,
            IndentCharacter = ' '
        };

    [Test]
    public async Task ParseTracorDataCollectionTest() {
        var tracorDataCollection = TracorDataSerialization.SerializeSimple(JsonData);
        await Verify(tracorDataCollection);
    }

    [Test]
    public async Task Deserialization() {
        var tracorDataRecordCollection = TracorDataSerialization.SerializeSimple(JsonData);
        await Assert.That(tracorDataRecordCollection).IsNotNull();
        await Assert.That(tracorDataRecordCollection.ListData.Count).IsEqualTo(3);

        await Assert.That(tracorDataRecordCollection.ListData.Select(tdr => tdr.TracorIdentitfier.Source ?? "").ToList()).IsEquivalentTo(["Activity", "Activity", "Activity"]);
        await Assert.That(tracorDataRecordCollection.ListData.Select(tdr => tdr.TracorIdentitfier.Scope ?? "")).IsEquivalentTo(["sample.test1.Stop", "sample.test1.Stop", "sample.test1.Stop"]);
        await Assert.That(tracorDataRecordCollection.ListData.Select(rec => (rec["operation"] as string) ?? "")).IsEquivalentTo(["test2", "test3", "test1"]);

        string jsonAct = TracorDataSerialization.ConvertToMinimizeStringJson(
            tracorDataRecordCollection,
            JsonSerializerOptions);

        var tracorDataCollectionAct = TracorDataSerialization.ParseTracorDataRecordCollectionCollection(jsonAct);
        await Assert.That(tracorDataRecordCollection).IsEquivalentTo(tracorDataCollectionAct);

        await Assert.That(jsonAct).IsEqualTo(JsonData);
    }


    [Test]
    public async Task DeSerializationAllTypesTest() {
        TracorDataCollection tracorDataCollection = new();
        tracorDataCollection.ListData.Add(
            new TracorDataRecord() {
                TracorIdentitfier = new("a", "b"),
                ListProperty = {
                    TracorDataProperty.CreateString("c","d"),
                    TracorDataProperty.CreateInteger("e",6),
                    TracorDataProperty.CreateLevelValue("f",LogLevel.Warning),
                    TracorDataProperty.CreateDateTime("g", new DateTime(2001,2,3,4,5,6)),
                    TracorDataProperty.CreateDateTimeOffset("h", new DateTimeOffset(new DateTime(2001,2,3,4,5,6), TimeSpan.Zero)),
                    TracorDataProperty.CreateBoolean("i", false),
                    TracorDataProperty.CreateBoolean("j", true),
                    TracorDataProperty.CreateInteger("k", 123123123),
                    TracorDataProperty.CreateFloat("l", 12312.5),
                }
            }
            );

        string json = TracorDataSerialization.ConvertToMinimizeStringJson(
            tracorDataCollection,
            JsonSerializerOptions);

        var act = TracorDataSerialization.ParseTracorDataRecordCollectionCollection(json);
        await Assert.That(tracorDataCollection).IsEquivalentTo(act);

        await Verify(json);
    }
}
#endif