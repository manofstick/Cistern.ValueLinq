using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.StructLinq
{
    partial class Dictionary
    {
        [Benchmark]
        public int CisternValueLinq()
        {
            var sum = 0;
            foreach (var keyValuePair in dico.OfEnumerable())
            {
                sum += keyValuePair.Key;
                sum += keyValuePair.Value.Length;
            }

            return sum;
        }
    }
}
