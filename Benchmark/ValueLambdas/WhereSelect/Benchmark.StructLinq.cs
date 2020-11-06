using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using StructLinq;

namespace Cistern.Benchmarks.ValueLambdas
{
    partial class WhereSelect
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

                var enumerable = list
                                      .ToStructEnumerable()
                                      .Where(ref @where, x => x)
                                      .Select(ref @select, x => x, x => x);
                var sum = 0;
                foreach (var i in enumerable)
                {
                    sum += i;
                }
                return sum;
            }

            static int AsArray(int[] array)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                var enumerable = array
                                 .ToStructEnumerable()
                                 .Where(ref @where, x => x)
                                 .Select(ref @select, x => x, x => x);
                var sum = 0;
                foreach (var i in enumerable)
                {
                    sum += i;
                }
                return sum;
            }

            static int AsEnumerable(IEnumerable<int> enumerable)
            {
                var where = new WherePredicate();
                var select = new SelectFunction();

                var structEnum = enumerable
                                 .ToStructEnumerable()
                                 .Where(ref @where, x => x)
                                 .Select(ref @select, x => x, x => x);
                var sum = 0;
                foreach (var i in structEnum)
                {
                    sum += i;
                }
                return sum;
            }
        }
    }
}
