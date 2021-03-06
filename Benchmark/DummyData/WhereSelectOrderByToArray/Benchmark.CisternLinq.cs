﻿using BenchmarkDotNet.Attributes;
using Cistern.Linq;

namespace Cistern.Benchmarks.DummyData
{
#if CISTERNLINQ
    partial class WhereSelectOrderByToArray
    {
        [Benchmark]
        public string[] CisternLinq()
        {
            return
                _data
                .Where(x => !x.Country.StartsWith('S'))
                .Select(x => x.Name)
                .OrderBy(x => x)
                .ToArray();
        }
    }
#endif
}
