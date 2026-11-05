using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class ForEach
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var range = Enumerable.Range(0, Count);
            foreach (var i in range)
            {
                count++;
            }
            return count;
        }

        [Benchmark]
        public int CisternValueLinqWithAction()
        {
            Enumerable.Range(0, Count).ForEach(action);
            return count;
        }
    }
}
