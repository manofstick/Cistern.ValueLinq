﻿using BenchmarkDotNet.Attributes;
using Cistern.Linq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double.SelectToList
{
#if CISTERNLINQ
    partial class Benchmark
    {
        [Benchmark]
        public List<double> CisternLinq() => _double.Select(x => x*2).ToList();
    }
#endif
}
