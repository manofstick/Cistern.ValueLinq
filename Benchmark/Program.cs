using BenchmarkDotNet.Running;
using System;

namespace Cistern.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DoubleDoubleDouble.SelectWhereAggregate.Benchmark.Validate();
            DoubleDoubleDouble.WhereSelectAggregate.Benchmark.Validate();
            Double.ToList.Benchmark.Validate();
            Double.SelectToList.Benchmark.Validate();

            var summary = BenchmarkRunner.Run<Double.SelectToList.Benchmark>();
        }
    }
}