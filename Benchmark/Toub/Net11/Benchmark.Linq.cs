using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.Toub
{
    partial class Net11
    {
        [Benchmark]
        public int N01_Linq() =>
            _source
            .Append(_appended)
            .Skip(10)
            .LastOrDefault();

        [Benchmark]
        public int N02_Linq() =>
            _intData
            .Sum();

        [Benchmark]
        public byte N03a_Linq() =>
            _bytes16
            .Max();

        [Benchmark]
        public byte N03b_Linq() =>
            _bytes64
            .Max();

        [Benchmark]
        public int N04a_Linq() =>
            _left
            .FullJoin(_right, l => l.Id, r => r.Id)
            .Count();

        [Benchmark]
        public int N04b_Linq() =>
            _left
            .GroupJoin(_right, l => l.Id, r => r.Id, (l, rs) => (l, rs))
            .SelectMany(x => x.rs.DefaultIfEmpty(), (x, r) => (x.l, r))
            .Concat(_right
                .Where(r => !_left.Any(l => l.Id == r.Id))
                .Select(r => (l: default((int Id, string Name)), r)))
            .Count();
    }
}
