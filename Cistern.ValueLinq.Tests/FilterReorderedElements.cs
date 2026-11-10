using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Filter over an ordering or a shuffle is applied below it, and over a Reverse alone stays above it; every element
    // accessor of the result is checked against the same chain with Where, which stays where it is written, over keys
    // with ties so a selection's choice among equal keys shows.
    public class FilterReorderedElements
    {
        private static readonly (int Key, int Id)[][] Data =
        [
            [],
            [(3, 0)],
            [(5, 0), (3, 1), (8, 2), (3, 3), (9, 4), (5, 5), (1, 6), (8, 7), (5, 8), (2, 9)],
        ];

        private static readonly Func<(int Key, int Id), bool>[] Predicates =
        [
            _ => true,
            _ => false,
            p => p.Key >= 5,
            p => p.Key == 3,
            p => p.Id == 4,
        ];

        public static IEnumerable<object[]> Cases()
        {
            for (var d = 0; d < Data.Length; ++d)
                for (var p = 0; p < Predicates.Length; ++p)
                    yield return [d, p];
        }

        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void OverAnArray(int data, int predicate) => RunAll(Data[data].OfArray(), Predicates[predicate]);

        [Theory]
        [MemberData(nameof(Cases))]
        public void OverAList(int data, int predicate) => RunAll(new List<(int, int)>(Data[data]).OfList(), Predicates[predicate]);

        [Theory]
        [MemberData(nameof(Cases))]
        public void OverAnEnumerable(int data, int predicate) => RunAll(Iterate(Data[data]).OfEnumerable(), Predicates[predicate]);

        private static void RunAll<TNode, TEnumerator>(ValueEnumerable<(int Key, int Id), TNode, TEnumerator> source, Func<(int Key, int Id), bool> predicate)
            where TNode : INode<(int Key, int Id)>
            where TEnumerator : struct, IPullEnumerator<(int Key, int Id)>, INodeEnumerator<TNode, TEnumerator>
        {
            Check(source.OrderBy(p => p.Key).Where(predicate).ToArray(), source.OrderBy(p => p.Key).Filter(predicate));
            Check(source.OrderByDescending(p => p.Key).Where(predicate).ToArray(), source.OrderByDescending(p => p.Key).Filter(predicate));
            Check(source.OrderBy(p => p.Key).ThenByDescending(p => p.Id).Where(predicate).ToArray(), source.OrderBy(p => p.Key).ThenByDescending(p => p.Id).Filter(predicate));
            Check(source.Reverse().Where(predicate).ToArray(), source.Reverse().Filter(predicate));
            Check(source.OrderBy(p => p.Key).Reverse().Where(predicate).ToArray(), source.OrderBy(p => p.Key).Reverse().Filter(predicate));
            Check(source.Reverse().OrderBy(p => p.Key).Where(predicate).ToArray(), source.Reverse().OrderBy(p => p.Key).Filter(predicate));
            Check(source.Where(predicate).ToArray(), source.Filter(predicate));
            Check(source.Where(predicate).ToArray(), source.Map(p => p).Filter(predicate));
            Check(source.Where(predicate).ToArray(), source.Filter(predicate).Map(p => p));

            var shuffled = source.Shuffle().Filter(predicate);
            var kept = source.Where(predicate).ToArray();
            Assert.Equal(kept.Length, shuffled.Count());
            if (kept.Length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => shuffled.First());
                Assert.Throws<InvalidOperationException>(() => shuffled.Last());
                Assert.Equal(default, shuffled.FirstOrDefault());
                Assert.Equal(default, shuffled.LastOrDefault());
            }
            else
            {
                Assert.Contains(shuffled.First(), kept);
                Assert.Contains(shuffled.Last(), kept);
                Assert.Contains(shuffled.ElementAt(^kept.Length), kept);
                Assert.Equal(default, shuffled.ElementAtOrDefault(kept.Length));
            }
        }

        private static void Check<TNode, TEnumerator>((int Key, int Id)[] expected, ValueEnumerable<(int Key, int Id), TNode, TEnumerator> actual)
            where TNode : INode<(int Key, int Id)>
            where TEnumerator : struct, IPullEnumerator<(int Key, int Id)>, INodeEnumerator<TNode, TEnumerator>
        {
            var n = expected.Length;
            var fallback = (-1, -1);
            Func<(int Key, int Id), bool> odd = p => p.Id % 2 == 1;
            var odds = Array.FindAll(expected, p => odd(p));

            Assert.Equal(expected, actual.ToArray());
            Assert.Equal(n, actual.Count());

            if (n == 0)
            {
                Assert.Throws<InvalidOperationException>(() => actual.First());
                Assert.Throws<InvalidOperationException>(() => actual.Last());
                Assert.Throws<InvalidOperationException>(() => actual.Single());
            }
            else
            {
                Assert.Equal(expected[0], actual.First());
                Assert.Equal(expected[^1], actual.Last());
                if (n == 1)
                    Assert.Equal(expected[0], actual.Single());
                else
                    Assert.Throws<InvalidOperationException>(() => actual.Single());
            }

            Assert.Equal(n == 0 ? default : expected[0], actual.FirstOrDefault());
            Assert.Equal(n == 0 ? default : expected[^1], actual.LastOrDefault());
            Assert.Equal(n == 0 ? fallback : expected[0], actual.FirstOrDefault(fallback));
            Assert.Equal(n == 0 ? fallback : expected[^1], actual.LastOrDefault(fallback));
            if (n <= 1)
                Assert.Equal(n == 0 ? default : expected[0], actual.SingleOrDefault());
            else
                Assert.Throws<InvalidOperationException>(() => actual.SingleOrDefault());

            if (odds.Length == 0)
            {
                Assert.Throws<InvalidOperationException>(() => actual.First(odd));
                Assert.Throws<InvalidOperationException>(() => actual.Last(odd));
            }
            else
            {
                Assert.Equal(odds[0], actual.First(odd));
                Assert.Equal(odds[^1], actual.Last(odd));
            }
            Assert.Equal(odds.Length == 0 ? default : odds[0], actual.FirstOrDefault(odd));
            Assert.Equal(odds.Length == 0 ? default : odds[^1], actual.LastOrDefault(odd));
            Assert.Equal(odds.Length == 0 ? fallback : odds[0], actual.FirstOrDefault(odd, fallback));
            Assert.Equal(odds.Length == 0 ? fallback : odds[^1], actual.LastOrDefault(odd, fallback));

            for (var i = 0; i < n; ++i)
            {
                Assert.Equal(expected[i], actual.ElementAt(i));
                Assert.Equal(expected[i], actual.ElementAt(new Index(i)));
                Assert.Equal(expected[n - 1 - i], actual.ElementAt(^(i + 1)));
                Assert.Equal(expected[i], actual.ElementAtOrDefault(i));
                Assert.Equal(expected[n - 1 - i], actual.ElementAtOrDefault(^(i + 1)));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => actual.ElementAt(n));
            Assert.Throws<ArgumentOutOfRangeException>(() => actual.ElementAt(^(n + 1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => actual.ElementAt(^0));
            Assert.Throws<ArgumentOutOfRangeException>(() => actual.ElementAt(-1));
            Assert.Equal(default, actual.ElementAtOrDefault(n));
            Assert.Equal(default, actual.ElementAtOrDefault(^(n + 1)));
            Assert.Equal(default, actual.ElementAtOrDefault(^0));
            Assert.Equal(default, actual.ElementAtOrDefault(-1));

            var pulled = new List<(int, int)>();
            foreach (var item in actual)
                pulled.Add(item);
            Assert.Equal(expected, pulled);
        }
    }
}
