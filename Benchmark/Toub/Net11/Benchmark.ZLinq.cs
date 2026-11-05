using BenchmarkDotNet.Attributes;
using ZLinq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net11
    {
        [Benchmark]
        public int N01_ZLinq() =>
            _source
            .AsValueEnumerable()
            .Append(_appended)
            .Skip(10)
            .LastOrDefault();

        [Benchmark]
        public int N02_ZLinq() =>
            _intData
            .AsValueEnumerable()
            .Sum();

        [Benchmark]
        public byte N03a_ZLinq() =>
            _bytes16
            .AsValueEnumerable()
            .Max();

        [Benchmark]
        public byte N03b_ZLinq() =>
            _bytes64
            .AsValueEnumerable()
            .Max();

        // N04a: ZLinq has no FullJoin
        // N04b: the composition FullJoin replaces, measured for System.Linq and Cistern
    }
}
