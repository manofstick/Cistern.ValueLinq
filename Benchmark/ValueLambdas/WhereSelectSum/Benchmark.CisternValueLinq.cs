using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class WhereSelectSum
    {
        struct DoubleAnInt : IFunc<int, int> { public int Invoke(int t) => t * 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        [Benchmark]
        public int CisternValueLinq() =>
            _ints
            .Where(new FilterEvenInts()) // ug, sugar please
            .Select(new DoubleAnInt(), default(int)) // ug, sugar please + better type inference...
            .Sum();
    }
}
