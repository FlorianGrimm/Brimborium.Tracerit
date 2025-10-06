#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CA2201 // Do not raise reserved exception types
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0039 // Use local function

using Brimborium.Tracerit.DataAccessor;

namespace Brimborium.Tracerit.CoyoteTest.Utility;

public class ReferenceCountPoolTests {
    private const int ThreadCount = 4;
    private const int TestSizePerThread = 4096;
    private const int TestSize = ThreadCount * TestSizePerThread;

    [Test]
    public void SingleThreadedRent() {
        // Arrange an array of new objects
        var sut = new LoggerTracorDataPoolForTest(TestSize);
        var datas = new LoggerTracorDataForTest[TestSize];
        for (int i = 0; i < TestSize; i++) {
            var item = sut.Rent();
            item.Arguments.Add(new(i.ToString(), i));
            datas[i] = item;
        }

        // Act to return them
        for (int i = 0; i < TestSize; i++) {
            datas[i].Dispose();
        }

        // Assert that the same references are returned
        for (int i = 0; i < TestSize; i++) {
            Microsoft.Coyote.Specifications.Specification.Assert(ReferenceEquals(sut.Rent(), datas[i]), "Rent", i);
            //await Assert.That(sut.Rent()).IsSameReferenceAs(datas[i]);
            // if (ReferenceEquals(sut.Rent(), datas[i])) {
            //     // OK
            // } else {
            //     throw new Exception();
            // }
        }
    }

    [Test]
    public async Task MultiThreadedRent() {
        // Arrange an array of new objects
        var sut = new LoggerTracorDataPool(TestSize);
        var datas = new LoggerTracorData[TestSize];
        for (int i = 0; i < TestSize; i++) {
            var item = sut.Rent();
            datas[i] = item;
            item.Index = i;
        }

        // Act to return them
        for (int i = 0; i < TestSize; i++) {
            datas[i].Dispose();
        }


        // Assert that the same references are returned
        void Loop() {
            var chunkItems = new LoggerTracorData[TestSizePerThread];

            for (int i = 0; i < TestSizePerThread; i++) {
                var itemRent = sut.Rent();
                chunkItems[i] = itemRent;
                //await Assert.That(itemRent).IsSameReferenceAs(datas[itemRent.Index]);
                if (ReferenceEquals(itemRent, datas[itemRent.Index])) {
                    // OK
                } else {
                    throw new Exception();
                }
            }

            for (int i = 0; i < TestSizePerThread; i++) {
                chunkItems[i].Dispose();
            }
        }

        Action repeatLoop = () => {
            for (int iRepeat = 0; iRepeat < 128; iRepeat++) {
                Loop();
            }
        };

        Task[] tasks = new Task[ThreadCount];
        for (int iThread = 0; iThread < ThreadCount; iThread++) {
            tasks[iThread] = Task.Run(repeatLoop);
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}

public sealed class LoggerTracorDataPoolForTest : ReferenceCountPool<LoggerTracorDataForTest> {
    public LoggerTracorDataPoolForTest(int capacity) : base(capacity) {
    }

    protected override LoggerTracorDataForTest Create() {
        return new(this);
    }
}

public sealed class LoggerTracorDataForTest : ReferenceCountObject, ITracorData {
    private readonly List<KeyValuePair<string, object?>> _Arguments;

    public LoggerTracorDataForTest(IReferenceCountPool? referenceCountPool) : base(referenceCountPool) {
        this._Arguments = new(128);
    }

    protected override void ResetState() {
        this.Arguments.Clear();
    }

    protected override bool IsStateReseted() => 0 == this.Arguments.Count && this.Arguments.Capacity <= 128;

    public List<KeyValuePair<string, object?>> Arguments => this._Arguments;

    public object? this[string propertyName] {
        get {
            if (this.TryGetPropertyValue(propertyName, out var propertyValue)) {
                return propertyValue;
            } else {
                return null;
            }
        }
    }

    public List<string> GetListPropertyName() {
        return this._Arguments.Select(i => i.Key).ToList();
    }

    public bool TryGetPropertyValue(string propertyName, out object? propertyValue) {
        foreach (var arg in this._Arguments) {
            if (arg.Key == propertyName) {
                propertyValue = arg.Value;
                return true;
            }
        }
        propertyValue = null;
        return false;
    }

    public TracorIdentitfier? TracorIdentitfier { get; set; }

    public void ConvertProperties(List<TracorDataProperty> listProperty) {
    }
}
