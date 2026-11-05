using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Order and OrderDescending of a type whose equal elements are bit-identical sort the elements themselves; the
    // result is the one a keyed sort (OrderBy through a delegate) gives.
    public class OrderedImplicitlyStable
    {
        private enum Colour : sbyte { Red = -3, Green = 0, Blue = 5, Teal = 5 }

        private static T[] Values<T>(Func<int, T> make)
        {
            var random = new Random(17);
            var values = new T[1000];
            for (var i = 0; i < values.Length; ++i)
                values[i] = make(random.Next(-50, 50));
            return values;
        }

        private static void Same<T>(T[] values)
        {
            Assert.Equal(values.OrderBy(x => x).ToArray(), values.Order().ToArray());
            Assert.Equal(values.OrderByDescending(x => x).ToArray(), values.OrderDescending().ToArray());
            Assert.Equal(values.OrderBy(x => x).ToList(), values.Order().ToList());

            var pulled = new List<T>();
            foreach (var value in values.Order())
                pulled.Add(value);
            Assert.Equal(values.OrderBy(x => x).ToArray(), pulled);
        }

        [Fact]
        public void Integers()
        {
            Same(Values(x => x));
            Same(Values(x => (long)x * 1_000_000_000_000L));
            Same(Values(x => (byte)x));
            Same(Values(x => (nint)x));
            Same(Values(x => (Int128)x << 100));
        }

        [Fact]
        public void CharBoolAndEnum()
        {
            Same(Values(x => (char)('a' + (x & 15))));
            Same(Values(x => x > 0));
            Same(Values(x => (Colour)(x % 4 switch { 0 => -3, 1 => 0, _ => 5 })));
        }

        [Fact]
        public void ThenByAndWindow()
        {
            var values = Values(x => x);

            Assert.Equal(values.OrderBy(x => x).ThenBy(x => -x).ToArray(), values.Order().ThenBy(x => -x).ToArray());
            Assert.Equal(values.OrderBy(x => x).Skip(10).Take(20).ToArray(), values.Order().Skip(10).Take(20).ToArray());
        }
    }
}
