using BenchmarkDotNet.Attributes;
using StructLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
{
    partial class Benchmark
    {
        struct WherePredicate : IFunction<int, bool> { public bool Eval(int element) => (element & 1) == 0; }
        struct SelectFunction : IFunction<int, int> { public int Eval(int element) => element * 2; }

        [Benchmark]
        public int StructLinq()
        {
            return _ints switch
            {
                int[] asArray => AsArray(asArray),
                List<int> asList => AsList(asList),
                var x => AsEnumerable(x)
            };

            static int AsList(List<int> list)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                return
                    list
                    .ToStructEnumerable()
                    .Where(ref @where, x => x)
                    .Select(ref @select, x => x, x => x)
                    .Sum(x=>x);
            }

            static int AsArray(int[] array)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                return
                    array
                    .ToStructEnumerable()
                    .Where(ref @where, x => x)
                    .Select(ref @select, x => x, x => x)
                    .Sum(x=>x);
            }

            static int AsEnumerable(IEnumerable<int> enumerable)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                return
                    enumerable
                    .ToStructEnumerable()
                    .Where(ref @where, x => x)
                    .Select(ref @select, x => x, x => x)
                    .Sum(x=>x);
            }
        }
    }
}
