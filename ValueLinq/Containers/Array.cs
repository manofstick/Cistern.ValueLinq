using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ArrayFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly T[] _array;
        private int _idx;

        public ArrayFastEnumerator(T[] array) => (_array, _idx) = (array, -1);

        public int? InitialSize => _array.Length;

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _idx + 1;
            if (idx < _array.Length)
            {
                current = _array[idx];
                _idx = idx;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ArrayNode<T>
        : INode
    {
        private readonly T[] _array;

        public ArrayNode(T[] array) => _array = array;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => ArrayNode.Create<T, Head, Tail, CreationType>(_array, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_XXX<T, TOuter>))
            {
                var toListSelect = (Optimizations.ToList_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ArrayNode.ToList(_array, toListSelect.Map);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_XXX<T>))
            {
                var toListWhere = (Optimizations.ToList_Where_XXX<T>)(object)request;
                result = (TResult)(object)ArrayNode.ToList(_array, toListWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_Select_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Where_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ArrayNode.ToList(_array, toListSelectWhere.Map, toListSelectWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_Where_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Select_Where_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ArrayNode.ToList(_array, toListSelectWhere.Filter, toListSelectWhere.Map);
                return true;
            }

            result = default;
            return false;
        }
    }

    static class ArrayNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(T[] array, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        public static List<U> ToList<T, U>(T[] array, Func<T, U> map)
        {
            var newList = new List<U>(array.Length);
            for (var i = 0; i < array.Length; ++i)
                newList.Add(map(array[i]));
            return newList;
        }

        public static List<U> ToList<T, U>(T[] array, Func<T, U> map, Func<U, bool> filter)
        {
            var newList = new List<U>();
            for (var i = 0; i < array.Length; ++i)
            {
                var mapped = map(array[i]);
                if (filter(mapped))
                    newList.Add(mapped);
            }
            return newList;
        }

        public static List<U> ToList<T, U>(T[] array, Func<T, bool> filter, Func<T, U> map)
        {
            var newList = new List<U>();
            for (var i = 0; i < array.Length; ++i)
            {
                if (filter(array[i]))
                    newList.Add(map(array[i]));
            }
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
    }
}
