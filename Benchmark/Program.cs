using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks
{
    [MemoryDiagnoser]
    public partial class Thing
    {
        IEnumerable<int> data;

        public Thing()
        {
            data = GetDataB();// System.Linq.Enumerable.ToList(System.Linq.Enumerable.Range(0, 1));

        }
        private IEnumerable<int> GetData()
        {
            for (var i = 0; i < 100000; ++i)
                yield return i;
        }

        private IEnumerable<int> GetDataB() =>
            System.Linq.Enumerable.ToList(System.Linq.Enumerable.Range(0, 1));

        [Benchmark]
        public int C()
        {
            var sum = 0;
            foreach (var z in Cistern.ValueLinq.Enumerable.Select(Cistern.ValueLinq.Enumerable.Where(data, x => x >= 0), x => x))
                sum += z;
            return sum;
        }

        [Benchmark(Baseline = true)]
        public int S()
        {
            var sum = 0;
            foreach (var z in System.Linq.Enumerable.Select(System.Linq.Enumerable.Where(data, x => x >= 0), x => x))
                sum += z;
            return sum;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            // For some sanity checking
            //DoubleDoubleDouble.SelectWhereAggregate.SanityCheck();
            //DoubleDoubleDouble.WhereSelectAggregate.SanityCheck();

            //Double.ToList.SanityCheck();
            //Double.SelectToList.SanityCheck();
            //Double.SelectSelectToList.SanityCheck();
            //Double.WhereToList.SanityCheck();
            //Double.SelectWhereToList.SanityCheck();
            //Double.WhereSelectToList.SanityCheck();
            //Double.WhereSelectIToList.SanityCheck();
            //Double.WhereWhereToList.SanityCheck();
            //Double.Sum.SanityCheck();
            //Double.Any.SanityCheck();
            //Double.SelectSum.SanityCheck();
            //Double.SelectManySum.SanityCheck();

            //ValueLambdas.WhereSelectSum.SanityCheck();
            //ValueLambdas.SelectWhereMax.SanityCheck();
            //ValueLambdas.WhereSelect.SanityCheck();

            //Span.StringToList.SanityCheck();

            //ImmutableArray.WhereSelectToList.SanityCheck();


            //var t = new Thing();
            //for (var j = 0; j < 10; ++j)
            //{
            //    for (var i = 0; i < 25/*00000*/; ++i)
            //        t.C();
            //    Console.Write('.');
            //}

            var summary = BenchmarkRunner.Run<Thing>();
        }
    }
}