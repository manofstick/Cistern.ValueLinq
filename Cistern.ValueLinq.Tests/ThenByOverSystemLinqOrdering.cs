using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ThenByOverSystemLinqOrdering
    {
        private static readonly IComparer<int> Reversed = Comparer<int>.Create((x, y) => y.CompareTo(x));

        private sealed class RecordingOrdering : System.Linq.IOrderedEnumerable<int>
        {
            public int Calls;
            public object KeySelector;
            public object Comparer;
            public bool Descending;
            public RecordingOrdering Result;

            System.Linq.IOrderedEnumerable<int> System.Linq.IOrderedEnumerable<int>.CreateOrderedEnumerable<TKey>(Func<int, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            {
                ++Calls;
                KeySelector = keySelector;
                Comparer = comparer;
                Descending = descending;

                Result = new RecordingOrdering();

                return Result;
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => throw new InvalidOperationException();

            IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();
        }

        private static int[] Scrambled(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = (i * 37 + 11) % 101;

            return items;
        }

        private static List<int> Pull(IEnumerable<int> source)
        {
            var items = new List<int>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static int[] Expected(int[] items, IComparer<int> secondary)
        {
            var indexes = new int[items.Length];
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            Array.Sort(indexes, compare);

            var expected = new int[items.Length];
            for (var i = 0; i < indexes.Length; ++i)
                expected[i] = items[indexes[i]];

            return expected;

            int compare(int x, int y)
            {
                var byLastDigit = (items[x] % 10).CompareTo(items[y] % 10);
                var bySecondary = secondary.Compare(items[x], items[y]);
                var bySource = x.CompareTo(y);

                var order =
                    (byLastDigit, bySecondary) switch
                    {
                        (not 0, _) => byLastDigit,
                        (0, not 0) => bySecondary,
                        _ => bySource,
                    };

                return order;
            }
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void OrdersTheTiesOfASystemLinqOrdering(int length)
        {
            var items = Scrambled(length);
            var ascending = Expected(items, Comparer<int>.Default);
            var reversed = Expected(items, Reversed);

            Assert.Equal(ascending, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenBy(x => x)));
            Assert.Equal(ascending, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenBy(x => x, null)));
            Assert.Equal(reversed, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenBy(x => x, Reversed)));
        }

        [Fact]
        public void HandsTheKeySelectorAndComparerToTheOrderingAndReturnsItsResult()
        {
            Func<int, int> keySelector = x => x;
            var ordering = new RecordingOrdering();

            var ordered = ordering.ThenBy(keySelector);

            Assert.Equal(1, ordering.Calls);
            Assert.Same(keySelector, ordering.KeySelector);
            Assert.Null(ordering.Comparer);
            Assert.False(ordering.Descending);
            Assert.Same(ordering.Result, ordered);

            var withComparer = new RecordingOrdering();

            var orderedWithComparer = withComparer.ThenBy(keySelector, Reversed);

            Assert.Equal(1, withComparer.Calls);
            Assert.Same(keySelector, withComparer.KeySelector);
            Assert.Same(Reversed, withComparer.Comparer);
            Assert.False(withComparer.Descending);
            Assert.Same(withComparer.Result, orderedWithComparer);
        }

        [Fact]
        public void ANullKeySelectorIsTheOrderingsToReject()
        {
            var ordering = new RecordingOrdering();

            ordering.ThenBy((Func<int, int>)null);

            Assert.Equal(1, ordering.Calls);
            Assert.Null(ordering.KeySelector);

            var items = Scrambled(7);

            Assert.Throws<ArgumentNullException>("keySelector", () => System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenBy((Func<int, int>)null));
            Assert.Throws<ArgumentNullException>("keySelector", () => System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenBy((Func<int, int>)null, Reversed));
        }

        [Fact]
        public void NullSourceThrowsBeforeTheKeySelectorIsChecked()
        {
            System.Linq.IOrderedEnumerable<int> source = null;

            Assert.Throws<ArgumentNullException>("source", () => source.ThenBy(x => x));
            Assert.Throws<ArgumentNullException>("source", () => source.ThenBy(x => x, Reversed));
            Assert.Throws<ArgumentNullException>("source", () => source.ThenBy((Func<int, int>)null));
        }
    }
}
