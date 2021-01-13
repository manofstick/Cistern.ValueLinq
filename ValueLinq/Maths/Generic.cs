using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cistern.ValueLinq.Maths
{
    static class MathOperationError
    {
        public static void Overflow() => throw new OverflowException();
    }

    interface IMathsOperations<T, Accumulator, Quotient>
        where T : struct
        where Accumulator : struct
        where Quotient : struct
    {
        bool SupportsVectorization { get; }
        bool SimdSumEqual { get; }
        Vector<Accumulator> Add(Vector<Accumulator> lhs, Vector<T> rhs);
        Vector<Accumulator> AddUnchecked(Vector<Accumulator> lhs, Vector<T> rhs);

        Accumulator Zero { get; }
        T One { get; }

        T MinValue { get; }
        T MaxValue { get; }

        bool HasNaNs { get; }
        T NaN { get; }

        Accumulator Add(Accumulator lhs, Accumulator rhs);
        Accumulator Add(Accumulator lhs, T rhs);
        Accumulator AddUnchecked(Accumulator lhs, Accumulator rhs);
        Accumulator AddUnchecked(Accumulator lhs, T rhs);
        Accumulator Add(Accumulator lhs, T? rhs);
        Accumulator AddInt(Accumulator lhs, int rhs);
        Quotient DivLong(Accumulator lhs, long rhs);

        T Cast(Accumulator a);
        T Cast(int a);

        Accumulator Cast(T a);

        bool IsNaN(T x);
        bool GreaterThan(T lhs, T rhs);
        bool LessThan(T lhs, T rhs);

        T MaxInit { get; }
        T MinInit { get; }
    }

    struct OpsDouble : IMathsOperations<double, double, double>
    {
        public bool SupportsVectorization => true;
        public bool SimdSumEqual => false;
        public bool HasNaNs => true;
        public double NaN => double.NaN;
        public double Zero => 0.0;
        public double One => 1.0;
        public double MinValue => double.MinValue;
        public double MaxValue => double.MaxValue;
        public double Add(double lhs, double rhs) => lhs + rhs;
        public double Add(double lhs, double? rhs) => lhs + rhs.GetValueOrDefault();
        public double AddInt(double lhs, int rhs) => lhs + rhs;
        public double DivLong(double lhs, long rhs) => lhs / rhs;
        public double Cast(double a) => a;
        public double Cast(int a) => a;
        public bool IsNaN(double x) => double.IsNaN(x);
        public bool GreaterThan(double lhs, double rhs) => lhs > rhs;
        public bool LessThan(double lhs, double rhs) => lhs < rhs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<double> Add(Vector<double> lhs, Vector<double> rhs) => Vector.Add(lhs, rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<double> AddUnchecked(Vector<double> lhs, Vector<double> rhs) => Vector.Add(lhs, rhs);
        public double AddUnchecked(double lhs, double rhs) => lhs + rhs;

        public double MaxInit => double.NaN;
        public double MinInit => double.PositiveInfinity;
    }

    struct OpsFloat : IMathsOperations<float, double, float>
    {
        public bool SupportsVectorization => true;
        public bool SimdSumEqual => false;
        public bool HasNaNs => true;
        public float NaN => float.NaN;
        public double Zero => 0.0;
        public float One => 1.0f;
        public float MinValue => float.MinValue;
        public float MaxValue => float.MaxValue;
        public double Add(double lhs, double rhs) => lhs + rhs;
        public double Add(double lhs, float rhs) => lhs + rhs;
        public double Add(double lhs, float? rhs) => lhs + rhs.GetValueOrDefault();
        public double AddInt(double lhs, int rhs) => lhs + rhs;
        public float DivLong(double lhs, long rhs) => (float)(lhs / rhs);
        public float Cast(double a) => (float)a;
        public float Cast(int a) => a;
        public double Cast(float a) => a;
        public bool IsNaN(float x) => float.IsNaN(x);
        public bool GreaterThan(float lhs, float rhs) => lhs > rhs;
        public bool LessThan(float lhs, float rhs) => lhs < rhs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<double> Add(Vector<double> lhs, Vector<float> rhs)
        {
            Vector.Widen(rhs, out var l, out var r);
            return Vector.Add(Vector.Add(lhs, l), r);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<double> AddUnchecked(Vector<double> lhs, Vector<float> rhs) => Add(lhs, rhs);
        public double AddUnchecked(double lhs, double rhs) => lhs + rhs;
        public double AddUnchecked(double lhs, float rhs) => lhs + rhs;

        public float MaxInit => float.NaN;
        public float MinInit => float.PositiveInfinity;
    }

    struct OpsInt : IMathsOperations<int, long, double>
    {
        public bool SupportsVectorization => true;
        public bool SimdSumEqual => true;

        public long Zero => 0;
        public int One => 1;

        public int MinValue => int.MinValue;
        public int MaxValue => int.MaxValue;
        public int MinInit => int.MaxValue;
        public int MaxInit => int.MinValue;

        public bool HasNaNs => false;
        public int NaN => default;
        public bool IsNaN(int x) => false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<long> Add(Vector<long> lhs, Vector<int> rhs)
        {
            // Adapted from Hacker's Delight p26, where both inputs need to be of the same sign, and that sign is different from result
            // (i.e. xors for diff, and for both, less and zero for checking sign bit set)
            Vector.Widen(rhs, out var rhs1, out var rhs2);

            var partialResult = Vector.Add(lhs, rhs1);
            var partialResultCheck = Vector.BitwiseAnd(Vector.Xor(partialResult, lhs), Vector.Xor(partialResult, rhs1));

            var finalResult = Vector.Add(partialResult, rhs2);
            var finalResultCheck = Vector.BitwiseAnd(Vector.Xor(finalResult, partialResult), Vector.Xor(finalResult, rhs2));

            if (Vector.LessThanAny(Vector.BitwiseOr(partialResultCheck, finalResultCheck), Vector<long>.Zero))
                MathOperationError.Overflow();

            return finalResult;
        }

        public long Add(long lhs, long rhs) { checked { return lhs + rhs; } }
        public long Add(long lhs, int rhs) { checked { return lhs + rhs; } }
        public long Add(long lhs, int? rhs) { checked { return lhs + rhs.GetValueOrDefault(); } }
        public long AddInt(long lhs, int rhs) { checked { return lhs + rhs; } }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<long> AddUnchecked(Vector<long> lhs, Vector<int> rhs)
        {
            Vector.Widen(rhs, out var rhs1, out var rhs2);
            return Vector.Add(Vector.Add(lhs, rhs1), rhs2);
        }

        public long AddUnchecked(long lhs, long rhs) => lhs + rhs;
        public long AddUnchecked(long lhs, int rhs) => lhs + rhs;

        public int Cast(long a)
        {
            if (a < int.MinValue || a > int.MaxValue)
                MathOperationError.Overflow();
            return (int)a;
        }
        public int Cast(int a) => a;
        long IMathsOperations<int, long, double>.Cast(int a) => (long)a;

        public double DivLong(long lhs, long rhs) => (double)lhs / rhs;
        public bool GreaterThan(int lhs, int rhs) => lhs > rhs;
        public bool LessThan(int lhs, int rhs) => lhs < rhs;
    }

    struct OpsLong : IMathsOperations<long, long, double>
    {
        public bool SupportsVectorization => true;
        public bool SimdSumEqual => true;
        public bool HasNaNs => false;
        public long NaN => default;
        public long Zero => 0;
        public long One => 1;
        public long MinValue => long.MinValue;
        public long MaxValue => long.MaxValue;
        public long Add(long lhs, long rhs) { checked { return lhs + rhs; } }
        public long Add(long lhs, long? rhs) { checked { return lhs + rhs.GetValueOrDefault(); } }
        public long AddInt(long lhs, int rhs) { checked { return lhs + rhs; } }
        public double DivLong(long lhs, long rhs) => (double)lhs / rhs;
        public long Cast(long a) => a;
        public long Cast(int a) => a;
        public bool IsNaN(long x) => false;
        public bool GreaterThan(long lhs, long rhs) => lhs > rhs;
        public bool LessThan(long lhs, long rhs) => lhs < rhs;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<long> Add(Vector<long> lhs, Vector<long> rhs)
        {
            var result = Vector.Add(lhs, rhs);
            if (Vector.LessThanAny(Vector.BitwiseAnd(Vector.Xor(result, lhs), Vector.Xor(result, rhs)), Vector<long>.Zero))
                throw new OverflowException();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector<long> AddUnchecked(Vector<long> lhs, Vector<long> rhs) => Vector.Add(lhs, rhs);
        public long AddUnchecked(long lhs, long rhs) => lhs + rhs;

        public long MaxInit => long.MinValue;
        public long MinInit => long.MaxValue;
    }


    struct OpsDecimal : IMathsOperations<decimal, decimal, decimal>
    {
        public bool SupportsVectorization => false;
        public bool SimdSumEqual => false;
        public bool HasNaNs => false;
        public decimal NaN => default;
        public decimal Zero => 0M;
        public decimal One => 1M;
        public decimal MinValue => decimal.MinValue;
        public decimal MaxValue => decimal.MaxValue;
        public decimal Add(decimal lhs, decimal rhs) => lhs + rhs;
        public decimal Add(decimal lhs, decimal? rhs) => lhs + rhs.GetValueOrDefault();
        public decimal AddInt(decimal lhs, int rhs) { checked { return lhs + rhs; } }
        public decimal DivLong(decimal lhs, long rhs) => lhs / rhs;
        public decimal Cast(decimal a) => a;
        public decimal Cast(int a) => a;
        public bool IsNaN(decimal x) => false;
        public bool GreaterThan(decimal lhs, decimal rhs) => lhs > rhs;
        public bool LessThan(decimal lhs, decimal rhs) => lhs < rhs;

        public Vector<decimal> Add(Vector<decimal> lhs, Vector<decimal> rhs) => throw new InvalidOperationException();

        public Vector<decimal> AddUnchecked(Vector<decimal> lhs, Vector<decimal> rhs) => throw new InvalidOperationException();

        public decimal AddUnchecked(decimal lhs, decimal rhs) => lhs + rhs;

        public decimal MaxInit => decimal.MinValue;
        public decimal MinInit => decimal.MaxValue;
    }
}
