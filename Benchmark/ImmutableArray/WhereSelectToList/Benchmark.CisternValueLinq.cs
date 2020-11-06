using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ImmutableArray
{
    partial class WhereSelectToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => 
            _double
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaSpan() => 
            Enumerable.FromSpan(_double, x => x.AsSpan())
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemory() =>
            _double
            .AsMemory()
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPool() =>
            _double
            .AsMemory()
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToListUseSharedPool();

        struct GreaterThanZero : IFunc<double, bool> { public bool Invoke(double x) => x > 0; }
        struct AddOne : IFunc<double, double> { public double Invoke(double x) => x + 1; }
        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPoolValueLambda() =>
            _double
            .AsMemory()
            .Where(new GreaterThanZero())
            .Select(new AddOne(), default(double))
            .ToListUseSharedPool();
    }
}
