using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // An int or long Sum over a span runs four vector accumulators and tests their overflow once at the end; it throws
    // exactly where System.Linq's does, wherever in the span (any accumulator, the remainder, the scalar tail) the
    // overflowing values sit.
    public class IntSumOverflow
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private static string Outcome(Func<object> sum)
        {
            try
            {
                return sum().ToString()!;
            }
            catch (OverflowException)
            {
                return "overflow";
            }
        }

        [Fact]
        public void Int()
        {
            foreach (var length in new[] { 3, 100, 1003 })
            {
                for (var position = 0; position < length; ++position)
                {
                    var values = new int[length];
                    values[position] = int.MaxValue;
                    values[(position + 37) % length] += 1;

                    var expected = Outcome(() => System.Linq.Enumerable.Sum(values));
                    Assert.Equal(expected, Outcome(() => values.Sum()));
                    Assert.Equal(expected, Outcome(() => new List<int>(values).Sum()));
                    Assert.Equal(expected, Outcome(() => Iterate(values).Sum()));

                    values[(position + 37) % length] -= 2;
                    Assert.Equal(Outcome(() => System.Linq.Enumerable.Sum(values)), Outcome(() => values.Sum()));
                }
            }
        }

        [Fact]
        public void IntOverflowingOnlyBeforeTheEnd()
        {
            var values = new int[1000];
            values[0] = int.MaxValue;
            values[1] = 1;
            values[2] = -1;

            Assert.Equal(Outcome(() => System.Linq.Enumerable.Sum(values)), Outcome(() => values.Sum()));
            Assert.Throws<OverflowException>(() => values.Sum());
        }

        [Fact]
        public void Long()
        {
            foreach (var length in new[] { 3, 100, 1003 })
            {
                for (var position = 0; position < length; ++position)
                {
                    var values = new long[length];
                    values[position] = long.MinValue;
                    values[(position + 37) % length] -= 1;

                    var expected = Outcome(() => System.Linq.Enumerable.Sum(values));
                    Assert.Equal(expected, Outcome(() => values.Sum()));
                    Assert.Equal(expected, Outcome(() => new List<long>(values).Sum()));
                }
            }
        }

        [Fact]
        public void Values()
        {
            var random = new Random(20261101);
            var values = new int[10_000];
            for (var i = 0; i < values.Length; ++i)
                values[i] = random.Next(-100_000, 100_000);

            Assert.Equal(System.Linq.Enumerable.Sum(values), values.Sum());
            Assert.Equal(System.Linq.Enumerable.Sum(values), new List<int>(values).Sum());
            Assert.Equal(System.Linq.Enumerable.Sum(values), values.Where(x => x != 0).Sum());
        }
    }
}
