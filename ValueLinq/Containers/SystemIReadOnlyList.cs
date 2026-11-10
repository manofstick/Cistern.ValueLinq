namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A window over an IReadOnlyList<T> that is neither an array nor a List<T>, read through its indexer rather than its
// enumerator: what such a source answers Skip, Take and element requests with, so an element is read by its index,
// and narrowed in place for a further Skip or Take.
public static partial class SystemIReadOnlyList
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
        internal readonly IReadOnlyList<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal Node(IReadOnlyList<T> list, int start, int count)
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

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            response = _count;
            if (request.IsDryRun)
                return true;

            var span = request.Span;
            for (var i = 0; i < _count; ++i)
                span[i] = _list[_start + i];
            return true;
        }

        public TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemIReadOnlyList.ExecutePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
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
        internal readonly IReadOnlyList<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal ReversedNode(IReadOnlyList<T> list, int start, int count)
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
            => SystemIReadOnlyList.ExecuteReversePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    internal static TResult ExecutePush<T, TResult, TPushEnumerator>(IReadOnlyList<T> list, int start, int count, TPushEnumerator fenum)
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

    private static void Loop<T, TPushEnumerator>(IReadOnlyList<T> list, int start, int count, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        var end = start + count;
        for (var i = start; i < end; ++i)
        {
            if (!fenum.ProcessNext(list[i]))
                break;
        }
    }

    internal static TResult ExecuteReversePush<T, TResult, TPushEnumerator>(IReadOnlyList<T> list, int start, int count, TPushEnumerator fenum)
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

    private static void ReverseLoop<T, TPushEnumerator>(IReadOnlyList<T> list, int start, int count, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        for (var i = start + count - 1; i >= start; --i)
        {
            if (!fenum.ProcessNext(list[i]))
                break;
        }
    }
}
