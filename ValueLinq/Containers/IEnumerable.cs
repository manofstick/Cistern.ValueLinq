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

        public EnumerableNode(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            _enumerable = source;
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _enumerable switch
            {
                T[] { Length : 0 } => EmptyNode.Create<T, Head, Tail, CreationType>(ref nodes),
                T[] array          => ArrayNode.Create<T, Head, Tail, CreationType>(array, ref nodes),
                List<T> list       => ListNode.Create<T, Head, Tail, CreationType>(list, ref nodes),
                INode node         => node.CreateObjectDescent<CreationType, Head, Tail>(ref nodes),
                _                  => EnumerableNode.Create<T, Head, Tail, CreationType>(_enumerable.GetEnumerator(), ref nodes)
            };

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_XXX<T, TOuter>))
            {
                var toListSelect = (Optimizations.ToList_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ToList(toListSelect.Map);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_XXX<T>))
            {
                var toListWhere = (Optimizations.ToList_Where_XXX<T>)(object)request;
                result = (TResult)(object)ToList(toListWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Where_Select_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Where_Select_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ToList(toListSelectWhere.Map, toListSelectWhere.Filter);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList_Select_Where_XXX<T, TOuter>))
            {
                var toListSelectWhere = (Optimizations.ToList_Select_Where_XXX<T, TOuter>)(object)request;
                result = (TResult)(object)ToList(toListSelectWhere.Filter, toListSelectWhere.Map);
                return true;
            }

            result = default;
            return false;
        }

        private readonly List<TOuter> ToList<TOuter>(Func<T, TOuter> map)
        {
            return _enumerable switch
            {
                T[] array    => ArrayNode.ToList(array, map),
                List<T> list => ListNode.ToList(list, map),
                _            => EnumerableNode.ToList(_enumerable, map),
            };
        }

        private readonly List<TOuter> ToList<TOuter>(Func<T, TOuter> map, Func<TOuter, bool> filter)
        {
            return _enumerable switch
            {
                T[] array    => ArrayNode.ToList(array, map, filter),
                List<T> list => ListNode.ToList(list, map, filter),
                _            => EnumerableNode.ToList(_enumerable, map, filter),
            };
        }

        private readonly List<TOuter> ToList<TOuter>(Func<T, bool> filter, Func<T, TOuter> map)
        {
            return _enumerable switch
            {
                T[] array    => ArrayNode.ToList(array, filter, map),
                List<T> list => ListNode.ToList(list, filter, map),
                _            => EnumerableNode.ToList(_enumerable, filter, map),
            };
        }

        private readonly List<T> ToList(Func<T, bool> filter)
        {
            return _enumerable switch
            {
                T[] array    => ArrayNode.ToList(array, filter),
                List<T> list => ListNode.ToList(list, filter),
                _            => EnumerableNode.ToList(_enumerable, filter),
            };
        }
    }

    static class EnumerableNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(IEnumerator<T> enumerator, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var e = new EnumerableFastEnumerator<T>(enumerator);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(ref e);
        }

        public static List<U> ToList<T,U>(IEnumerable<T> enumerable, Func<T, U> map)
        {
            var newList = new List<U>();
            foreach (var item in enumerable)
                newList.Add(map(item));
            return newList;
        }

        public static List<U> ToList<T, U>(IEnumerable<T> enumerable, Func<T, U> map, Func<U, bool> filter)
        {
            var newList = new List<U>();
            foreach (var item in enumerable)
            {
                var mapped = map(item);
                if (filter(mapped))
                    newList.Add(mapped);
            }
            return newList;
        }

        public static List<U> ToList<T, U>(IEnumerable<T> enumerable, Func<T, bool> filter, Func<T, U> map)
        {
            var newList = new List<U>();
            foreach (var item in enumerable)
            {
                if (filter(item))
                    newList.Add(map(item));
            }
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
    }
}
