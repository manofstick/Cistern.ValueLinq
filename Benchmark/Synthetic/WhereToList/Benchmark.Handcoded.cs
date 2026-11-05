using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Synthetic
{
    partial class WhereToList
    {
        [Benchmark(Baseline = true)]
        public List<int> Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _data)
            {
                if (x % 2 == 0)
                    result.Add(x);
            }
            return result;
        }
    }
}
