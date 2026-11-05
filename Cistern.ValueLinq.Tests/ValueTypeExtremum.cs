using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Min, Max, MinBy and MaxBy over a value type (or value-type key) that is not an integer, or that arrives element by
    // element, compare through Comparer<T>.Default directly, with System.Linq's answers: the first of equal keys kept,
    // nullables and references still ordered with their nulls.
    public class ValueTypeExtremum
    {
        private static IEnumerable<T> Iterate<T>(T[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        private static int[] Values()
        {
            var random = new Random(20261101);
            var values = new int[1003];
            for (var i = 0; i < values.Length; ++i)
                values[i] = random.Next(-1000, 1000);
            return values;
        }

        [Fact]
        public void MinAndMax()
        {
            var values = Values();
            var spans = Array.ConvertAll(values, x => TimeSpan.FromTicks(x));
            var ulongs = Array.ConvertAll(values, x => (ulong)(x + 1000));

            Assert.Equal(System.Linq.Enumerable.Min(spans), spans.Min());
            Assert.Equal(System.Linq.Enumerable.Max(spans), new List<TimeSpan>(spans).Max());
            Assert.Equal(System.Linq.Enumerable.Min(spans), Iterate(spans).Min());
            Assert.Equal(System.Linq.Enumerable.Max(ulongs), Iterate(ulongs).Max());
            Assert.Equal(System.Linq.Enumerable.Min(values, x => (short)x), values.Min(x => (short)x));
            Assert.Throws<InvalidOperationException>(() => new TimeSpan[0].Min());
            Assert.Throws<InvalidOperationException>(() => Iterate(new ulong[0]).Max());
        }

        [Fact]
        public void NullsAndReferences()
        {
            var nullables = new TimeSpan?[] { null, TimeSpan.FromTicks(5), null, TimeSpan.FromTicks(-3) };
            var words = new[] { "b", null, "a", "C" };

            Assert.Equal(System.Linq.Enumerable.Min(nullables), nullables.Min());
            Assert.Equal(System.Linq.Enumerable.Max(nullables), nullables.Max());
            Assert.Equal(System.Linq.Enumerable.Min(words), words.Min());
            Assert.Equal(System.Linq.Enumerable.Max(words), words.Max());
        }

        [Fact]
        public void ByKey()
        {
            var values = Values();
            Func<int, int> key = x => x % 7;

            Assert.Equal(System.Linq.Enumerable.MinBy(values, key), values.MinBy(key));
            Assert.Equal(System.Linq.Enumerable.MaxBy(values, key), values.MaxBy(key));
            Assert.Equal(System.Linq.Enumerable.MinBy(values, key), Iterate(values).MinBy(key));
            Assert.Equal(System.Linq.Enumerable.MaxBy(values, key), Iterate(values).MaxBy(key));
            Assert.Equal(System.Linq.Enumerable.MinBy(values, key, Comparer<int>.Default), values.MinBy(key, Comparer<int>.Default));

            Func<int, int?> nullableKey = x => x % 5 == 0 ? null : x;
            Assert.Equal(System.Linq.Enumerable.MinBy(values, nullableKey), values.MinBy(nullableKey));
            Assert.Equal(System.Linq.Enumerable.MaxBy(values, nullableKey), Iterate(values).MaxBy(nullableKey));
        }
    }
}
