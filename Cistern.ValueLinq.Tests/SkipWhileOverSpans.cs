using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // SkipWhile over a span tests elements until the first that fails the predicate, then hands the downstream the rest
    // of the span, and every later span, as they stand when the downstream takes spans, or one by one when it does not.
    // Either way the elements are those from that one on, and the predicate is called up to and including it.
    public class SkipWhileOverSpans
    {
        public static IEnumerable<object[]> Cases()
        {
            foreach (var length in new[] { 8, 63, 64, 65, 130, 200 })
                foreach (var stop in new[] { 0, 1, 49, 50, 51, 63, 64, 65, 129, int.MaxValue })
                    yield return [length, Math.Min(stop, length)];
        }

        private static int[] Data(int length)
        {
            var data = new int[length];
            for (var i = 0; i < length; ++i)
                data[i] = i;
            return data;
        }

        private sealed class Predicate(int stop)
        {
            public int Calls;
            public bool Test(int x) { ++Calls; return x < stop; }
        }

        // the elements are their own indexes, so an index out of step with the element is recorded
        private sealed class IndexedPredicate(int stop)
        {
            public int Calls;
            public bool IndexOutOfStep;
            public bool Test(int x, int index) { ++Calls; IndexOutOfStep |= x != index; return x < stop; }
        }

        private static void AssertCalls(Predicate predicate, int length, int stop) =>
            Assert.Equal(Math.Min(stop + 1, length), predicate.Calls);

        private static void AssertCalls(IndexedPredicate predicate, int length, int stop)
        {
            Assert.Equal(Math.Min(stop + 1, length), predicate.Calls);
            Assert.False(predicate.IndexOutOfStep);
        }

        // the source as consecutive arrays of 50, each handed down as its own span by the SelectMany
        private static IEnumerable<int[]> InSpans(int length)
        {
            var data = Data(length);
            for (var start = 0; start < length; start += 50)
                yield return data[start..Math.Min(length, start + 50)];
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Array_ToArray(int length, int stop)
        {
            var predicate = new Predicate(stop);
            Assert.Equal(Data(length)[stop..], Data(length).SkipWhile(predicate.Test).ToArray());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Array_SelectSum(int length, int stop)
        {
            var predicate = new Predicate(stop);
            var expected = 0L;
            for (var i = stop; i < length; ++i)
                expected += i * 2;
            Assert.Equal(expected, Data(length).SkipWhile(predicate.Test).Select(x => (long)x * 2).Sum());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Array_ThroughAForwarderWithoutSpans(int length, int stop)
        {
            var predicate = new Predicate(stop);
            Assert.Equal(Data(length)[Math.Min(length, stop + 1)..], Data(length).SkipWhile(predicate.Test).Skip(1).ToArray());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void List_ToList(int length, int stop)
        {
            var predicate = new Predicate(stop);
            Assert.Equal(Data(length)[stop..], new List<int>(Data(length)).SkipWhile(predicate.Test).ToList());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void AcrossSpans_ToArray(int length, int stop)
        {
            var predicate = new Predicate(stop);
            Assert.Equal(Data(length)[stop..], InSpans(length).SelectMany(span => span).SkipWhile(predicate.Test).ToArray());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void AcrossSpans_ThroughAForwarderWithoutSpans(int length, int stop)
        {
            var predicate = new Predicate(stop);
            Assert.Equal(Data(length)[Math.Min(length, stop + 1)..], InSpans(length).SelectMany(span => span).SkipWhile(predicate.Test).Skip(1).ToArray());
            AssertCalls(predicate, length, stop);
        }

        // The exception from a predicate that throws while skipping reaches the caller, and the predicate is not called
        // past it; one from the downstream, while it is handed the rest, reaches the caller too.
        [Theory]
        [InlineData(0)]
        [InlineData(63)]
        [InlineData(64)]
        [InlineData(100)]
        public void PredicateThrows(int at)
        {
            var calls = 0;
            Assert.Throws<InvalidOperationException>(() =>
                Data(200).SkipWhile(x => { ++calls; return x == at ? throw new InvalidOperationException() : true; }).ToArray());
            Assert.Equal(at + 1, calls);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 63)]
        [InlineData(10, 64)]
        [InlineData(100, 150)]
        public void DownstreamThrows(int stop, int at) =>
            Assert.Throws<InvalidOperationException>(() =>
                Data(200).SkipWhile(x => x < stop).Select(x => x == at ? throw new InvalidOperationException() : x).ToArray());

        [Theory]
        [MemberData(nameof(Cases))]
        public void Indexed_Array_ToArray(int length, int stop)
        {
            var predicate = new IndexedPredicate(stop);
            Assert.Equal(Data(length)[stop..], Data(length).SkipWhile(predicate.Test).ToArray());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Indexed_AcrossSpans_ToArray(int length, int stop)
        {
            var predicate = new IndexedPredicate(stop);
            Assert.Equal(Data(length)[stop..], InSpans(length).SelectMany(span => span).SkipWhile(predicate.Test).ToArray());
            AssertCalls(predicate, length, stop);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Indexed_Array_ThroughAForwarderWithoutSpans(int length, int stop)
        {
            var predicate = new IndexedPredicate(stop);
            Assert.Equal(Data(length)[Math.Min(length, stop + 1)..], Data(length).SkipWhile(predicate.Test).Skip(1).ToArray());
            AssertCalls(predicate, length, stop);
        }
    }
}
