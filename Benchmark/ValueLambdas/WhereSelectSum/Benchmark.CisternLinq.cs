//using BenchmarkDotNet.Attributes;
//using Cistern.Linq;

//namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
//{
//#if CISTERNLINQ
//    partial class Benchmark
//    {
//        [Benchmark]
//        public double CisternLinq() =>
//            _ints
//            .Where(x => (x & 1) == 0)
//            .Select(x => x * 2)
//            .Sum();
//    }
//#endif
//}
