using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class JoinedSequences
    {
        private static readonly IEqualityComparer<int> ByTens = EqualityComparer<int>.Create((x, y) => x % 10 == y % 10, x => x % 10);

        private static int[] Scrambled(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = (i * 37 + 11) % 101;

            return items;
        }

        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static int OuterKey(int x) => x % 23;

        private static int InnerKey(int x) => x % 17;

        private static (int, int) Pair(int outer, int inner) => (outer, inner);

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static List<TResult> Expected<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            var equality = comparer ?? EqualityComparer<TKey>.Default;
            var expected = new List<TResult>();
            foreach (var outerItem in outer)
            {
                var outerKey = outerKeySelector(outerItem);
                foreach (var innerItem in inner)
                {
                    var innerKey = innerKeySelector(innerItem);
                    if (innerKey is not null
                        && equality.Equals(outerKey, innerKey))
                    {
                        expected.Add(resultSelector(outerItem, innerItem));
                    }
                }
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Inners() => [[], [0], [3, 20, 3, 37, 54], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsEachMatchInInnerOrder(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var inner in Inners())
            {
                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
                {
                    var expected = Expected(items, inner, OuterKey, InnerKey, Pair, comparer);

                    Assert.Equal(expected, items.Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.OfList().Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, enumerable.Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.Join(Iterate(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.Join(new List<int>(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).Join(inner, OuterKey, InnerKey, Pair, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.Join(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(list.Join(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().Join(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(enumerable.Join(Iterate(inner), OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).Join(inner, OuterKey, InnerKey, Pair, comparer)));
                }

                var withoutComparer = Expected(items, inner, OuterKey, InnerKey, Pair, null);

                Assert.Equal(withoutComparer, items.Join(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, list.Join(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, enumerable.Join(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).Join(inner, OuterKey, InnerKey, Pair).ToArray());
            }
        }

        [Fact]
        public void EachOuterElementYieldsItsMatchesInInnerOrder()
        {
            int[] outer = [1, 2, 3, 2];
            string[] inner = ["b2", "a1", "c2", "d4", "e1"];
            (int, string)[] expected = [(1, "a1"), (1, "e1"), (2, "b2"), (2, "c2"), (2, "b2"), (2, "c2")];

            Assert.Equal(expected, outer.Join(inner, x => x, s => s[1] - '0', (o, i) => (o, i)).ToArray());
            Assert.Equal(expected, Pull(outer.Join(inner, x => x, s => s[1] - '0', (o, i) => (o, i))));
            Assert.Equal(expected, Pull(Iterate(outer).Join(Iterate(inner), x => x, s => s[1] - '0', (o, i) => (o, i))));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] inner = [2, 7, 7, 30, 11, 0, 25];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).Join(inner, OuterKey, InnerKey, (o, i) => o + i);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).Join(Iterate(inner), OuterKey, InnerKey, (o, i) => o * i);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).Join(inner, x => x % 31, InnerKey, Pair, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item <= keepAbove)
                        continue;

                    foreach (var match in inner)
                    {
                        if (OuterKey(item) == InnerKey(match))
                            handCoded.Add(item + match);
                    }
                }

                var sum = 0;
                foreach (var item in handCoded)
                    sum += item;

                Assert.Equal(handCoded, pulled);
                Assert.Equal(pulled, filtered.ToList());
                Assert.Equal(pulled, filtered.ToArray());
                Assert.Equal(sum, filtered.Sum());
                Assert.Equal(pulled.Count, filtered.Count());
                Assert.Equal(pulled, filtered.Select(x => x).ToList());
                Assert.Equal(Pull(iterated), iterated.ToList());
                Assert.Equal(Pull(iterated), iterated.Select(x => x).ToArray());
                Assert.Equal(Pull(concatenated), concatenated.ToList());
                Assert.Equal(Pull(concatenated), concatenated.Select(x => x).ToList());
            }

            var ranged = Enumerable.Range(3, length).Join(inner, OuterKey, InnerKey, Pair);

            Assert.Equal(Pull(ranged), ranged.ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            int[] inner = [1, 4, 9, 16, 20, 22, 0, 5, 18];
            var expected = Expected(items, inner, OuterKey, InnerKey, Pair, null).ToArray();

            var reversed = ((int, int)[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<(int, int)>[] sequences =
            [
                items.Join(inner, OuterKey, InnerKey, Pair),
                new List<int>(items).Join(inner, OuterKey, InnerKey, Pair),
                Iterate(items).Join(Iterate(inner), OuterKey, InnerKey, Pair),
                items.Select(x => x).Join(inner, OuterKey, InnerKey, Pair),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(reversed, sequence.Reverse().ToArray());
                Assert.Equal((0, 0), sequence.ElementAtOrDefault(expected.Length));

                for (var i = 0; i < expected.Length; i += Math.Max(1, expected.Length / 7))
                {
                    Assert.Equal(expected[i], sequence.ElementAt(i));
                    Assert.Equal(expected[^(i + 1)], sequence.ElementAt(^(i + 1)));
                }

                foreach (var count in new[] { 0, 1, 2, expected.Length, expected.Length + 1 })
                {
                    var skipped = expected[Math.Min(count, expected.Length)..];
                    var taken = expected[..Math.Min(count, expected.Length)];

                    Assert.Equal(skipped, sequence.Skip(count).ToArray());
                    Assert.Equal(taken, sequence.Take(count).ToArray());
                }

                if (expected is [var first, ..])
                {
                    Assert.Equal(first, sequence.First());
                    Assert.Equal(expected[^1], sequence.Last());
                }
            }
        }

        [Fact]
        public void OnlyAnEmptyOuterSequenceGivesACountWithoutEnumerating()
        {
            int[] inner = [1, 2];

            Assert.True(Array.Empty<int>().Join(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out var empty));
            Assert.Equal(0, empty);

            Assert.False(new[] { 1 }.Join(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(new[] { 1 }.Join(Array.Empty<int>(), x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(Iterate(new[] { 1 }).Join(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
        }

        private sealed class Logged(string name, int[] items, List<string> events)
            : IEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator()
            {
                events.Add($"{name}.GetEnumerator");

                var enumerator = new Enumerator(name, items, events);

                return enumerator;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private sealed class Enumerator(string name, int[] items, List<string> events)
                : IEnumerator<int>
            {
                private int _index = -1;

                public int Current
                {
                    get
                    {
                        var current = items[_index];
                        events.Add($"{name}.Current {current}");

                        return current;
                    }
                }

                object IEnumerator.Current => Current;

                public bool MoveNext()
                {
                    var moved = _index + 1 < items.Length;
                    if (moved)
                        ++_index;

                    events.Add(moved ? $"{name}.MoveNext" : $"{name}.end");

                    return moved;
                }

                public void Reset() => throw new NotSupportedException();

                public void Dispose() => events.Add($"{name}.Dispose");
            }
        }

        private static string[] Read(string name, params int[] items)
        {
            var events = new List<string>();
            foreach (var item in items)
            {
                events.Add($"{name}.MoveNext");
                events.Add($"{name}.Current {item}");
            }

            return [.. events];
        }

        [Fact]
        public void InnerIsReadWholeOnceOuterHasAnElementAndBeforeTheOuterKeyIsTaken()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1, 2, 3, 2], events);
            var inner = new Logged("inner", [2, 3, 2, 5], events);
            var sequence = outer.Join(inner, logOuterKey, logInnerKey, logResult);

            Assert.Empty(events);

            Assert.Equal(["2:2", "2:2", "3:3", "2:2", "2:2"], sequence.ToList());

            string[] pushed =
            [
                "outer.GetEnumerator", .. Read("outer", 1),
                "inner.GetEnumerator", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 3", "ik 3", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 5", "ik 5", "inner.end", "inner.Dispose",
                "ok 1", .. Read("outer", 2), "ok 2", "rs 2:2", "rs 2:2", .. Read("outer", 3), "ok 3", "rs 3:3", .. Read("outer", 2), "ok 2", "rs 2:2", "rs 2:2",
                "outer.end", "outer.Dispose",
            ];

            Assert.Equal(pushed, events);

            events.Clear();

            Assert.Equal(["2:2", "2:2", "3:3", "2:2", "2:2"], Pull(sequence));
            Assert.Equal(pushed, events);

            string logResult(int o, int i)
            {
                var result = $"{o}:{i}";
                events.Add($"rs {result}");

                return result;
            }

            int logOuterKey(int x)
            {
                events.Add($"ok {x}");

                return x;
            }

            int logInnerKey(int x)
            {
                events.Add($"ik {x}");

                return x;
            }
        }

        [Fact]
        public void AnEmptyOuterSequenceNeverReadsInner()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [], events);
            var inner = new Logged("inner", [1], events);

            Assert.Empty(outer.Join(inner, x => x, x => x, Pair).ToList());
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Pull(outer.Join(inner, x => x, x => x, Pair)));
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Array.Empty<int>().Join(inner, x => x, x => x, Pair).ToList());
            Assert.Empty(Pull(Array.Empty<int>().Join(inner, x => x, x => x, Pair)));
            Assert.Empty(events);
        }

        [Fact]
        public void AnEmptyLookupStopsTheOuterSequenceAtItsFirstElement()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1, 2, 3], events);
            var inner = new Logged("inner", [], events);
            var outerKeys = 0;

            string[] expected = ["outer.GetEnumerator", "outer.MoveNext", "outer.Current 1", "inner.GetEnumerator", "inner.end", "inner.Dispose", "outer.Dispose"];

            Assert.Empty(outer.Join(inner, countOuterKey, x => x, Pair).ToList());
            Assert.Equal(expected, events);

            events.Clear();

            using (var enumerator = outer.Join(inner, countOuterKey, x => x, Pair).GetEnumerator())
            {
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            Assert.Equal(expected, events);

            events.Clear();

            Assert.Empty(outer.Join(new int?[] { null, null }, x => (int?)countOuterKey(x), x => x, (o, i) => o).ToList());
            Assert.Equal(["outer.GetEnumerator", "outer.MoveNext", "outer.Current 1", "outer.Dispose"], events);

            Assert.Equal(0, outerKeys);

            int countOuterKey(int x)
            {
                ++outerKeys;

                return x;
            }
        }

        [Fact]
        public void AnEnumeratorNeverMovedNeverReadsInner()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1], events);
            var inner = new Logged("inner", [1], events);

            using (outer.Join(inner, x => x, x => x, Pair).GetEnumerator())
            {
            }

            Assert.DoesNotContain("inner.GetEnumerator", events);
        }

        [Fact]
        public void InnerIsReadAgainEachTimeTheSequenceIsRun()
        {
            var inner = new List<int> { 1 };
            int[] outer = [1, 2, 3];
            var sequence = outer.Join(inner, x => x, x => x, Pair);

            Assert.Equal([(1, 1)], sequence.ToArray());

            inner.Add(3);

            Assert.Equal([(1, 1), (3, 3)], sequence.ToArray());
            Assert.Equal([(1, 1), (3, 3)], Pull(sequence));

            inner.Clear();

            Assert.Empty(Pull(sequence));
            Assert.Empty(sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheSelectorsRunOncePerElementAndMatchOnEachRun()
        {
            var items = Scrambled(50);
            int[] inner = [3, 7, 20, 3];
            var outerSeen = new List<int>();
            var innerSeen = new List<int>();
            var results = 0;
            var expected = Expected(items, inner, OuterKey, InnerKey, Pair, null);
            var sequence = items.Join(inner, logOuterKey, logInnerKey, countResult);

            foreach (var run in new Func<List<(int, int)>>[] { () => sequence.ToList(), () => Pull(sequence), () => sequence.Select(x => x).ToList() })
            {
                outerSeen.Clear();
                innerSeen.Clear();
                results = 0;

                Assert.Equal(expected, run());
                Assert.Equal(items, outerSeen);
                Assert.Equal(inner, innerSeen);
                Assert.Equal(expected.Count, results);
            }

            (int, int) countResult(int o, int i)
            {
                ++results;

                return (o, i);
            }

            int logOuterKey(int x)
            {
                outerSeen.Add(x);

                return OuterKey(x);
            }

            int logInnerKey(int x)
            {
                innerSeen.Add(x);

                return InnerKey(x);
            }
        }

        [Fact]
        public void TheOuterKeySelectorRunsOnlyAsFarAsTheConsumerReads()
        {
            int[] outer = [24, 1, 2, 25, 3, 4];
            int[] inner = [1, 2, 3];
            var seen = new List<int>();

            Assert.Equal((24, 1), outer.Join(inner, logged, x => x, Pair).First());
            Assert.Equal([24], seen);

            seen.Clear();

            Assert.Equal([(24, 1), (1, 1)], outer.Join(inner, logged, x => x, Pair).Take(2).ToArray());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal([(24, 1), (1, 1)], Iterate(outer).Join(inner, logged, x => x, Pair).Take(2).ToList());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal((2, 2), outer.Select(x => x).Join(inner, logged, x => x, Pair).First(x => x.Item2 == 2));
            Assert.Equal([24, 1, 2], seen);

            int logged(int x)
            {
                seen.Add(x);

                return OuterKey(x);
            }
        }

        [Fact]
        public void TheComparerMatchesOuterKeysToInnerKeys()
        {
            string[] outer = ["apple", "Banana", "cherry"];
            string[] inner = ["APRICOT", "blueberry", "Cranberry", "avocado"];

            Assert.Equal([("apple", "avocado")], outer.Join(inner, x => x[..1], x => x[..1], (o, i) => (o, i)).ToArray());

            (string, string)[] ignoringCase = [("apple", "APRICOT"), ("apple", "avocado"), ("Banana", "blueberry"), ("cherry", "Cranberry")];

            Assert.Equal(ignoringCase, outer.Join(inner, x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(ignoringCase, Pull(Iterate(outer).Join(Iterate(inner), x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void InnerElementsWithANullKeyAreLeftOut()
        {
            string[] outer = ["a", null, "b"];
            string[] inner = [null, "b", "a", null];

            Assert.Equal(["aa", "bb"], outer.Join(inner, x => x, x => x, (o, i) => o + i).ToArray());
            Assert.Equal(["aa", "bb"], Pull(Iterate(outer).Join(Iterate(inner), x => x, x => x, (o, i) => o + i)));

            int?[] nullables = [null, 2, null];

            Assert.Equal([(2, 2)], nullables.Join(nullables, x => x, x => x, pairNullables).ToArray());
            Assert.Equal([(2, 2)], Pull(nullables.Join(nullables, x => x, x => x, pairNullables)));

            static (int?, int?) pairNullables(int? o, int? i) => (o, i);
        }

        [Fact]
        public void ANullOuterKeyMatchesWhereTheComparerHashesAnInnerKeyAsNull()
        {
            var everythingEqual = EqualityComparer<string>.Create((x, y) => true, x => 0);
            string[] outer = [null, "x"];
            string[] inner = ["b"];

            Assert.Equal(["-b", "xb"], outer.Join(inner, x => x, x => x, (o, i) => (o ?? "-") + i, everythingEqual).ToArray());
            Assert.Equal(["-b", "xb"], Pull(outer.Join(inner, x => x, x => x, (o, i) => (o ?? "-") + i, everythingEqual)));
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullKey = null;
            Func<int, int, int> nullResult = null;
            IEnumerable<int> nullInner = null;
            int[] inner = [1];

            Assert.Throws<ArgumentNullException>("outer", () => ((int[])null).Join(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outer", () => ((List<int>)null).Join(inner, x => x, x => x, Pair));
            Assert.Throws<ArgumentNullException>("outer", () => ((IEnumerable<int>)null).Join(inner, x => x, x => x, Pair, ByTens));

            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.Join(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.Select(x => x).Join(nullInner, nullKey, nullKey, nullResult, ByTens));

            Assert.Throws<ArgumentNullException>("outerKeySelector", () => new[] { 1 }.Join(inner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outerKeySelector", () => Iterate(new[] { 1 }).Join(inner, nullKey, x => x, Pair));

            Assert.Throws<ArgumentNullException>("innerKeySelector", () => new List<int> { 1 }.Join(inner, x => x, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("innerKeySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().Join(inner, x => x, nullKey, Pair));

            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.Join(inner, x => x, x => x, nullResult));
            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.Select(x => x).Join(inner, x => x, x => x, nullResult, ByTens));
        }

        private sealed class Tracked
        {
            public int Disposed;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                        yield return i;
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void TheOuterSequenceIsDisposedWhenTheOperatorStopsEarly()
        {
            var tracked = new Tracked();
            int[] inner = [3, 5, 7];

            foreach (var _ in tracked.Iterate(10).Join(inner, x => x, x => x, Pair))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal((3, 3), tracked.Iterate(10).Join(inner, x => x, x => x, Pair).First());
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([(3, 3), (5, 5)], tracked.Iterate(10).Join(inner, x => x, x => x, Pair).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Join(inner, x => x < 5 ? x : throw new InvalidOperationException(), x => x, Pair).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).Join(inner, x => x, x => x, (o, i) => o < 5 ? o : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void InnerIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] outer = [1, 5, 2];

            Assert.Equal([(1, 1), (2, 2)], outer.Join(tracked.Iterate(3), x => x, x => x, Pair).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([(1, 1), (2, 2)], Pull(outer.Join(tracked.Iterate(3), x => x, x => x, Pair)));
            Assert.Equal(2, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => outer.Join(tracked.Iterate(3), x => x, x => x < 2 ? x : throw new InvalidOperationException(), Pair).ToList());
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
