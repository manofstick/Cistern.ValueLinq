using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using StructLinq;

namespace Cistern.Benchmarks.StructLinq
{
    [MemoryDiagnoser]
    public partial class Dictionary
    {

        private Dictionary<int, string> dico;

        [Params(2, 100, 1000)]
        public int ItemCount { get; set; }
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            dico = Enumerable
                   .Range(0, ItemCount)
                   .ToDictionary(x=> x, x=> x.ToString());
        }

        [Benchmark(Baseline = true)]
        public int LINQ()
        {
            var sum = 0;
            foreach (var keyValuePair in dico)
            {
                sum += keyValuePair.Key;
                sum += keyValuePair.Value.Length;
            }

            return sum;
        }

        [Benchmark]
        public int StructLINQ()
        {
            var sum = 0;
            foreach (var keyValuePair in dico.ToStructEnumerable())
            {
                sum += keyValuePair.Key;
                sum += keyValuePair.Value.Length;
            }

            return sum;
        }

    }
}
