using System;
using System.Collections.Generic;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A chain over an IEnumerable<T> handed out as one heap enumerator (through IEnumerable<T>.GetEnumerator or
    // Rebuild) is rebuilt over the container the source holds (an array, a List<T>, a collection's struct
    // enumerator) or over the source's IEnumerator<T>, and over a Cistern chain is the chain as built; either way it
    // yields the same elements, runs a selector once per element, and disposes its source exactly once.
    public class DevirtualizedChains
    {
        private static readonly int[] Data = [3, 1, 4, 1, 5, 9, 2, 6];
        private static readonly long[] Expected = [9, 81, 36];

        private sealed class Tracked
        {
            public int Disposed;

            public IEnumerable<int> Iterate(int[] values)
            {
                try
                {
                    foreach (var value in values)
                        yield return value;
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        private static ValueEnumerable<long, Nodes.Select.Node<int, long, Nodes.Where.Node<int, SystemIEnumerable.Node<int>>>, Nodes.Select.Pull<int, long, Nodes.Where.Node<int, SystemIEnumerable.Node<int>>, Nodes.Where.Pull<int, SystemIEnumerable.Node<int>, ErasedEnumerator<int, SystemIEnumerable.Node<int>>>>> Query(IEnumerable<int> source)
            => source.Where(x => x % 3 == 0).Select(x => (long)x * x);

        private static Type HeapChainOf<T>(IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();
            return enumerator.GetType().GetGenericArguments()[0];
        }

        private static Type ChainOf<T, TNode, TEnumerator>(in ValueEnumerable<T, TNode, TEnumerator> _)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
            => typeof(TEnumerator);

        private static IEnumerator<T> Heap<T>(IEnumerable<T> source) => source.GetEnumerator();

        private static List<T> Pull<T>(IEnumerator<T> enumerator)
        {
            var items = new List<T>();
            using (enumerator)
            {
                while (enumerator.MoveNext())
                    items.Add(enumerator.Current);
            }
            return items;
        }

        [Fact]
        public void OverArray_IsRebuiltOverTheArray()
        {
            var chain = HeapChainOf(Query(Data));

            Assert.Equal(typeof(Nodes.Select.Pull<int, long, Nodes.Where.Node<int, SystemArray.Node<int>>, Nodes.Where.Pull<int, SystemArray.Node<int>, SystemArray.Pull<int>>>), chain);
        }

        [Fact]
        public void OverList_IsRebuiltOverTheList()
        {
            var chain = HeapChainOf(Query(new List<int>(Data)));

            Assert.Equal(typeof(Nodes.Select.Pull<int, long, Nodes.Where.Node<int, SystemList.Node<int>>, Nodes.Where.Pull<int, SystemList.Node<int>, SystemList.Pull<int>>>), chain);
        }

        [Fact]
        public void OverHashSet_IsRebuiltOverItsStructEnumerator()
        {
            var chain = HeapChainOf(Query(new HashSet<int>(Data)));

            Assert.Equal(typeof(Nodes.Select.Pull<int, long, Nodes.Where.Node<int, SystemEnumerableGeneric.Node<int, HashSet<int>, HashSet<int>.Enumerator>>, Nodes.Where.Pull<int, SystemEnumerableGeneric.Node<int, HashSet<int>, HashSet<int>.Enumerator>, SystemEnumerableGeneric.NodeEnumerator<int, HashSet<int>, HashSet<int>.Enumerator>>>), chain);
        }

        [Fact]
        public void OverIterator_IsRebuiltOverItsEnumerator()
        {
            var chain = HeapChainOf(Query(new Tracked().Iterate(Data))).ToString();

            Assert.Contains("SystemIEnumerable+Pull", chain);
            Assert.DoesNotContain(typeof(ErasedEnumerator<int, SystemIEnumerable.Node<int>>).Name, chain);
        }

        [Fact]
        public void OverDictionary_IsRebuiltOverItsEnumerator()
        {
            IEnumerable<KeyValuePair<int, string>> source = new Dictionary<int, string> { [1] = "a", [2] = "bb", [3] = "ccc" };
            var query = source.Where(pair => pair.Key != 2).Select(pair => pair.Value.Length);

            var chain = HeapChainOf(query).ToString();

            Assert.Contains("SystemIEnumerable+Pull", chain);
            Assert.Equal([1, 3], query.ToArray());
        }

        [Fact]
        public void OverCisternChain_IsTheChainAsBuilt()
        {
            IEnumerable<int> source = Data.OfArray().Select(x => x);
            var query = Query(source);

            Assert.Equal(ChainOf(query), HeapChainOf(query));
        }

        [Fact]
        public void OverTypedContainer_IsTheChainAsBuilt()
        {
            var query = Data.OfArray().Where(x => x % 3 == 0).Select(x => (long)x * x);

            Assert.Equal(ChainOf(query), HeapChainOf(query));
        }

        [Fact]
        public void ThroughEveryLevel_ReachesTheSource()
        {
            IEnumerable<int> source = Data;
            var query = source.Skip(1).Where(x => x > 1).Take(5).Select(x => x + 1).TakeWhile(x => x < 100);

            var chain = HeapChainOf(query);

            Assert.Contains("SystemArray+Pull", chain.ToString());
            Assert.DoesNotContain(typeof(ErasedEnumerator<int, SystemIEnumerable.Node<int>>).Name, chain.ToString());
            Assert.Equal([5, 6, 10, 3, 7], query.ToArray());
        }

        [Fact]
        public void Zip_RebuildsBothSequences()
        {
            IEnumerable<int> first = Data;
            IEnumerable<int> second = new List<int>(Data);
            var query = first.Zip(second);

            var chain = HeapChainOf(query).ToString();

            Assert.Contains("SystemArray+Pull", chain);
            Assert.Contains(typeof(SystemList.Pull<int>).Name, chain);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void HeapEnumerator_YieldsTheChainsElements(int sourceKind)
        {
            IEnumerable<int> source =
                sourceKind switch
                {
                    0 => Data,
                    1 => new List<int>(Data),
                    _ => new Tracked().Iterate(Data),
                };

            var items = Pull(Heap(Query(source)));

            Assert.Equal(Expected, items.ToArray());
        }

        [Fact]
        public void HeapEnumerator_CurrentIsDefaultAfterTheEnd()
        {
            using var enumerator = Heap(Query(Data));

            while (enumerator.MoveNext())
            {
            }

            Assert.Equal(0L, enumerator.Current);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void HeapEnumerator_RunsTheSelectorOncePerElement()
        {
            var calls = 0;
            IEnumerable<int> source = Data;
            var query = source.Select(x => { ++calls; return x; }).Where(x => x > 2);

            var items = Pull(Heap(query));

            Assert.Equal([3, 4, 5, 9, 6], items.ToArray());
            Assert.Equal(Data.Length, calls);
        }

        [Fact]
        public void HeapEnumerator_DisposesTheSourceOnceAtTheEnd()
        {
            var tracked = new Tracked();

            var enumerator = Heap(Query(tracked.Iterate(Data)));
            while (enumerator.MoveNext())
            {
            }
            enumerator.Dispose();

            Assert.Equal(1, tracked.Disposed);
        }

        [Fact]
        public void HeapEnumerator_DisposesTheSourceOnceWhenStoppedEarly()
        {
            var tracked = new Tracked();

            using (var enumerator = Heap(Query(tracked.Iterate(Data))))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current > 0)
                        break;
                }
            }

            Assert.Equal(1, tracked.Disposed);
        }
    }
}
