using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A sequence a chain pulls from inside itself (a Concat part, a SelectMany inner sequence, Zip's second) that is a
    // Cistern chain reached as IEnumerable<T>, either directly or through a wrapper that hands out the chain's own
    // enumerator, gives the same elements, has GetEnumerator called once, and has its source disposed exactly once
    // whether the query runs to the end, stops early or throws. The enumerator a wrapper handed out still reports the
    // element it last gave.
    public class PulledInnerSequences
    {
        private sealed class Tracked
        {
            public int Started;
            public int Disposed;

            public IEnumerable<int> Iterate(int count, int throwAt = -1)
            {
                ++Started;
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        if (i == throwAt)
                            throw new InvalidOperationException("source");
                        yield return i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }

            public void AssertOneRun()
            {
                Assert.Equal(1, Started);
                Assert.Equal(1, Disposed);
            }
        }

        private sealed class Forwarding<T>(IEnumerable<T> source)
            : IEnumerable<T>
        {
            public int GetEnumeratorCalls;
            public IEnumerator<T>? Handed;

            public IEnumerator<T> GetEnumerator()
            {
                ++GetEnumeratorCalls;
                return Handed = source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static int[] Sequence(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = i;
            return items;
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);
            return items;
        }

        private static IEnumerable<int> Wrap(IEnumerable<int> chain, bool forward) => forward ? new Forwarding<int>(chain) : chain;

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Concat_Parts(bool forward)
        {
            var first = Wrap(Sequence(3).Select(x => x * 2), forward);
            var second = Wrap(Sequence(2).Select(x => x * 2), forward);

            Assert.Equal([0, 2, 4, 0, 2, 100], Pull(first.Concat(second).Concat([100])));
            if (forward)
            {
                Assert.Equal(1, ((Forwarding<int>)first).GetEnumeratorCalls);
                Assert.Equal(1, ((Forwarding<int>)second).GetEnumeratorCalls);
            }
        }

        // the shape of a concat built through a wrapper at every level: no level can see the concat below it
        [Fact]
        public void Concat_NestedThroughWrappers()
        {
            var wrappers = new List<Forwarding<int>>();
            IEnumerable<int> chain = Array.Empty<int>();
            for (var i = 0; i < 10; ++i)
            {
                var wrapper = new Forwarding<int>(chain);
                wrappers.Add(wrapper);
                chain = wrapper.Concat([i]);
            }

            Assert.Equal(Sequence(10), Pull(chain));
            foreach (var wrapper in wrappers)
                Assert.Equal(1, wrapper.GetEnumeratorCalls);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Concat_RunsToTheEnd_DisposesPartSourceOnce(bool forward)
        {
            var tracked = new Tracked();

            Assert.Equal([0, 2, 4, 100], Pull(Wrap(tracked.Iterate(3).Select(x => x * 2), forward).Concat([100])));
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Concat_StopsEarly_DisposesPartSourceOnce(bool forward)
        {
            var tracked = new Tracked();

            Assert.Equal([0, 2], Pull(Wrap(tracked.Iterate(100).Select(x => x * 2), forward).Concat([100]).Take(2)));
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Concat_ExceptionPartWay_DisposesPartSource(bool forward)
        {
            var tracked = new Tracked();

            Assert.Throws<InvalidOperationException>(() => Pull(Wrap(tracked.Iterate(100, throwAt: 50).Select(x => x * 2), forward).Concat([100])));
            tracked.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SelectMany_InnerSequences_StopsEarly_DisposesEachStartedSourceOnce(bool forward)
        {
            var tracked = new List<Tracked>();
            IEnumerable<int> Inner(int n)
            {
                var source = new Tracked();
                tracked.Add(source);
                return Wrap(source.Iterate(n).Select(x => x + 10 * n), forward);
            }

            Assert.Equal([10, 20, 21, 30], Pull(new[] { 1, 2, 3 }.SelectMany(Inner).Take(4)));
            Assert.Equal(3, tracked.Count);
            foreach (var source in tracked)
                source.AssertOneRun();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Zip_SecondSequence_DisposesSourceOnce(bool forward)
        {
            var tracked = new Tracked();
            var items = new List<(int, int)>();

            foreach (var item in Sequence(5).Select(x => x * 2).Zip(Wrap(tracked.Iterate(3).Select(x => x * 2), forward)))
                items.Add(item);

            Assert.Equal([(0, 0), (2, 2), (4, 4)], items);
            tracked.AssertOneRun();
        }

        [Fact]
        public void HandedOutEnumerator_ReportsTheElementItLastGave()
        {
            var wrapper = new Forwarding<int>(Sequence(5).Select(x => x * 2));

            foreach (var item in wrapper.Concat([100]))
            {
                if (item == 100)
                    break;
                Assert.Equal(item, wrapper.Handed!.Current);
            }
        }
    }
}
