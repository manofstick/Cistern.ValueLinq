using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Min and Max of an integer type with no overload of its own (the generic forms), and with a comparer that is
    // Comparer<T>.Default, run a span a vector at a time; the answer is the element loop's, a Where's chunks carry the
    // running extremum, and any other comparer is still called.
    public class IntegerExtremumSpans
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private static short[] Shorts(int count)
        {
            var random = new Random(20261101);
            var values = new short[count];
            for (var i = 0; i < count; ++i)
                values[i] = (short)random.Next(short.MinValue, short.MaxValue + 1);
            return values;
        }

        [Fact]
        public void Generic()
        {
            foreach (var length in new[] { 1, 7, 100, 1003 })
            {
                var values = Shorts(length);

                Assert.Equal(Iterate(values).Min(), values.Min());
                Assert.Equal(Iterate(values).Max(), values.Max());
                Assert.Equal(Iterate(values).Min(), new List<short>(values).Min());
                Assert.Equal(Iterate(values).Max(), new List<short>(values).Max());
            }

            var chunked = Shorts(1003);
            Assert.Equal(Iterate(chunked).Where(x => x % 3 != 0).Min(), chunked.Where(x => x % 3 != 0).Min());

            var bytes = new byte[] { 7, 3, 250, 0, 9 };
            Assert.Equal((byte)0, bytes.Min());
            Assert.Equal((byte)250, bytes.Max());
            var unsigned = new ulong[] { 7, ulong.MaxValue, 3 };
            Assert.Equal(3UL, unsigned.Min());
            Assert.Equal(ulong.MaxValue, unsigned.Max());

            Assert.Throws<InvalidOperationException>(() => new short[0].Min());
            Assert.Throws<InvalidOperationException>(() => new short[0].Max());
        }

        [Fact]
        public void DefaultComparer()
        {
            var values = new int[1003];
            var random = new Random(20261101);
            for (var i = 0; i < values.Length; ++i)
                values[i] = random.Next();

            Assert.Equal(Iterate(values).Min(), values.Min(Comparer<int>.Default));
            Assert.Equal(Iterate(values).Max(), new List<int>(values).Max(Comparer<int>.Default));
            Assert.Equal(Iterate(values).Max(), values.Max((IComparer<int>?)null));
            Assert.Throws<InvalidOperationException>(() => new int[0].Max(Comparer<int>.Default));
        }

        [Fact]
        public void OtherComparer()
        {
            var values = new[] { 3, -9, 5 };
            var calls = 0;
            var reversed = Comparer<int>.Create((x, y) => { ++calls; return y.CompareTo(x); });

            Assert.Equal(5, values.Min(reversed));
            Assert.Equal(-9, values.Max(reversed));
            Assert.Equal(4, calls);
        }

        [Fact]
        public void OrderedEnds()
        {
            var values = Shorts(1003);

            Assert.Equal(Iterate(values).Min(), values.Order().First());
            Assert.Equal(Iterate(values).Max(), values.OrderDescending().First());
            Assert.Equal(Iterate(values).Max(), new List<short>(values).Order().Last());
        }
    }
}
