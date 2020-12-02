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
                if (_predicate(current = _enumerator.Current))
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

    struct ListFastWhereSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, bool> _predicate;
        private Func<T, U> _map;

        public ListFastWhereSelectEnumerator(List<T>.Enumerator e, Func<T, bool> predicate, Func<T, U> map) => (_enumerator, _predicate, _map) = (e, predicate, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U currentU)
        {
            while (_enumerator.MoveNext())
            {
                var currentT = _enumerator.Current;
                if (_predicate(currentT))
                {
                    currentU = _map(currentT);
                    return true;
                }
            }
            currentU = default;
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
            => ListNode.Create<T, Head, Tail, CreationType>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)ListSegmentNode.ToArray(_list, 0, _list.Count);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new ReversedListSegmentNode<T>(_list, 0, _list.Count));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.Skip(new ListSegment<T>(_list, 0, _list.Count), skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.Take(new ListSegment<T>(_list, 0, _list.Count), take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_list.Count;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => ListSegmentNode.FastEnumerate<T, TResult, FEnumerator>(new ListSegment<T>(_list, 0, _list.Count), fenum);
    }

    static class ListNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceList<T>, CreationType>(new Optimizations.SourceList<T> { List = list }, out var creation))
                return creation;

            var enumerator = new ListFastEnumerator<T>(list.GetEnumerator());
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(ref enumerator);
        }
    }
}