using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Count over a SelectMany, with or without a result selector and over an outer sequence that is an array or an
    // iterator, counts every inner sequence whatever it is: one whose length is known, one that answers the count
    // itself, and a Cistern chain met through IEnumerable<T> that has to be run to be counted. A selector in an inner
    // chain runs once per element, the collection selector once per outer element, the result selector never; every
    // started source is disposed exactly once, and an exception part way leaves no later inner sequence started.
    public class SelectManyCount
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int start, int count, int throwAt = -1)
            {
                ++Started;
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        if (start + i == throwAt)
                            throw new InvalidOperationException("source");
                        yield return start + i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        private const int InnerLength = 100;
        private const int Inners = 4;

        private sealed class Built
        {
            public required Tracked Outer;
            public required Tracked[] Sources;
            public required Func<int, IEnumerable<int>> Inner;
            public required bool InnersAreTracked;
            public int CollectionSelectorCalls;
            public int ElementSelectorCalls;

            public void AssertStarted(int count)
            {
                Assert.Equal((1, 1), (Outer.Started, Outer.Disposed));
                for (var i = 0; i < Sources.Length; ++i)
                {
                    var expected = InnersAreTracked && i < count ? 1 : 0;
                    Assert.Equal(expected, Sources[i].Started);
                    Assert.Equal(expected, Sources[i].Disposed);
                }
            }
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        // the inner sequences' elements that are counted, per kind
        private static int CountedPerInner(string kind) => kind.StartsWith("where") ? InnerLength / 2 : InnerLength;

        private static bool UsesIterator(string kind) => kind.EndsWith("Iterator") || kind == "iterator";

        private static Built Build(string kind, int throwAt = -1)
        {
            var sources = new Tracked[Inners];
            for (var i = 0; i < Inners; ++i)
                sources[i] = new Tracked();

            Built? built = null;
            int Counted(int x)
            {
                ++built!.ElementSelectorCalls;
                return x;
            }

            IEnumerable<int> Inner(int i)
            {
                ++built!.CollectionSelectorCalls;
                var start = i * InnerLength;
                return kind switch
                {
                    "array" => Sequence(start, InnerLength),
                    "iterator" => sources[i].Iterate(start, InnerLength, throwAt),
                    "selectOverArray" => Sequence(start, InnerLength).Select(Counted),
                    "selectOverIterator" => sources[i].Iterate(start, InnerLength, throwAt).Select(Counted),
                    "whereOverArray" => Sequence(start, InnerLength).Where(x => x % 2 == 0),
                    "whereOverIterator" => sources[i].Iterate(start, InnerLength, throwAt).Where(x => x % 2 == 0),
                    _ => throw new ArgumentOutOfRangeException(nameof(kind)),
                };
            }

            built = new Built { Outer = new Tracked(), Sources = sources, Inner = Inner, InnersAreTracked = UsesIterator(kind) };
            return built;
        }

        private static int Count(Built built, bool outerIsIterator, bool resultSelector)
        {
            int Paired(int index, int item) => throw new InvalidOperationException("the result selector runs when counting");

            if (outerIsIterator)
            {
                var outer = built.Outer.Iterate(0, Inners);
                return resultSelector ? outer.SelectMany(built.Inner, Paired).Count() : outer.SelectMany(built.Inner).Count();
            }

            ++built.Outer.Started;
            ++built.Outer.Disposed;
            int[] indices = [0, 1, 2, 3];
            return resultSelector ? indices.SelectMany(built.Inner, Paired).Count() : indices.SelectMany(built.Inner).Count();
        }

        public static IEnumerable<object[]> Cases(bool iteratorsOnly)
        {
            foreach (var kind in new[] { "array", "iterator", "selectOverArray", "selectOverIterator", "whereOverArray", "whereOverIterator" })
            {
                if (iteratorsOnly && !UsesIterator(kind))
                    continue;

                foreach (var outerIsIterator in new[] { false, true })
                {
                    foreach (var resultSelector in new[] { false, true })
                        yield return [kind, outerIsIterator, resultSelector];
                }
            }
        }

        [Theory]
        [MemberData(nameof(Cases), false)]
        public void CountsEveryInner(string kind, bool outerIsIterator, bool resultSelector)
        {
            var built = Build(kind);

            Assert.Equal(Inners * CountedPerInner(kind), Count(built, outerIsIterator, resultSelector));
            Assert.Equal(Inners, built.CollectionSelectorCalls);
            Assert.Equal(kind.StartsWith("select") ? Inners * InnerLength : 0, built.ElementSelectorCalls);
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases), true)]
        public void ThrowsInAMiddleInner_StartedSourcesDisposed(string kind, bool outerIsIterator, bool resultSelector)
        {
            var built = Build(kind, throwAt: 2 * InnerLength + 5);

            Assert.Throws<InvalidOperationException>(() => Count(built, outerIsIterator, resultSelector));
            Assert.Equal(3, built.CollectionSelectorCalls);
            built.AssertStarted(3);
        }
    }
}
