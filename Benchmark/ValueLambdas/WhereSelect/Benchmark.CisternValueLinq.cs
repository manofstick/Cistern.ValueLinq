using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelect
{
    partial class Benchmark
    {
        struct HalveAnInt : IFunc<int, int> { public int Invoke(int t) => t / 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        
        [Benchmark]
        public int CisternValueLinq()
        {
            var enumerable = _ints
                             .Where(new FilterEvenInts()) // ug, sugar please
                             .Select(new HalveAnInt(), default(int)); // ug, sugar please + better type inference...
            var sum = 0;
            foreach (var i in enumerable)
            {
                sum += i;
            }
            return sum;
        }

        [Benchmark]
        public int CisternValueLinq_ViaIEnumerable()
        {
            var enumerable = _ints
                             .Where(new FilterEvenInts()) // ug, sugar please
                             .Select(new HalveAnInt(), default(int)); // ug, sugar please + better type inference...
            return DoSum(enumerable);

            static int DoSum(IEnumerable<int> enumerable)
            {
                var sum = 0;
                foreach (var i in enumerable)
                {
                    sum += i;
                }
                return sum;
            }
        }

        [Benchmark]
        public int CisternValueLinq_ViaAggregate()
        {
            return
                _ints
                .Where(new FilterEvenInts()) // ug, sugar please
                .Select(new HalveAnInt(), default(int)) // ug, sugar please + better type inference...
                .Aggregate(0, (l, r) => l + r);
        }

        [Benchmark]
        public int CisternValueLinq_ViaSum()
        {
            return
                _ints
                .Where(new FilterEvenInts()) // ug, sugar please
                .Select(new HalveAnInt(), default(int)) // ug, sugar please + better type inference...
                .Sum();
        }
    }
}
