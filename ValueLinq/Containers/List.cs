using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ListPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        
        public ListPullEnumerator(List<T>.Enumerator e) => (_enumerator) = (e);

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

    struct ListWherePullEnumerator<T>
        : IPullEnumerator<T>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, bool> _predicate;

        public ListWherePullEnumerator(List<T>.Enumerator e, Func<T, bool> predicate) => (_enumerator, _predicate) = (e, predicate);

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

    struct ListSelectPullEnumerator<T, U>
        : IPullEnumerator<U>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, U> _map;

        public ListSelectPullEnumerator(List<T>.Enumerator e, Func<T, U> map) => (_enumerator, _map) = (e, map);

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

    struct ListWhereSelectPullEnumerator<T, U>
        : IPullEnumerator<U>
    {
        private List<T>.Enumerator _enumerator;
        private Func<T, bool> _predicate;
        private Func<T, U> _map;

        public ListWhereSelectPullEnumerator(List<T>.Enumerator e, Func<T, bool> predicate, Func<T, U> map) => (_enumerator, _predicate, _map) = (e, predicate, map);

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

    public struct ListNode<TSource>
        : INode<TSource>
    {
        private readonly List<TSource> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListNode(List<TSource> list) => _list = list;

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => ListNode.Create<TSource, TNodes, CreationType>(_list, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => ListSegmentNode.CheckForOptimization<TSource, TRequest, TResult>(new ListSegment<TSource>(_list, 0, _list.Count), in request, out result);

        TResult INode<TSource>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => ListSegmentNode.ExecutePush<TSource, TResult, TPushEnumerator>(new ListSegment<TSource>(_list, 0, _list.Count), fenum);
    }

    static class ListNode
    {
        public static CreationType Create<T, TNodes, CreationType>(List<T> list, ref TNodes nodes)
            where TNodes : INodes
        {
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceList<T>, CreationType>(new Optimizations.SourceList<T> { List = list }, out var creation))
                return creation;

            var enumerator = new ListPullEnumerator<T>(list.GetEnumerator());
            return nodes.CreateObject<CreationType, T, ListPullEnumerator<T>>(ref enumerator);
        }
    }
}