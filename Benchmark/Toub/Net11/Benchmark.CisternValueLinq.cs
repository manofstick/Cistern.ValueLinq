using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using Cistern.ValueLinq.Maths;

namespace Cistern.Benchmarks.Toub
{
    partial class Net11
    {
        [Benchmark]
        public int N01_CisternValueLinq() =>
            _source
            .Append(_appended)
            .Skip(10)
            .LastOrDefault();

        [Benchmark]
        public int N02_CisternValueLinq() =>
            _intData
            .Sum();

        // Cistern's option to sum without overflow checks
        [Benchmark]
        public int N02_CisternValueLinqFastest() =>
            _intData
            .Sum(SIMDOptions.Fastest);

        [Benchmark]
        public byte N03a_CisternValueLinq() =>
            _bytes16
            .Max();

        [Benchmark]
        public byte N03b_CisternValueLinq() =>
            _bytes64
            .Max();

        [Benchmark]
        public int N04a_CisternValueLinq() =>
            _left
            .FullJoin(_right, l => l.Id, r => r.Id)
            .Count();

        [Benchmark]
        public int N04b_CisternValueLinq() =>
            _left
            .GroupJoin(_right, l => l.Id, r => r.Id, (l, rs) => (l, rs))
            .SelectMany(x => x.rs.DefaultIfEmpty(), (x, r) => (x.l, r))
            .Concat(_right
                .Where(r => !_left.Any(l => l.Id == r.Id))
                .Select(r => (l: default((int Id, string Name)), r)))
            .Count();
    }
}
