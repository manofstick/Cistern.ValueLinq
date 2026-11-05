namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

// A List<T> held whole: its count is read when asked, so a list grown since the node was made is seen at its new
// length; pushed as a span over its storage, and a Skip, Take or Reverse request is answered with a SystemListSkipTake
// window over it. Pulled through List<T>.Enumerator, so a change to the list during enumeration is rejected as the
// list itself rejects it, or by index under USE_LIST_BY_INDEX.
public static partial class SystemList
{
    [Answerer(Forwards = true)]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationAny
    {
        internal readonly List<T> _list;

        public Node(List<T> list) => _list = list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var offset = request.OffsetIn(_list.Count);
            response = (offset >= 0, offset < 0 ? default! : _list[offset]);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            response = _list.Count != 0;
            return true;
        }

        // every other request is answered by a window over the whole list at its count now
        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
            => new SystemListSkipTake.Node<T>(_list, 0, _list.Count).TryPushOptimization<TRequest, TRequestResult>(in request, out result);

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemList.ExecutePush<T, TPushResult, TPushEnumerator>(_list, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    [Puller]
    public partial struct Pull<T>
    {
#if USE_LIST_BY_INDEX
        private readonly List<T> _list;
        private readonly int _lastIdx;
        private int _currentIdx;

        internal Pull(in Node<T> node)
        {
            _list = node._list;
            _currentIdx = -1;
            _lastIdx = _list.Count - 1;
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            var idx = _currentIdx + 1;
            if (idx > _lastIdx)
                return false;
            _currentIdx = idx;
            return true;
        }

        public T Current => _list[_currentIdx];
#else
        private List<T>.Enumerator _enumerator;

        internal Pull(in Node<T> node) => _enumerator = node._list.GetEnumerator();

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.MoveNext();

        public T Current => _enumerator.Current;
#endif

        private static Pull<T> Create(in Node<T> node)
            => new(in node);
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(List<TSource> list, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        if (list == null)
            return SystemIEnumerable.NullSource<TSource, TResult, TPushEnumerator>(fenum);

        return SystemListSkipTake.ExecutePush<TSource, TResult, TPushEnumerator>(list, 0, list.Count, fenum);
    }
}
