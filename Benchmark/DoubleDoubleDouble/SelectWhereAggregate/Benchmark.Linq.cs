﻿using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.DoubleDoubleDouble
{
    partial class SelectWhereAggregate
    {
        [Benchmark(Baseline = true)]
        public (double, double, double) Linq()
        {
            return
                _doubledoubledoubles
                .Select(((double x, double y, double z) d) => (x: d.x * d.x, y: d.y * d.y, z: d.z * d.z))
                .Where(d => d.x > 0.25 && d.y > 0.25 && d.z > 0.25)
                .Aggregate((x: 0.0, y: 0.0, z: 0.0), (a, d) => (a.x + d.x, a.y + d.y, a.z + d.z));
        }
    }
}
