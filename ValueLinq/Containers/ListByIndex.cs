using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListByIndexFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly List<T> _list;
        private int _idx;

        public ListByIndexFastEnumerator(List<T> list) => (_list, _idx) = (list, -1);

        public (bool, int)? InitialSize => (true, _list.Count);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _idx + 1;
            if (idx >= _list.Count)
            {
                current = default;
                return false;
            }
            current = _list[idx];
            _idx = idx;
            return true;
        }
    }

    public struct ListByIndexNode<T>
        : INode
    {
        private readonly List<T> _list;

        public ListByIndexNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => ListByIndexNode.Create<T, Head, Tail, CreationType>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_XXX<T, TOuter>))
            {
                var toListSelect = (Optimizations.ToList_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ListByIndexNode.ToList(_list, toListSelect.Map);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_XXX<T>))
            {
                var toListWhere = (Optimizations.ToList_Where_XXX<T>)(object)request;
                result = (TResult)(object)ListByIndexNode.ToList(_list, toListWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_Select_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Where_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ListByIndexNode.ToList(_list, toListSelectWhere.Map, toListSelectWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_Where_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Select_Where_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ListByIndexNode.ToList(_list, toListSelectWhere.Filter, toListSelectWhere.Map);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) 
            => ListByIndexNode.FastEnumerate<TIn, TResult, FEnumerator>((List<TIn>)(object)_list, fenum);
    }

    static class ListByIndexNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListByIndexFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListByIndexFastEnumerator<T>>(ref enumerator);
        }

        public static List<U> ToList<T, U>(List<T> list, Func<T, U> map)
        {
            var newList = new List<U>(list.Count);
            for (var i = 0; i < list.Count; ++i)
                newList.Add(map(list[i]));
            return newList;
        }
        public static List<U> ToList<T, U>(List<T> list, Func<T, U> map, Func<U, bool> filter)
        {
            var newList = new List<U>();
            for (var i = 0; i < list.Count; ++i)
            {
                var mapped = map(list[i]);
                if (filter(mapped))
                    newList.Add(mapped);
            }
            return newList;
        }

        public static List<U> ToList<T, U>(List<T> list, Func<T, bool> filter, Func<T, U> map)
        {
            var newList = new List<U>();
            for (var i = 0; i < list.Count; ++i)
            {
                var item = list[i];
                if (filter(item))
                    newList.Add(map(item));
            }
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

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(List<TIn> list, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            fenum.Init(list.Count);
            DoLoop(list, ref fenum);
            return fenum.GetResult<TResult>();
        }

        private static void DoLoop<TIn, FEnumerator>(List<TIn> list, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (!fenum.ProcessNext(list[i]))
                    break;
            }
        }
    }

}
