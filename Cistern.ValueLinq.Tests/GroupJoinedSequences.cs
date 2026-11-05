using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class GroupJoinedSequences
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

        private static string Describe(int outer, IEnumerable<int> matches)
        {
            var text = $"{outer}:[{string.Join(" ", Pull(matches))}]";

            return text;
        }

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static List<TResult> Expected<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer)
        {
            var equality = comparer ?? EqualityComparer<TKey>.Default;
            var expected = new List<TResult>();
            foreach (var outerItem in outer)
            {
                var outerKey = outerKeySelector(outerItem);
                var matches = new List<TInner>();
                foreach (var innerItem in inner)
                {
                    var innerKey = innerKeySelector(innerItem);
                    if (innerKey is not null
                        && equality.Equals(outerKey, innerKey))
                    {
                        matches.Add(innerItem);
                    }
                }

                expected.Add(resultSelector(outerItem, matches));
            }

            return expected;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        private static int[][] Inners() => [[], [0], [3, 20, 3, 37, 54], Scrambled(40), [100, 200]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeYieldsEachOuterElementWithItsMatchesInInnerOrder(int length)
        {
            var items = Scrambled(length);
            var list = new List<int>(items);
            ReadOnlyMemory<int> memory = items;
            IEnumerable<int> enumerable = Iterate(items);

            foreach (var inner in Inners())
            {
                foreach (var comparer in new[] { null, EqualityComparer<int>.Default, ByTens })
                {
                    var expected = Expected(items, inner, OuterKey, InnerKey, Describe, comparer);

                    Assert.Equal(expected, items.GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, list.GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, memory.OfMemory().GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, list.OfList().GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, enumerable.GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, items.GroupJoin(Iterate(inner), OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, items.GroupJoin(new List<int>(inner), OuterKey, InnerKey, Describe, comparer).ToArray());
                    Assert.Equal(expected, items.Select(x => x).GroupJoin(inner, OuterKey, InnerKey, Describe, comparer).ToArray());

                    Assert.Equal(expected, Pull(items.GroupJoin(inner, OuterKey, InnerKey, Describe, comparer)));
                    Assert.Equal(expected, Pull(list.GroupJoin(inner, OuterKey, InnerKey, Describe, comparer)));
                    Assert.Equal(expected, Pull(memory.OfMemory().GroupJoin(inner, OuterKey, InnerKey, Describe, comparer)));
                    Assert.Equal(expected, Pull(enumerable.GroupJoin(Iterate(inner), OuterKey, InnerKey, Describe, comparer)));
                    Assert.Equal(expected, Pull(items.Select(x => x).GroupJoin(inner, OuterKey, InnerKey, Describe, comparer)));
                }

                var withoutComparer = Expected(items, inner, OuterKey, InnerKey, Describe, null);

                Assert.Equal(withoutComparer, items.GroupJoin(inner, OuterKey, InnerKey, Describe).ToArray());
                Assert.Equal(withoutComparer, list.GroupJoin(inner, OuterKey, InnerKey, Describe).ToArray());
                Assert.Equal(withoutComparer, enumerable.GroupJoin(inner, OuterKey, InnerKey, Describe).ToArray());
                Assert.Equal(withoutComparer, items.Select(x => x).GroupJoin(inner, OuterKey, InnerKey, Describe).ToArray());
            }
        }

        [Fact]
        public void EachOuterElementYieldsOneResultOverItsMatchesInInnerOrder()
        {
            int[] outer = [1, 2, 3, 2];
            string[] inner = ["b2", "a1", "c2", "d4", "e1"];
            string[] expected = ["1:a1 e1", "2:b2 c2", "3:", "2:b2 c2"];

            Assert.Equal(expected, outer.GroupJoin(inner, x => x, s => s[1] - '0', describe).ToArray());
            Assert.Equal(expected, Pull(outer.GroupJoin(inner, x => x, s => s[1] - '0', describe)));
            Assert.Equal(expected, Pull(Iterate(outer).GroupJoin(Iterate(inner), x => x, s => s[1] - '0', describe)));

            static string describe(int o, IEnumerable<string> matches)
            {
                var text = $"{o}:{string.Join(" ", Pull(matches))}";

                return text;
            }
        }

        [Fact]
        public void MatchesAreTheReadOnlyGroupingAndAnUnmatchedKeyHasTheSharedEmptyArray()
        {
            int[] outer = [7, 1, 8];
            int[] inner = [1, 1];

            foreach (var run in new Func<List<IEnumerable<int>>>[] { copied, pulled, pushed })
            {
                var matches = run();

                Assert.Same(Array.Empty<int>(), matches[0]);
                Assert.Same(Array.Empty<int>(), matches[2]);

                var grouping = Assert.IsAssignableFrom<System.Linq.IGrouping<int, int>>(matches[1]);
                Assert.Equal(1, grouping.Key);
                Assert.Equal([1, 1], Pull(grouping));

                var collection = Assert.IsAssignableFrom<IList<int>>(matches[1]);
                Assert.True(collection.IsReadOnly);
                Assert.Equal(2, collection.Count);
                Assert.Throws<NotSupportedException>(() => collection.Add(3));
                Assert.Throws<NotSupportedException>(() => collection[0] = 3);
            }

            Assert.Same(Array.Empty<int>(), new[] { 9 }.GroupJoin(Array.Empty<int>(), x => x, x => x, (o, m) => m).First());

            List<IEnumerable<int>> copied() =>
                outer.GroupJoin(inner, x => x, x => x, (o, m) => m).ToList();

            List<IEnumerable<int>> pulled() =>
                Pull(outer.GroupJoin(inner, x => x, x => x, (o, m) => m));

            List<IEnumerable<int>> pushed() =>
                Iterate(outer).GroupJoin(Iterate(inner), x => x, x => x, (o, m) => m).ToList();
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void PushAgreesWithPull(int length)
        {
            var items = Scrambled(length);
            int[] inner = [2, 7, 7, 30, 11, 0, 25];

            foreach (var keepAbove in new[] { -1, 50, 1000 })
            {
                var filtered = items.Where(x => x > keepAbove).GroupJoin(inner, OuterKey, InnerKey, sumMatches);
                var iterated = Iterate(items).Where(x => x > keepAbove).Select(x => x + 1).GroupJoin(Iterate(inner), OuterKey, InnerKey, Describe);
                var concatenated = items.Where(x => x > keepAbove).Concat(Scrambled(length / 2)).GroupJoin(inner, x => x % 31, InnerKey, Describe, ByTens);

                var pulled = Pull(filtered);

                var handCoded = new List<int>();
                foreach (var item in items)
                {
                    if (item <= keepAbove)
                        continue;

                    var total = item;
                    foreach (var match in inner)
                    {
                        if (OuterKey(item) == InnerKey(match))
                            total += match;
                    }

                    handCoded.Add(total);
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

            var ranged = Enumerable.Range(3, length).GroupJoin(inner, OuterKey, InnerKey, Describe);

            Assert.Equal(Pull(ranged), ranged.ToList());

            static int sumMatches(int outer, IEnumerable<int> matches)
            {
                var total = outer;
                foreach (var match in matches)
                    total += match;

                return total;
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void TheSpanPathAgreesWithTheElementPath(int length)
        {
            var items = Scrambled(length);
            int[] inner = [2, 7, 7, 30, 11, 0, 25];
            var expected = Expected(items, inner, OuterKey, InnerKey, countMatches, null);

            var sum = 0;
            foreach (var item in expected)
                sum += item;

            Assert.Equal(sum, items.GroupJoin(inner, OuterKey, InnerKey, countMatches).Sum());
            Assert.Equal(sum, Iterate(items).GroupJoin(inner, OuterKey, InnerKey, countMatches).Sum());
            Assert.Equal(sum, items.Concat(Array.Empty<int>()).GroupJoin(inner, OuterKey, InnerKey, countMatches).Sum());

            var aboveFifty = 0;
            for (var i = 0; i < items.Length; ++i)
            {
                if (items[i] > 50)
                    aboveFifty += expected[i];
            }

            Assert.Equal(aboveFifty, items.Where(x => x > 50).GroupJoin(inner, OuterKey, InnerKey, countMatches).Sum());

            if (expected is [var first, ..])
            {
                Assert.Equal(first, items.GroupJoin(inner, OuterKey, InnerKey, countMatches).First());
                Assert.Equal(expected.GetRange(0, Math.Min(3, expected.Count)), items.GroupJoin(inner, OuterKey, InnerKey, countMatches).Take(3).ToList());
            }

            static int countMatches(int outer, IEnumerable<int> matches)
            {
                var count = 0;
                foreach (var _ in matches)
                    ++count;

                return count;
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void RequestsAgreeWithRunningTheSequence(int length)
        {
            var items = Scrambled(length);
            int[] inner = [1, 4, 9, 16, 20, 22, 0, 5, 18];
            var expected = Expected(items, inner, OuterKey, InnerKey, Describe, null).ToArray();

            var reversed = (string[])expected.Clone();
            Array.Reverse(reversed);

            IEnumerable<string>[] sequences =
            [
                items.GroupJoin(inner, OuterKey, InnerKey, Describe),
                new List<int>(items).GroupJoin(inner, OuterKey, InnerKey, Describe),
                Iterate(items).GroupJoin(Iterate(inner), OuterKey, InnerKey, Describe),
                items.Select(x => x).GroupJoin(inner, OuterKey, InnerKey, Describe),
                items.Where(x => x >= 0).GroupJoin(inner, OuterKey, InnerKey, Describe),
            ];

            foreach (var sequence in sequences)
            {
                Assert.Equal(expected.Length, sequence.Count());
                Assert.Equal(expected.Length, sequence.Count(ignorePotentialSideEffects: true));
                Assert.Equal(expected.Length, sequence.LongCount());
                Assert.Equal(expected, sequence.ToArray());
                Assert.Equal(expected, sequence.ToList());
                Assert.Equal(reversed, sequence.Reverse().ToArray());
                Assert.Null(sequence.ElementAtOrDefault(expected.Length));

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
        public void ACountSkippingSideEffectsIsOutersWithoutReadingInnerOrRunningTheSelectors()
        {
            var events = new List<string>();
            var inner = new Logged("inner", [1, 2], events);
            int[] outer = [1, 2, 3, 4, 5];

            Assert.Equal(5, outer.GroupJoin(inner, logOuterKey, x => x, logResult).Count(ignorePotentialSideEffects: true));
            Assert.Equal(3, outer.Where(x => x > 2).GroupJoin(inner, logOuterKey, x => x, logResult).Count(ignorePotentialSideEffects: true));
            Assert.Empty(events);

            Assert.Equal(3, outer.Where(x => x > 2).GroupJoin(inner, logOuterKey, x => x, logResult).Count());

            string[] counted = ["inner.GetEnumerator", .. Read("inner", 1, 2), "inner.end", "inner.Dispose", "ok 3", "rs 3", "ok 4", "rs 4", "ok 5", "rs 5"];

            Assert.Equal(counted, events);

            string logResult(int o, IEnumerable<int> matches)
            {
                events.Add($"rs {o}");

                return "";
            }

            int logOuterKey(int x)
            {
                events.Add($"ok {x}");

                return x;
            }
        }

        [Fact]
        public void ElementAtAndSkipRunTheSelectorsOnTheElementsBeforeTheOneAsked()
        {
            int[] outer = [1, 2, 3, 4];
            int[] inner = [1, 2, 3];
            var seen = new List<string>();

            Assert.Equal("3:[3]", outer.GroupJoin(inner, logOuterKey, x => x, logResult).ElementAt(2));
            Assert.Equal(["ok 1", "rs 1", "ok 2", "rs 2", "ok 3", "rs 3"], seen);

            seen.Clear();

            Assert.Equal(["3:[3]", "4:[]"], outer.GroupJoin(inner, logOuterKey, x => x, logResult).Skip(2).ToArray());
            Assert.Equal(["ok 1", "rs 1", "ok 2", "rs 2", "ok 3", "rs 3", "ok 4", "rs 4"], seen);

            seen.Clear();

            Assert.Equal("4:[]", outer.GroupJoin(inner, logOuterKey, x => x, logResult).Last());
            Assert.Equal(["ok 1", "rs 1", "ok 2", "rs 2", "ok 3", "rs 3", "ok 4", "rs 4"], seen);

            seen.Clear();

            Assert.Equal(["4:[]", "3:[3]", "2:[2]", "1:[1]"], outer.GroupJoin(inner, logOuterKey, x => x, logResult).Reverse().ToArray());
            Assert.Equal(["ok 1", "rs 1", "ok 2", "rs 2", "ok 3", "rs 3", "ok 4", "rs 4"], seen);

            string logResult(int o, IEnumerable<int> matches)
            {
                seen.Add($"rs {o}");

                var text = Describe(o, matches);

                return text;
            }

            int logOuterKey(int x)
            {
                seen.Add($"ok {x}");

                return x;
            }
        }

        [Fact]
        public void TheCountWithoutEnumeratingIsOutersWhereItsLengthCannotChange()
        {
            int[] inner = [1, 2];

            Assert.True(Array.Empty<int>().GroupJoin(inner, x => x, x => x, Describe).TryGetNonEnumeratedCount(out var empty));
            Assert.Equal(0, empty);

            Assert.True(new[] { 1, 2, 3 }.GroupJoin(inner, x => x, x => x, Describe).TryGetNonEnumeratedCount(out var three));
            Assert.Equal(3, three);

            Assert.False(new List<int> { 1 }.GroupJoin(inner, x => x, x => x, Describe).TryGetNonEnumeratedCount(out _));
            Assert.False(Iterate(new[] { 1 }).GroupJoin(inner, x => x, x => x, Describe).TryGetNonEnumeratedCount(out _));
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
            var sequence = outer.GroupJoin(inner, logOuterKey, logInnerKey, logResult);

            Assert.Empty(events);

            Assert.Equal(["1:[]", "2:[2 2]", "3:[3]", "2:[2 2]"], sequence.ToList());

            string[] pushed =
            [
                "outer.GetEnumerator", .. Read("outer", 1),
                "inner.GetEnumerator", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 3", "ik 3", "inner.MoveNext", "inner.Current 2", "ik 2", "inner.MoveNext", "inner.Current 5", "ik 5", "inner.end", "inner.Dispose",
                "ok 1", "rs 1", .. Read("outer", 2), "ok 2", "rs 2", .. Read("outer", 3), "ok 3", "rs 3", .. Read("outer", 2), "ok 2", "rs 2",
                "outer.end", "outer.Dispose",
            ];

            Assert.Equal(pushed, events);

            events.Clear();

            Assert.Equal(["1:[]", "2:[2 2]", "3:[3]", "2:[2 2]"], Pull(sequence));
            Assert.Equal(pushed, events);

            string logResult(int o, IEnumerable<int> matches)
            {
                events.Add($"rs {o}");

                var text = Describe(o, matches);

                return text;
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
        public void AnArrayCopiedOutRunsTheSelectorsInTheOrderThePushRunsThem()
        {
            var events = new List<string>();
            var inner = new Logged("inner", [2, 3], events);

            Assert.Equal(["1:[]", "2:[2]", "3:[3]"], new[] { 1, 2, 3 }.GroupJoin(inner, logOuterKey, x => x, logResult).ToArray());

            string[] copied = ["inner.GetEnumerator", .. Read("inner", 2, 3), "inner.end", "inner.Dispose", "ok 1", "rs 1", "ok 2", "rs 2", "ok 3", "rs 3"];

            Assert.Equal(copied, events);

            events.Clear();

            Assert.Equal(["1:[]", "2:[2]", "3:[3]"], new List<int> { 1, 2, 3 }.GroupJoin(inner, logOuterKey, x => x, logResult).ToList());
            Assert.Equal(copied, events);

            events.Clear();

            Assert.Empty(Array.Empty<int>().GroupJoin(inner, logOuterKey, x => x, logResult).ToArray());
            Assert.Empty(new List<int>().GroupJoin(inner, logOuterKey, x => x, logResult).ToList());
            Assert.Empty(events);

            string logResult(int o, IEnumerable<int> matches)
            {
                events.Add($"rs {o}");

                var text = Describe(o, matches);

                return text;
            }

            int logOuterKey(int x)
            {
                events.Add($"ok {x}");

                return x;
            }
        }

        [Fact]
        public void AnEmptyOuterSequenceNeverReadsInner()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [], events);
            var inner = new Logged("inner", [1], events);

            Assert.Empty(outer.GroupJoin(inner, x => x, x => x, Describe).ToList());
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Pull(outer.GroupJoin(inner, x => x, x => x, Describe)));
            Assert.Equal(["outer.GetEnumerator", "outer.end", "outer.Dispose"], events);

            events.Clear();

            Assert.Empty(Array.Empty<int>().GroupJoin(inner, x => x, x => x, Describe).ToList());
            Assert.Empty(Pull(Array.Empty<int>().GroupJoin(inner, x => x, x => x, Describe)));
            Assert.Empty(events);
        }

        [Fact]
        public void AnEmptyLookupYieldsEveryOuterElementWithNoMatches()
        {
            var events = new List<string>();
            var outer = new Logged("outer", [1, 2], events);
            var inner = new Logged("inner", [], events);

            string[] expected =
            [
                "outer.GetEnumerator", .. Read("outer", 1), "inner.GetEnumerator", "inner.end", "inner.Dispose",
                "ok 1", "rs 1", .. Read("outer", 2), "ok 2", "rs 2", "outer.end", "outer.Dispose",
            ];

            Assert.Equal(["1:[]", "2:[]"], outer.GroupJoin(inner, logOuterKey, x => x, logResult).ToList());
            Assert.Equal(expected, events);

            events.Clear();

            using (var enumerator = outer.GroupJoin(inner, logOuterKey, x => x, logResult).GetEnumerator())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal("1:[]", enumerator.Current);
                Assert.True(enumerator.MoveNext());
                Assert.Equal("2:[]", enumerator.Current);
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            Assert.Equal(expected, events);

            string logResult(int o, IEnumerable<int> matches)
            {
                events.Add($"rs {o}");

                var text = Describe(o, matches);

                return text;
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

            using (outer.GroupJoin(inner, x => x, x => x, Describe).GetEnumerator())
            {
            }

            Assert.DoesNotContain("inner.GetEnumerator", events);
        }

        [Fact]
        public void InnerIsReadAgainEachTimeTheSequenceIsRun()
        {
            var inner = new List<int> { 1 };
            int[] outer = [1, 2, 3];
            var sequence = outer.GroupJoin(inner, x => x, x => x, Describe);

            Assert.Equal(["1:[1]", "2:[]", "3:[]"], sequence.ToArray());

            inner.Add(3);
            inner.Add(3);

            Assert.Equal(["1:[1]", "2:[]", "3:[3 3]"], sequence.ToArray());
            Assert.Equal(["1:[1]", "2:[]", "3:[3 3]"], Pull(sequence));

            inner.Clear();

            Assert.Equal(["1:[]", "2:[]", "3:[]"], Pull(sequence));
            Assert.Equal(["1:[]", "2:[]", "3:[]"], sequence.Select(x => x).ToList());
        }

        [Fact]
        public void TheSelectorsRunOncePerElementOnEachRun()
        {
            var items = Scrambled(50);
            int[] inner = [3, 7, 20, 3];
            var outerSeen = new List<int>();
            var innerSeen = new List<int>();
            var resultSeen = new List<int>();
            var expected = Expected(items, inner, OuterKey, InnerKey, Describe, null);
            var sequence = items.GroupJoin(inner, logOuterKey, logInnerKey, logResult);

            foreach (var run in new Func<List<string>>[] { () => sequence.ToList(), () => Pull(sequence), () => sequence.Select(x => x).ToList(), () => Pull(Iterate(items).GroupJoin(inner, logOuterKey, logInnerKey, logResult)) })
            {
                outerSeen.Clear();
                innerSeen.Clear();
                resultSeen.Clear();

                Assert.Equal(expected, run());
                Assert.Equal(items, outerSeen);
                Assert.Equal(inner, innerSeen);
                Assert.Equal(items, resultSeen);
            }

            string logResult(int o, IEnumerable<int> matches)
            {
                resultSeen.Add(o);

                var text = Describe(o, matches);

                return text;
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
        public void TheSelectorsRunOnlyAsFarAsTheConsumerReads()
        {
            int[] outer = [24, 1, 2, 25, 3, 4];
            int[] inner = [1, 2, 3];
            var seen = new List<int>();

            Assert.Equal("24:[1]", outer.GroupJoin(inner, logged, x => x, Describe).First());
            Assert.Equal([24], seen);

            seen.Clear();

            Assert.Equal(["24:[1]", "1:[1]"], outer.GroupJoin(inner, logged, x => x, Describe).Take(2).ToArray());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal(["24:[1]", "1:[1]"], Iterate(outer).GroupJoin(inner, logged, x => x, Describe).Take(2).ToList());
            Assert.Equal([24, 1], seen);

            seen.Clear();

            Assert.Equal("25:[2]", outer.Select(x => x).GroupJoin(inner, logged, x => x, Describe).First(x => x.StartsWith("25")));
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

            string[] exact = ["apple:avocado", "Banana:", "cherry:", "date:"];

            Assert.Equal(exact, outer.GroupJoin(inner, x => x[..1], x => x[..1], describe).ToArray());

            string[] ignoringCase = ["apple:APRICOT avocado", "Banana:blueberry", "cherry:Cranberry", "date:"];

            Assert.Equal(ignoringCase, outer.GroupJoin(inner, x => x[..1], x => x[..1], describe, StringComparer.OrdinalIgnoreCase).ToArray());
            Assert.Equal(ignoringCase, Pull(Iterate(outer).GroupJoin(Iterate(inner), x => x[..1], x => x[..1], describe, StringComparer.OrdinalIgnoreCase)));

            static string describe(string o, IEnumerable<string> matches)
            {
                var text = $"{o}:{string.Join(" ", Pull(matches))}";

                return text;
            }
        }

        [Fact]
        public void InnerElementsWithANullKeyAreLeftOutAndANullOuterKeyMatchesNothing()
        {
            string[] outer = ["a", null, "b"];
            string[] inner = [null, "b", "a", null];

            Assert.Equal(["a:a", "-:", "b:b"], outer.GroupJoin(inner, x => x, x => x, describe).ToArray());
            Assert.Equal(["a:a", "-:", "b:b"], Pull(Iterate(outer).GroupJoin(Iterate(inner), x => x, x => x, describe)));

            static string describe(string o, IEnumerable<string> matches)
            {
                var text = $"{o ?? "-"}:{string.Join(" ", Pull(matches))}";

                return text;
            }
        }

        [Fact]
        public void ANullOuterKeyMatchesWhereTheComparerHashesAnInnerKeyAsNull()
        {
            var everythingEqual = EqualityComparer<string>.Create((x, y) => true, x => 0);
            string[] outer = ["b", null];
            string[] inner = [null, "x"];

            Assert.Equal(["b:x", "-:x"], outer.GroupJoin(inner, x => x, x => x, describe, everythingEqual).ToArray());
            Assert.Equal(["b:x", "-:x"], Pull(outer.GroupJoin(inner, x => x, x => x, describe, everythingEqual)));

            static string describe(string o, IEnumerable<string> matches)
            {
                var text = $"{o ?? "-"}:{string.Join(" ", Pull(matches))}";

                return text;
            }
        }

        [Fact]
        public void NullArgumentsThrowWhenTheSequenceIsMadeInSystemLinqsOrder()
        {
            Func<int, int> nullKey = null;
            Func<int, IEnumerable<int>, int> nullResult = null;
            IEnumerable<int> nullInner = null;
            int[] inner = [1];

            Assert.Throws<ArgumentNullException>("outer", () => ((int[])null).GroupJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outer", () => ((List<int>)null).GroupJoin(inner, x => x, x => x, Describe));
            Assert.Throws<ArgumentNullException>("outer", () => ((IEnumerable<int>)null).GroupJoin(inner, x => x, x => x, Describe, ByTens));

            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.GroupJoin(nullInner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("inner", () => new[] { 1 }.Select(x => x).GroupJoin(nullInner, nullKey, nullKey, nullResult, ByTens));

            Assert.Throws<ArgumentNullException>("outerKeySelector", () => new[] { 1 }.GroupJoin(inner, nullKey, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("outerKeySelector", () => Iterate(new[] { 1 }).GroupJoin(inner, nullKey, x => x, Describe));

            Assert.Throws<ArgumentNullException>("innerKeySelector", () => new List<int> { 1 }.GroupJoin(inner, x => x, nullKey, nullResult));
            Assert.Throws<ArgumentNullException>("innerKeySelector", () => ((ReadOnlyMemory<int>)new[] { 1 }).OfMemory().GroupJoin(inner, x => x, nullKey, Describe));

            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.GroupJoin(inner, x => x, x => x, nullResult));
            Assert.Throws<ArgumentNullException>("resultSelector", () => new[] { 1 }.Select(x => x).GroupJoin(inner, x => x, x => x, nullResult, ByTens));
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

            foreach (var _ in tracked.Iterate(10).GroupJoin(inner, x => x, x => x, Describe))
                break;

            Assert.Equal(1, tracked.Disposed);

            Assert.Equal("3:[3]", tracked.Iterate(10).GroupJoin(inner, x => x, x => x, Describe).First(x => x.EndsWith("[3]")));
            Assert.Equal(2, tracked.Disposed);

            Assert.Equal(["0:[]", "1:[]"], tracked.Iterate(10).GroupJoin(inner, x => x, x => x, Describe).Take(2).ToList());
            Assert.Equal(3, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).GroupJoin(inner, x => x < 5 ? x : throw new InvalidOperationException(), x => x, Describe).ToList());
            Assert.Equal(4, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => Pull(tracked.Iterate(10).GroupJoin(inner, x => x, x => x, (o, m) => o < 5 ? o : throw new InvalidOperationException())));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void InnerIsDisposedOnceRead()
        {
            var tracked = new Tracked();
            int[] outer = [1, 5, 2];

            Assert.Equal(["1:[1]", "5:[]", "2:[2]"], outer.GroupJoin(tracked.Iterate(3), x => x, x => x, Describe).ToArray());
            Assert.Equal(1, tracked.Disposed);

            Assert.Equal(["1:[1]", "5:[]", "2:[2]"], Pull(outer.GroupJoin(tracked.Iterate(3), x => x, x => x, Describe)));
            Assert.Equal(2, tracked.Disposed);

            Assert.Throws<InvalidOperationException>(() => outer.GroupJoin(tracked.Iterate(3), x => x, x => x < 2 ? x : throw new InvalidOperationException(), Describe).ToList());
            Assert.Equal(3, tracked.Disposed);
        }
    }
}
