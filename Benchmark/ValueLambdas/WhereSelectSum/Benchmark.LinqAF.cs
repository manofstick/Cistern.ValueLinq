//using BenchmarkDotNet.Attributes;
//using LinqAF;

//namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
//{
//#if LINQAF
//    partial class Benchmark
//    {
//        [Benchmark]
//        public double LinqAF() =>
//            _ints
//            .Where(x => (x & 1) == 0)
//            .Select(x => x * 2)
//            .Sum();

//        [Benchmark]
//        public double LinqAF_Foreach()
//        {
//            var total = 0.0;
//            foreach (var item in
//                _ints
//                .Where(x => (x & 1) == 0)
//                .Select(x => x * 2)
//            )
//            {
//                total += item;
//            }
//            return total;
//        }
//    }
//#endif
//}
