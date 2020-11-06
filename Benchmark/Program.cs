using BenchmarkDotNet.Running;
using System;

namespace Cistern.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // For some sanity checking
            DoubleDoubleDouble.SelectWhereAggregate.SanityCheck();
            DoubleDoubleDouble.WhereSelectAggregate.SanityCheck();

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

            ValueLambdas.WhereSelectSum.SanityCheck();
            ValueLambdas.SelectWhereMax.SanityCheck();
            ValueLambdas.WhereSelect.SanityCheck();

            Span.StringToList.SanityCheck();

            ImmutableArray.WhereSelectToList.SanityCheck();

            var summary = BenchmarkRunner.Run<ImmutableArray.WhereSelectToList>();
        }
    }
}