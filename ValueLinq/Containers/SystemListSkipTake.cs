namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A window over a List<T>, SystemList's answer to a Skip, Take or Reverse request: pushed as a span over the list's
// storage where the chain takes one, else looped by index, pulled by index, and narrowed in place for a further
// Skip or Take.
public static partial class SystemListSkipTake
{
    [Answerer]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationAsSpanSource<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationAny
    {
        internal readonly List<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal Node(List<T> list, int start, int count)
        {
            _list = list;
            _start = start;
            _count = count;
        }

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, false);

        // a list's span is only valid while the list is not mutated, so it is not handed to an asker that holds it
        private bool AsSpanSource(in Optimizations.AsSpanSource request, out Optimizations.SpanSource<T> response)
        {
            if (request.Stable)
            {
                response = default;

                return false;
            }

            response = Optimizations.SpanSource<T>.FromList(_list, _start, _count);
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            request.Of(_count, out var skipped, out var taken);
            if (taken == 0)
                response.SetEmpty();
            else
                response.SetNode(new Node<T>(_list, _start + skipped, taken));

            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new ReversedNode<T>(_list, _start, _count));
            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = SystemListSkipTake.ToArray(_list, _start, _count);
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var offset = request.OffsetIn(_count);
            response = (offset >= 0, offset < 0 ? default! : _list[_start + offset]);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            response = _count != 0;
            return true;
        }

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemListSkipTake.ExecutePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    [Puller]
    public partial struct Pull<T>
    {
        private readonly List<T> _list;
        private readonly int _lastIdx;
        private int _currentIdx;

        internal Pull(in Node<T> node)
        {
            _list = node._list;
            _currentIdx = node._start - 1;
            _lastIdx = node._start + node._count - 1;
        }

        private static Pull<T> Create(in Node<T> node)
            => new(in node);

        public bool MoveNext()
        {
            var idx = _currentIdx + 1;
            if (idx > _lastIdx)
                return false;
            _currentIdx = idx;
            return true;
        }

        public T Current => _list[_currentIdx];
    }

    [Answerer]
    public readonly partial struct ReversedNode<T>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
    {
        // the list is still in forward order
        internal readonly List<T> _list;
        internal readonly int _start;
        internal readonly int _count;

        internal ReversedNode(List<T> list, int start, int count)
        {
            _list = list;
            _start = start;
            _count = count;
        }

        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException("This node is only used in forward context");

        // the window is in forward order and the sequence is its reverse, so a slice from the sequence's start is one
        // from the window's end
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            request.Of(_count, out var start, out var count);
            if (count == 0)
                response.SetEmpty();
            else
                response.SetNode(new ReversedNode<T>(_list, _start + _count - start - count, count));

            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new Node<T>(_list, _start, _count));
            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = SystemListSkipTake.ToArrayReverse(_list, _start, _count);
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

            var destination = request.Span[.._count];
            AsSpan(_list, _start, _count).CopyTo(destination);
            destination.Reverse();
            return true;
        }

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemListSkipTake.ExecuteReversePush<T, TPushResult, TPushEnumerator>(_list, _start, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    private static ReadOnlySpan<T> AsSpan<T>(List<T> list, int start, int count)
    {
        var fullSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
        if (start == 0 && count == list.Count)
            return fullSpan;
        return fullSpan.Slice(start, count);
    }

    public static T[] ToArray<T>(List<T> srcList, int start, int count) =>
        SystemSpan.ToArray<T>(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(srcList).Slice(start, count));

    public static T[] ToArrayReverse<T>(List<T> srcList, int start, int count)
    {
        var array = ToArray(srcList, start, count);
        Array.Reverse(array);
        return array;
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(List<TSource> list, int start, int count, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            // The loops are written here rather than in helpers taking fenum by ref, for the reason given at
            // SystemArray.ExecutePush.
            //
            // Measured as SystemArray.ExecutePush's threshold is, over lists. The crossover comes earlier than for arrays
            // because the short loop reads through List<T>'s indexer, which bounds-checks every element, so the span
            // dispatch repays itself sooner.
            if (count < 6)
            {
                var end = start + count - 1;

                for (var i = start; i <= end; ++i)
                {
                    if (!fenum.ProcessNext(list[i]))
                        break;
                }
            }
            else
            {
                var span = AsSpan(list, start, count);
                if (!TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
                {
                    for (var i = 0; i < span.Length; ++i)
                    {
                        if (!fenum.ProcessNext(span[i]))
                            break;
                    }
                }
                else
                {
                    var batch = new Batch<TSource>(span, BatchKinds.Container);
                    _ = fenum.ProcessBatch(ref batch);
                }
            }
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    internal static TResult ExecuteReversePush<TElement, TResult, TPushEnumerator>(List<TElement> list, int start, int count, TPushEnumerator pushee)
        where TPushEnumerator : IPushEnumerator<TElement>
    {
        try
        {
            ReverseLoop<TElement, TPushEnumerator>(list, start, count, ref pushee);
            return pushee.GetResult<TResult>();
        }
        finally
        {
            pushee.Dispose();
        }
    }

    internal static void ReverseLoop<TElement, TPushEnumerator>(List<TElement> list, int start, int count, ref TPushEnumerator pushee)
        where TPushEnumerator : IPushEnumerator<TElement>
    {
        var end = start + count - 1;

        for (var i = end; i >= start; --i)
        {
            if (!pushee.ProcessNext(list[i]))
                break;
        }
    }
}
