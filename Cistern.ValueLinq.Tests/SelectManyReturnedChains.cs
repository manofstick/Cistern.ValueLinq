using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // SelectMany whose selector returns a Cistern chain, over an array, a List, a ReadOnlyMemory, an IEnumerable and a
    // Cistern chain, with and without a result selector, gives the elements in order however it is consumed: an inner
    // sequence is started only if the query reaches it, every started inner source is disposed exactly once whether the
    // query runs to the end, halts or throws, and the downstream is disposed exactly once.
    public class SelectManyReturnedChains
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int start, int count, int throwAt)
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

        private sealed class CountingPool : ArrayPool<int>
        {
            private readonly HashSet<int[]> _outstanding = new(ReferenceEqualityComparer.Instance);

            public int Rented { get; private set; }

            public override int[] Rent(int minimumLength)
            {
                var array = new int[Math.Max(minimumLength, 16)];
                _outstanding.Add(array);
                ++Rented;
                return array;
            }

            public override void Return(int[] array, bool clearArray = false) =>
                Assert.True(_outstanding.Remove(array), "an array was returned that is not out on loan");

            public void AssertAllReturned()
            {
                Assert.True(Rented > 0, "nothing was rented, so the query did not exercise the pool");
                Assert.Empty(_outstanding);
            }
        }

        private const int InnerLength = 100;
        private const int Inners = 4;
        private const int Total = Inners * InnerLength;
        private static readonly int[] Indices = [0, 1, 2, 3];

        private static IEnumerable<int> IterateIndices()
        {
            for (var i = 0; i < Inners; ++i)
                yield return i;
        }

        // the inner element, after checking it was paired with its own outer element
        private static int Paired(int index, int item)
        {
            Assert.Equal(index, item / InnerLength);
            return item;
        }

        private sealed class Built(int throwAt)
        {
            public readonly Tracked[] Sources = [new(), new(), new(), new()];

            public ValueEnumerable<int, Nodes.Select.Node<int, int, SystemIEnumerable.Node<int>>, Nodes.Select.Pull<int, int, SystemIEnumerable.Node<int>, ErasedEnumerator<int, SystemIEnumerable.Node<int>>>> Inner(int i) =>
                Sources[i].Iterate(i * InnerLength, InnerLength, throwAt).OfEnumerable().Select(x => x);

            public void AssertStarted(int count)
            {
                for (var i = 0; i < Sources.Length; ++i)
                {
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Started);
                    Assert.Equal(i < count ? 1 : 0, Sources[i].Disposed);
                }
            }
        }

        public static IEnumerable<object[]> Cases()
        {
            foreach (var source in new[] { "array", "list", "memory", "enumerable", "chain" })
            {
                foreach (var resultSelector in new[] { false, true })
                    yield return [source, resultSelector];
            }
        }

        private static int Sum(Built built, string source, bool resultSelector) =>
            (source, resultSelector) switch
            {
                ("array", false) => Indices.SelectMany(i => built.Inner(i)).Sum(),
                ("array", true) => Indices.SelectMany(i => built.Inner(i), Paired).Sum(),
                ("list", false) => new List<int>(Indices).SelectMany(i => built.Inner(i)).Sum(),
                ("list", true) => new List<int>(Indices).SelectMany(i => built.Inner(i), Paired).Sum(),
                ("memory", false) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i)).Sum(),
                ("memory", true) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired).Sum(),
                ("enumerable", false) => IterateIndices().SelectMany(i => built.Inner(i)).Sum(),
                ("enumerable", true) => IterateIndices().SelectMany(i => built.Inner(i), Paired).Sum(),
                ("chain", false) => Indices.Select(x => x).SelectMany(i => built.Inner(i)).Sum(),
                ("chain", true) => Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired).Sum(),
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };

        private static int Count(Built built, string source, bool resultSelector) =>
            (source, resultSelector) switch
            {
                ("array", false) => Indices.SelectMany(i => built.Inner(i)).Count(),
                ("array", true) => Indices.SelectMany(i => built.Inner(i), Paired).Count(),
                ("list", false) => new List<int>(Indices).SelectMany(i => built.Inner(i)).Count(),
                ("list", true) => new List<int>(Indices).SelectMany(i => built.Inner(i), Paired).Count(),
                ("memory", false) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i)).Count(),
                ("memory", true) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired).Count(),
                ("enumerable", false) => IterateIndices().SelectMany(i => built.Inner(i)).Count(),
                ("enumerable", true) => IterateIndices().SelectMany(i => built.Inner(i), Paired).Count(),
                ("chain", false) => Indices.Select(x => x).SelectMany(i => built.Inner(i)).Count(),
                ("chain", true) => Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired).Count(),
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };

        private static List<int> ToList(Built built, string source, bool resultSelector, ArrayPool<int> pool) =>
            (source, resultSelector) switch
            {
                ("array", false) => Indices.SelectMany(i => built.Inner(i)).ToListUsePool(pool),
                ("array", true) => Indices.SelectMany(i => built.Inner(i), Paired).ToListUsePool(pool),
                ("list", false) => new List<int>(Indices).SelectMany(i => built.Inner(i)).ToListUsePool(pool),
                ("list", true) => new List<int>(Indices).SelectMany(i => built.Inner(i), Paired).ToListUsePool(pool),
                ("memory", false) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i)).ToListUsePool(pool),
                ("memory", true) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired).ToListUsePool(pool),
                ("enumerable", false) => IterateIndices().SelectMany(i => built.Inner(i)).ToListUsePool(pool),
                ("enumerable", true) => IterateIndices().SelectMany(i => built.Inner(i), Paired).ToListUsePool(pool),
                ("chain", false) => Indices.Select(x => x).SelectMany(i => built.Inner(i)).ToListUsePool(pool),
                ("chain", true) => Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired).ToListUsePool(pool),
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };

        private static List<int> Pull(Built built, string source, bool resultSelector) =>
            (source, resultSelector) switch
            {
                ("array", false) => Pull(Indices.SelectMany(i => built.Inner(i))),
                ("array", true) => Pull(Indices.SelectMany(i => built.Inner(i), Paired)),
                ("list", false) => Pull(new List<int>(Indices).SelectMany(i => built.Inner(i))),
                ("list", true) => Pull(new List<int>(Indices).SelectMany(i => built.Inner(i), Paired)),
                ("memory", false) => Pull(new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i))),
                ("memory", true) => Pull(new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired)),
                ("enumerable", false) => Pull(IterateIndices().SelectMany(i => built.Inner(i))),
                ("enumerable", true) => Pull(IterateIndices().SelectMany(i => built.Inner(i), Paired)),
                ("chain", false) => Pull(Indices.Select(x => x).SelectMany(i => built.Inner(i))),
                ("chain", true) => Pull(Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired)),
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };

        private static int FirstSeen(Built built, string source, bool resultSelector, List<int> seen, Func<int, bool> predicate)
        {
            int Record(int x)
            {
                seen.Add(x);
                return x;
            }

            return (source, resultSelector) switch
            {
                ("array", false) => Indices.SelectMany(i => built.Inner(i)).Select(Record).First(predicate),
                ("array", true) => Indices.SelectMany(i => built.Inner(i), Paired).Select(Record).First(predicate),
                ("list", false) => new List<int>(Indices).SelectMany(i => built.Inner(i)).Select(Record).First(predicate),
                ("list", true) => new List<int>(Indices).SelectMany(i => built.Inner(i), Paired).Select(Record).First(predicate),
                ("memory", false) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i)).Select(Record).First(predicate),
                ("memory", true) => new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired).Select(Record).First(predicate),
                ("enumerable", false) => IterateIndices().SelectMany(i => built.Inner(i)).Select(Record).First(predicate),
                ("enumerable", true) => IterateIndices().SelectMany(i => built.Inner(i), Paired).Select(Record).First(predicate),
                ("chain", false) => Indices.Select(x => x).SelectMany(i => built.Inner(i)).Select(Record).First(predicate),
                ("chain", true) => Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired).Select(Record).First(predicate),
                _ => throw new ArgumentOutOfRangeException(nameof(source)),
            };
        }

        // each compiles only for a query whose inner sequence is the chain itself, not an SystemIEnumerable.Node wrapping it
        private static void BindsToChain<T, NodeU, NodeEU, NodeUEnumerator, NodeEUEnumerator>(in ValueEnumerable<T, Nodes.SelectMany.Node<T,ValueEnumerable<T, NodeU, NodeUEnumerator>, NodeEU>, Nodes.SelectMany.Pull<T,ValueEnumerable<T, NodeU, NodeUEnumerator>, NodeEU, NodeEUEnumerator, ValueEnumerableEnumerator<T, NodeU, NodeUEnumerator>>> query)
            where NodeU : INode<T>
            where NodeEU : INode<ValueEnumerable<T, NodeU, NodeUEnumerator>>
            where NodeUEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<NodeU, NodeUEnumerator>
            where NodeEUEnumerator : struct, IPullEnumerator<ValueEnumerable<T, NodeU, NodeUEnumerator>>, INodeEnumerator<NodeEU, NodeEUEnumerator>
        {
        }

        private static void BindsToChain<TSource, TCollection, TResult, NodeU, NodeSrcCol, NodeUEnumerator, NodeSrcColEnumerator>(in ValueEnumerable<TResult, Nodes.SelectManyResult.Node<TSource, TCollection, TResult, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>, NodeSrcCol>, Nodes.SelectManyResult.Pull<TSource, TCollection, TResult, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>, NodeSrcCol, NodeSrcColEnumerator, ValueEnumerableEnumerator<TCollection, NodeU, NodeUEnumerator>>> query)
            where NodeU : INode<TCollection>
            where NodeSrcCol : INode<(TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>)>
            where NodeUEnumerator : struct, IPullEnumerator<TCollection>, INodeEnumerator<NodeU, NodeUEnumerator>
            where NodeSrcColEnumerator : struct, IPullEnumerator<(TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>)>, INodeEnumerator<NodeSrcCol, NodeSrcColEnumerator>
        {
        }

        [Fact]
        public void ASelectorReturningAChainBindsToTheOverloadTakingIt()
        {
            var built = new Built(-1);

            BindsToChain(Indices.SelectMany(i => built.Inner(i)));
            BindsToChain(new List<int>(Indices).SelectMany(i => built.Inner(i)));
            BindsToChain(new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i)));
            BindsToChain(IterateIndices().SelectMany(i => built.Inner(i)));
            BindsToChain(Indices.Select(x => x).SelectMany(i => built.Inner(i)));

            BindsToChain(Indices.SelectMany(i => built.Inner(i), Paired));
            BindsToChain(new List<int>(Indices).SelectMany(i => built.Inner(i), Paired));
            BindsToChain(new ReadOnlyMemory<int>(Indices).SelectMany(i => built.Inner(i), Paired));
            BindsToChain(IterateIndices().SelectMany(i => built.Inner(i), Paired));
            BindsToChain(Indices.Select(x => x).SelectMany(i => built.Inner(i), Paired));
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        private static int[] Sequence(int start, int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = start + i;
            return items;
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void SumInOrder(string source, bool resultSelector)
        {
            var built = new Built(-1);
            var expected = 0;
            foreach (var item in Sequence(0, Total))
                expected += item;

            Assert.Equal(expected, Sum(built, source, resultSelector));
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void CountAll(string source, bool resultSelector)
        {
            var built = new Built(-1);

            Assert.Equal(Total, Count(built, source, resultSelector));
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ToList_DownstreamDisposedOnce(string source, bool resultSelector)
        {
            var built = new Built(-1);
            var pool = new CountingPool();

            Assert.Equal(Sequence(0, Total), ToList(built, source, resultSelector, pool));
            built.AssertStarted(Inners);
            pool.AssertAllReturned();
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void Pull_SameElements(string source, bool resultSelector)
        {
            var built = new Built(-1);

            Assert.Equal(Sequence(0, Total), Pull(built, source, resultSelector));
            built.AssertStarted(Inners);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void HaltsInAMiddleInner_LaterInnersNotStarted(string source, bool resultSelector)
        {
            var built = new Built(-1);
            var seen = new List<int>();
            var target = InnerLength + 5;

            Assert.Equal(target, FirstSeen(built, source, resultSelector, seen, x => x == target));
            Assert.Equal(Sequence(0, target + 1), seen);
            built.AssertStarted(2);
        }

        [Theory]
        [MemberData(nameof(Cases))]
        public void ThrowsInAMiddleInner_StartedSourcesAndDownstreamDisposed(string source, bool resultSelector)
        {
            var built = new Built(2 * InnerLength + 5);
            var pool = new CountingPool();

            Assert.Throws<InvalidOperationException>(() => ToList(built, source, resultSelector, pool));
            built.AssertStarted(3);
            pool.AssertAllReturned();
        }
    }
}
