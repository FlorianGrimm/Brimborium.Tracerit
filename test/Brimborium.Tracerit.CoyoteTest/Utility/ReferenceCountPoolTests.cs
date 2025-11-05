#pragma warning disable IDE0079
#pragma warning disable CA1515 // Consider making public types internal
#pragma warning disable CA1305 // Specify IFormatProvider
#pragma warning disable SA1500 // Braces for multi-line statements should not share line
#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable CA2201 // Do not raise reserved exception types
#pragma warning disable CA1002 // Do not expose generic lists
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0039 // Use local function

namespace Brimborium.Tracerit.CoyoteTest.Utility;

public class ReferenceCountPoolTests {
    private const int _ThreadCount = 4;
    private const int _TestSizePerThread = 4096;
    private const int _TestSize = _ThreadCount * _TestSizePerThread;

    [Test]
    public void SingleThreadedRent() {
        // Arrange an array of new objects
        var sut = new TracorDataRecordPool(_TestSize);
        var listData = new TracorDataRecord[_TestSize];
        for (int i = 0; i < _TestSize; i++) {
            var item = sut.Rent();
            item.ListProperty.Add(new(i.ToString(), i));
            listData[i] = item;
        }

        // Act to return them
        for (int i = 0; i < _TestSize; i++) {
            listData[i].Dispose();
        }

        // Assert that the same references are returned
        for (int i = 0; i < _TestSize; i++) {
            Microsoft.Coyote.Specifications.Specification.Assert(ReferenceEquals(sut.Rent(), listData[i]), "Rent", i);
        }
    }

    [Test]
    public async Task MultiThreadedRent() {
        // Arrange an array of new objects
        var sut = new TracorDataRecordPool(_TestSize);
        var listData = new TracorDataRecord[_TestSize];
        for (int i = 0; i < _TestSize; i++) {
            var item = sut.Rent();
            listData[i] = item;
            item.ListProperty.Add(new TracorDataProperty("Index", i));
        }

        // Act to return them
        for (int i = 0; i < _TestSize; i++) {
            listData[i].Dispose();
        }


        // Assert that the same references are returned
        void Loop() {
            var chunkItems = new TracorDataRecord[_TestSizePerThread];

            for (int i = 0; i < _TestSizePerThread; i++) {
                var itemRent = sut.Rent();
                chunkItems[i] = itemRent;

                if (!itemRent.TryGetPropertyValueInteger("Index", out var index)) {
                    throw new Exception();
                }
                if (ReferenceEquals(itemRent, listData[index])) {
                    // OK
                } else {
                    throw new Exception();
                }
            }

            for (int i = 0; i < _TestSizePerThread; i++) {
                chunkItems[i].Dispose();
            }
        }

        Action repeatLoop = () => {
            for (int iRepeat = 0; iRepeat < 128; iRepeat++) {
                Loop();
            }
        };

        Task[] tasks = new Task[_ThreadCount];
        for (int iThread = 0; iThread < _ThreadCount; iThread++) {
            tasks[iThread] = Task.Run(repeatLoop);
        }
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
