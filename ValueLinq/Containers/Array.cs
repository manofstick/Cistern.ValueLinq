using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ArrayPullEnumerator<TSource>
        : IPullEnumerator<TSource>
    {
        private readonly TSource[] _array;
        private int _idx;
        private int _end;

        public ArrayPullEnumerator(TSource[] array, int start, int count)
            => (_array, _idx, _end) = (array, start-1, start+count);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TSource current)
        {
            if (++_idx < _end)
            {
                current = _array[_idx];
                return true;
            }
            current = default;
            return false;
        }
    }

    struct ArrayWherePullEnumerator<TSource>
        : IPullEnumerator<TSource>
    {
        private readonly TSource[] _array;
        private readonly Func<TSource, bool> _predicate;
        private int _idx;
        private int _end;

        public ArrayWherePullEnumerator(TSource[] array, int start, int count, Func<TSource, bool> predicate) => (_array, _predicate, _idx, _end) = (array, predicate, start-1, Math.Min(array.Length, start+count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TSource current)
        {
            while (++_idx < _end)
            {
                current = _array[_idx];
                if (_predicate(current))
                    return true;
            }
            current = default;
            return false;
        }
    }

    struct ArraySelectPullEnumerator<TSource, TResult>
        : IPullEnumerator<TResult>
    {
        private readonly TSource[] _array;
        private readonly Func<TSource, TResult> _map;
        private int _idx;
        private int _end;

        public ArraySelectPullEnumerator(TSource[] array, int start, int count, Func<TSource, TResult> map)
            => (_array, _map, _idx, _end) = (array, map, start - 1, Math.Min(array.Length, start + count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TResult current)
        {
            if (++_idx < _end)
            {
                current = _map(_array[_idx]);
                return true;
            }
            current = default;
            return false;
        }
    }

    struct ArrayWhereSelectPullEnumerator<TSource, TResult>
        : IPullEnumerator<TResult>
    {
        private readonly TSource[] _array;
        private readonly Func<TSource, bool> _predicate;
        private readonly Func<TSource, TResult> _map;
        private int _idx;
        private int _end;

        public ArrayWhereSelectPullEnumerator(TSource[] array, int start, int count, Func<TSource, bool> predicate, Func<TSource, TResult> map) => (_array, _predicate, _map, _idx, _end) = (array, predicate, map, start - 1, Math.Min(array.Length, start + count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TResult current)
        {
            while (++_idx < _end)
            {
                if (_predicate(_array[_idx]))
                {
                    current = _map(_array[_idx]);
                    return true;
                }
            }
            current = default;
            return false;
        }
    }

    public struct ArrayNode<TSource>
        : INode<TSource>
    {
        private readonly TSource[] _array;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_array.Length, true);

        public ArrayNode(TSource[] array)
        {
            if (array == null)
                throw new ArgumentNullException("source");

            _array = array;
        }

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            if (_array.Length == 0)
                return EmptyNode.Create<TSource, TNodes, CreationType>(ref nodes);

            return ArrayNode.Create<TSource, TNodes, CreationType>(_array, ref nodes);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            MemoryNode.TryPushOptimization<TSource, TRequest, TResult>(_array, in request, out result);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<TSource>
            => ArrayNode.ExecutePush<TSource, TResult, TPushEnumerator>(_array, fenum);
    }

    static class ArrayNode
    {
        internal static CreationType Create<T, Nodes, CreationType>(T[] array, ref Nodes nodes)
            where Nodes : INodes
            => Create<T, Nodes, CreationType>(array, 0, array.Length, ref nodes);

        internal static CreationType Create<T, Nodes, CreationType>(T[] array, int start, int count, ref Nodes nodes)
            where Nodes : INodes
        {
            var sa = new Optimizations.SourceArray<T> { Array = array, Start = start, Count = count };
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceArray<T>, CreationType>(in sa, out var creation))
                return creation;

            var enumerator = new ArrayPullEnumerator<T>(array, start, count);
            return nodes.CreateObject<CreationType, T, ArrayPullEnumerator<T>>(ref enumerator);
        }

        internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(TSource[] source, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            try
            {
                ProcessArray<TSource, TPushEnumerator>(source, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ProcessArray<TSource, TPushEnumerator>(TSource[] source, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            if (source == null)
                throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

            if (source.Length < 20 // magic number! skip small, as cost of trying for batch outweighs benefit (and no "perfect" value)
             || BatchProcessResult.Unavailable == fenum.TryProcessBatch<TSource[], GetSpan<TSource[], TSource>>(source, in Optimizations.UseSpan<TSource>.FromArray))
            {
                // Used to share Span version, but getting span isn't free (albeit cheap), and Loop logic is trivial anyway
                Loop<TSource, TPushEnumerator>(source, ref fenum);
            }
        }

        internal static void Loop<TSource, TPushEnumerator>(TSource[] source, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            for (var i = 0; i < source.Length; ++i)
            {
                if (!fenum.ProcessNext(source[i]))
                    break;
            }
        }
    }
}
