using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Span
{
#if CISTERNLINQ
    partial class StringToList
    {
        [Benchmark]
        public List<char> CisternLinq() => _text.ToList();
    }
#endif
}
