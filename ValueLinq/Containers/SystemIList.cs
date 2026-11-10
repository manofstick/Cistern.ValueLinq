namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A window over an IList<T> that is neither an array nor a List<T>, read through its indexer rather than its
// enumerator: what such a source answers Skip, Take and element requests with, so an element is read by its index,
// and narrowed in place for a further Skip or Take.
public static partial class SystemIList
{
    [Answerer]
    internal readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationReverse<T>
    {
        internal readonly IList<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal Node(IList<T> list, int start, int count)
        {
            _list = list;
            _start = start;
            _count = count;
        }

        public void GetCountInformation(out CountInformation info) => info = new CountInformation(_count, false);

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            request.Of(_count, out var skipped, out var taken);
            Unsafe.SkipInit(out response);
            if (taken == 0)
                response.SetEmpty();
            else
                response.SetNode(new Node<T>(_list, _start + skipped, taken));

            return true;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            response = _count;
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new ReversedNode<T>(_list, _start, _count));
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var offset = request.OffsetIn(_count);
            response = (offset >= 0, offset < 0 ? default! : _list[_start + offset]);
            return true;
        }

        // the whole list into an array goes through ICollection<T>.CopyTo, as a ToArray of the list itself does;
        // anything narrower, or into a list, is read by index
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            response = _count;
            if (request.IsDryRun)
                return true;

            if (_start == 0 && _count == _list.Count && request.TryGetArray(out var array, out var offset))
                _list.CopyTo(array, offset);
            else
            {
                var span = request.Span;
                for (var i = 0; i < _count; ++i)
                    span[i] = _list[_start + i];
            }

            return true;
        }

        public TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemIList.ExecutePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    // the window read by index from its end, what the window answers a Reverse request with
    [Answerer]
    internal readonly partial struct ReversedNode<T>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
    {
        // the window is in forward order
        internal readonly IList<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal ReversedNode(IList<T> list, int start, int count)
        {
            _list = list;
            _start = start;
            _count = count;
        }

        public void GetCountInformation(out CountInformation info) => info = new CountInformation(_count, false);

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            request.Of(_count, out var skipped, out var taken);
            Unsafe.SkipInit(out response);
            if (taken == 0)
                response.SetEmpty();
            else
                response.SetNode(new ReversedNode<T>(_list, _start + _count - skipped - taken, taken));

            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new Node<T>(_list, _start, _count));
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            response = _count;
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var offset = request.OffsetIn(_count);
            response = (offset >= 0, offset < 0 ? default! : _list[_start + _count - 1 - offset]);
            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            response = _count;
            if (request.IsDryRun)
                return true;

            var span = request.Span;
            var last = _start + _count - 1;
            for (var i = 0; i < _count; ++i)
                span[i] = _list[last - i];
            return true;
        }

        public TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemIList.ExecuteReversePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    // the window read by index, as it is pushed
    [Puller]
    internal partial struct Pull<T>
    {
        private readonly IList<T> _list;
        private readonly int _end;
        private int _index;

        internal Pull(in Node<T> node)
        {
            _list = node._list;
            _index = node._start - 1;
            _end = node._start + node._count;
        }

        public bool MoveNext() => ++_index < _end;

        public T Current => _list[_index];

        private static Pull<T> Create(in Node<T> node)
            => new(in node);
    }

    internal static TResult ExecutePush<T, TResult, TPushEnumerator>(IList<T> list, int start, int count, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        try
        {
            Loop<T, TPushEnumerator>(list, start, count, ref fenum);
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void Loop<T, TPushEnumerator>(IList<T> list, int start, int count, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        var end = start + count;
        for (var i = start; i < end; ++i)
        {
            if (!fenum.ProcessNext(list[i]))
                break;
        }
    }

    internal static TResult ExecuteReversePush<T, TResult, TPushEnumerator>(IList<T> list, int start, int count, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        try
        {
            ReverseLoop<T, TPushEnumerator>(list, start, count, ref fenum);
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void ReverseLoop<T, TPushEnumerator>(IList<T> list, int start, int count, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        for (var i = start + count - 1; i >= start; --i)
        {
            if (!fenum.ProcessNext(list[i]))
                break;
        }
    }
}
