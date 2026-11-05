using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class LeftJoinedSequences
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
                var matched = false;
                foreach (var innerItem in inner)
                {
                    var innerKey = innerKeySelector(innerItem);
                    if (innerKey is not null
                        && equality.Equals(outerKey, innerKey))
                    {
                        expected.Add(resultSelector(outerItem, innerItem));
                        matched = true;
                    }
                }

                if (!matched)
                    expected.Add(resultSelector(outerItem, default));
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Inners() => [[], [0], [3, 20, 3, 37, 54], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsEachMatchInInnerOrderOrTheDefault(int length)
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

                    Assert.Equal(expected, items.LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.OfList().LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, enumerable.LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.LeftJoin(Iterate(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.LeftJoin(new List<int>(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).LeftJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.LeftJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(list.LeftJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().LeftJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(enumerable.LeftJoin(Iterate(inner), OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).LeftJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                }

                var withoutComparer = Expected(items, inner, OuterKey, InnerKey, Pair, null);

                Assert.Equal(withoutComparer, items.LeftJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, list.LeftJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, enumerable.LeftJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).LeftJoin(inner, OuterKey, InnerKey, Pair).ToArray());
            }
        }

        [Fact]
        public void EachOuterElementYieldsItsMatchesInInnerOrderOrOneResultWithTheDefault()
        {
            int[] outer = [1, 2, 3, 2];
            string[] inner = ["b2", "a1", "c2", "d4", "e1"];
            (int, string)[] expected = [(1, "a1"), (1, "e1"), (2, "b2"), (2, "c2"), (3, null), (2, "b2"), (2, "c2")];

            Assert.Equal(expected, outer.LeftJoin(inner, x => x, s => s[1] - '0', (o, i) => (o, i)).ToArray());
            Assert.Equal(expected, Pull(outer.LeftJoin(inner, x => x, s => s[1] - '0', (o, i) => (o, i))));
            Assert.Equal(expected, Pull(Iterate(outer).LeftJoin(Iterate(inner), x => x, s => s[1] - '0', (o, i) => (o, i))));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] inner = [2, 7, 7, 30, 11, 0, 25];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).LeftJoin(inner, OuterKey, InnerKey, (o, i) => o + i);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).LeftJoin(Iterate(inner), OuterKey, InnerKey, (o, i) => o * i);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).LeftJoin(inner, x => x % 31, InnerKey, Pair, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item <= keepAbove)
                        continue;

                    var matched = false;
                    foreach (var match in inner)
                    {
                        if (OuterKey(item) == InnerKey(match))
                        {
                            handCoded.Add(item + match);
                            matched = true;
                        }
                    }

                    if (!matched)
                        handCoded.Add(item);
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

            var ranged = Enumerable.Range(3, length).LeftJoin(inner, OuterKey, InnerKey, Pair);

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
                items.LeftJoin(inner, OuterKey, InnerKey, Pair),
                new List<int>(items).LeftJoin(inner, OuterKey, InnerKey, Pair),
                Iterate(items).LeftJoin(Iterate(inner), OuterKey, InnerKey, Pair),
                items.Select(x => x).LeftJoin(inner, OuterKey, InnerKey, Pair),
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

            Assert.True(Array.Empty<int>().LeftJoin(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out var empty));
            Assert.Equal(0, empty);

            Assert.False(new[] { 1 }.LeftJoin(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(new[] { 1 }.LeftJoin(Array.Empty<int>(), x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(Iterate(new[] { 1 }).LeftJoin(inner, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
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
            var sequence = outer.LeftJoin(inner, logOuterKey, logInnerKey, logResult);

            Assert.Empty(events);

            Assert.Equal(["1:0", "2:2", "2:2", "3:3", "2:2", "2:2"], sequence.ToList());

            string[] pushed =
            [
                "outer.GetEnumerator", .. Read("outer", 1),
                "inner.GetEnumerator", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 3", "ik 3", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 5", "ik 5", "inner.end", "inner.Dispose",
                "ok 1", "rs 1:0", .. Read("outer", 2), "ok 2", "rs 2:2", "rs 2:2", .. Read("outer", 3), "ok 3", "rs 3:3", .. Read("outer", 2), "ok 2", "rs 2:2", "rs 2:2",
                "outer.end", "outer.Dispose",
            ];

            Assert.Equal(pushed, events);

            events.Clear();

            Assert.Equal(["1:0", "2:2", "2:2", "3:3", "2:2", "2:2"], Pull(sequence));
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

            Assert.Empty(outer.LeftJoin(inner, x => x, x => x, Pair).ToList());
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Pull(outer.LeftJoin(inner, x => x, x => x, Pair)));
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Array.Empty<int>().LeftJoin(inner, x => x, x => x, Pair).ToList());
            Assert.Empty(Pull(Array.Empty<int>().LeftJoin(inner, x => x, x => x, Pair)));
            Assert.Empty(events);
        }

        [Fact]
        public void AnEmptyLookupYieldsTheDefaultForEveryOuterElement()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1, 2], events);
            var inner = new Logged("inner", [], events);

            string[] expected =
            [
                "outer.GetEnumerator", .. Read("outer", 1), "inner.GetEnumerator", "inner.end", "inner.Dispose",
                "ok 1", "rs 1:0", .. Read("outer", 2), "ok 2", "rs 2:0", "outer.end", "outer.Dispose",
            ];

            Assert.Equal(["1:0", "2:0"], outer.LeftJoin(inner, logOuterKey, x => x, logResult).ToList());
            Assert.Equal(expected, events);

            events.Clear();

            using (var enumerator = outer.LeftJoin(inner, logOuterKey, x => x, logResult).GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal("1:0", enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Equal("2:0", enumerator.Current);
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            Assert.Equal(expected, events);

            (int, int?)[] unmatched = [(1, null), (2, null)];

            Assert.Equal(unmatched, new[] { 1, 2 }.LeftJoin(new int?[] { null, null }, x => (int?)x, x => x, (o, i) => (o, i)).ToArray());

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
        }

        [Fact]
        public void AnEnumeratorNeverMovedNeverReadsInner()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1], events);
            var inner = new Logged("inner", [1], events);

            using (outer.LeftJoin(inner, x => x, x => x, Pair).GetEnumerator())
            {
            }

            Assert.DoesNotContain("inner.GetEnumerator", events);
        }

        [Fact]
        public void InnerIsReadAgainEachTimeTheSequenceIsRun()
        {
            var inner = new List<int> { 1 };
            int[] outer = [1, 2, 3];
            var sequence = outer.LeftJoin(inner, x => x, x => x, Pair);

            Assert.Equal([(1, 1), (2, 0), (3, 0)], sequence.ToArray());

            inner.Add(3);

            Assert.Equal([(1, 1), (2, 0), (3, 3)], sequence.ToArray());
            Assert.Equal([(1, 1), (2, 0), (3, 3)], Pull(sequence));

            inner.Clear();

            Assert.Equal([(1, 0), (2, 0), (3, 0)], Pull(sequence));
            Assert.Equal([(1, 0), (2, 0), (3, 0)], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheSelectorsRunOncePerElementAndResultOnEachRun()
        {
            var items = Scrambled(50);
            int[] inner = [3, 7, 20, 3];
            var outerSeen = new List<int>();
            var innerSeen = new List<int>();
            var results = 0;
            var expected = Expected(items, inner, OuterKey, InnerKey, Pair, null);
            var sequence = items.LeftJoin(inner, logOuterKey, logInnerKey, countResult);

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

            Assert.Equal((24, 1), outer.LeftJoin(inner, logged, x => x, Pair).First());
            Assert.Equal([24], seen);

            seen.Clear();

            Assert.Equal([(24, 1), (1, 1)], outer.LeftJoin(inner, logged, x => x, Pair).Take(2).ToArray());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal([(24, 1), (1, 1)], Iterate(outer).LeftJoin(inner, logged, x => x, Pair).Take(2).ToList());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal((25, 2), outer.Select(x => x).LeftJoin(inner, logged, x => x, Pair).First(x => x.Item1 == 25));
            Assert.Equal([24, 1, 2, 25], seen);

            int logged(int x)
            {
                seen.Add(x);

                return OuterKey(x);
            }
        }

        [Fact]
        public void TheComparerMatchesOuterKeysToInnerKeys()
        {
            string[] outer = ["apple", "Banana", "cherry", "date"];
            string[] inner = ["APRICOT", "blueberry", "Cranberry", "avocado"];

            (string, string)[] exact = [("apple", "avocado"), ("Banana", null), ("cherry", null), ("date", null)];

            Assert.Equal(exact, outer.LeftJoin(inner, x => x[..1], x => x[..1], (o, i) => (o, i)).ToArray());

            (string, string)[] ignoringCase = [("apple", "APRICOT"), ("apple", "avocado"), ("Banana", "blueberry"), ("cherry", "Cranberry"), ("date", null)];

            Assert.Equal(ignoringCase, outer.LeftJoin(inner, x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(ignoringCase, Pull(Iterate(outer).LeftJoin(Iterate(inner), x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void InnerElementsWithANullKeyAreLeftOutAndANullOuterKeyMatchesNothing()
        {
            string[] outer = ["a", null, "b"];
            string[] inner = [null, "b", "a", null];

            Assert.Equal(["aa", "-~", "bb"], outer.LeftJoin(inner, x => x, x => x, (o, i) => (o ?? "-") + (i ?? "~")).ToArray());
            Assert.Equal(["aa", "-~", "bb"], Pull(Iterate(outer).LeftJoin(Iterate(inner), x => x, x => x, (o, i) => (o ?? "-") + (i ?? "~"))));

            int?[] nullables = [null, 2, null];

            (int?, int?)[] paired = [(null, null), (2, 2), (null, null)];

            Assert.Equal(paired, nullables.LeftJoin(nullables, x => x, x => x, pairNullables).ToArray());
            Assert.Equal(paired, Pull(nullables.LeftJoin(nullables, x => x, x => x, pairNullables)));

            static (int?, int?) pairNullables(int? o, int? i) => (o, i);
        }

        [Fact]
        public void ANullOuterKeyMatchesWhereTheComparerHashesAnInnerKeyAsNull()
        {
            var everythingEqual = EqualityComparer<string>.Create((x, y) => true, x => 0);
            string[] outer = [null, "x"];
            string[] inner = ["b"];

            Assert.Equal(["-b", "xb"], outer.LeftJoin(inner, x => x, x => x, (o, i) => (o ?? "-") + i, everythingEqual).ToArray());
            Assert.Equal(["-b", "xb"], Pull(outer.LeftJoin(inner, x => x, x => x, (o, i) => (o ?? "-") + i, everythingEqual)));
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullKey = null;
            Func<int, int, int> nullResult = null;
            IEnumerable<int> nullInner = null;
            int[] inner = [1];

            Assert.Throws<ArgumentNullException>("outer", () => ((int[])null).LeftJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outer", () => ((List<int>)null).LeftJoin(inner, x => x, x => x, Pair));
            Assert.Throws<ArgumentNullException>("outer", () => ((IEnumerable<int>)null).LeftJoin(inner, x => x, x => x, Pair, ByTens));

            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.LeftJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.Select(x => x).LeftJoin(nullInner, nullKey, nullKey, nullResult, ByTens));

            Assert.Throws<ArgumentNullException>("outerKeySelector", () => new[] { 1 }.LeftJoin(inner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outerKeySelector", () => Iterate(new[] { 1 }).LeftJoin(inner, nullKey, x => x, Pair));

            Assert.Throws<ArgumentNullException>("innerKeySelector", () => new List<int> { 1 }.LeftJoin(inner, x => x, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("innerKeySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().LeftJoin(inner, x => x, nullKey, Pair));

            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.LeftJoin(inner, x => x, x => x, nullResult));
            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.Select(x => x).LeftJoin(inner, x => x, x => x, nullResult, ByTens));
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

            foreach (var _ in tracked.Iterate(10).LeftJoin(inner, x => x, x => x, Pair))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal((3, 3), tracked.Iterate(10).LeftJoin(inner, x => x, x => x, Pair).First(x => x.Item2 != 0));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([(0, 0), (1, 0)], tracked.Iterate(10).LeftJoin(inner, x => x, x => x, Pair).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).LeftJoin(inner, x => x < 5 ? x : throw new InvalidOperationException(), x => x, Pair).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).LeftJoin(inner, x => x, x => x, (o, i) => o < 5 ? o : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void InnerIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] outer = [1, 5, 2];

            Assert.Equal([(1, 1), (5, 0), (2, 2)], outer.LeftJoin(tracked.Iterate(3), x => x, x => x, Pair).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([(1, 1), (5, 0), (2, 2)], Pull(outer.LeftJoin(tracked.Iterate(3), x => x, x => x, Pair)));
            Assert.Equal(2, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => outer.LeftJoin(tracked.Iterate(3), x => x, x => x < 2 ? x : throw new InvalidOperationException(), Pair).ToList());
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
