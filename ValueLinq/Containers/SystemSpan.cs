namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Runtime.CompilerServices;

// A span fetched from an object each time it is needed, as a span cannot be held in a node: pushed whole where the
// chain takes one, else looped by index.
public static partial class SystemSpan
{
    public delegate ReadOnlySpan<TElement> GetSpan<TObject, TElement>(TObject obj);

    [Answerer]
    public partial struct Node<TObject, TElement>
        : INode<TElement>
        , Optimizations.IOptimizationToArray<TElement>
        , Optimizations.IOptimizationReverse<TElement>
        , Optimizations.IOptimizationSkipTake<TElement>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationCopyTo<TElement>
    {
        internal TObject _obj;
        internal readonly GetSpan<TObject, TElement> _getSpan;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_getSpan(_obj).Length, true);

        public Node(TObject obj, GetSpan<TObject, TElement> getSpan)
        {
            _obj = obj;
            _getSpan = getSpan;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out TElement[] response)
        {
            response = SystemSpan.ToArray(_getSpan(_obj));
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<TElement> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new ReversedNode<TObject, TElement>(_obj, _getSpan));
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<TElement> response)
        {
            Unsafe.SkipInit(out response);
            return SystemSpan.MaybeSlice(_obj, _getSpan, in request, ref response);
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            response = _getSpan(_obj).Length;
            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<TElement> request, out int response)
        {
            var source = _getSpan(_obj);
            response = source.Length;
            if (!request.IsDryRun)
                source.CopyTo(request.Span);

            return true;
        }

        TPushResult INode<TElement>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => SystemSpan.ExecutePush<TElement, TPushResult, TPushEnumerator, TObject>(_obj, _getSpan, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    public partial struct Pull<TObject, TElement>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, TElement> _getSpan;
        private int _idx;

        internal Pull(TObject obj, GetSpan<TObject, TElement> getSpan)
        {
            _obj = obj;
            _getSpan = getSpan;
            _idx = -1;
        }

        private static Pull<TObject, TElement> Create(in Node<TObject, TElement> node)
            => new(node._obj, node._getSpan);

        // kept from MoveNext, which has the span in hand, rather than fetching the span a second time
        [AllowNull] private TElement _current;

        public bool MoveNext()
        {
            var idx = _idx + 1;
            var span = _getSpan(_obj);
            if (idx < span.Length)
            {
                _current = span[idx];
                _idx = idx;
                return true;
            }
            return false;
        }

        public TElement Current => _current;
    }

    [Answerer]
    public partial struct ReversedNode<TObject, T>
        : INode<T>
        , Optimizations.IOptimizationCopyTo<T>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, T> _getSpan;


        public ReversedNode(TObject obj, GetSpan<TObject, T> getSpan)
        {
            _obj = obj;
            _getSpan = getSpan;
        }

#region "This node is only used in forward context, so most of interface is not supported"
        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException();
        #endregion

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            var source = _getSpan(_obj);
            response = source.Length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span[..source.Length];
            source.CopyTo(destination);
            destination.Reverse();
            return true;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => SystemSpan.ExecuteReversePush<T, TPushResult, TPushEnumerator>(_getSpan(_obj), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    internal static T[] ToArray<T>(ReadOnlySpan<T> span)
    {
        if (span.Length == 0)
            return Array.Empty<T>();

        // span.ToArray() zeroes the array it then copies over
        var array = GC.AllocateUninitializedArray<T>(span.Length);
        span.CopyTo(array);
        return array;
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator, TObject>(TObject obj, GetSpan<TObject, TSource> getSpan, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            var span = getSpan(obj);
            if (!TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
                Loop(span, ref fenum);
            else
            {
                var batch = new Batch<TSource>(span, BatchKinds.Container);
                _ = fenum.ProcessBatch(ref batch);
            }

            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    internal static void Loop<TSource, TPushEnumerator>(ReadOnlySpan<TSource> span, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        for (var i = 0; i < span.Length; ++i)
        {
            if (!fenum.ProcessNext(span[i]))
                break;
        }
    }

    internal static TResult ExecuteReversePush<TSource, TResult, FEnumerator>(ReadOnlySpan<TSource> span, FEnumerator fenum)
        where FEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            ReverseLoop<TSource, FEnumerator>(span, ref fenum);
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    internal static void ReverseLoop<TSource, TPushEnumerator>(ReadOnlySpan<TSource> span, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        for (var i = span.Length - 1; i >= 0; --i)
        {
            if (!fenum.ProcessNext(span[i]))
                break;
        }
    }

    internal static bool MaybeSlice<TObject, TElement>(TObject obj, GetSpan<TObject, TElement> getSpan, in Optimizations.SkipTake slice, ref NodeContainer<TElement> container)
    {
        var span = getSpan(obj);
        slice.Of(span.Length, out var start, out var count);
        if (count == 0)
        {
            container.SetEmpty();
            return true;
        }

        if (start == 0 && count == span.Length)
        {
            container.SetNode(new Node<TObject, TElement>(obj, getSpan));
            return true;
        }

        if (span.Length < 100) // magic number; weighing up between skipping the skip and creating some garbage. What's good??
            return false;

        container.SetNode(new Node<TObject, TElement>(obj, o => getSpan(o).Slice(start, count)));
        return true;
    }
}
