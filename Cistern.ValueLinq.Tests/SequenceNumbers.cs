using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class SequenceNumbers
    {
        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static T[] Arithmetic<T>(T start, T endInclusive, T step)
            where T : INumber<T>
        {
            var first = BigInteger.CreateTruncating(start);
            var last = BigInteger.CreateTruncating(endInclusive);
            var stride = BigInteger.CreateTruncating(step);

            var items = new List<T>();
            var value = first;
            while (isWithin(value))
            {
                items.Add(T.CreateTruncating(value));
                value += stride;
            }

            return items.ToArray();

            bool isWithin(BigInteger candidate)
            {
                var within =
                    stride.Sign switch
                    {
                        0 => items.Count == 0,
                        > 0 => candidate <= last,
                        < 0 => candidate >= last,
                    };

                return within;
            }
        }

        private static T[] Added<T>(T start, T endInclusive, T step)
            where T : INumber<T>
        {
            var items = new List<T>();
            for (var value = start; value <= endInclusive; value += step)
                items.Add(value);

            return items.ToArray();
        }

        private static T[] Reversed<T>(T[] values)
        {
            var reversed = (T[])values.Clone();
            Array.Reverse(reversed);

            return reversed;
        }

        private static void AssertIntegers<T>(T start, T endInclusive, T step)
            where T : INumber<T>
        {
            var expected = Arithmetic(start, endInclusive, step);

            AssertSequence(start, endInclusive, step, expected);
        }

        private static void AssertSequence<T>(T start, T endInclusive, T step, T[] expected)
            where T : INumber<T>
        {
            var sequence = Enumerable.Sequence(start, endInclusive, step);
            IEnumerable<T> enumerable = sequence;

            Assert.Equal(expected, Pull(sequence));
            Assert.Equal(expected, Pull(enumerable));
            Assert.Equal(expected, sequence.ToArray());
            Assert.Equal(expected, sequence.ToList());
            Assert.Equal(expected, enumerable.ToArray());
            Assert.Equal(expected, sequence.Select(x => x).ToList());
            Assert.Equal(expected, sequence.Where(x => true).ToArray());
            Assert.Equal(expected.Length, sequence.Count());
            Assert.Equal(expected.Length, sequence.LongCount());
            Assert.Equal(expected.Length, enumerable.Count());
            Assert.Equal(expected[0], sequence.First());
            Assert.Equal(expected[^1], sequence.Last());
            Assert.Equal(Reversed(expected), sequence.Reverse().ToArray());
            Assert.Equal(Reversed(expected), Pull(sequence.Reverse()));
            Assert.Equal(Reversed(expected), enumerable.Reverse().ToList());
            Assert.Equal(expected, sequence.Reverse().Reverse().ToArray());
            Assert.Equal(default, sequence.ElementAtOrDefault(expected.Length));
            Assert.Equal(default, sequence.ElementAtOrDefault(^(expected.Length + 1)));

            for (var i = 0; i < expected.Length; i += Math.Max(1, expected.Length / 7))
            {
                Assert.Equal(expected[i], sequence.ElementAt(i));
                Assert.Equal(expected[^(i + 1)], sequence.ElementAt(^(i + 1)));
                Assert.Equal(expected[i], enumerable.ElementAt(i));

                foreach (var probe in new[] { expected[i], expected[i] + T.One, expected[i] - T.One })
                {
                    var held = Array.IndexOf(expected, probe) >= 0;
                    Assert.Equal(held, sequence.Contains(probe));
                    Assert.Equal(held, enumerable.Contains(probe));
                }
            }

            foreach (var probe in new[] { endInclusive, endInclusive + T.One, endInclusive - T.One, start - T.One, start + T.One })
                Assert.Equal(Array.IndexOf(expected, probe) >= 0, sequence.Contains(probe));

            foreach (var count in new[] { 0, 1, 2, expected.Length / 2, expected.Length - 1, expected.Length, expected.Length + 1 })
            {
                var bounded = Math.Clamp(count, 0, expected.Length);
                var skipped = expected[bounded..];
                var taken = expected[..bounded];
                var afterOne = skipped[Math.Min(1, skipped.Length)..];
                var skippedThenTaken = afterOne[..Math.Min(2, afterOne.Length)];

                Assert.Equal(skipped, sequence.Skip(count).ToArray());
                Assert.Equal(skipped, Pull(sequence.Skip(count)));
                Assert.Equal(skipped.Length, sequence.Skip(count).Count());
                Assert.Equal(taken, sequence.Take(count).ToArray());
                Assert.Equal(taken, Pull(sequence.Take(count)));
                Assert.Equal(skippedThenTaken, sequence.Skip(count).Skip(1).Take(2).ToArray());
                Assert.Equal(skippedThenTaken, enumerable.Skip(count).Skip(1).Take(2).ToList());
                Assert.Equal(Reversed(skipped), sequence.Skip(count).Reverse().ToArray());
                Assert.Equal(Reversed(expected)[bounded..], sequence.Reverse().Skip(count).ToArray());

                if (skipped is [var firstSkipped, .., var lastSkipped])
                {
                    Assert.Equal(firstSkipped, sequence.Skip(count).ElementAt(0));
                    Assert.Equal(lastSkipped, sequence.Skip(count).ElementAt(^1));
                    Assert.Equal(lastSkipped, sequence.Skip(count).Last());
                }
            }
        }

        [Fact]
        public void EveryIntegerTypeYieldsTheArithmeticSequenceToItsLimits()
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

                AssertIntegers(T.Zero, T.Zero, T.Zero);
                AssertIntegers(T.Zero, T.Zero, three);
                AssertIntegers(T.Zero, ten, T.One);
                AssertIntegers(T.Zero, ten, three);
                AssertIntegers(T.Zero, ten, ten);
                AssertIntegers(T.Zero, ten, T.MaxValue);
                AssertIntegers(T.MaxValue - ten, T.MaxValue, T.One);
                AssertIntegers(T.MaxValue - ten, T.MaxValue, three);
                AssertIntegers(T.MaxValue - ten, T.MaxValue - T.One, three);
                AssertIntegers(T.MaxValue, T.MaxValue, T.MaxValue);
                AssertIntegers(T.MinValue, T.MinValue + ten, three);
                AssertIntegers(T.MinValue, T.MaxValue, T.MaxValue);
                AssertIntegers(T.MinValue, T.MaxValue, largeStep);

                if (T.IsNegative(T.MinValue))
                {
                    var minusOne = T.Zero - T.One;
                    var minusThree = T.Zero - three;

                    AssertIntegers(ten, T.Zero - ten, minusOne);
                    AssertIntegers(ten, T.Zero - ten, minusThree);
                    AssertIntegers(T.MinValue + ten, T.MinValue, minusThree);
                    AssertIntegers(T.MinValue, T.MinValue, minusThree);
                    AssertIntegers(T.MaxValue, T.MinValue, T.MinValue);
                    AssertIntegers(T.MaxValue, T.MinValue, T.Zero - largeStep);
                }
            }
        }

        [Fact]
        public void BigIntegerYieldsTheArithmeticSequence()
        {
            var huge = BigInteger.Pow(10, 40);

            AssertIntegers(BigInteger.Zero, new BigInteger(10), new BigInteger(3));
            AssertIntegers(huge, huge + 10, new BigInteger(4));
            AssertIntegers(huge, -huge, -huge / 3);
            AssertIntegers(huge, huge, BigInteger.Zero);
        }

        [Fact]
        public void ASteppedSequenceAcrossTheWholeRangeIsYieldedEitherWay()
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

            static void validate<T>()
                where T : INumber<T>, IMinMaxValue<T>
            {
                var step = T.MaxValue / T.CreateTruncating(50);

                AssertIntegers(T.MinValue, T.MaxValue, step);

                if (T.IsNegative(T.MinValue))
                    AssertIntegers(T.MaxValue, T.MinValue, T.Zero - step);
            }
        }

        [Fact]
        public void TheFullRangeOfASmallTypeIsYieldedEitherWay()
        {
            AssertIntegers(byte.MinValue, byte.MaxValue, (byte)1);
            AssertIntegers(sbyte.MinValue, sbyte.MaxValue, (sbyte)1);
            AssertIntegers(sbyte.MaxValue, sbyte.MinValue, (sbyte)-1);
            AssertIntegers(short.MinValue, short.MaxValue, (short)1);
            AssertIntegers(short.MaxValue, short.MinValue, (short)-1);
            AssertIntegers(ushort.MinValue, ushort.MaxValue, (ushort)1);
            AssertIntegers(char.MinValue, char.MaxValue, (char)7);
        }

        [Fact]
        public void AFloatingPointSequenceAddsTheStepToTheLastElement()
        {
            var largest = float.MaxValue;
            var half = largest / 2;
            var quarter = largest / 4;

            AssertSequence(0.0, 1.0, 0.1, Added(0.0, 1.0, 0.1));
            AssertSequence(half, largest, quarter, Added(half, largest, quarter));
            AssertSequence(0.5, -0.5, -0.25, [0.5, 0.25, 0.0, -0.25, -0.5]);
            AssertSequence(0.1m, 1.0m, 0.1m, [0.1m, 0.2m, 0.3m, 0.4m, 0.5m, 0.6m, 0.7m, 0.8m, 0.9m, 1.0m]);
            AssertSequence(1.0f, 1.0f, 0.0f, [1.0f]);
            AssertSequence(16_777_216f, largest, 1.0f, [16_777_216f]);
            AssertSequence(-16_777_217f, float.MinValue, -1.0f, [-16_777_216f]);
            AssertSequence(half, largest, largest, [half]);
            AssertSequence(largest, largest, largest, [largest]);
            AssertSequence(double.NegativeInfinity, double.PositiveInfinity, 1.0, [double.NegativeInfinity]);
            AssertSequence((Half)1, (Half)2, (Half)0.25, [(Half)1, (Half)1.25, (Half)1.5, (Half)1.75, (Half)2]);
        }

        [Fact]
        public void ADecimalOverflowThrowsAsItsAdditionDoes()
        {
            Assert.Equal([decimal.MaxValue - 1, decimal.MaxValue], Enumerable.Sequence(decimal.MaxValue - 1, decimal.MaxValue, 1m).ToArray());
            Assert.Throws<OverflowException>(() => Enumerable.Sequence(decimal.MaxValue - 1, decimal.MaxValue, 2m).ToArray());
            Assert.Throws<OverflowException>(() => Pull(Enumerable.Sequence(decimal.MaxValue - 1, decimal.MaxValue, 2m)));
        }

        [Fact]
        public void ALengthBeyondAnIntIsWalked()
        {
            var longs = Enumerable.Sequence(0L, long.MaxValue, 1L);
            var ulongs = Enumerable.Sequence(0UL, ulong.MaxValue, 1UL);

            Assert.Equal([0L, 1L, 2L], longs.Take(3).ToArray());
            Assert.Equal([0L, 1L, 2L], Pull(longs.Take(3)));
            Assert.Equal([5L, 6L], longs.Skip(5).Take(2).ToArray());
            Assert.Equal(7L, longs.ElementAt(7));
            Assert.Equal(0L, longs.First());
            Assert.Equal([0UL, 1UL], ulongs.Take(2).ToList());
            Assert.False(longs.TryGetNonEnumeratedCount(out _));
            Assert.False(ulongs.TryGetNonEnumeratedCount(out _));
        }

        [Fact]
        public void TheCountIsKnownWithoutRunningForAnIntegerSequence()
        {
            Assert.True(Enumerable.Sequence(0, 100, 7).TryGetNonEnumeratedCount(out var stepped));
            Assert.Equal(15, stepped);

            Assert.True(Enumerable.Sequence(byte.MinValue, byte.MaxValue, (byte)1).TryGetNonEnumeratedCount(out var bytes));
            Assert.Equal(256, bytes);

            Assert.True(Enumerable.Sequence(long.MaxValue, long.MinValue, long.MinValue).TryGetNonEnumeratedCount(out var wide));
            Assert.Equal(2, wide);

            Assert.True(Enumerable.Sequence(0.5, 0.5, 0.0).TryGetNonEnumeratedCount(out var single));
            Assert.Equal(1, single);

            Assert.False(Enumerable.Sequence(0.0, 1.0, 0.1).TryGetNonEnumeratedCount(out _));
            Assert.False(Enumerable.Sequence(Int128.Zero, Int128.One, Int128.One).TryGetNonEnumeratedCount(out _));
        }

        [Fact]
        public void AnInvalidArgumentThrowsWhenTheSequenceIsMade()
        {
            Assert.Throws<ArgumentOutOfRangeException>("start", () => Enumerable.Sequence(double.NaN, double.NaN, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>("endInclusive", () => Enumerable.Sequence(1.0, double.NaN, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>("step", () => Enumerable.Sequence(1.0, 2.0, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>("step", () => Enumerable.Sequence(1, 2, 0));
            Assert.Throws<ArgumentOutOfRangeException>("step", () => Enumerable.Sequence(1.0f, 1.5f, -0.0f));
            Assert.Throws<ArgumentOutOfRangeException>("endInclusive", () => Enumerable.Sequence(2, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>("endInclusive", () => Enumerable.Sequence(1, 2, -1));
            Assert.Throws<ArgumentOutOfRangeException>("endInclusive", () => Enumerable.Sequence(1u, 0u, 1u));
            Assert.Throws<ArgumentOutOfRangeException>("endInclusive", () => Enumerable.Sequence(1.0, 2.0, -0.5));
        }

        [Fact]
        public void PushAndPullStopWhereTheConsumerStops()
        {
            var lengthCalls = 0;
            var walkCalls = 0;

            Assert.Equal(3, Enumerable.Sequence(0, 100, 1).Select(countLength).First(x => x == 3));
            Assert.Equal(1.5, Enumerable.Sequence(0.0, 100.0, 0.5).Select(countWalk).First(x => x == 1.5));
            Assert.Equal(4, lengthCalls);
            Assert.Equal(4, walkCalls);

            IEnumerable<double>[] sequences =
            [
                Enumerable.Sequence(0.0, 1.0, 0.5),
                Enumerable.Sequence(0.0, 1.0, 0.3),
                Enumerable.Sequence(0.0, 0.0, 0.0),
                Enumerable.Sequence(0.0, 3.0, 1.0).Select(x => (double)(int)x),
            ];

            foreach (var sequence in sequences)
            {
                using var enumerator = sequence.GetEnumerator();
                var pulled = 0;
                while (enumerator.MoveNext())
                    ++pulled;

                Assert.Equal(Pull(sequence).Count, pulled);
                Assert.False(enumerator.MoveNext());
                Assert.False(enumerator.MoveNext());
            }

            int countLength(int x)
            {
                ++lengthCalls;

                return x;
            }

            double countWalk(double x)
            {
                ++walkCalls;

                return x;
            }
        }

        [Fact]
        public void EachEnumerationStartsAgain()
        {
            var lengths = Enumerable.Sequence(1, 9, 2);
            var walk = Enumerable.Sequence(1.0, 2.0, 0.5);

            Assert.Equal(Pull(lengths), Pull(lengths));
            Assert.Equal(lengths.ToArray(), lengths.ToArray());
            Assert.Equal(Pull(walk), Pull(walk));
            Assert.Equal(walk.ToArray(), walk.ToArray());
        }
    }
}
