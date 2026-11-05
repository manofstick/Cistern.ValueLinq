using System;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Parity
{
    // What a Parity class's SanityCheck asks of each entry: the same result from every other version (Cistern, ZLinq,
    // hand-coded) as from System.Linq, and, for an entry whose saving is user code not run, no more calls of it from
    // Cistern than from System.Linq.
    public static class Check
    {
        public static void Same(string entry, int linq, int other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, long linq, long other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, short linq, short other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, float linq, float other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, double linq, double other)
        {
            if (!linq.Equals(other))
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, decimal linq, decimal other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, ulong linq, ulong other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, TimeSpan linq, TimeSpan other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, int? linq, int? other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, long? linq, long? other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, decimal? linq, decimal? other)
        {
            if (linq != other)
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, double? linq, double? other)
        {
            if (!(linq.HasValue == other.HasValue && linq.GetValueOrDefault().Equals(other.GetValueOrDefault())))
                throw new Exception($"{entry}: System.Linq {linq}, {other}");
        }

        public static void Same(string entry, int[][] linq, int[][] other)
        {
            if (linq.Length != other.Length)
                throw new Exception($"{entry}: System.Linq {linq.Length} rows, {other.Length}");
            for (var i = 0; i < linq.Length; ++i)
                if (!linq[i].SequenceEqual(other[i]))
                    throw new Exception($"{entry}: row {i} differs");
        }

        public static void Same<T>(string entry, IEnumerable<T> linq, IEnumerable<T> other)
        {
            if (!linq.SequenceEqual(other))
                throw new Exception($"{entry}: results differ");
        }

        // a finding rather than a broken benchmark, so it is printed and the run goes on
        public static void NoMoreCalls<T, TResult>(string entry, Func<T, TResult> selector, Action<Func<T, TResult>> use, Func<object?> linq, Func<object?> cistern)
        {
            var calls = 0;
            use(x => { ++calls; return selector(x); });

            linq();
            var linqCalls = calls;
            calls = 0;
            cistern();
            use(selector);

            if (calls > linqCalls)
                Console.WriteLine($"PARITY {entry}: System.Linq calls the user code {linqCalls} times, Cistern {calls}");
        }
    }
}
