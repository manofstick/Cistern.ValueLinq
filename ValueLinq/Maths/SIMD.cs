using System;
using System.Numerics;
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
        /// Ignore any potential issues such as overflow
        /// </summary>
        Fastest
    };

    static class SIMD
    {
        public static BatchProcessResult Min<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        {
            Math math = default;

            var localResult = result;

            noData &= source.Length == 0;

            var idx = 0;

            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && ((source.Length - idx) / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(source);
                var mins = new Vector<T>(localResult);
                if (math.HasNaNs)
                {
                    var nan = new Vector<T>(math.NaN);
                    foreach (var v in asVector)
                    {
                        if (Vector.EqualsAny(Vector.Xor(v, nan), Vector<T>.Zero))
                        {
                            result = math.NaN;
                            return BatchProcessResult.SuccessAndHalt;
                        }
                        mins = Vector.Min(mins, v);
                    }
                }
                else
                {
                    foreach (var v in asVector)
                    {
                        mins = Vector.Min(mins, v);
                    }
                }

                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = mins[i];
                    if (math.LessThan(input, localResult))
                        localResult = input;
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (math.LessThan(input, localResult))
                    localResult = input;
                else if (math.IsNaN(input))
                {
                    localResult = input;
                    break;
                }
            }

            result = localResult;

            return BatchProcessResult.SuccessAndContinue;
        }


        internal static BatchProcessResult Max<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, ref T result, ref bool noData)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        {
            Math math = default;

            var localResult = result;

            noData &= source.Length == 0;
            var idx = 0;
            for (; math.IsNaN(localResult) && idx < source.Length; ++idx)
            {
                localResult = source[idx];
            }

            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && ((source.Length - idx) / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var remainder = source.Slice(idx);
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(remainder);
                var maxes = new Vector<T>(localResult);
                foreach (var v in asVector)
                {
                    maxes = Vector.Max(maxes, v);
                }
                for (var i = 0; i < Vector<T>.Count; ++i)
                {
                    var input = maxes[i];
                    if (math.GreaterThan(input, localResult))
                        localResult = input;
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                if (math.GreaterThan(input, localResult))
                    localResult = input;
            }

            result = localResult;

            return BatchProcessResult.SuccessAndContinue;
        }


        internal static BatchProcessResult Sum<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, SIMDOptions simdOptions, ref Accumulator result)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        {
            return simdOptions switch
            {
                SIMDOptions.OnlyIfSame => SumNoSimd<T, Accumulator, Quotient, Math>(source, ref result),
                SIMDOptions.WithOverflowHandling => SumSimdChecked<T, Accumulator, Quotient, Math>(source, ref result),
                SIMDOptions.Fastest => SumSimdUnchecked<T, Accumulator, Quotient, Math>(source, ref result),
                _ => throw new InvalidOperationException()
            };
        }

        internal static BatchProcessResult SumNoSimd<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, ref Accumulator result)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        {
            Math math = default;

            var sum = result;
            foreach (var x in source)
                sum = math.Add(sum, x);
            result = sum;

            return BatchProcessResult.SuccessAndContinue;
        }

        internal static BatchProcessResult SumSimdChecked<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, ref Accumulator result)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        { 
            Math math = default;

            var localResult = result;

            var idx = 0;
            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && (source.Length / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var remainder = source.Slice(idx);
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(remainder);
                var summation = new Vector<Accumulator>(localResult);
                foreach (var v in asVector)
                {
                    summation = math.Add(summation, v);
                }
                for (var i = 0; i < Vector<Accumulator>.Count; ++i)
                {
                    var input = summation[i];
                    localResult = math.Add(input, localResult);
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                localResult = math.Add(localResult, input);
            }

            result = localResult;

            return BatchProcessResult.SuccessAndContinue;
        }

        internal static BatchProcessResult SumSimdUnchecked<T, Accumulator, Quotient, Math>(ReadOnlySpan<T> source, ref Accumulator result)
            where T : struct
            where Accumulator : struct
            where Quotient : struct
            where Math : struct, IMathsOperations<T, Accumulator, Quotient>
        {
            Math math = default;

            var localResult = result;

            var idx = 0;
            const int NumberOfVectorsToMakeThisWorthwhile = 5; // from some random testing
            if (Vector.IsHardwareAccelerated && math.SupportsVectorization && (source.Length / Vector<T>.Count > NumberOfVectorsToMakeThisWorthwhile))
            {
                var remainder = source.Slice(idx);
                var asVector = MemoryMarshal.Cast<T, Vector<T>>(remainder);
                var summation = new Vector<Accumulator>(localResult);
                foreach (var v in asVector)
                {
                    summation = math.AddUnchecked(summation, v);
                }
                for (var i = 0; i < Vector<Accumulator>.Count; ++i)
                {
                    var input = summation[i];
                    localResult = math.AddUnchecked(input, localResult);
                }

                idx += asVector.Length * Vector<T>.Count;
            }

            for (; idx < source.Length; ++idx)
            {
                var input = source[idx];
                localResult = math.AddUnchecked(localResult, input);
            }

            result = localResult;

            return BatchProcessResult.SuccessAndContinue;
        }

    }
}
