using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Optimizations
{
    struct SourceArray<T>
    {
        public T[] Array;
        public int Start;
        public int Count;
    }

    struct SourceArrayWhere<T>
    {
        public T[] Array;
        public int Start;
        public int Count;
        public Func<T, bool> Predicate;
    }

    struct SourceList<T>
    {
        public List<T> List;
    }
    struct SourceListWhere<T>
    {
        public List<T> List;
        public Func<T, bool> Predicate;
    }

    struct SourceEnumerable<T>
    {
        public IEnumerable<T> Enumerable;
    }
    struct SourceEnumerableWhere<T>
    {
        public IEnumerable<T> Enumerable;
        public Func<T, bool> Predicate;
    }


    struct CheckForWhere { }
}
