using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class InfiniteSequenceNumbers
    {
        private const int Length = 40;

        private static List<T> PullFirst<T>(IEnumerable<T> source, int count)
        {
            var items = new List<T>();
            using var enumerator = source.GetEnumerator();
            while (items.Count < count
                && enumerator.MoveNext())
            {
                items.Add(enumerator.Current);
            }

            return items;
        }

        private static T[] Added<T>(T start, T step, int count)
            where T : IAdditionOperators<T, T, T>
        {
            var items = new T[count];
            var value = start;
            for (var i = 0; i < count; ++i)
            {
                if (i > 0)
                    value += step;

                items[i] = value;
            }

            return items;
        }

        private static void AssertSequence<T>(T start, T step)
            where T : IAdditionOperators<T, T, T>
        {
            var walked = Added(start, step, Length + 3);
            var expected = walked[..Length];
            var sequence = Enumerable.InfiniteSequence(start, step);
            IEnumerable<T> enumerable = sequence;

            Assert.Equal(expected, PullFirst(sequence, Length));
            Assert.Equal(expected, PullFirst(enumerable, Length));
            Assert.Equal(expected, sequence.Take(Length).ToArray());
            Assert.Equal(expected, sequence.Take(Length).ToList());
            Assert.Equal(expected, enumerable.Take(Length).ToArray());
            Assert.Equal(expected, PullFirst(sequence.Take(Length), Length + 1));
            Assert.Equal(expected, sequence.Select(x => x).Take(Length).ToArray());
            Assert.Equal(expected, sequence.Where(x => true).Take(Length).ToArray());
            Assert.Equal(expected[0], sequence.First());
            Assert.Equal(Length, sequence.Take(Length).Count());
            Assert.Equal(expected[^1], sequence.Take(Length).Last());
            Assert.Equal(default, sequence.Take(Length).ElementAtOrDefault(Length));
            Assert.Equal(default, sequence.Take(Length).ElementAtOrDefault(^(Length + 1)));
            Assert.False(sequence.TryGetNonEnumeratedCount(out _));
            Assert.False(sequence.Take(Length).TryGetNonEnumeratedCount(out _));

            for (var i = 0; i < Length; i += 3)
            {
                Assert.Equal(expected[i], sequence.ElementAt(i));
                Assert.Equal(expected[i], enumerable.ElementAt(i));
                Assert.Equal(expected[i], sequence.Take(Length).ElementAt(i));
                Assert.Equal(expected[^(i + 1)], sequence.Take(Length).ElementAt(^(i + 1)));
            }

            foreach (var count in new[] { 0, 1, 2, Length / 2, Length - 1 })
            {
                var remaining = Length - count;
                var skipped = expected[count..];
                var prefix = expected[..count];
                var skippedThenTaken = walked[(count + 1)..(count + 3)];
                var takenSkippedThenTaken = expected[(count + 1)..Math.Min(count + 3, Length)];

                Assert.Equal(skipped, sequence.Skip(count).Take(remaining).ToArray());
                Assert.Equal(skipped, PullFirst(sequence.Skip(count), remaining));
                Assert.Equal(skipped, sequence.Take(Length).Skip(count).ToArray());
                Assert.Equal(skipped, PullFirst(sequence.Take(Length).Skip(count), Length));
                Assert.Equal(remaining, sequence.Take(Length).Skip(count).Count());
                Assert.Equal(prefix, sequence.Take(count).ToArray());
                Assert.Equal(prefix, PullFirst(sequence.Take(count), Length));
                Assert.Equal(skippedThenTaken, sequence.Skip(count).Skip(1).Take(2).ToArray());
                Assert.Equal(takenSkippedThenTaken, sequence.Take(Length).Skip(count).Skip(1).Take(2).ToArray());
                Assert.Equal(skippedThenTaken, enumerable.Skip(count).Skip(1).Take(2).ToList());
                Assert.Equal(expected[count], sequence.Skip(count).First());
                Assert.Equal(expected[count], sequence.Skip(count).ElementAt(0));
                Assert.Equal(expected[^1], sequence.Skip(count).Take(remaining).ElementAt(^1));
            }
        }

        [Fact]
        public void EveryIntegerTypeAddsTheStepWrappingPastItsLimits()
        {
            validate<byte>();
            validate<sbyte>();
            validate<short>();
            validate<ushort>();
            validate<char>();
            validate<int>();
            validate<uint>();
            validate<long>();
            validate<ulong>();
            validate<nint>();
            validate<nuint>();
            validate<Int128>();
            validate<UInt128>();

            static void validate<T>()
                where T : INumber<T>, IMinMaxValue<T>
            {
                var three = T.CreateTruncating(3);
                var ten = T.CreateTruncating(10);
                var largeStep = T.MaxValue / three;

                AssertSequence(T.Zero, T.Zero);
                AssertSequence(T.Zero, T.One);
                AssertSequence(T.Zero, three);
                AssertSequence(T.MaxValue - ten, T.One);
                AssertSequence(T.MaxValue - ten, three);
                AssertSequence(T.MaxValue, T.MaxValue);
                AssertSequence(T.MinValue, largeStep);
                AssertSequence(T.MinValue, T.MaxValue);
                AssertSequence(T.MaxValue, T.Zero - T.One);
                AssertSequence(T.MinValue + ten, T.Zero - three);
            }
        }

        [Fact]
        public void BigIntegerAddsTheStepExactly()
        {
            var huge = BigInteger.Pow(10, 40);

            AssertSequence(BigInteger.Zero, new BigInteger(3));
            AssertSequence(huge, huge);
            AssertSequence(-huge, -huge / 3);
        }

        [Fact]
        public void AFloatingPointOrDecimalSequenceAddsTheStepToTheLastElement()
        {
            AssertSequence(0.0, 0.1);
            AssertSequence(0.1f, 0.3f);
            AssertSequence(float.MaxValue / 2, float.MaxValue / 4);
            AssertSequence(16_777_216f, 1.0f);
            AssertSequence(double.NegativeInfinity, 1.0);
            AssertSequence((Half)1, (Half)0.1);
            AssertSequence(0.1m, 1m / 3m);
        }

        [Fact]
        public void AnAdditionIsMadeOnlyWhereTheWalkMakesIt()
        {
            var tally = new Tally();
            var sequence = Enumerable.InfiniteSequence(new Counted(0, tally), new Counted(1, tally));

            AssertSequence(new Counted(0, new Tally()), new Counted(1, new Tally()));

            Assert.Equal([0, 1, 2, 3, 4], sequence.Take(5).Select(x => x.Value).ToArray());
            Assert.Equal(4, tally.Additions);

            tally.Additions = 0;
            Assert.Equal([0, 1, 2, 3, 4], PullFirst(sequence.Take(5), 6).Select(x => x.Value).ToArray());
            Assert.Equal(4, tally.Additions);

            tally.Additions = 0;
            Assert.Equal([2, 3], sequence.Skip(2).Take(2).Select(x => x.Value).ToArray());
            Assert.Equal(3, tally.Additions);

            tally.Additions = 0;
            Assert.Equal(3, sequence.ElementAt(3).Value);
            Assert.Equal(3, tally.Additions);
        }

        [Fact]
        public void AFloatingPointElementIsTheSumOfItsAdditions()
        {
            var sequence = Enumerable.InfiniteSequence(0.0, 0.1);
            var tenth = Added(0.0, 0.1, 11)[10];

            Assert.NotEqual(1.0, tenth);
            Assert.Equal(tenth, sequence.ElementAt(10));
            Assert.Equal(tenth, sequence.Skip(10).First());
            Assert.Equal(tenth, sequence.Take(11).Last());
            Assert.Equal(tenth, sequence.Take(11).ElementAt(^1));
        }

        [Fact]
        public void ADecimalOverflowThrowsOnlyWhereItsAdditionIsMade()
        {
            var sequence = Enumerable.InfiniteSequence(decimal.MaxValue - 1, 1m);

            Assert.Equal([decimal.MaxValue - 1, decimal.MaxValue], sequence.Take(2).ToArray());
            Assert.Equal([decimal.MaxValue - 1, decimal.MaxValue], PullFirst(sequence.Take(2), 3));
            Assert.Equal([decimal.MaxValue - 1, decimal.MaxValue], sequence.Where(x => true).Take(2).ToArray());
            Assert.Equal(decimal.MaxValue, sequence.ElementAt(1));
            Assert.Throws<OverflowException>(() => sequence.Take(3).ToArray());
            Assert.Throws<OverflowException>(() => sequence.Take(3).ToList());
            Assert.Throws<OverflowException>(() => PullFirst(sequence, 3));
            Assert.Throws<OverflowException>(() => sequence.ElementAt(2));
            Assert.Throws<OverflowException>(() => sequence.Skip(2).First());
        }

        [Fact]
        public void ANullArgumentThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentNullException>("start", () => Enumerable.InfiniteSequence(null!, new Counted(1, new Tally())));
            Assert.Throws<ArgumentNullException>("step", () => Enumerable.InfiniteSequence(new Counted(0, new Tally()), null!));
        }

        [Fact]
        public void PushAndPullStopWhereTheConsumerStops()
        {
            var integerCalls = 0;
            var floatingCalls = 0;

            Assert.Equal(3, Enumerable.InfiniteSequence(0, 1).Select(countInteger).First(x => x == 3));
            Assert.Equal(1.5, Enumerable.InfiniteSequence(0.0, 0.5).Select(countFloating).First(x => x == 1.5));
            Assert.Equal(4, integerCalls);
            Assert.Equal(4, floatingCalls);

            IEnumerable<double>[] sequences =
            [
                Enumerable.InfiniteSequence(0.0, 0.5).Take(3),
                Enumerable.InfiniteSequence(0.0, 0.3).Skip(2).Take(1),
                Enumerable.InfiniteSequence(0.0, 0.0).Take(1),
                Enumerable.InfiniteSequence(0, 1).Skip(4).Take(5).Select(x => (double)x),
            ];

            foreach (var sequence in sequences)
            {
                using var enumerator = sequence.GetEnumerator();
                var pulled = 0;
                while (enumerator.MoveNext())
                    ++pulled;

                Assert.Equal(sequence.Count(), pulled);
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            int countInteger(int x)
            {
                ++integerCalls;

                return x;
            }

            double countFloating(double x)
            {
                ++floatingCalls;

                return x;
            }
        }

        [Fact]
        public void EachEnumerationStartsAgain()
        {
            var integers = Enumerable.InfiniteSequence(1, 2);
            var floating = Enumerable.InfiniteSequence(1.0, 0.5);

            Assert.Equal(PullFirst(integers, 5), PullFirst(integers, 5));
            Assert.Equal(integers.Take(5).ToArray(), integers.Take(5).ToArray());
            Assert.Equal(PullFirst(floating, 5), PullFirst(floating, 5));
            Assert.Equal(floating.Take(5).ToArray(), floating.Take(5).ToArray());
        }

        // pushed only, as a pull steps through every skipped element
        [Fact]
        public void SkipsAddUpPastIntMaxValue()
        {
            var sequence = Enumerable.InfiniteSequence(0L, 1L);

            Assert.Equal(int.MaxValue + 2L, sequence.Skip(int.MaxValue).Skip(2).First());
            Assert.Equal(int.MaxValue + 1L, sequence.Skip(int.MaxValue).Take(5).Skip(1).First());
            Assert.Equal(new[] { int.MaxValue + 1L, int.MaxValue + 2L }, sequence.Skip(int.MaxValue).Skip(1).Take(2).ToArray());
            Assert.Equal(2L * int.MaxValue, sequence.Chunk(2).Skip(int.MaxValue).First()[0]);
        }

        private sealed class Tally
        {
            public int Additions;
        }

        private sealed class Counted(int value, Tally tally)
            : IAdditionOperators<Counted, Counted, Counted>
            , IEquatable<Counted>
        {
            public int Value { get; } = value;

            public Tally Tally { get; } = tally;

            public static Counted operator +(Counted left, Counted right)
            {
                ++left.Tally.Additions;

                var sum = new Counted(left.Value + right.Value, left.Tally);

                return sum;
            }

            public bool Equals(Counted? other)
            {
                var equal =
                    other is not null
                    && other.Value == Value;

                return equal;
            }

            public override bool Equals(object? obj) => Equals(obj as Counted);

            public override int GetHashCode() => Value;
        }
    }
}
