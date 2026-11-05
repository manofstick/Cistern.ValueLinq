using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class ThenByDescendingOverSystemLinqOrdering
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
                var byDescendingSecondary = secondary.Compare(items[y], items[x]);
                var bySource = x.CompareTo(y);

                var order =
                    (byLastDigit, byDescendingSecondary) switch
                    {
                        (not 0, _) => byLastDigit,
                        (0, not 0) => byDescendingSecondary,
                        _ => bySource,
                    };

                return order;
            }
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [7], [101], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void OrdersTheTiesOfASystemLinqOrderingDescending(int length)
        {
            var items = Scrambled(length);
            var descending = Expected(items, Comparer<int>.Default);
            var descendingReversed = Expected(items, Reversed);

            Assert.Equal(descending, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenByDescending(x => x)));
            Assert.Equal(descending, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenByDescending(x => x, null)));
            Assert.Equal(descendingReversed, Pull(System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenByDescending(x => x, Reversed)));
        }

        [Fact]
        public void HandsTheKeySelectorAndComparerToTheOrderingAndReturnsItsResult()
        {
            Func<int, int> keySelector = x => x;
            var ordering = new RecordingOrdering();

            var ordered = ordering.ThenByDescending(keySelector);

            Assert.Equal(1, ordering.Calls);
            Assert.Same(keySelector, ordering.KeySelector);
            Assert.Null(ordering.Comparer);
            Assert.True(ordering.Descending);
            Assert.Same(ordering.Result, ordered);

            var withComparer = new RecordingOrdering();

            var orderedWithComparer = withComparer.ThenByDescending(keySelector, Reversed);

            Assert.Equal(1, withComparer.Calls);
            Assert.Same(keySelector, withComparer.KeySelector);
            Assert.Same(Reversed, withComparer.Comparer);
            Assert.True(withComparer.Descending);
            Assert.Same(withComparer.Result, orderedWithComparer);
        }

        [Fact]
        public void ANullKeySelectorIsTheOrderingsToReject()
        {
            var ordering = new RecordingOrdering();

            ordering.ThenByDescending((Func<int, int>)null);

            Assert.Equal(1, ordering.Calls);
            Assert.Null(ordering.KeySelector);

            var items = Scrambled(7);

            Assert.Throws<ArgumentNullException>("keySelector", () => System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenByDescending((Func<int, int>)null));
            Assert.Throws<ArgumentNullException>("keySelector", () => System.Linq.Enumerable.OrderBy(items, x => x % 10).ThenByDescending((Func<int, int>)null, Reversed));
        }

        [Fact]
        public void NullSourceThrowsBeforeTheKeySelectorIsChecked()
        {
            System.Linq.IOrderedEnumerable<int> source = null;

            Assert.Throws<ArgumentNullException>("source", () => source.ThenByDescending(x => x));
            Assert.Throws<ArgumentNullException>("source", () => source.ThenByDescending(x => x, Reversed));
            Assert.Throws<ArgumentNullException>("source", () => source.ThenByDescending((Func<int, int>)null));
        }
    }
}
