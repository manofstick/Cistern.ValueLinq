using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Maths
{
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

    }
}
