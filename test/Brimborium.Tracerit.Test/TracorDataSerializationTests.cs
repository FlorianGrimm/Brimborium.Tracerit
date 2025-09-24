namespace Brimborium.Tracerit.Test;

public class TracorDataSerializationTests {
    public const string JsonData = """
        [
            [
                "Source:str:Activity",
                "Scope:str:sample.test1/Stop",
                "operation:str:test2"
            ],
            [
                "Source:str:Activity",
                "Scope:str:sample.test1/Stop",
                "operation:str:test3"
            ],
            [
                "Source:str:Activity",
                "Scope:str:sample.test1/Stop",
                "operation:str:test1"
            ]
        ]
        """;

    public readonly static System.Text.Json.JsonSerializerOptions JsonSerializerOptions
        = new System.Text.Json.JsonSerializerOptions() {
            WriteIndented = true,
            IndentSize = 4,
            IndentCharacter = ' '
        };

#warning TODO
    [Test, Explicit]
    public async Task ParseTracorDataCollectionTest() {
        var tracorDataCollection = TracorDataSerialization.ParseTracorDataCollection(JsonData);
        await Verify(tracorDataCollection);
    }

#warning TODO
    [Test, Explicit]
    public async Task Deserialization() {
        var tracorDataCollection = TracorDataSerialization.ParseTracorDataCollection(JsonData);
        await Assert.That(tracorDataCollection).IsNotNull();
        await Assert.That(tracorDataCollection.ListData.Count).IsEqualTo(3);

        await Assert.That(tracorDataCollection.ListData.Select(tdr => tdr.TracorIdentitfier?.Source ?? "").ToList()).IsEquivalentTo(["Activity", "Activity", "Activity"]);
        await Assert.That(tracorDataCollection.ListData.Select(tdr => tdr.TracorIdentitfier?.Scope ?? "")).IsEquivalentTo(["sample.test1/Stop", "sample.test1/Stop", "sample.test1/Stop"]);
        await Assert.That(tracorDataCollection.ListData.Select(rec => (rec["operation"] as string) ?? "")).IsEquivalentTo(["test2", "test3", "test1"]);

        string jsonAct = TracorDataSerialization.ToTracorDataCollectionJson(
            tracorDataCollection.ToListTracorIdentitfierData(),
            JsonSerializerOptions);

        var tracorDataCollectionAct = TracorDataSerialization.ParseTracorDataCollection(jsonAct);
        await Assert.That(tracorDataCollection).IsEquivalentTo(tracorDataCollectionAct);

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
                    TracorDataProperty.CreateLong("k", 123123123),
                    TracorDataProperty.CreateDouble("l", 12312.5),
                }
            }
            );

        string json = TracorDataSerialization.ToTracorDataCollectionJson(
            tracorDataCollection.ToListTracorIdentitfierData(),
            JsonSerializerOptions);

        var act = TracorDataSerialization.ParseTracorDataCollection(json);
        await Assert.That(tracorDataCollection).IsEquivalentTo(act);

        await Verify(json);
    }
}
