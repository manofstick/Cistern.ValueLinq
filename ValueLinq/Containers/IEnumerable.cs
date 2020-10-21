using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct EnumerableFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public EnumerableFastEnumerator(IEnumerator<T> enumerator) => _enumerator = enumerator;

        public int? InitialSize => null;

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (_enumerator.MoveNext())
            {
                current = _enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct EnumerableNode<T>
        : INode
    {
        private readonly IEnumerable<T> _enumerable;

        public EnumerableNode(IEnumerable<T> enumerable) => _enumerable = enumerable;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _enumerable switch
            {
                List<T> list => EnumerableNode.AsList<T, Head, Tail, CreationType>(list, ref nodes),
                T[] array    => EnumerableNode.AsArray<T, Head, Tail, CreationType>(array, ref nodes),
                _            => EnumerableNode.AsEnumerator<T, Head, Tail, CreationType>(_enumerable.GetEnumerator(), ref nodes)
            };

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToListSelect<T, TOuter>))
            {
                var toListSelect = (Optimizations.ToListSelect<T, TOuter>)(object)request;
                result = (TResult)(object)ToList(toListSelect.Map);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToListWhere<T>))
            {
                var toListWhere = (Optimizations.ToListWhere<T>)(object)request;
                result = (TResult)(object)ToList(toListWhere.Filter);
                return true;
            }

            result = default;
            return false;
        }

        private readonly List<TOuter> ToList<TOuter>(Func<T, TOuter> map)
        {
            return _enumerable switch
            {
                List<T> list => EnumerableNode.ToList(list, map),
                T[] array    => EnumerableNode.ToList(array, map),
                _            => EnumerableNode.ToList(_enumerable, map),
            };
        }

        private readonly List<T> ToList(Func<T, bool> filter)
        {
            return _enumerable switch
            {
                List<T> list => EnumerableNode.ToList(list, filter),
                T[] array    => EnumerableNode.ToList(array, filter),
                _            => EnumerableNode.ToList(_enumerable, filter),
            };
        }
    }

    static class EnumerableNode
    {
        public static CreationType AsArray<T, Head, Tail, CreationType>(T[] array, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        public static CreationType AsEnumerator<T, Head, Tail, CreationType>(IEnumerator<T> enumerator, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var e = new EnumerableFastEnumerator<T>(enumerator);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(ref e);
        }

        public static CreationType AsList<T, Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(ref enumerator);
        }


        public static List<U> ToList<T,U>(IEnumerable<T> enumerable, Func<T, U> map)
        {
            var newList = new List<U>();
            foreach (var item in enumerable)
                newList.Add(map(item));
            return newList;
        }

        public static List<U> ToList<T, U>(T[] array, Func<T, U> map)
        {
            var newList = new List<U>(array.Length);
            for(var i=0; i < array.Length; ++i)
                newList.Add(map(array[i]));
            return newList;
        }

        public static List<U> ToList<T, U>(List<T> list, Func<T, U> map)
        {
            var newList = new List<U>(list.Count);
            for(var i=0; i < list.Count; ++i)
                newList.Add(map(list[i]));
            return newList;
        }

        public static List<T> ToList<T>(IEnumerable<T> enumerable, Func<T, bool> map)
        {
            var newList = new List<T>();
            foreach (var item in enumerable)
                if (map(item))
                    newList.Add(item);
            return newList;
        }

        public static List<T> ToList<T>(T[] array, Func<T, bool> map)
        {
            var newList = new List<T>();
            foreach (var item in array)
                if (map(item))
                    newList.Add(item);
            return newList;
        }

        public static List<T> ToList<T>(List<T> list, Func<T, bool> map)
        {
            var newList = new List<T>();
            for (var i = 0; i < list.Count; ++i)
                if (map(list[i]))
                    newList.Add(list[i]);
            return newList;
        }
    }
}
