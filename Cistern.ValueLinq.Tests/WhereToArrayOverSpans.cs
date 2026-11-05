using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Where over a container with a span answers ToArray itself: one pass records the predicate's results as bits, a
    // second fills an exactly sized array. The elements are those the predicate accepts, in order, and the predicate is
    // called once per element, in order, whatever the length relative to the 32-element masks and to the masks kept on
    // the stack.
    public class WhereToArrayOverSpans
    {
        public static IEnumerable<object[]> Cases()
        {
            foreach (var length in new[] { 0, 1, 31, 32, 33, 64, 95, 100, 8191, 8192, 8193, 8224, 20000 })
                foreach (var pattern in new[] { "none", "all", "even", "thirds", "runs" })
                    yield return [length, pattern];
        }

        private static Func<int, bool> Pattern(string pattern) =>
            pattern switch
            {
                "none" => x => false,
                "all" => x => true,
                "even" => x => x % 2 == 0,
                "thirds" => x => x % 3 == 0,
                "runs" => x => (x / 40) % 2 == 0, // runs longer than a mask, crossing mask boundaries
                _ => throw new ArgumentException(pattern),
            };

        private static int[] Data(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i;
            return data;
        }

        private static int[] Expected(int[] data, Func<int, bool> predicate)
        {
            var expected = new List<int>();
            foreach (var x in data)
            {
                if (predicate(x))
                    expected.Add(x);
            }
            return expected.ToArray();
        }

        // the elements are their own indexes, so a call out of order or repeated is recorded
        private sealed class Recorder(Func<int, bool> predicate)
        {
            public int Calls;
            public bool OutOfOrder;
            public bool Test(int x) { OutOfOrder |= x != Calls; ++Calls; return predicate(x); }
        }

        private static void Check(int length, string pattern, Func<int[], Func<int, bool>, int[]> run)
        {
            var data = Data(length);
            var recorder = new Recorder(Pattern(pattern));

            var result = run(data, recorder.Test);

            Assert.Equal(Expected(data, Pattern(pattern)), result);
            Assert.Equal(length, recorder.Calls);
            Assert.False(recorder.OutOfOrder);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Array(int length, string pattern) =>
            Check(length, pattern, (data, predicate) => data.OfArray().Where(predicate).ToArray());

        [Theory]
        [MemberData(nameof(Cases))]
        public void List(int length, string pattern) =>
            Check(length, pattern, (data, predicate) => new List<int>(data).OfList().Where(predicate).ToArray());

        [Theory]
        [MemberData(nameof(Cases))]
        public void Memory(int length, string pattern) =>
            Check(length, pattern, (data, predicate) => new ReadOnlyMemory<int>(data).OfMemory().Where(predicate).ToArray());

        [Theory]
        [InlineData(33)]
        [InlineData(8193)]
        public void ReferenceElements(int length)
        {
            var data = new string[length];
            for (var i = 0; i < length; ++i)
                data[i] = i.ToString();

            var result = data.OfArray().Where(s => s.Length % 2 == 1).ToArray();

            Assert.Equal(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(data, s => s.Length % 2 == 1)), result);
        }

        // the exception reaches the caller and the predicate is not called past the element that threw
        [Theory]
        [InlineData(0)]
        [InlineData(31)]
        [InlineData(32)]
        [InlineData(9000)]
        public void PredicateThrows(int at)
        {
            var calls = 0;
            Assert.Throws<InvalidOperationException>(() =>
                Data(10000).OfArray().Where(x => { ++calls; return x == at ? throw new InvalidOperationException() : true; }).ToArray());
            Assert.Equal(at + 1, calls);
        }
    }
}
