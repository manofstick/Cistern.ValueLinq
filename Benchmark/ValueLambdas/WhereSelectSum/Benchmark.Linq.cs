using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
{
    partial class Benchmark
    {
        [Benchmark]
        public double Linq() =>
            _ints
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();

        //[Benchmark]
        //public double Linq_Foreach()
        //{
        //    var total = 0.0;
        //    foreach (var item in
        //        _ints
        //        .Where(x => (x & 1) == 0)
        //        .Select(x => x * 2)
        //    )
        //    {
        //        total += item;
        //    }
        //    return total;
        //}


        //[Benchmark]
        //public double Flatout()
        //{
        //    var total = 0.0;
        //    foreach (var x in _ints)
        //    {
        //        if ((x & 1) == 0)
        //            total += x * 2;
        //    }
        //    return total;
        //}
    }
}
