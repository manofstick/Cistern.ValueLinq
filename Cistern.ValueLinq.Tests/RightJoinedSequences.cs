using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class RightJoinedSequences
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
            foreach (var innerItem in inner)
            {
                var innerKey = innerKeySelector(innerItem);
                var matched = false;
                foreach (var outerItem in outer)
                {
                    var outerKey = outerKeySelector(outerItem);
                    if (outerKey is not null
                        && equality.Equals(outerKey, innerKey))
                    {
                        expected.Add(resultSelector(outerItem, innerItem));
                        matched = true;
                    }
                }

                if (!matched)
                    expected.Add(resultSelector(default, innerItem));
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Others() => [[], [0], [3, 20, 3, 37, 54], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsEachMatchInOuterOrderOrTheDefault(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var inner in Others())
            {
                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
                {
                    var expected = Expected(items, inner, OuterKey, InnerKey, Pair, comparer);

                    Assert.Equal(expected, items.RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, list.OfList().RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, enumerable.RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.RightJoin(Iterate(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.RightJoin(new List<int>(inner), OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).RightJoin(inner, OuterKey, InnerKey, Pair, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.RightJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(list.RightJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().RightJoin(inner, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(enumerable.RightJoin(Iterate(inner), OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).RightJoin(inner, OuterKey, InnerKey, Pair, comparer)));

                    var swapped = Expected(inner, items, OuterKey, InnerKey, Pair, comparer);

                    Assert.Equal(swapped, inner.RightJoin(items, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(swapped, inner.RightJoin(list, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(swapped, inner.RightJoin(enumerable, OuterKey, InnerKey, Pair, comparer).ToArray());
                    Assert.Equal(swapped, Pull(inner.RightJoin(list, OuterKey, InnerKey, Pair, comparer)));
                    Assert.Equal(swapped, Pull(inner.RightJoin(enumerable, OuterKey, InnerKey, Pair, comparer)));
                }

                var withoutComparer = Expected(items, inner, OuterKey, InnerKey, Pair, null);

                Assert.Equal(withoutComparer, items.RightJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, list.RightJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, enumerable.RightJoin(inner, OuterKey, InnerKey, Pair).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).RightJoin(inner, OuterKey, InnerKey, Pair).ToArray());
            }
        }

        [Fact]
        public void EachInnerElementYieldsItsMatchesInOuterOrderOrOneResultWithTheDefault()
        {
            string[] outer = ["b2", "a1", "c2", "d4", "e1"];
            int[] inner = [1, 2, 3, 2];
            (string, int)[] expected = [("a1", 1), ("e1", 1), ("b2", 2), ("c2", 2), (null, 3), ("b2", 2), ("c2", 2)];

            Assert.Equal(expected, outer.RightJoin(inner, s => s[1] - '0', x => x, (o, i) => (o, i)).ToArray());
            Assert.Equal(expected, Pull(outer.RightJoin(inner, s => s[1] - '0', x => x, (o, i) => (o, i))));
            Assert.Equal(expected, Pull(Iterate(outer).RightJoin(Iterate(inner), s => s[1] - '0', x => x, (o, i) => (o, i))));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] outer = [2, 7, 7, 30, 11, 0, 25];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = outer.RightJoin(items.Where(x => x > keepAbove), OuterKey, InnerKey, (o, i) => o + i);
                var iterated = Iterate(outer).Where(x => x > 5).RightJoin(Iterate(items).Where(x => x > keepAbove).Select(x => x + 1), OuterKey, InnerKey, (o, i) => o * i);
                var concatenated = outer.Concat(Scrambled(length / 2)).RightJoin(items.Where(x => x > keepAbove).Concat(Scrambled(length / 3)), x => x % 31, InnerKey, Pair, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item <= keepAbove)
                        continue;

                    var matched = false;
                    foreach (var match in outer)
                    {
                        if (OuterKey(match) == InnerKey(item))
                        {
                            handCoded.Add(match + item);
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

            var ranged = Enumerable.Range(3, length).RightJoin(outer, OuterKey, InnerKey, Pair);
            var rangedInner = outer.RightJoin(Enumerable.Range(3, length), OuterKey, InnerKey, Pair);

            Assert.Equal(Pull(ranged), ranged.ToList());
            Assert.Equal(Pull(rangedInner), rangedInner.ToList());
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            int[] outer = [1, 4, 9, 16, 20, 22, 0, 5, 18];
            var expected = Expected(outer, items, OuterKey, InnerKey, Pair, null).ToArray();

            var reversed = ((int, int)[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<(int, int)>[] sequences =
            [
                outer.RightJoin(items, OuterKey, InnerKey, Pair),
                new List<int>(outer).RightJoin(new List<int>(items), OuterKey, InnerKey, Pair),
                Iterate(outer).RightJoin(Iterate(items), OuterKey, InnerKey, Pair),
                outer.Select(x => x).RightJoin(items.Select(x => x), OuterKey, InnerKey, Pair),
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
        public void OnlyAnEmptyInnerSequenceGivesACountWithoutEnumerating()
        {
            int[] outer = [1, 2];

            Assert.True(outer.RightJoin(Array.Empty<int>(), x => x, x => x, Pair).TryGetNonEnumeratedCount(out var empty));
            Assert.Equal(0, empty);

            Assert.False(outer.RightJoin(new[] { 1 }, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(Array.Empty<int>().RightJoin(new[] { 1 }, x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
            Assert.False(outer.RightJoin(Iterate(new[] { 1 }), x => x, x => x, Pair).TryGetNonEnumeratedCount(out _));
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
        public void OuterIsReadWholeOnceInnerHasAnElementAndBeforeTheInnerKeyIsTaken()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1, 2, 3, 2], events);
            var inner = new Logged("inner", [2, 3, 2, 5], events);
            var sequence = outer.RightJoin(inner, logOuterKey, logInnerKey, logResult);

            Assert.Empty(events);

            Assert.Equal(["2:2", "2:2", "3:3", "2:2", "2:2", "0:5"], sequence.ToList());

            string[] pushed =
            [
                "inner.GetEnumerator", .. Read("inner", 2),
                "outer.GetEnumerator", "outer.MoveNext", "outer.Current 1", "ok 1", "outer.MoveNext", "outer.Current 2", "ok 2", "outer.MoveNext", "outer.Current 3", "ok 3", "outer.MoveNext", "outer.Current 2", "ok 2", "outer.end", "outer.Dispose",
                "ik 2", "rs 2:2", "rs 2:2", .. Read("inner", 3), "ik 3", "rs 3:3", .. Read("inner", 2), "ik 2", "rs 2:2", "rs 2:2", .. Read("inner", 5), "ik 5", "rs 0:5",
                "inner.end", "inner.Dispose",
            ];

            Assert.Equal(pushed, events);

            events.Clear();

            Assert.Equal(["2:2", "2:2", "3:3", "2:2", "2:2", "0:5"], Pull(sequence));
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
        public void AnEmptyInnerSequenceNeverReadsOuter()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1], events);
            var inner = new Logged("inner", [], events);

            Assert.Empty(outer.RightJoin(inner, x => x, x => x, Pair).ToList());
            Assert.Equal(["inner.GetEnumerator", "inner.end", "inner.Dispose"], events);

            events.Clear();

            Assert.Empty(Pull(outer.RightJoin(inner, x => x, x => x, Pair)));
            Assert.Equal(["inner.GetEnumerator", "inner.end", "inner.Dispose"], events);

            events.Clear();

            Assert.Empty(outer.RightJoin(Array.Empty<int>(), x => x, x => x, Pair).ToList());
            Assert.Empty(Pull(outer.RightJoin(Array.Empty<int>(), x => x, x => x, Pair)));
            Assert.Empty(events);
        }

        [Fact]
        public void AnEmptyLookupYieldsTheDefaultForEveryInnerElement()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [], events);
            var inner = new Logged("inner", [1, 2], events);

            string[] expected =
            [
                "inner.GetEnumerator", .. Read("inner", 1), "outer.GetEnumerator", "outer.end", "outer.Dispose",
                "ik 1", "rs 0:1", .. Read("inner", 2), "ik 2", "rs 0:2", "inner.end", "inner.Dispose",
            ];

            Assert.Equal(["0:1", "0:2"], outer.RightJoin(inner, x => x, logInnerKey, logResult).ToList());
            Assert.Equal(expected, events);

            events.Clear();

            using (var enumerator = outer.RightJoin(inner, x => x, logInnerKey, logResult).GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal("0:1", enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Equal("0:2", enumerator.Current);
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            Assert.Equal(expected, events);

            (int?, int)[] unmatched = [(null, 1), (null, 2)];

            Assert.Equal(unmatched, new int?[] { null, null }.RightJoin(new[] { 1, 2 }, x => x, x => (int?)x, (o, i) => (o, i)).ToArray());

            string logResult(int o, int i)
            {
                var result = $"{o}:{i}";
                events.Add($"rs {result}");

                return result;
            }

            int logInnerKey(int x)
            {
                events.Add($"ik {x}");

                return x;
            }
        }

        [Fact]
        public void AnEnumeratorNeverMovedNeverReadsOuter()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1], events);
            var inner = new Logged("inner", [1], events);

            using (outer.RightJoin(inner, x => x, x => x, Pair).GetEnumerator())
            {
            }

            Assert.DoesNotContain("outer.GetEnumerator", events);
        }

        [Fact]
        public void OuterIsReadAgainEachTimeTheSequenceIsRun()
        {
            var outer = new List<int> { 1 };
            int[] inner = [1, 2, 3];
            var sequence = outer.RightJoin(inner, x => x, x => x, Pair);

            Assert.Equal([(1, 1), (0, 2), (0, 3)], sequence.ToArray());

            outer.Add(3);

            Assert.Equal([(1, 1), (0, 2), (3, 3)], sequence.ToArray());
            Assert.Equal([(1, 1), (0, 2), (3, 3)], Pull(sequence));

            outer.Clear();

            Assert.Equal([(0, 1), (0, 2), (0, 3)], Pull(sequence));
            Assert.Equal([(0, 1), (0, 2), (0, 3)], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheSelectorsRunOncePerElementAndResultOnEachRun()
        {
            var items = Scrambled(50);
            int[] outer = [3, 7, 20, 3];
            var outerSeen = new List<int>();
            var innerSeen = new List<int>();
            var results = 0;
            var expected = Expected(outer, items, OuterKey, InnerKey, Pair, null);
            var sequence = outer.RightJoin(items, logOuterKey, logInnerKey, countResult);

            foreach (var run in new Func<List<(int, int)>>[] { () => sequence.ToList(), () => Pull(sequence), () => sequence.Select(x => x).ToList() })
            {
                outerSeen.Clear();
                innerSeen.Clear();
                results = 0;

                Assert.Equal(expected, run());
                Assert.Equal(outer, outerSeen);
                Assert.Equal(items, innerSeen);
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
        public void TheInnerKeySelectorRunsOnlyAsFarAsTheConsumerReads()
        {
            int[] outer = [1, 2, 3];
            int[] inner = [18, 1, 2, 19, 3, 4];
            var seen = new List<int>();

            Assert.Equal((1, 18), outer.RightJoin(inner, x => x, logged, Pair).First());
            Assert.Equal([18], seen);

            seen.Clear();

            Assert.Equal([(1, 18), (1, 1)], outer.RightJoin(inner, x => x, logged, Pair).Take(2).ToArray());
            Assert.Equal([18, 1], seen);

            seen.Clear();

            Assert.Equal([(1, 18), (1, 1)], outer.RightJoin(Iterate(inner), x => x, logged, Pair).Take(2).ToList());
            Assert.Equal([18, 1], seen);

            seen.Clear();

            Assert.Equal((2, 19), outer.Select(x => x).RightJoin(inner, x => x, logged, Pair).First(x => x.Item2 == 19));
            Assert.Equal([18, 1, 2, 19], seen);

            int logged(int x)
            {
                seen.Add(x);

                return InnerKey(x);
            }
        }

        [Fact]
        public void TheComparerMatchesInnerKeysToOuterKeys()
        {
            string[] outer = ["APRICOT", "blueberry", "Cranberry", "avocado"];
            string[] inner = ["apple", "Banana", "cherry", "date"];

            (string, string)[] exact = [("avocado", "apple"), (null, "Banana"), (null, "cherry"), (null, "date")];

            Assert.Equal(exact, outer.RightJoin(inner, x => x[..1], x => x[..1], (o, i) => (o, i)).ToArray());

            (string, string)[] ignoringCase = [("APRICOT", "apple"), ("avocado", "apple"), ("blueberry", "Banana"), ("Cranberry", "cherry"), (null, "date")];

            Assert.Equal(ignoringCase, outer.RightJoin(inner, x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(ignoringCase, Pull(Iterate(outer).RightJoin(Iterate(inner), x => x[..1], x => x[..1], (o, i) => (o, i), StringComparer.OrdinalIgnoreCase)));
        }

        [Fact]
        public void OuterElementsWithANullKeyAreLeftOutAndANullInnerKeyMatchesNothing()
        {
            string[] outer = [null, "b", "a", null];
            string[] inner = ["a", null, "b"];

            Assert.Equal(["aa", "-~", "bb"], outer.RightJoin(inner, x => x, x => x, (o, i) => (o ?? "-") + (i ?? "~")).ToArray());
            Assert.Equal(["aa", "-~", "bb"], Pull(Iterate(outer).RightJoin(Iterate(inner), x => x, x => x, (o, i) => (o ?? "-") + (i ?? "~"))));

            int?[] nullables = [null, 2, null];

            (int?, int?)[] paired = [(null, null), (2, 2), (null, null)];

            Assert.Equal(paired, nullables.RightJoin(nullables, x => x, x => x, pairNullables).ToArray());
            Assert.Equal(paired, Pull(nullables.RightJoin(nullables, x => x, x => x, pairNullables)));

            static (int?, int?) pairNullables(int? o, int? i) => (o, i);
        }

        [Fact]
        public void ANullInnerKeyMatchesWhereTheComparerHashesAnOuterKeyAsNull()
        {
            var everythingEqual = EqualityComparer<string>.Create((x, y) => true, x => 0);
            string[] outer = ["b"];
            string[] inner = [null, "x"];

            Assert.Equal(["b~", "bx"], outer.RightJoin(inner, x => x, x => x, (o, i) => o + (i ?? "~"), everythingEqual).ToArray());
            Assert.Equal(["b~", "bx"], Pull(outer.RightJoin(inner, x => x, x => x, (o, i) => o + (i ?? "~"), everythingEqual)));
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullKey = null;
            Func<int, int, int> nullResult = null;
            IEnumerable<int> nullInner = null;
            int[] inner = [1];

            Assert.Throws<ArgumentNullException>("outer", () => ((int[])null).RightJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outer", () => ((List<int>)null).RightJoin(inner, x => x, x => x, Pair));
            Assert.Throws<ArgumentNullException>("outer", () => ((IEnumerable<int>)null).RightJoin(inner, x => x, x => x, Pair, ByTens));

            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.RightJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.Select(x => x).RightJoin(nullInner, nullKey, nullKey, nullResult, ByTens));

            Assert.Throws<ArgumentNullException>("outerKeySelector", () => new[] { 1 }.RightJoin(inner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outerKeySelector", () => Iterate(new[] { 1 }).RightJoin(inner, nullKey, x => x, Pair));

            Assert.Throws<ArgumentNullException>("innerKeySelector", () => new List<int> { 1 }.RightJoin(inner, x => x, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("innerKeySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().RightJoin(inner, x => x, nullKey, Pair));

            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.RightJoin(inner, x => x, x => x, nullResult));
            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.Select(x => x).RightJoin(inner, x => x, x => x, nullResult, ByTens));
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
        public void TheInnerSequenceIsDisposedWhenTheOperatorStopsEarly()
        {
            var tracked = new Tracked();
            int[] outer = [3, 5, 7];

            foreach (var _ in outer.RightJoin(tracked.Iterate(10), x => x, x => x, Pair))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal((3, 3), outer.RightJoin(tracked.Iterate(10), x => x, x => x, Pair).First(x => x.Item1 != 0));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal([(0, 0), (0, 1)], outer.RightJoin(tracked.Iterate(10), x => x, x => x, Pair).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => outer.RightJoin(tracked.Iterate(10), x => x, x => x < 5 ? x : throw new InvalidOperationException(), Pair).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(outer.RightJoin(tracked.Iterate(10), x => x, x => x, (o, i) => i < 5 ? i : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void OuterIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] inner = [1, 5, 2];

            Assert.Equal([(1, 1), (0, 5), (2, 2)], tracked.Iterate(3).RightJoin(inner, x => x, x => x, Pair).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal([(1, 1), (0, 5), (2, 2)], Pull(tracked.Iterate(3).RightJoin(inner, x => x, x => x, Pair)));
            Assert.Equal(2, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(3).RightJoin(inner, x => x < 2 ? x : throw new InvalidOperationException(), x => x, Pair).ToList());
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
