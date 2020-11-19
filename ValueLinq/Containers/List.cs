using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ListFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        
        public ListFastEnumerator(List<T>.Enumerator e) => (_enumerator) = (e);

        public void Dispose() =>_enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    struct ListFastWhereEnumerator<T>
        : IFastEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, bool> _predicate;

        public ListFastWhereEnumerator(List<T>.Enumerator e, Func<T, bool> predicate) => (_enumerator, _predicate) = (e, predicate);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
        {
            while (_enumerator.MoveNext())
            {
                if (_predicate(current = _enumerator.Current)) ;
                return true;
            }
            current = default;
            return false;
        }
    }

    struct ListFastSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, U> _map;

        public ListFastSelectEnumerator(List<T>.Enumerator e, Func<T, U> map) => (_enumerator, _map) = (e, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
        {
            if (_enumerator.MoveNext())
            {
                current = _map(_enumerator.Current);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ListNode<T>
        : INode<T>
    {
        private readonly List<T> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => ListNode.Create<T, Head, Tail, CreationType>(_list.GetEnumerator(), ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_list);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => ListByIndexNode.FastEnumerate<T, TResult, FEnumerator>(_list, fenum);
    }

    static class ListNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(List<T>.Enumerator list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(ref enumerator);
        }

        public static CreationType Create<T, Head, Tail, CreationType>(List<T>.Enumerator list, Func<T, bool> predicate, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastWhereEnumerator<T>(list, predicate);
            return nodes.CreateObject<CreationType, T, ListFastWhereEnumerator<T>>(ref enumerator);
        }

        public static CreationType Create<T, U, Head, Tail, CreationType>(List<T>.Enumerator list, Func<T, U> map, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastSelectEnumerator<T, U>(list, map);
            return nodes.CreateObject<CreationType, U, ListFastSelectEnumerator<T, U>>(ref enumerator);
        }
    }
}