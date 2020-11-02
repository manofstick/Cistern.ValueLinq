using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelect
{
    partial class Benchmark
    {
        struct HalveAnInt : IFunc<int, int> { public int Invoke(int t) => t / 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        
        [Benchmark]
        public int CisternValueLinq()
        {
            var enumerable = _ints
                             .Where(new FilterEvenInts()) // ug, sugar please
                             .Select(new HalveAnInt(), default(int)); // ug, sugar please + better type inference...
            var sum = 0;
            foreach (var i in enumerable)
            {
                sum += i;
            }
            return sum;
        }
    }
}
