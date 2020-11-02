using BenchmarkDotNet.Attributes;
using StructLinq;
using System.Collections.Generic;

namespace Cistern.Benchmarks.ValueLambdas.SelectWhereMax
{
    partial class Benchmark
    {
        struct WherePredicate : IFunction<int, bool> { public bool Eval(int element) => (element & 1) == 0; }
        struct SelectFunction : IFunction<int, int> { public int Eval(int element) => element / 2; }

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
                    .Select(ref @select, x => x, x => x)
                    .Where(ref @where, x => x)
                    .Max(x=>x);
            }

            static int AsArray(int[] array)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                return
                    array
                    .ToStructEnumerable()
                    .Select(ref @select, x => x, x => x)
                    .Where(ref @where, x => x)
                    .Max(x=>x);
            }

            static int AsEnumerable(IEnumerable<int> enumerable)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                return
                    enumerable
                    .ToStructEnumerable()
                    .Select(ref @select, x => x, x => x)
                    .Where(ref @where, x => x)
                    .Max(x=>x);
            }
        }
    }
}
