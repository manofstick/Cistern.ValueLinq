using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.ValueLambdas.SelectWhereMax
{
    partial class Benchmark
    {
        struct HalveAnInt : IFunc<int, int> { public int Invoke(int t) => t / 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        [Benchmark]
        public int CisternValueLinq() =>
            _ints
            .Select(new HalveAnInt(), default(int)) // ug, sugar please + better type inference...
            .Where(new FilterEvenInts()) // ug, sugar please
            .Max();
    }
}
