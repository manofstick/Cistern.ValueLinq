using System;

namespace Cistern.ValueLinq.Optimizations
{
    struct ToListSelectWhere<T,U>
    {
        public readonly Func<T, U> Map;
        public readonly Func<U, bool> Filter;

        public ToListSelectWhere(Func<T, U> map, Func<U, bool> filter) => (Map, Filter) = (map, filter);
    }
}
