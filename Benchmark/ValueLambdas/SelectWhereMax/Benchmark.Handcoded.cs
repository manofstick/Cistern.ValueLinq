using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class SelectWhereMax
    {
        [Benchmark(Baseline = true)]
        public double Handcoded()
        {
            return _ints switch
            {
                int[] asArray => AsArray(asArray),
                List<int> asList => AsList(asList),
                var x => AsEnumerable(x)
            };

            static int AsList(List<int> asList)
            {
                var max = int.MinValue;
                foreach (var n in asList)
                {
                    var x = n / 2;
                    if ((x & 1) == 0)
                    {
                        if (x > max)
                            max = x;
                    }
                }
                return max;
            }
            
            static int AsArray(int[] asArray)
            {
                var max = int.MinValue;
                foreach (var n in asArray)
                {
                    var x = n / 2;
                    if ((x & 1) == 0)
                    {
                        if (x > max)
                            max = x;
                    }
                }
                return max;
            }

            static int AsEnumerable(IEnumerable<int> asArray)
            {
                var max = int.MinValue;
                foreach (var n in asArray)
                {
                    var x = n / 2;
                    if ((x & 1) == 0)
                    {
                        if (x > max)
                            max = x;
                    }
                }
                return max;
            }
        }
    }
}
