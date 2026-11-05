using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class SingleOrDefaultValue
    {
        private const int Missing = -1;
        private const int Threw = -2;

        private static int[] Sequence(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = i;

            return items;
        }

        private static IEnumerable<int> Iterate(int count)
        {
            for (var i = 0; i < count; ++i)
                yield return i;
        }

        private static int Outcome(Func<int> call)
        {
            try
            {
                return call();
            }
            catch (InvalidOperationException)
            {
                return Threw;
            }
        }

        private static int Pulled(IEnumerable<int> source, int defaultValue) => Pulled(source, _ => true, defaultValue);

        private static int Pulled(IEnumerable<int> source, Func<int, bool> predicate, int defaultValue)
        {
            var found = false;
            var single = defaultValue;
            foreach (var item in source)
            {
                if (!predicate(item))
                    continue;

                if (found)
                    return Threw;

                found = true;
                single = item;
            }

            return single;
        }

        private static bool IsFive(int x) => x == 5;
        private static bool OverFive(int x) => x > 5;

        public static IEnumerable<object[]> Lengths() => [[0], [1], [2], [6], [7], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersItsOnlyElement(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;
            IEnumerable<int> enumerable = Iterate(length);

            var single = length switch { 0 => Missing, 1 => 0, _ => Threw };
            Assert.Equal(single, Outcome(() => array.SingleOrDefault(Missing)));
            Assert.Equal(single, Outcome(() => list.SingleOrDefault(Missing)));
            Assert.Equal(single, Outcome(() => memory.SingleOrDefault(Missing)));
            Assert.Equal(single, Outcome(() => enumerable.SingleOrDefault(Missing)));
            Assert.Equal(single, Outcome(() => ((IEnumerable<int>)array).SingleOrDefault(Missing)));
            Assert.Equal(single, Outcome(() => ((IEnumerable<int>)list).SingleOrDefault(Missing)));

            var five = length > 5 ? 5 : Missing;
            Assert.Equal(five, Outcome(() => array.SingleOrDefault(IsFive, Missing)));
            Assert.Equal(five, Outcome(() => list.SingleOrDefault(IsFive, Missing)));
            Assert.Equal(five, Outcome(() => memory.SingleOrDefault(IsFive, Missing)));
            Assert.Equal(five, Outcome(() => enumerable.SingleOrDefault(IsFive, Missing)));
            Assert.Equal(five, Outcome(() => ((IEnumerable<int>)array).SingleOrDefault(IsFive, Missing)));
            Assert.Equal(five, Outcome(() => ((IEnumerable<int>)list).SingleOrDefault(IsFive, Missing)));

            var overFive = length switch { <= 6 => Missing, 7 => 6, _ => Threw };
            Assert.Equal(overFive, Outcome(() => array.SingleOrDefault(OverFive, Missing)));
            Assert.Equal(overFive, Outcome(() => list.SingleOrDefault(OverFive, Missing)));
            Assert.Equal(overFive, Outcome(() => memory.SingleOrDefault(OverFive, Missing)));
            Assert.Equal(overFive, Outcome(() => enumerable.SingleOrDefault(OverFive, Missing)));
            Assert.Equal(overFive, Outcome(() => ((IEnumerable<int>)array).SingleOrDefault(OverFive, Missing)));
            Assert.Equal(overFive, Outcome(() => ((IEnumerable<int>)list).SingleOrDefault(OverFive, Missing)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerablePushAgreesWithPull(int length)
        {
            var array = Sequence(length);

            var selected = array.Select(x => x * 3);
            Assert.Equal(Pulled(selected, Missing), Outcome(() => selected.SingleOrDefault(Missing)));
            Assert.Equal(Pulled(selected, OverFive, Missing), Outcome(() => selected.SingleOrDefault(OverFive, Missing)));
            Assert.Equal(Pulled(selected, IsFive, Missing), Outcome(() => selected.SingleOrDefault(IsFive, Missing)));

            var filtered = array.Where(x => x % 5 == 4);
            Assert.Equal(Pulled(filtered, Missing), Outcome(() => filtered.SingleOrDefault(Missing)));
            Assert.Equal(Pulled(filtered, OverFive, Missing), Outcome(() => filtered.SingleOrDefault(OverFive, Missing)));

            var iterated = Iterate(length).Select(x => x + 1).Where(x => x % 2 == 0);
            Assert.Equal(Pulled(iterated, Missing), Outcome(() => iterated.SingleOrDefault(Missing)));
            Assert.Equal(Pulled(iterated, OverFive, Missing), Outcome(() => iterated.SingleOrDefault(OverFive, Missing)));

            var ranged = Enumerable.Range(5, length);
            Assert.Equal(Pulled(ranged, Missing), Outcome(() => ranged.SingleOrDefault(Missing)));
            Assert.Equal(Pulled(ranged, IsFive, Missing), Outcome(() => ranged.SingleOrDefault(IsFive, Missing)));

            var ordered = Sequence(length).OrderByDescending(x => x);
            Assert.Equal(Pulled(ordered, Missing), Outcome(() => ordered.SingleOrDefault(Missing)));
            Assert.Equal(Pulled(ordered, OverFive, Missing), Outcome(() => ordered.SingleOrDefault(OverFive, Missing)));
        }

        [Fact]
        public void SpansAcrossConcatPartsCountTogether()
        {
            static int Single(int[] first, int[] second) => Outcome(() => first.Select(x => x).Concat(second.Select(x => x)).SingleOrDefault(Missing));
            static int SingleOverFive(int[] first, int[] second) => Outcome(() => first.Select(x => x).Concat(second.Select(x => x)).SingleOrDefault(OverFive, Missing));

            Assert.Equal(7, Single([], [7]));
            Assert.Equal(7, Single([7], []));
            Assert.Equal(Missing, Single([], []));
            Assert.Equal(Threw, Single([7], [8]));
            Assert.Equal(Threw, Single([7, 8], []));

            Assert.Equal(7, SingleOverFive([1, 2], [7, 3]));
            Assert.Equal(7, SingleOverFive([7, 2], [1, 3]));
            Assert.Equal(Missing, SingleOverFive([1, 2], [3]));
            Assert.Equal(Threw, SingleOverFive([1, 7], [3, 8]));
            Assert.Equal(Threw, SingleOverFive([7, 8], [1]));
        }

        [Fact]
        public void PredicateStopsAtTheSecondMatch()
        {
            var calls = 0;
            bool Counted(int x)
            {
                ++calls;
                return x > 5;
            }

            Assert.Throws<InvalidOperationException>(() => Sequence(10).SingleOrDefault(Counted, Missing));
            Assert.Equal(8, calls);

            calls = 0;
            Assert.Throws<InvalidOperationException>(() => Iterate(10).SingleOrDefault(Counted, Missing));
            Assert.Equal(8, calls);

            calls = 0;
            Assert.Throws<InvalidOperationException>(() => Sequence(10).Select(x => x).SingleOrDefault(Counted, Missing));
            Assert.Equal(8, calls);

            calls = 0;
            Assert.Equal(6, Sequence(7).SingleOrDefault(Counted, Missing));
            Assert.Equal(7, calls);
        }

        [Fact]
        public void ReferenceDefaultValueIsReturnedAsGiven()
        {
            var fallback = "fallback";
            Assert.Same(fallback, Array.Empty<string>().SingleOrDefault(fallback));
            Assert.Same(fallback, new[] { "a", "b" }.SingleOrDefault(s => s.Length > 1, fallback));
            Assert.Same(fallback, new List<string>().Select(s => s).SingleOrDefault(fallback));
            Assert.Null(new[] { "a" }.Where(s => s.Length > 1).SingleOrDefault((string)null!));
            Assert.Equal("b", new[] { "a", "b" }.Select(s => s).SingleOrDefault(s => s == "b", fallback));
            Assert.Throws<InvalidOperationException>(() => new[] { "a", "b" }.Select(s => s).SingleOrDefault(fallback));
        }

        [Fact]
        public void NullArgumentsThrow()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).SingleOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).SingleOrDefault(Missing));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).SingleOrDefault(IsFive, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).SingleOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => new List<int>(Sequence(3)).SingleOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Iterate(3).SingleOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Sequence(3).Select(x => x).SingleOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Array.Empty<int>().SingleOrDefault(null!, Missing));
            Assert.Throws<ArgumentNullException>("predicate", () => Enumerable.Range(0, 0).SingleOrDefault(null!, Missing));
        }

        private sealed class Tracked
        {
            public int Disposed;
            public int Yielded;

            public IEnumerable<int> Iterate(int count)
            {
                try
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ++Yielded;
                        yield return i;
                    }
                }
                finally
                {
                    ++Disposed;
                }
            }
        }

        [Fact]
        public void SourceIsDisposedWhenTheSecondElementStopsThePush()
        {
            var tracked = new Tracked();

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).SingleOrDefault(Missing));
            Assert.Equal(1, tracked.Disposed);
            Assert.Equal(2, tracked.Yielded);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).SingleOrDefault(OverFive, Missing));
            Assert.Equal(2, tracked.Disposed);
            Assert.Equal(2 + 8, tracked.Yielded);

            Assert.Throws<InvalidOperationException>(() => tracked.Iterate(10).Select(x => x * 2).SingleOrDefault(OverFive, Missing));
            Assert.Equal(3, tracked.Disposed);

            Assert.Equal(Missing, tracked.Iterate(3).SingleOrDefault(OverFive, Missing));
            Assert.Equal(4, tracked.Disposed);

            Assert.Equal(0, tracked.Iterate(1).Select(x => x).SingleOrDefault(Missing));
            Assert.Equal(5, tracked.Disposed);
        }

        [Fact]
        public void SourceIsDisposedWhenThePredicateThrows()
        {
            var tracked = new Tracked();

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).SingleOrDefault(x => x == 4 ? throw new DivideByZeroException() : false, Missing));
            Assert.Equal(1, tracked.Disposed);

            Assert.Throws<DivideByZeroException>(() => tracked.Iterate(10).Select(x => x).SingleOrDefault(x => x == 4 ? throw new DivideByZeroException() : false, Missing));
            Assert.Equal(2, tracked.Disposed);
        }
    }
}
