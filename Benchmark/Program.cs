using BenchmarkDotNet.Running;
using System;

namespace AnotherBenchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var check = new Benchmark();

            check.Length = 100;
            check.SetupData();

            var a = check.Linq();
            var b = check.LinqAF();
            var c = check.ValueLinq();
            var d = check.ValueLinqByRef();
            var e = d;// check.HyperLinq();

            if (a != b || b != c || c != d || d != e)
                throw new System.Exception($"({a} != {b} || {b} != {c} || {c} != {d}) || {d} != {e}");

            var summary = BenchmarkRunner.Run<Benchmark>();
        }
    }
}