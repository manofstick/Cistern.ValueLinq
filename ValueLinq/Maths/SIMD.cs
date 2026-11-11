using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Maths
{
    public enum SIMDOptions
    {
        /// <summary>
        /// SIMD instruction will only be used if the result of the operation is exactly the same as the usage
        /// without. i.e. Min and Max are unaffected by the usage of SIMD, but summing a set of floating point
        /// numbers can give a (slightly) different result due to the different ordering in which the numbers
        /// are added due to intermediate rounding
        /// </summary>
        OnlyIfSame,
        /// <summary>
        /// For integer operations, System.Linq uses checked operations for adding, which are not supported
        /// directly with the SIMD instruction set, so additional code is used to handle overflow. Note that
        /// due to the different ordering, and hence differnt interim states, it's possible for either System.Linq
        /// or this version to throw an exception even if the final result would of been within bounds.
        /// (i.e. think "int.Max + int.Max - int.Max" would throw, even though the correct answer is bounded
        /// within he int range). Floating point results are not guarenteed to be same due to rounding.
        /// </summary>
        WithOverflowHandling,
        /// <summary>
        /// Ignore any potential issues such as overflow or floating point rounding errors.
        /// </summary>
        Fastest
    };

    // Sum and Average take System.Linq's loops (dotnet/runtime, src/libraries/System.Linq/src/System/Linq/Sum.cs at
    // commit 83f1a5d0678f616f35f5c97de3a6f63d88d9ae74 and Average.cs at commit 3d39be4fb0ea65746959ed2298414d0ee63a8e7b)
    // and their thresholds; Min and Max over an integer span hand it to MemoryExtensions, as System.Linq's span forms do,
    // where the framework has it
    static class SIMD
    {
        private static void ThrowOverflow() => throw new OverflowException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool UseVectors<T>(int length)
            where T : struct
            => Vector.IsHardwareAccelerated && Vector<T>.IsSupported && length >= Vector<T>.Count;

        /// <summary>
        /// Vector widening has no generic form, so the pairs the library accumulates across are named here.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Widen<T, TAccumulator>(Vector<T> source, out Vector<TAccumulator> lower, out Vector<TAccumulator> upper)
            where T : struct
            where TAccumulator : struct
        {
            if (typeof(T) == typeof(int) && typeof(TAccumulator) == typeof(long))
            {
                Vector.Widen(source.As<T, int>(), out var lo, out var hi);
                lower = lo.As<long, TAccumulator>();
                upper = hi.As<long, TAccumulator>();
            }
            else if (typeof(T) == typeof(float) && typeof(TAccumulator) == typeof(double))
            {
                Vector.Widen(source.As<T, float>(), out var lo, out var hi);
                lower = lo.As<double, TAccumulator>();
                upper = hi.As<double, TAccumulator>();
            }
            else
            {
                ThrowHelper.ThrowNotSupportedException();
                lower = upper = default;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<TAccumulator> AddUnchecked<T, TAccumulator>(Vector<TAccumulator> sum, Vector<T> values)
            where T : struct
            where TAccumulator : struct
        {
            if (typeof(T) == typeof(TAccumulator))
                return Vector.Add(sum, values.As<T, TAccumulator>());

            Widen(values, out Vector<TAccumulator> lower, out var upper);
            return Vector.Add(Vector.Add(sum, lower), upper);
        }

        /// <summary>
        /// Hacker's Delight p26: a signed addition overflowed when both operands carry the sign the result lacks,
        /// so the sign bit of the returned lanes is set where it did.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector<TAccumulator> OverflowBits<TAccumulator>(Vector<TAccumulator> lhs, Vector<TAccumulator> rhs, Vector<TAccumulator> result)
            where TAccumulator : struct
            => Vector.BitwiseAnd(Vector.Xor(result, lhs), Vector.Xor(result, rhs));

        // an integer span goes to MemoryExtensions, as System.Linq's does; the rest keep the vector loop, as MemoryExtensions
        // compares float and double one at a time
        public static bool Min<T>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct, INumber<T>
        {
            if (Number<T>.IsIntegral)
            {
                if (source.IsEmpty)
                    return true;

                var least = MemoryExtensions.Min(source);
                if (noData || least < result)
                    result = least;

                noData = false;

                return true;
            }

            return MinVector(source, ref result, ref noData);
        }

        internal static bool Max<T>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct, INumber<T>
        {
            if (Number<T>.IsIntegral)
            {
                if (source.IsEmpty)
                    return true;

                var greatest = MemoryExtensions.Max(source);
                if (noData || greatest > result)
                    result = greatest;

                noData = false;

                return true;
            }

            return MaxVector(source, ref result, ref noData);
        }

        // a NaN lane is the one value that does not equal itself; callers test IsIntegral themselves, so an integer loop
        // drops the call where the JIT declines to inline it
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsNaNIn<T>(Vector<T> v, ref T result)
            where T : struct, INumber<T>
        {
            if (Vector.EqualsAll(v, v))
                return false;

            for (var i = 0; i < Vector<T>.Count; ++i)
            {
                if (T.IsNaN(v[i]))
                {
                    result = v[i];
                    break;
                }
            }
            return true;
        }

        private static bool MinVector<T>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct, INumber<T>
        {
            if (source.Length == 0)
                return true;

            var idx = 0;
            var localResult = result;
            if (noData)
            {
                noData = false;
                localResult = source[0];
                idx = 1;
            }

            // NaN is the minimum of any sequence holding one
            if (T.IsNaN(localResult))
            {
                result = localResult;
                return false;
            }

            // read from index 0 rather than past the seed, so the loads keep the span's alignment, and finished with one
            // vector overlapping the last, both safe as the minimum of an element already seen is unchanged
            if (UseVectors<T>(source.Length))
            {
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(source);
                var mins = new Vector<T>(localResult);
                foreach (var v in asVector)
                {
                    if (!Number<T>.IsIntegral && IsNaNIn(v, ref result))
                        return false;
                    mins = Vector.Min(mins, v);
                }

                if (source.Length % Vector<T>.Count != 0)
                {
                    var last = new Vector<T>(source[^Vector<T>.Count..]);
                    if (!Number<T>.IsIntegral && IsNaNIn(last, ref result))
                        return false;
                    mins = Vector.Min(mins, last);
                }

                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = mins[i];
                    if (input < localResult)
                        localResult = input;
                }

                idx = source.Length;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (input < localResult)
                    localResult = input;
                else if (T.IsNaN(input))
                {
                    result = input;
                    return false;
                }
            }

            result = localResult;

            return true;
        }

        private static bool MaxVector<T>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct, INumber<T>
        {
            if (source.Length == 0)
                return true;

            var idx = 0;
            var localResult = result;
            if (noData)
            {
                noData = false;
                localResult = source[0];
                idx = 1;
            }

            // NaN sorts below every number, so it stands as the maximum only until a number arrives
            for (; T.IsNaN(localResult) && idx < source.Length; ++idx)
                localResult = source[idx];

            // from index 0 and with an overlapping last vector, as Min; MaxNumber passes over the leading NaNs skipped above
            if (UseVectors<T>(source.Length))
            {
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(source);
                var maxes = new Vector<T>(localResult);
                foreach (var v in asVector)
                    maxes = Vector.MaxNumber(maxes, v); // MaxNumber keeps the lane that is a number when the other is NaN

                if (source.Length % Vector<T>.Count != 0)
                    maxes = Vector.MaxNumber(maxes, new Vector<T>(source[^Vector<T>.Count..]));

                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = maxes[i];
                    if (input > localResult)
                        localResult = input;
                }

                idx = source.Length;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (input > localResult)
                    localResult = input;
            }

            result = localResult;

            return true;
        }

        internal static bool Average<T, TAccumulator>(ReadOnlySpan<T> source, SIMDOptions simdOptions, ref TAccumulator result, ref long counter)
            where T : struct, INumber<T>
            where TAccumulator : struct, INumber<TAccumulator>
        {
            counter += source.Length;

            return simdOptions switch
            {
                SIMDOptions.OnlyIfSame when Number<TAccumulator>.IsIntegral => SumChecked(source, ref result),
                SIMDOptions.OnlyIfSame => SumNoSimd(source, ref result),
                SIMDOptions.WithOverflowHandling => SumChecked(source, ref result),
                SIMDOptions.Fastest => SumSimdUnchecked(source, ref result),

                _ => throw new InvalidOperationException()
            };
        }

        internal static bool Sum<T, TAccumulator>(ReadOnlySpan<T> source, SIMDOptions simdOptions, ref TAccumulator result)
            where T : struct, INumber<T>
            where TAccumulator : struct, INumber<TAccumulator>
        {
            return simdOptions switch
            {
                SIMDOptions.OnlyIfSame when Number<TAccumulator>.IsIntegral => SumChecked(source, ref result),
                SIMDOptions.OnlyIfSame => SumNoSimd(source, ref result),
                SIMDOptions.WithOverflowHandling => SumChecked(source, ref result),
                SIMDOptions.Fastest => SumSimdUnchecked(source, ref result),

                _ => throw new InvalidOperationException()
            };
        }

        internal static bool SumNoSimd<T, TAccumulator>(ReadOnlySpan<T> source, ref TAccumulator result)
            where T : struct, INumber<T>
            where TAccumulator : struct, INumber<TAccumulator>
        {
            var sum = result;
            foreach (var x in source)
                sum = checked(sum + TAccumulator.CreateChecked(x));
            result = sum;

            return true;
        }

        // an integer sum vectorises only where System.Linq's does: a signed sum of its own type, and an int sum into long;
        // floating point has nothing to overflow, so takes the unchecked loop
        internal static bool SumChecked<T, TAccumulator>(ReadOnlySpan<T> source, ref TAccumulator result)
            where T : struct, INumber<T>
            where TAccumulator : struct, INumber<TAccumulator>
        {
            if (!Number<TAccumulator>.IsIntegral)
                return SumSimdUnchecked(source, ref result);

            if (typeof(T) == typeof(TAccumulator) && Number<TAccumulator>.IsSigned)
                return SumSignedIntegers(MemoryMarshal.Cast<T, TAccumulator>(source), ref result);

            if (typeof(T) == typeof(int) && typeof(TAccumulator) == typeof(long))
                return SumWidened(MemoryMarshal.Cast<T, int>(source), ref result);

            return SumNoSimd(source, ref result);
        }

        // System.Linq's SumSignedIntegersVectorized: four vectors a step, the overflow of every lane gathered and tested
        // once after the loop, from four vectors' length and where a vector holds more than two lanes
        private static bool SumSignedIntegers<T>(ReadOnlySpan<T> span, ref T result)
            where T : struct, INumber<T>
        {
            if (!Vector.IsHardwareAccelerated
                || !Vector<T>.IsSupported
                || Vector<T>.Count <= 2
                || span.Length < Vector<T>.Count * 4)
            {
                return SumNoSimd(span, ref result);
            }

            var accumulator = Vector<T>.Zero;
            var overflowTracking = Vector<T>.Zero;

            // the accumulators alternate, so no step needs a move
            while (span.Length >= Vector<T>.Count * 4)
            {
                var data = new Vector<T>(span);
                var accumulator2 = accumulator + data;
                overflowTracking |= OverflowBits(accumulator, data, accumulator2);

                data = new Vector<T>(span.Slice(Vector<T>.Count));
                accumulator = accumulator2 + data;
                overflowTracking |= OverflowBits(accumulator2, data, accumulator);

                data = new Vector<T>(span.Slice(Vector<T>.Count * 2));
                accumulator2 = accumulator + data;
                overflowTracking |= OverflowBits(accumulator, data, accumulator2);

                data = new Vector<T>(span.Slice(Vector<T>.Count * 3));
                accumulator = accumulator2 + data;
                overflowTracking |= OverflowBits(accumulator2, data, accumulator);

                span = span.Slice(Vector<T>.Count * 4);
            }

            while (span.Length >= Vector<T>.Count)
            {
                var data = new Vector<T>(span);
                var accumulator2 = accumulator + data;
                overflowTracking |= OverflowBits(accumulator, data, accumulator2);
                accumulator = accumulator2;

                span = span.Slice(Vector<T>.Count);
            }

            if (Vector.LessThanAny(overflowTracking, Vector<T>.Zero))
                ThrowOverflow();

            var localResult = result;
            for (var lane = 0; lane < Vector<T>.Count; ++lane)
                localResult = checked(localResult + accumulator[lane]);

            foreach (var value in span)
                localResult = checked(localResult + value);

            result = localResult;

            return true;
        }

        // System.Linq's Average over int: widened, a lane sums at most 2^31 values below 2^31, so cannot overflow a long;
        // the fold into the running total stays checked
        private static bool SumWidened<TAccumulator>(ReadOnlySpan<int> span, ref TAccumulator result)
            where TAccumulator : struct, INumber<TAccumulator>
        {
            var sum = 0L;
            var i = 0;

            if (Vector.IsHardwareAccelerated && span.Length >= Vector<int>.Count)
            {
                var vectors = MemoryMarshal.Cast<int, Vector<int>>(span);
                var sums = Vector<long>.Zero;
                foreach (var data in vectors)
                {
                    Vector.Widen(data, out var low, out var high);
                    sums += low;
                    sums += high;
                }

                sum += Vector.Sum(sums);
                i = vectors.Length * Vector<int>.Count;
            }

            for (; (uint)i < (uint)span.Length; ++i)
                sum += span[i];

            result = checked(result + TAccumulator.CreateChecked(sum));

            return true;
        }

        internal static bool SumSimdUnchecked<T, TAccumulator>(ReadOnlySpan<T> source, ref TAccumulator result)
            where T : struct, INumber<T>
            where TAccumulator : struct, INumber<TAccumulator>
        {
            var localResult = result;

            var idx = 0;
            if (UseVectors<T>(source.Length) && Vector<TAccumulator>.IsSupported)
            {
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(source);
                var summation = Vector<TAccumulator>.Zero; // the running total is folded in once with the lanes below
                foreach (var v in asVector)
                    summation = AddUnchecked(summation, v);

                for (var i = 0; i < Vector<TAccumulator>.Count; ++i)
                    localResult += summation[i];

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
                localResult += TAccumulator.CreateChecked(source[idx]);

            result = localResult;

            return true;
        }

        // the types Extremum runs, a constant per T
        internal static bool HasExtremum<T>() =>
            typeof(T) == typeof(int)
            || typeof(T) == typeof(long)
            || typeof(T) == typeof(uint)
            || typeof(T) == typeof(ulong)
            || typeof(T) == typeof(short)
            || typeof(T) == typeof(ushort)
            || typeof(T) == typeof(byte)
            || typeof(T) == typeof(sbyte);

        /// <summary>
        /// The least or greatest of an integer span under its default ordering, through Min or Max, for a T that
        /// HasExtremum. Callable over an unconstrained T: each integer's IntegerExtremum is an IIntegerExtremum&lt;T&gt;
        /// only where it is T, so the span passes as the type it already is.
        /// </summary>
        internal static bool Extremum<T>(ReadOnlySpan<T> source, bool least, ref T result, ref bool noData)
        {
            if (typeof(T) == typeof(int))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<int>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(long))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<long>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(uint))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<uint>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(ulong))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<ulong>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(short))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<short>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(ushort))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<ushort>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(byte))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<byte>)).Run(source, least, ref result, ref noData);
            if (typeof(T) == typeof(sbyte))
                return ((IIntegerExtremum<T>)(object)default(IntegerExtremum<sbyte>)).Run(source, least, ref result, ref noData);

            throw new InvalidOperationException();
        }
    }

    interface IIntegerExtremum<T>
    {
        bool Run(ReadOnlySpan<T> source, bool least, ref T result, ref bool noData);
    }

    readonly struct IntegerExtremum<T>
        : IIntegerExtremum<T>
        where T : struct, INumber<T>
    {
        public bool Run(ReadOnlySpan<T> source, bool least, ref T result, ref bool noData) =>
            least
                ? SIMD.Min(source, ref result, ref noData)
                : SIMD.Max(source, ref result, ref noData);
    }
}
