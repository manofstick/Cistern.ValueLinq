namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Utils;
using System;

// An array held as itself: pushed as a span where the chain takes one, else looped by index.
public static partial class SystemArray
{
    [Answerer(Forwards = true)]
    public readonly partial struct Node<TSource>
        : INode<TSource>
        , Optimizations.IOptimizationAsSpanSource<TSource>
        , Optimizations.IOptimizationElementAt<TSource>
        , Optimizations.IOptimizationAny
    {
        internal readonly TSource[] _array;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_array.Length, true);

        public Node(TSource[] array)
        {
            if (array == null)
                throw new ArgumentNullException("source");

            _array = array;
        }

        // unchecked, for an inner collection produced per element (SelectMany's selector): a null array faults at its
        // first use instead
        internal Node(TSource[] array, bool _) => _array = array;

        private bool AsSpanSource(in Optimizations.AsSpanSource request, out Optimizations.SpanSource<TSource> response)
        {
            response = Optimizations.SpanSource<TSource>.FromArray(_array);
            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, TSource) response)
        {
            var offset = request.OffsetIn(_array.Length);
            response = (offset >= 0, offset < 0 ? default! : _array[offset]);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            response = _array.Length != 0;
            return true;
        }

        // every other request is answered as the array's memory answers it
        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
            => new SystemMemory.Node<TSource>(_array).TryPushOptimization<TRequest, TRequestResult>(in request, out result);

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<TSource>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemArray.ExecutePush<TSource, TPushResult, TPushEnumerator>(_array, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<TSource>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    [Puller]
    public partial struct Pull<TSource>
    {
        private readonly TSource[] _array;
        private int _idx;
        private int _end;

        internal Pull(TSource[] array, int start, int count)
        {
            _array = array;
            _idx = start-1;
            _end = start+count;
        }

        private static Pull<TSource> Create(in Node<TSource> node)
            => new(node._array, 0, node._array.Length);

        public bool MoveNext() => ++_idx < _end;

        public TSource Current => _array[_idx];
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(TSource[] source, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            if (source == null)
                throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

            // The loop is written here rather than in a helper taking fenum by ref: a ref to fenum passed to a call
            // that is not inlined makes it address-exposed, and the JIT then keeps the whole aggregator in memory.
            //
            // Below this length the span dispatch costs more than the element loop saves. Measured on .NET 11 by building
            // the library twice, once always offering the span and once never, and timing Sum, Select.Sum, Where.Sum,
            // Min and ElementAt over arrays of 2 to 32 elements, each length in its own process; this is the length at
            // which the streaming aggregators break even. ElementAt gains from the span at every length, so it does not
            // set the value. SystemMemory.ExecutePush shares it, its span being the same shape.
            if (source.Length < 8
             || !TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
            {
                for (var i = 0; i < source.Length; ++i)
                {
                    if (!fenum.ProcessNext(source[i]))
                        break;
                }
            }
            else
            {
                var batch = new Batch<TSource>(source, BatchKinds.Container);
                _ = fenum.ProcessBatch(ref batch);
            }
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    // a segment of an array, written as the whole array's push is
    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(TSource[] source, int start, int count, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        try
        {
            var span = new ReadOnlySpan<TSource>(source, start, count);
            if (span.Length < 8
             || !TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
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
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    // a buffer a node built for this push alone (Reverse's, Shuffle's, OrderBy's), written as the array's push is; it is
    // returned to its pool once the loop ends, by exception too, before anything else is called on the chain
    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(PooledArray<TSource> source, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        var released = false;
        try
        {
            ReadOnlySpan<TSource> span = source.Span;
            if (span.Length < 8
             || !TPushEnumerator.AllowBatchesWhen.Takes(BatchKinds.Container))
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

            released = true;
            source.Return();
            return fenum.GetResult<TResult>();
        }
        finally
        {
            if (!released)
                source.Return();
            fenum.Dispose();
        }
    }

    // Skip and Take over an array or a memory push the slice themselves: the SkipTake answer's container switch fills a
    // method too large for the push's calls to be inlined into it. The type tests are constants per instantiation.
    internal static bool TryPushSlice<T, NodeT, TPushResult, TPushEnumerator, TPushFiller>(in NodeT node, long skip, long take, in TPushFiller filler, [MaybeNullWhen(false)] out TPushResult result)
        where NodeT : INode<T>
        where TPushEnumerator : IPushEnumerator<T>
        where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
    {
        if (typeof(NodeT) == typeof(Node<T>))
        {
            var array = ((Node<T>)(object)node!)._array;
            var start = (int)Math.Min(skip, array.Length);
            var count = (int)Math.Max(0, Math.Min(take, array.Length - start));
            result = ExecutePush<T, TPushResult, TPushEnumerator>(array, start, count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
            return true;
        }

        if (typeof(NodeT) == typeof(SystemMemory.Node<T>))
        {
            var memory = ((SystemMemory.Node<T>)(object)node!)._memory;
            var start = (int)Math.Min(skip, memory.Length);
            var count = (int)Math.Max(0, Math.Min(take, memory.Length - start));
            result = SystemMemory.ExecutePush<T, TPushResult, TPushEnumerator>(memory.Slice(start, count), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
            return true;
        }

        result = default;
        return false;
    }
}
