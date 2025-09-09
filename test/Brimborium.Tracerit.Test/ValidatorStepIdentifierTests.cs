namespace Brimborium.Tracerit.Test;

public class ValidatorStepIdentifierTests {
    [Test]
    public async Task GetChildIdentifierInOrder() {
        var rootIdentifier = new ValidatorStepIdentifier(0, 1);
        List<ValidatorStepIdentifier> listActual = new(11);
        for (int idx = 0; idx <= 10; idx++) {
            listActual.Add(rootIdentifier.GetChildIdentifier(idx));
        }
        for (int idx = 0; idx <= 10; idx++) {
            var expected = listActual[idx];
            var act = rootIdentifier.GetChildIdentifier(idx);
            await Assert.That(act).IsSameReferenceAs(expected);
        }

    }

    [Test]
    public async Task GetChildIdentifierWithSpace() {
        var rootIdentifier = new ValidatorStepIdentifier(0, 1);
        var id10_1 = rootIdentifier.GetChildIdentifier(10);
        _ = rootIdentifier.GetChildIdentifier(5);
        var id10_2 = rootIdentifier.GetChildIdentifier(10);
        await Assert.That(id10_1).IsSameReferenceAs(id10_2);
    }
}