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
        public List<double> CisternValueLinq_ViaMemorySharedPool_Push() =>
            _double
            .AsMemory()
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToListUsePool(viaPull: false);

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPool_Pull() =>
            _double
            .AsMemory()
            .Where(x => x > 0)
            .Select(x => x + 1)
            .ToListUsePool(viaPull: true);

        struct GreaterThanZero : IFunc<double, bool> { public bool Invoke(double x) => x > 0; }
        struct AddOne : IFunc<double, double> { public double Invoke(double x) => x + 1; }
        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPoolValueLambda_Push() =>
            _double
            .AsMemory()
            .Where(new GreaterThanZero())
            .Select(new AddOne(), default(double))
            .ToListUsePool(viaPull:false);
        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPoolValueLambda_Pull() =>
            _double
            .AsMemory()
            .Where(new GreaterThanZero())
            .Select(new AddOne(), default(double))
            .ToListUsePool(viaPull:true);
    }
}
