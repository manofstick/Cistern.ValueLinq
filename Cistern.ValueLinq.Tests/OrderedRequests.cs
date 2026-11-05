using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An ordering answers Any and Contains from its source, the last match by one selection, an index by one
    // quickselect, and an integer Order's ends from its span: each answer is the one a full stable sort gives, ties
    // included, and the key selector is not called where nothing needs ordering.
    public class OrderedRequests
    {
        // pairs whose keys tie, so the answers depend on the sort's stability
        private static (int Key, int Id)[] Pairs(int length)
        {
            var pairs = new (int, int)[length];
            for (var i = 0; i < length; ++i)
                pairs[i] = ((i * 7919) % 13, i);
            return pairs;
        }

        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(50)]
        public void AnyAndContains_CallNoKey(int length)
        {
            var calls = 0;
            Func<int, int> key = x => { ++calls; return -x; };
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i;

            Assert.Equal(length > 0, values.OrderBy(key).Any());
            Assert.Equal(length > 0, Iterate(values).OrderBy(key).Any());
            Assert.Equal(length > 1, values.OrderBy(key).Contains(1));
            Assert.False(values.OrderBy(key).Contains(-1));
            Assert.Equal(0, calls);
        }

        // a window of an ordering, reversal or shuffle holds an element exactly when the source reaches past the skip,
        // answered from the source with no key computed
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void AnyOfAWindow_CallsNoKey(int length)
        {
            var calls = 0;
            Func<int, int> key = x => { ++calls; return -x; };
            var values = new int[length];
            for (var i = 0; i < length; ++i)
                values[i] = i;

            foreach (var skip in new[] { 0, 1, 4, 5, 9 })
            {
                foreach (var take in new[] { 1, 3 })
                {
                    var expected = length > skip;
                    Assert.Equal(expected, values.OrderBy(key).Skip(skip).Take(take).Any());
                    Assert.Equal(expected, Iterate(values).OrderBy(key).Skip(skip).Take(take).Any());
                    Assert.Equal(expected, values.Reverse().Skip(skip).Take(take).Any());
                    Assert.Equal(expected, Iterate(values).Shuffle().Skip(skip).Take(take).Any());
                }
            }
            Assert.Equal(0, calls);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(37)]
        [InlineData(600)]
        public void LastMatch_IsTheStableSortsLast(int length)
        {
            var pairs = Pairs(length);
            foreach (var divisor in new[] { 1, 2, 3, 1000 })
            {
                Func<(int Key, int Id), bool> predicate = p => p.Id % divisor == 0 && p.Id < 500;
                var expected = System.Linq.Enumerable.LastOrDefault(System.Linq.Enumerable.OrderBy(pairs, p => p.Key), predicate);
                var found = System.Linq.Enumerable.Any(pairs, predicate);

                Assert.Equal(expected, pairs.OrderBy(p => p.Key).LastOrDefault(predicate));
                Assert.Equal(System.Linq.Enumerable.LastOrDefault(System.Linq.Enumerable.OrderByDescending(pairs, p => p.Key), predicate), pairs.OrderByDescending(p => p.Key).LastOrDefault(predicate));
                Assert.Equal(System.Linq.Enumerable.LastOrDefault(System.Linq.Enumerable.OrderBy(pairs, p => p.Key), predicate, (-1, -1)), pairs.OrderBy(p => p.Key).LastOrDefault(predicate, (-1, -1)));

                if (found)
                    Assert.Equal(expected, pairs.OrderBy(p => p.Key).Last(predicate));
                else
                    Assert.Throws<InvalidOperationException>(() => pairs.OrderBy(p => p.Key).Last(predicate));
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(37)]
        [InlineData(600)]
        public void ElementAt_IsTheStableSortsElement(int length)
        {
            var pairs = Pairs(length);
            var sorted = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OrderBy(pairs, p => p.Key));
            var sortedDescending = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.OrderByDescending(pairs, p => p.Key));

            for (var i = 0; i < length; ++i)
            {
                Assert.Equal(sorted[i], pairs.OrderBy(p => p.Key).ElementAt(i));
                Assert.Equal(sorted[^(i + 1)], pairs.OrderBy(p => p.Key).ElementAt(^(i + 1)));
                Assert.Equal(sortedDescending[i], Iterate(pairs).OrderByDescending(p => p.Key).ElementAt(i));
                Assert.Equal(sorted[i], pairs.OrderBy(p => p.Key).Skip(i).First());
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => pairs.OrderBy(p => p.Key).ElementAt(length));
            Assert.Equal(default, pairs.OrderBy(p => p.Key).ElementAtOrDefault(length));
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [7], [64], [1000]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void Order_Ends_AreTheLeastAndGreatest(int length)
        {
            var random = new Random(length);
            var ints = new int[length];
            for (var i = 0; i < length; ++i)
                ints[i] = random.Next(-50, 50);

            Check(ints);
            Check(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(ints, x => (long)x * 1_000_000_007L)));
            Check(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(ints, x => (byte)(x + 50))));
            Check(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(ints, x => (sbyte)x)));
            Check(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(ints, x => (uint)(x + 50))));

            var list = new List<int>(ints);
            if (length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => list.Order().First());
                Assert.Equal(0, list.OrderDescending().LastOrDefault());
            }
            else
            {
                Assert.Equal(System.Linq.Enumerable.Min(ints), list.Order().First());
                Assert.Equal(System.Linq.Enumerable.Min(ints), list.OrderDescending().Last());
            }
        }

        private static void Check<T>(T[] values)
            where T : IComparable<T>
        {
            if (values.Length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => values.Order().First());
                Assert.Throws<InvalidOperationException>(() => values.OrderDescending().Last());
                Assert.Equal(default, values.Order().FirstOrDefault());
                return;
            }

            var min = System.Linq.Enumerable.Min(values);
            var max = System.Linq.Enumerable.Max(values);
            Assert.Equal(min, values.Order().First());
            Assert.Equal(max, values.Order().Last());
            Assert.Equal(max, values.OrderDescending().First());
            Assert.Equal(min, values.OrderDescending().Last());
            Assert.Equal(max, values.Order(Comparer<T>.Create((x, y) => y.CompareTo(x))).First());
        }
    }
}
