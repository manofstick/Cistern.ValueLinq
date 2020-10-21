using System;

namespace Cistern.ValueLinq.Optimizations
{
    struct ToListWhere<T>
    {
        public readonly Func<T, bool> Filter;
        public ToListWhere(Func<T, bool> filter) => Filter = filter;
    }
}
