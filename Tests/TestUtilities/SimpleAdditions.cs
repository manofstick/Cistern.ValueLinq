using System;
using System.Collections.Generic;
using System.Text;

#if CISTERN_LINQ
using Cistern.Linq;
#elif CISTERN_VALUELINQ
using Cistern.ValueLinq;
#else
using System.Linq;
#endif

namespace Linqs.Tests.TestUtilities
{
    static class SimpleAdditions
    {
        public static IEnumerable<T> InjectWhere<T>(this IEnumerable<T> t) => t.Where(_ => true);
        public static IEnumerable<T> InjectWhereSelect<T>(this IEnumerable<T> t) => t.Where(_ => true).Select(x => x);
        public static IEnumerable<T> InjectSelect<T>(this IEnumerable<T> t) => t.Select(x => x);
        public static IEnumerable<T> InjectSelectWhere<T>(this IEnumerable<T> t) => t.Select(x => x).Where(_ => true);
        public static IEnumerable<T> InjectSelectMany<T>(this IEnumerable<T> t) => t.SelectMany(x => new [] { x });
    }
}
