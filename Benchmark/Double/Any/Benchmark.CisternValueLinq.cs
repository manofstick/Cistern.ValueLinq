using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double.Any
{
    partial class Benchmark
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
}
