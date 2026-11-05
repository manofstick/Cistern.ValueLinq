using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using ZLinq;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class SelectWhereMax
    {
        [Benchmark]
        public int ZLinq()
        {
            return _ints switch
            {
                int[] asArray => AsArray(asArray),
                List<int> asList => AsList(asList),
                var x => AsEnumerable(x)
            };

            static int AsArray(int[] array) =>
                array
                .AsValueEnumerable()
                .Select(x => x / 2)
                .Where(x => (x & 1) == 0)
                .Max();

            static int AsList(List<int> list) =>
                list
                .AsValueEnumerable()
                .Select(x => x / 2)
                .Where(x => (x & 1) == 0)
                .Max();

            static int AsEnumerable(IEnumerable<int> enumerable) =>
                enumerable
                .AsValueEnumerable()
                .Select(x => x / 2)
                .Where(x => (x & 1) == 0)
                .Max();
        }
    }
}
