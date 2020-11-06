using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using NetFabric.Hyperlinq;

namespace Cistern.Benchmarks.Double
{
    partial class SelectSelectToList
    {
        //[Benchmark]
        //public List<double> HyperLinq() => _double.ToList<IEnumerable<double>, IEnumerable<double>>();
    }
}
