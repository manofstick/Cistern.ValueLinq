using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelect
{
    partial class Benchmark
    {
        [Benchmark(Baseline = true)]
        public int Handcoded()
        {
            return _ints switch
            {
                int[] asArray    => AsArray(asArray),
                List<int> asList => AsList(asList),
                var x            => AsEnumerable(x)
            };

            static int AsList(List<int> asList)
            {
                var total = 0;
                foreach (var x in asList)
                {
                    if ((x & 1) == 0)
                        total += x / 2;
                }

                return total;
            }

            static int AsArray(int[] asArray)
            {
                var total = 0;
                foreach (var x in asArray)
                {
                    if ((x & 1) == 0)
                        total += x / 2;
                }

                return total;
            }

            static int AsEnumerable(IEnumerable<int> asArray)
            {
                var total = 0;
                foreach (var x in asArray)
                {
                    if ((x & 1) == 0)
                        total += x / 2;
                }

                return total;
            }
        }
    }
}
