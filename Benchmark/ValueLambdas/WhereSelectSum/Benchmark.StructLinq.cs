using BenchmarkDotNet.Attributes;
//using StructLinq;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
{
    partial class Benchmark
    {
        //struct WherePredicate : IFunction<int, bool>
        //{
        //    public readonly bool Eval(int element)
        //    {
        //        return (element & 1) == 0;
        //    }
        //}

        //struct SelectFunction : IFunction<int, int>
        //{
        //    public readonly int Eval(int element)
        //    {
        //        return element * 2;
        //    }
        //}

        //[Benchmark]
        //public int StructLinq() =>
        //    _ints
        //    .ToStructEnumerable()
        //    .Where(x => (x & 1) == 0)
        //    .Select(x => x * 2)
        //    .Sum();


        //[Benchmark]
        //public int StructLinq_Structs()
        //{
        //    var where = new WherePredicate();
        //    var select = new SelectFunction();

        //    return 
        //        _ints
        //        .ToStructEnumerable()
        //        .Where(ref @where, x => x)
        //        .Select(ref @select, x => x, x => x)
        //        .Sum();
        //}

        ////[Benchmark]
        ////public double StructLinqLinq_Foreach()
        ////{
        ////    var total = 0.0;
        ////    foreach (var item in 
        ////        _ints
        ////        .ToStructEnumerable()
        ////        .Where(x => (x & 1) == 0)
        ////        .Select(x => x * 2)
        ////    )
        ////    {
        ////        total += item;
        ////    }
        ////    return total;
        ////}
    }
}
