using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class DeepChainEnumerator
    {
        private static IEnumerable<int> BuildCisternValueLinq_Depth4(IEnumerable<int> source) =>
            source
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0);

        private static IEnumerable<int> BuildCisternValueLinq_Depth16(IEnumerable<int> source) =>
            source
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0);

        private static IEnumerable<int> BuildCisternValueLinq_Depth32(IEnumerable<int> source) =>
            source
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0)
            .Select(x => x + 1).Where(x => x >= 0);

        // a Concat's builder carries the other side's node while its own side is rebuilt, so both sides are deep
        private static IEnumerable<int> BuildCisternValueLinq_Concat(IEnumerable<int> source)
        {
            var start =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            var finish =
                source
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0)
                .Select(x => x + 1).Where(x => x >= 0);

            var concat =
                start
                .Concat(finish)
                .Select(x => x + 1).Where(x => x >= 0);

            return concat;
        }

        [BenchmarkCategory("Depth4"), Benchmark]
        public long CisternValueLinq_Depth4() => Sum(_depth4);

        [BenchmarkCategory("Depth16"), Benchmark]
        public long CisternValueLinq_Depth16() => Sum(_depth16);

        [BenchmarkCategory("Depth32"), Benchmark]
        public long CisternValueLinq_Depth32() => Sum(_depth32);

        [BenchmarkCategory("Concat"), Benchmark]
        public long CisternValueLinq_Concat() => Sum(_concat);

        private static long Sum(IEnumerable<int> chain)
        {
            var sum = 0L;
            foreach (var x in chain)
                sum += x;
            return sum;
        }
    }
}
