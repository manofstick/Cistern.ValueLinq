﻿using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
    partial class SelectToList
    {
        [Benchmark]
        public List<double> CisternValueLinq() => _double.Select(x => x * 2).ToList();
    }
}
