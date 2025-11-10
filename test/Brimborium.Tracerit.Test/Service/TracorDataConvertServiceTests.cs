using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brimborium.Tracerit.Test.Service;

public class TracorDataConvertServiceTests {
    [Test]
    public async Task ConvertListPropertyForITracorConvertToListProperty() {
        var sut = new TracorDataConvertService(new TracorDataRecordPool(0));
        sut.AddTracorConvertObjectToListProperty([new SomethingTracorConvertToListProperty()]);
        Something given = new("A", 2);
        TracorDataRecord act = new();
        
        sut.ConvertValueToListProperty(true, 1, string.Empty, given, act.ListProperty);

        await Assert.That(act.ListProperty.Count).IsEqualTo(2);
        await Assert.That(act.ListProperty[0].TryGetStringValue(out _)).IsTrue();
        await Assert.That(act.ListProperty[0].TryGetStringValue(out var a) ? a : "").IsEqualTo("A");
        await Assert.That(act.ListProperty[1].TryGetIntegerValue(out var b) ? b : 0).IsEqualTo(2);
    }
    private record class Something(string A, int B);
    private class SomethingTracorConvertToListProperty : TracorConvertValueToListProperty<Something> {
        public override void ConvertValueToListProperty(bool isPublic, int levelWatchDog, string name, Something value, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
            if (levelWatchDog < 0) { return; }
            var prefix = name is { Length: 0 } ? name : $"{name}.";

            listProperty.Add(TracorDataProperty.CreateStringValue($"{prefix}{nameof(value.A)}", value.A));
            listProperty.Add(TracorDataProperty.CreateIntegerValue($"{prefix}{nameof(value.B)}", value.B));
        }
    }

    [Test]
    public async Task ConvertListPropertyForITracorConvertSelfToListProperty() {
        var sut = new TracorDataConvertService(new TracorDataRecordPool(0));
        
        AnotherThing given = new("A", 2);
        TracorDataRecord act = new();

        sut.ConvertValueToListProperty(true, 1, string.Empty, given, act.ListProperty);

        await Assert.That(act.ListProperty.Count).IsEqualTo(2);
        await Assert.That(act.ListProperty[0].TryGetStringValue(out _)).IsTrue();
        await Assert.That(act.ListProperty[0].TryGetStringValue(out var a) ? a : "").IsEqualTo("A");
        await Assert.That(act.ListProperty[1].TryGetIntegerValue(out var b) ? b : 0).IsEqualTo(2);
    }

    private record class AnotherThing(string A, int B) : ITracorConvertSelfToListProperty {
        public void ConvertSelfToListProperty(bool isPublic, int levelWatchDog, string name, ITracorDataConvertService dataConvertService, List<TracorDataProperty> listProperty) {
            if (levelWatchDog < 0) { return; }
            var prefix = name is { Length: 0 } ? name : $"{name}.";

            listProperty.Add(TracorDataProperty.CreateStringValue($"{prefix}{nameof(this.A)}", this.A));
            listProperty.Add(TracorDataProperty.CreateIntegerValue($"{prefix}{nameof(this.B)}", this.B));
        }
    }

}
