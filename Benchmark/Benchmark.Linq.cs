using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnotherBenchmark
{
    public partial class Benchmark
    {
#if true
        [Benchmark(Baseline = true)]
        public (double, double, double) Linq()
        {
#if A
            return
                _doubledoubledoubles
                .Select(((double x, double y, double z) d) => (d.x * d.x, d.y * d.y, d.z * d.z))
                .Where(((double x, double y, double z) d) => d.x > 0.25 && d.y > 0.25 && d.z > 0.25)
                .Aggregate((0.0,0.0,0.0), ((double x, double y, double z) a, (double x, double y, double z) d) => (a.x+d.x, a.y+d.y, a.z+d.z));
#else
            var total = 0.0;
            foreach (var item in _doubledoubledoubles.Select(((double x, double y, double z) d) => d.x * d.y * d.z))
            //foreach (var item in _doubles.Select(x => x + 0.5))
                total += item;
            return total;
#endif
        }

#else

        [Benchmark(Baseline = true)]
        public List<double> Linq()
        {
            return
                _doubledoubledoubles
                .Select(((double x, double y, double z) d) => (d.x * d.x, d.y * d.y, d.z * d.z))
//                .Where(((double x, double y, double z) d) => d.x > 0.25 && d.y > 0.25 && d.z > 0.25)
                .Select(((double x, double y, double z) d) => d.x * d.y * d.z)
                .ToList();
        }
#endif
    }
}
