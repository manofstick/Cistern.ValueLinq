using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        static string[][] Source = new[] { new[] { "foo", "bar" }, new[] { "fizz", "buzz" }, new[] { "hello", "world" } };
        static Func<string[], string> CollectionSelector = arr => arr[0];
        static Func<string[], int, string> CollectionSelectorIndexed = (arr, ix) => arr[0];
        static Func<string[], char, string> ResultSelector = (a, b) => a[0];


        public static IEnumerable<char> Stuff()
        {
            for (var i = 0; i < 100; ++i)
                yield return 'a';
            //yield return 'a';
            //yield return 'b';
            //yield return 'c';
            //yield return 'd';
            //yield return 'e';
            //yield return 'f';
            //yield return 'g';
        }

        public static void Main(string[] args)
        {
            //var Z = new DummyData.OrderByToArray();
            //Z.Length = 5;
            //Z.SetupData();

            //for (var j = 0; j < 10; ++j)
            //{
            //    for (var i = 0; i < 1000000; ++i)
            //    {
            //        Z.CisternValueLinq();
            //    }
            //    Console.Write(".");
            //}
            //return;



            //var z =
            //    Enumerable
            //    .Range(0, 100)
            //    .Select(x => x * -1)
            //    .OrderBy(x => x)
            //    .ThenBy(x => x)
            //    .ThenBy(x => x)
            //    .First();




            //for (var j = 0; j < 10; ++j)
            //{
            //    var sw = Stopwatch.StartNew();
            //    for (var i = 0; i < 1000000; ++i)
            //    {
            //        //Source
            //        //    .OfArray()
            //        //    .SelectMany(
            //        //        arr => arr[0].AsMemory().OfMemory(),
            //        //        ResultSelector)
            //        //    .ForEach(_ => { });

            //        //foreach (var item in Source.OfArray().SelectMany(arr => arr[0].AsMemory().OfMemory(), ResultSelector))
            //        //foreach (var item in Source.SelectMany(CollectionSelector, ResultSelector))
            //        foreach (var item in System.Linq.Enumerable.SelectMany(Source, CollectionSelector, ResultSelector))
            //        {
            //        }
            //    }
            //    Console.WriteLine(sw.ElapsedMilliseconds);
            //}
            //return;




            //For some sanity checking
            DoubleDoubleDouble.SelectWhereAggregate.SanityCheck();
            DoubleDoubleDouble.WhereSelectAggregate.SanityCheck();
            DoubleDoubleDouble.OrderByThenByThenByToArray.SanityCheck();

            DummyData.OrderByToArray.SanityCheck();

            Double.ToList.SanityCheck();
            Double.SelectToList.SanityCheck();
            Double.SelectSelectToList.SanityCheck();
            Double.WhereToList.SanityCheck();
            Double.SelectWhereToList.SanityCheck();
            Double.WhereSelectToList.SanityCheck();
            Double.WhereSelectIToList.SanityCheck();
            Double.WhereWhereToList.SanityCheck();
            Double.Sum.SanityCheck();
            Double.Any.SanityCheck();
            Double.SelectSum.SanityCheck();
            Double.SelectManySum.SanityCheck();
            Double.SkipReverseSkipToArray.SanityCheck();

            ValueLambdas.WhereSelectSum.SanityCheck();
            ValueLambdas.SelectWhereMax.SanityCheck();
            ValueLambdas.WhereSelect.SanityCheck();

            Span.StringToList.SanityCheck();

            ImmutableArray.WhereSelectToList.SanityCheck();


            //var t = new Thing();
            //for (var j = 0; j < 10; ++j)
            //{
            //    for (var i = 0; i < 25/*00000*/; ++i)
            //        t.C();
            //    Console.Write('.');
            //}





            var summary = BenchmarkRunner.Run<Double.SelectManySum>();
        }
    }
}