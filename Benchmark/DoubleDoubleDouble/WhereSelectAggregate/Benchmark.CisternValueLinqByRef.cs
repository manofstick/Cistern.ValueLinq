﻿using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;

namespace Cistern.Benchmarks.DoubleDoubleDouble.WhereSelectAggregate
{
    partial class Benchmark
    {
        [Benchmark]
        public (double, double, double) CisternValueLinqByRef()
        {
            return
                _doubledoubledoubles
                .Where((in (double x, double y, double z) d) => d.x > 0.25 && d.y > 0.25 && d.z > 0.25)
                .Select((in (double x, double y, double z) d) => (d.x * d.x, d.y * d.y, d.z * d.z))
                .Aggregate((0.0, 0.0, 0.0), ((double x, double y, double z) a, (double x, double y, double z) d) => (a.x + d.x, a.y + d.y, a.z + d.z));
        }
    }
}