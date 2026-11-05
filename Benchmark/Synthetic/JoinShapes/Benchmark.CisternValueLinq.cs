using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class JoinShapes
    {
        [Benchmark, BenchmarkCategory("FullJoinFewKeys")]
        public int CisternValueLinq_FullJoinFewKeys() =>
            _fewOuter
            .FullJoin(_fewInner, o => o, i => i.Key)
            .Count();

        [Benchmark, BenchmarkCategory("JoinFewKeys")]
        public int CisternValueLinq_JoinFewKeys() =>
            _fewOuter
            .Join(_fewInner, o => o, i => i.Key, (o, i) => i.Value)
            .Count();

        [Benchmark, BenchmarkCategory("FullJoinWide")]
        public int CisternValueLinq_FullJoinWide() =>
            _distinctOuter
            .FullJoin(_wideInner, o => o, i => i.Key)
            .Count();

        [Benchmark, BenchmarkCategory("JoinWide")]
        public int CisternValueLinq_JoinWide() =>
            _distinctOuter
            .Join(_wideInner, o => o, i => i.Key, (o, i) => i.A)
            .Count();

        [Benchmark, BenchmarkCategory("GroupJoin")]
        public int CisternValueLinq_GroupJoin() =>
            _distinctOuter
            .GroupJoin(_smallInner, o => o, i => i.Key, (o, matches) => matches.Count())
            .Sum();
    }
}
