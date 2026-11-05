using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A Skip or Take over OrderBy, Reverse or Shuffle is a window over the node's buffer, built once when the window is
    // run; a request the Skip or Take puts through the window first builds nothing.
    public class BufferedWindows
    {
        private sealed class Source
        {
            public int Enumerations { get; private set; }

            public IEnumerable<int> Iterate(int count)
            {
                ++Enumerations;
                for (var i = count; i > 0; --i)
                    yield return i;
            }
        }

        public static IEnumerable<object[]> Shapes()
        {
            foreach (var shape in new[] { "OrderBy", "Reverse", "Shuffle" })
                yield return [shape];
        }

        private static IEnumerable<int> Buffered(string shape, IEnumerable<int> source, Func<int, int> key) =>
            shape switch
            {
                "OrderBy" => source.OrderBy(key),
                "Reverse" => source.Reverse(),
                "Shuffle" => source.Shuffle(),
                _ => throw new ArgumentOutOfRangeException(nameof(shape)),
            };

        [Theory]
        [MemberData(nameof(Shapes))]
        public void ToListThroughSkipOrTakeRunsTheSourceOnce(string shape)
        {
            var source = new Source();
            var keys = 0;
            Func<int, int> key = i => { ++keys; return i; };

            var taken = Buffered(shape, source.Iterate(10), key).Take(3).ToList();
            var skipped = Buffered(shape, source.Iterate(10), key).Skip(3).ToList();

            Assert.Equal(3, taken.Count);
            Assert.Equal(7, skipped.Count);
            Assert.Equal(2, source.Enumerations);
            Assert.Equal(shape == "OrderBy" ? 20 : 0, keys);
            if (shape != "Shuffle")
            {
                Assert.Equal(new[] { 1, 2, 3 }, taken);
                Assert.Equal(new[] { 4, 5, 6, 7, 8, 9, 10 }, skipped);
            }
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void CountThroughSkipOrTakeBuildsNoBuffer(string shape)
        {
            var source = new Source();
            var keys = 0;
            Func<int, int> key = i => { ++keys; return i; };

            Assert.Equal(3, Buffered(shape, source.Iterate(5), key).Take(3).Count());
            Assert.Equal(2, Buffered(shape, source.Iterate(5), key).Skip(3).Count());
            Assert.Equal(1, Buffered(shape, source.Iterate(5), key).Skip(1).Take(3).Skip(2).Count());
            Assert.Equal(0, keys);
        }

        [Theory]
        [MemberData(nameof(Shapes))]
        public void ToArrayAndPushThroughSkipAndTakeRunTheSourceOnce(string shape)
        {
            var source = new Source();
            var keys = 0;
            Func<int, int> key = i => { ++keys; return i; };

            var array = Buffered(shape, source.Iterate(10), key).Skip(2).Take(5).ToArray();
            var sum = Buffered(shape, source.Iterate(10), key).Skip(2).Take(5).Sum();

            Assert.Equal(5, array.Length);
            Assert.Equal(2, source.Enumerations);
            Assert.Equal(shape == "OrderBy" ? 20 : 0, keys);
            if (shape != "Shuffle")
            {
                Assert.Equal(new[] { 3, 4, 5, 6, 7 }, array);
                Assert.Equal(25, sum);
            }
        }

        [Fact]
        public void ElementAtThroughSkipOrTake()
        {
            IEnumerable<int> source = new List<int> { 5, 4, 3, 2, 1 };

            Assert.Equal(1, source.OrderBy(i => i).Take(3).First());
            Assert.Equal(3, source.OrderBy(i => i).Skip(1).Take(3).ElementAt(1));
            Assert.Equal(2, source.Reverse().Skip(1).ElementAt(0));
            Assert.Equal(0, source.OrderBy(i => i).Take(2).ElementAtOrDefault(2));
        }
    }
}
