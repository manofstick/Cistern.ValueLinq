using System;

namespace Cistern.ValueLinq.Optimizations
{
    struct SelectToList<T, U>
    {
        public  readonly Func<T, U> Map;
        public SelectToList(Func<T, U> map) => Map = map;
    }
}
