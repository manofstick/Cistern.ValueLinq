using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.Double
{
    partial class Any
    {
        [Benchmark]
        public bool CisternValueLinq() =>
            _filter == null
                ? _double.Any()
                : _double.Any(_filter);
    }
}
