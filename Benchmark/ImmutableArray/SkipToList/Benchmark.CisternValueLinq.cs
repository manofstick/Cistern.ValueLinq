using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ImmutableArray
{
    partial class SkipToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => 
            _double
            .Skip(Length/2)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaSpan() => 
            Enumerable.FromSpan(_double, x => x.AsSpan())
            .Skip(Length / 2)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemory() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToList();

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPool_Push() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToListUsePool(viaPull: false);

        [Benchmark]
        public List<double> CisternValueLinq_ViaMemorySharedPool_Pull() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToListUsePool(viaPull: true);

    }
}
