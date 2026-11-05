using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Synthetic
{
    partial class JoinShapesSingleCall
    {
        [Benchmark, BenchmarkCategory("FullJoinFewKeys")]
        public int CisternValueLinq_FullJoinFewKeys() =>
            _fewOuter
            .FullJoin(_fewInner, o => o, i => i.Key)
            .Count();

        [Benchmark, BenchmarkCategory("FullJoinDistinct")]
        public int CisternValueLinq_FullJoinDistinct() =>
            _distinctOuter
            .FullJoin(_distinctInner, o => o, i => i.Key)
            .Count();
    }
}
