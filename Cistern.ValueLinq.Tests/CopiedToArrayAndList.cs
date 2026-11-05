using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToArray and ToList size their result once and have each part of the sequence copy itself into its slice, when
    // every part can; a part that cannot (an iterator, a Select) sends the whole sequence down the push path instead.
    public class CopiedToArrayAndList
    {
        private static readonly int[] Data = [10, 11, 12, 13, 14];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var item in values)
                yield return item;
        }

        private static void AssertBoth(IEnumerable<int> expected, int[] array, List<int> list)
        {
            Assert.Equal(expected, array);
            Assert.Equal(expected, list);
        }

        [Fact]
        public void Containers()
        {
            AssertBoth(Data, Data.OfArray().ToArray(), Data.OfArray().ToList());
            AssertBoth(Data, new List<int>(Data).OfList().ToArray(), new List<int>(Data).OfList().ToList());
            AssertBoth(Data, new ReadOnlyMemory<int>(Data).OfMemory().ToArray(), new ReadOnlyMemory<int>(Data).OfMemory().ToList());
            AssertBoth([3, 4, 5, 6], Enumerable.Range(3, 4).ToArray(), Enumerable.Range(3, 4).ToList());
            AssertBoth([7, 7, 7], Enumerable.Repeat(7, 3).ToArray(), Enumerable.Repeat(7, 3).ToList());
            AssertBoth([14, 13, 12, 11, 10], Data.Reverse().ToArray(), Data.Reverse().ToList());
            AssertBoth([14, 13, 12, 11, 10], new List<int>(Data).OfList().Reverse().ToArray(), new List<int>(Data).OfList().Reverse().ToList());
            AssertBoth([11, 12, 13], Data.Skip(1).Take(3).ToArray(), Data.Skip(1).Take(3).ToList());
            AssertBoth([], Enumerable.Range(3, 0).ToArray(), Enumerable.Range(3, 0).ToList());
        }

        [Fact]
        public void AppendPrepend_OverEachKindOfSource()
        {
            AssertBoth([9, 10, 11, 12, 13, 14, 15], Data.Prepend(9).Append(15).ToArray(), Data.Prepend(9).Append(15).ToList());
            AssertBoth([9, 10, 11, 12, 13, 14, 15], new List<int>(Data).Prepend(9).Append(15).ToArray(), new List<int>(Data).Prepend(9).Append(15).ToList());
            AssertBoth([9, 10, 11, 12, 13, 14, 15], new ReadOnlyMemory<int>(Data).Prepend(9).Append(15).ToArray(), new ReadOnlyMemory<int>(Data).Prepend(9).Append(15).ToList());
            AssertBoth([0, 3, 4, 5, 9], Enumerable.Range(3, 3).Prepend(0).Append(9).ToArray(), Enumerable.Range(3, 3).Prepend(0).Append(9).ToList());
            AssertBoth([0, 7, 7, 9], Enumerable.Repeat(7, 2).Prepend(0).Append(9).ToArray(), Enumerable.Repeat(7, 2).Prepend(0).Append(9).ToList());
            AssertBoth([0, 14, 13, 12, 11, 10, 9], Data.Reverse().Prepend(0).Append(9).ToArray(), Data.Reverse().Prepend(0).Append(9).ToList());
            AssertBoth([0, 9], Array.Empty<int>().Prepend(0).Append(9).ToArray(), Array.Empty<int>().Prepend(0).Append(9).ToList());
        }

        [Fact]
        public void AppendPrepend_Spilled()
        {
            var chain = Data.Prepend(3).Prepend(2).Prepend(1).Append(20).Append(21).Append(22);

            AssertBoth([1, 2, 3, 10, 11, 12, 13, 14, 20, 21, 22], chain.ToArray(), chain.ToList());
        }

        [Fact]
        public void AppendPrepend_ThroughSkipAndTake()
        {
            var inner = Data.Prepend(9).Append(15); // 9 10 11 12 13 14 15

            AssertBoth([10, 11, 12, 13, 14, 15], inner.Skip(1).ToArray(), inner.Skip(1).ToList());
            AssertBoth([12, 13, 14, 15], inner.Skip(3).ToArray(), inner.Skip(3).ToList());
            AssertBoth([15], inner.Skip(6).ToArray(), inner.Skip(6).ToList());
            AssertBoth([0, 11, 12, 13, 14, 15, 99], inner.Skip(2).Prepend(0).Append(99).ToArray(), inner.Skip(2).Prepend(0).Append(99).ToList());
            AssertBoth([0, 11, 12, 13, 99], Data.Skip(1).Take(3).Prepend(0).Append(99).ToArray(), Data.Skip(1).Take(3).Prepend(0).Append(99).ToList());
        }

        private sealed class ReadOnlyOnly(int[] values) : IReadOnlyList<int>
        {
            public int this[int index] => values[index];
            public int Count => values.Length;
            public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)values).GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Fact]
        public void Concat()
        {
            AssertBoth([10, 11, 12, 13, 14, 3, 4], Data.OfArray().Concat(Enumerable.Range(3, 2)).ToArray(), Data.OfArray().Concat(Enumerable.Range(3, 2)).ToList());

            IEnumerable<int> nested = Data;
            nested = nested.Concat(new List<int> { 1, 2 }).Concat(Enumerable.Repeat(7, 2)).Concat(Array.Empty<int>()).Concat(new[] { 8 });
            AssertBoth([10, 11, 12, 13, 14, 1, 2, 7, 7, 8], nested.ToArray(), nested.ToList());

            var withIterator = Data.OfArray().Concat(Iterate([1, 2]).OfEnumerable());
            AssertBoth([10, 11, 12, 13, 14, 1, 2], withIterator.ToArray(), withIterator.ToList());

            var inAppend = Data.OfArray().Concat(Enumerable.Range(3, 2)).Prepend(0).Append(9);
            AssertBoth([0, 10, 11, 12, 13, 14, 3, 4, 9], inAppend.ToArray(), inAppend.ToList());
        }

        [Fact]
        public void IListAndIReadOnlyList()
        {
            var collection = new System.Collections.ObjectModel.Collection<int>(new List<int>(Data));
            var readOnly = new ReadOnlyOnly(Data);

            AssertBoth([9, 10, 11, 12, 13, 14, 15], collection.Prepend(9).Append(15).ToArray(), collection.Prepend(9).Append(15).ToList());
            AssertBoth([9, 10, 11, 12, 13, 14, 15], readOnly.Prepend(9).Append(15).ToArray(), readOnly.Prepend(9).Append(15).ToList());
            AssertBoth([9, 11, 12, 13, 15], collection.Skip(1).Take(3).Prepend(9).Append(15).ToArray(), collection.Skip(1).Take(3).Prepend(9).Append(15).ToList());
            AssertBoth([9, 11, 12, 13, 15], readOnly.Skip(1).Take(3).Prepend(9).Append(15).ToArray(), readOnly.Skip(1).Take(3).Prepend(9).Append(15).ToList());
        }

        [Fact]
        public void ReversedSpan()
        {
            var reversed = Enumerable.FromSpan(Data, static (int[] data) => data.AsSpan()).Reverse();

            AssertBoth([14, 13, 12, 11, 10], reversed.ToArray(), reversed.ToList());
            AssertBoth([0, 14, 13, 12, 11, 10, 9], reversed.Prepend(0).Append(9).ToArray(), reversed.Prepend(0).Append(9).ToList());
        }

        [Fact]
        public void AppendPrepend_OverASourceThatCannotCopy()
        {
            AssertBoth([9, 10, 11, 12, 13, 14, 15], Iterate(Data).Prepend(9).Append(15).ToArray(), Iterate(Data).Prepend(9).Append(15).ToList());
            AssertBoth([9, 20, 22, 24, 26, 28, 15], Data.Select(x => x * 2).Prepend(9).Append(15).ToArray(), Data.Select(x => x * 2).Prepend(9).Append(15).ToList());
            AssertBoth([9, 10, 12, 14, 15], Data.Where(x => x % 2 == 0).Prepend(9).Append(15).ToArray(), Data.Where(x => x % 2 == 0).Prepend(9).Append(15).ToList());
        }
    }
}
