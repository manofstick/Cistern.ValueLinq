using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Cistern.ValueLinq;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using Microsoft.Diagnostics.Tracing.Parsers.FrameworkEventSource;
//using NetFabric.Hyperlinq;
using System;
//using System.Linq;
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
//            for (var j = 0; j < 10; ++j)
//            {
//                var sw = Stopwatch.StartNew();

//                double z = 0;
//                for (var i = 0; i < 10000; ++i)
//                {
//#if truex
//                    var reified =
//                        Enumerable
//                        .Range(100, 10000)
//                        .Select(x => (long)x)
//                        .Where(x => true)
//                        .ToArray();

//                    var last = reified.Last();
//                    var average = reified.Count();
//                    var count = reified.Average();
//#else
//                    var (last, average, count) =
//                        Enumerable
//                        .Range(100, 10000)
//                        .Select(x => (long)x)
//                        .Where(x => true)
//                        .Fork(
//                            x => x.Last(),
//                            x => x.Count(),
//                            x => x.Average())
//                        ;
//#endif
//                    //Console.WriteLine($"{last}, {min}, {max}");
//                    z += average;
//                }

//                Console.WriteLine($"{sw.ElapsedMilliseconds} ({z})");
//            }

//            return;




            //            var (min, max) = y.Fork(x => x.Select(y => (byte)y).Min(), x => x.Select(y => (decimal)y).Max());


            //var a = new[] { 1, 2, 3, 4, 5, 6, 7 };
            //var b = a.Skip(1);
            //var c = a.Zip(b);

            //for (var j = 0; j < 10; ++j)
            //{
            //    var sw = Stopwatch.StartNew();
            //    for (var i = 0; i < 1000000; ++i)
            //    {
            //        var d = c.Count();
            //    }
            //    Console.WriteLine(sw.ElapsedMilliseconds);
            //}

            //return;



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
            DummyData.WhereSelectOrderByToArray.SanityCheck();

            Double.ToList.SanityCheck();
            Double.SelectToList.SanityCheck();
            Double.SelectSelectToList.SanityCheck();
            Double.WhereToList.SanityCheck();
            Double.SelectWhereToList.SanityCheck();
            Double.WhereSelectToList.SanityCheck();
            Double.WhereSelectIToList.SanityCheck();
            Double.WhereWhereToList.SanityCheck();
            Double.Max.SanityCheck();
            Double.Sum.SanityCheck();
            Double.Any.SanityCheck();
            Double.SelectSum.SanityCheck();
            Double.SelectManySum.SanityCheck();
            Double.SkipReverseSkipToArray.SanityCheck();

            Float.Sum.SanityCheck();

            Int.Sum.SanityCheck();
            Int.Average.SanityCheck();

            ValueLambdas.WhereSelectSum.SanityCheck();
            ValueLambdas.SelectWhereMax.SanityCheck();
            ValueLambdas.WhereSelect.SanityCheck();

            Span.StringToList.SanityCheck();

            ImmutableArray.WhereSelectToList.SanityCheck();

            Fork.RangeSelectWhereFork2.SanityCheck();
            Fork.RangeSelectWhereFork3.SanityCheck();
            Fork.ArrayWhereSelectFork2.SanityCheck();
            Fork.ArrayWhereSelectFork3.SanityCheck();
            Fork.ArrayFork3Optimized.SanityCheck();

            //var t = new Thing();
            //for (var j = 0; j < 10; ++j)
            //{
            //    for (var i = 0; i < 25/*00000*/; ++i)
            //        t.C();
            //    Console.Write('.');
            //}

            var summary = BenchmarkRunner.Run<Int.Average>();
        }
    }
}