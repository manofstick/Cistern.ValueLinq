﻿using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using NetFabric.Hyperlinq;

namespace Cistern.Benchmarks.Double.ToList
{
    partial class Benchmark
    {
        //[Benchmark]
        //public List<double> HyperLinq() => _double.ToList<IEnumerable<double>, IEnumerable<double>>();
    }
}
