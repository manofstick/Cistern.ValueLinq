using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Cistern.Benchmarks.Span
{
    partial class StringToList
    {
        [Benchmark(Baseline = true)]
        public List<char> Linq() => _text.ToList();
    }
}
