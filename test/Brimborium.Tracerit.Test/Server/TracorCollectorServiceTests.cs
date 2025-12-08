// MIT - Florian Grimm

using Brimborium.Tracerit.Server;

namespace Brimborium.Tracerit.Test.Server;

public class TracorCollectorServiceTests {
    [Test]
    public async Task PushMaxItemsTest() {
        var tracorCollectorService = new TracorServerCollectorServiceReadAndWrite(
            Options.Create<TracorCollectorOptions>(new TracorCollectorOptions() {
                Capacity = 128
            }));
        TracorDataRecordPool tracorDataRecordPool = new TracorDataRecordPool();
        for (int index = 0; index < 128; index++) {
            var tracorDataRecord = tracorDataRecordPool.Rent();
            tracorDataRecord.Add(TracorDataProperty.CreateIntegerValue("test", index));
            tracorCollectorService.Push(tracorDataRecord, null);
        }

        var list = tracorCollectorService.GetListTracorDataRecord(null);

        await Assert.That(list).IsNotNull().And.HasProperty(a => a.ListData.Count).IsEqualTo(128);
    }

    [Test]
    public async Task PushMoreItemsTest() {
        var tracorCollectorService = new TracorServerCollectorServiceReadAndWrite(
            Options.Create<TracorCollectorOptions>(new TracorCollectorOptions() {
                Capacity = 128
            }));
        TracorDataRecordPool tracorDataRecordPool = new TracorDataRecordPool();

        var tracorDataRecord0 = tracorDataRecordPool.Rent();
        {
            var canBeReturned0BeforePush = ((IReferenceCountObject)tracorDataRecord0).CanBeReturned();
            await Assert.That(canBeReturned0BeforePush).IsEqualTo(1);
        }

        tracorDataRecord0.Add(TracorDataProperty.CreateIntegerValue("test", -1));
        tracorCollectorService.Push(tracorDataRecord0, null);
        {
            var canBeReturned0AfterPush = ((IReferenceCountObject)tracorDataRecord0).CanBeReturned();
            await Assert.That(canBeReturned0AfterPush).IsEqualTo(2);
        }

        for (int index = 0; index < 127; index++) {
            using (var tracorDataRecord = tracorDataRecordPool.Rent()) {
                tracorDataRecord.Add(TracorDataProperty.CreateIntegerValue("test", index));
                tracorCollectorService.Push(tracorDataRecord, null);
            }
        }

        var tracorDataRecord127 = tracorDataRecordPool.Rent();
        tracorDataRecord127.Add(TracorDataProperty.CreateIntegerValue("test", 127));
        tracorCollectorService.Push(tracorDataRecord127, null);

        {
            var canBeReturned127a = ((IReferenceCountObject)tracorDataRecord127).CanBeReturned();
            await Assert.That(canBeReturned127a).IsEqualTo(2);
        }

        {
            var canBeReturned0c = ((IReferenceCountObject)tracorDataRecord0).CanBeReturned();
            await Assert.That(canBeReturned0c).IsEqualTo(1);
        }

        {
            tracorDataRecord0.Dispose();
            var canBeReturned0d = ((IReferenceCountObject)tracorDataRecord0).CanBeReturned();
            await Assert.That(canBeReturned0d).IsEqualTo(0);
        }

        var list = tracorCollectorService.GetListTracorDataRecord(null);

        await Assert.That(list).IsNotNull().And.HasProperty(a => a.ListData.Count).IsEqualTo(128);
    }


    [Test]
    public async Task GetListTracorDataRecordNamedTest() {
        var tracorCollectorService = new TracorServerCollectorServiceReadAndWrite(
            Options.Create<TracorCollectorOptions>(new TracorCollectorOptions() {
                Capacity = 128
            }));
        TracorDataRecordPool tracorDataRecordPool = new TracorDataRecordPool();
        {
            var list = tracorCollectorService.GetListTracorDataRecord("a");
            await Assert.That(list.ListData.Count).IsEquatableTo(0);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("b");
            await Assert.That(list.ListData.Count).IsEquatableTo(0);
        }
        for (int index = 0; index < 16; index++) {
            var tracorDataRecord = tracorDataRecordPool.Rent();
            tracorDataRecord.Add(TracorDataProperty.CreateIntegerValue("test", index));
            tracorCollectorService.Push(tracorDataRecord, null);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("a");
            await Assert.That(list.ListData.Count).IsEquatableTo(16);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("c");
            await Assert.That(list.ListData.Count).IsEquatableTo(16);
        }
        for (int index = 16; index < 32; index++) {
            var tracorDataRecord = tracorDataRecordPool.Rent();
            tracorDataRecord.Add(TracorDataProperty.CreateIntegerValue("test", index));
            tracorCollectorService.Push(tracorDataRecord, null);
        }
        { 
            var list = tracorCollectorService.GetListTracorDataRecord(null);
            await Assert.That(list).IsNotNull().And.HasProperty(a => a.ListData.Count).IsEqualTo(32);
        }

        {
            var list = tracorCollectorService.GetListTracorDataRecord("a");
            await Assert.That(list.ListData.Count).IsEquatableTo(16);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("b");
            await Assert.That(list.ListData.Count).IsEquatableTo(32);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("c");
            await Assert.That(list.ListData.Count).IsEquatableTo(16);
        }
        {
            var list = tracorCollectorService.GetListTracorDataRecord("d");
            await Assert.That(list.ListData.Count).IsEquatableTo(32);
        }
    }
}
