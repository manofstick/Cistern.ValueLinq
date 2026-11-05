using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.Synthetic
{
    partial class JoinShapesSingleCall
    {
        [Benchmark, BenchmarkCategory("FullJoinFewKeys")]
        public int Handcoded_FullJoinFewKeys() =>
            JoinShapes.FullJoinCount(_fewOuter, _fewInner);

        [Benchmark, BenchmarkCategory("FullJoinDistinct")]
        public int Handcoded_FullJoinDistinct() =>
            JoinShapes.FullJoinCount(_distinctOuter, _distinctInner);
    }
}
