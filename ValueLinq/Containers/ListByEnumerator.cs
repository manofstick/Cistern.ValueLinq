using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListByEnumeratorFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        private readonly int _count;

        public ListByEnumeratorFastEnumerator(List<T> list) => (_enumerator, _count) = (list.GetEnumerator(), list.Count);

        public (bool, int)? InitialSize => (true, _count);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (!_enumerator.MoveNext())
            {
                current = default;
                return false;
            }
            current = _enumerator.Current;
            return true;
        }
    }

    public struct ListByEnumeratorNode<T>
        : INode
    {
        private readonly List<T> _list;

        public ListByEnumeratorNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => ListByEnumeratorNode.Create<T, Head, Tail, CreationType>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            // To me these are somewhat dubious, because we're using the List<T>.Enumerator to track changes to the collection
            // yet we're using the indexer here. Anyway, it's the same in System.Linq's version, so there you go

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
    }

    static class ListByEnumeratorNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListByEnumeratorFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListByEnumeratorFastEnumerator<T>>(ref enumerator);
        }
    }
}