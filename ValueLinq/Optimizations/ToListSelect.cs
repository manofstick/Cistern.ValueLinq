using System;

namespace Cistern.ValueLinq.Optimizations
{
    struct ToListSelect<T, U>
    {
        public  readonly Func<T, U> Map;
        public ToListSelect(Func<T, U> map) => Map = map;
    }
}
