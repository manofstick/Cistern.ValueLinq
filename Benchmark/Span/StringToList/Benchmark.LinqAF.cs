using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Span
{
#if LINQAF
    partial class StringToList
    {
        [Benchmark]
        public List<char> LinqAF() => _text.ToList();
    }
#endif
}
