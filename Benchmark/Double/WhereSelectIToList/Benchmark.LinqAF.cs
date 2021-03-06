﻿using BenchmarkDotNet.Attributes;
using LinqAF;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Double
{
#if LINQAF
    partial class WhereSelectIToList
    {
        [Benchmark]
        public List<double> LinqAF() => _double.Where(x => x > 0).Select((x,i) => x + i).ToList();
    }
#endif
}
