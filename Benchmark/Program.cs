using BenchmarkDotNet.Running;
using System;

namespace Cistern.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DoubleDoubleDouble.SelectWhereAggregate.Benchmark.Validate();

            var summary = BenchmarkRunner.Run<DoubleDoubleDouble.WhereSelectAggregate.Benchmark>();
        }
    }
}