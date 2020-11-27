using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ImmutableArray
{
    partial class SkipToArray
    {
        [Benchmark]
        public double[] CisternValueLinq() => 
            _double
            .Skip(Length/2)
            .ToArray();

        [Benchmark]
        public double[] CisternValueLinq_ViaSpan() => 
            Enumerable.FromSpan(_double, x => x.AsSpan())
            .Skip(Length / 2)
            .ToArray();

        [Benchmark]
        public double[] CisternValueLinq_ViaMemory() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToArray();

        [Benchmark]
        public double[] CisternValueLinq_ViaMemorySharedPool_Push() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToArrayUsePool(viaPull: false);

        [Benchmark]
        public double[] CisternValueLinq_ViaMemorySharedPool_Pull() =>
            _double
            .AsMemory()
            .Skip(Length / 2)
            .ToArrayUsePool(viaPull: true);

    }
}
