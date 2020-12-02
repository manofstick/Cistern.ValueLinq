using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ArrayFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly T[] _array;
        private int _idx;
        private int _end;

        public ArrayFastEnumerator(T[] array, int start, int count) => (_array, _idx, _end) = (array, start-1, start+count);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
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

    struct ArrayFastWhereEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly T[] _array;
        private readonly Func<T, bool> _predicate;
        private int _idx;
        private int _end;

        public ArrayFastWhereEnumerator(T[] array, int start, int count, Func<T, bool> predicate) => (_array, _predicate, _idx, _end) = (array, predicate, start-1, Math.Min(array.Length, start+count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
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

    struct ArrayFastSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private readonly T[] _array;
        private readonly Func<T, U> _map;
        private int _idx;
        private int _end;

        public ArrayFastSelectEnumerator(T[] array, int start, int count, Func<T, U> map) => (_array, _map, _idx, _end) = (array, map, start - 1, Math.Min(array.Length, start + count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
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

    struct ArrayFastWhereSelectEnumerator<T, U>
        : IFastEnumerator<U>
    {
        private readonly T[] _array;
        private readonly Func<T, bool> _predicate;
        private readonly Func<T, U> _map;
        private int _idx;
        private int _end;

        public ArrayFastWhereSelectEnumerator(T[] array, int start, int count, Func<T, bool> predicate, Func<T, U> map) => (_array, _predicate, _map, _idx, _end) = (array, predicate, map, start - 1, Math.Min(array.Length, start + count));

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
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

    public struct ArrayNode<T>
        : INode<T>
    {
        private readonly T[] _array;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_array.Length, true);

        public ArrayNode(T[] array) => _array = array;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            if (_array.Length == 0)
                return EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes);

            return ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(_array, ref nodes);
        }

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        public bool CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            MemoryNode.CheckForOptimization<T, TRequest, TResult>(_array, in request, out result);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        public TResult CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
            => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(_array, fenum);
    }

    static class ArrayNode
    {
        internal static T[] ToArray<T>(T[] array)
        {
            // https://marcelltoth.net/article/41/fastest-way-to-copy-a-net-array
            if (array.Length == 0)
                return Array.Empty<T>();

            if (array.Length <= 500) // for word sized Value types; this seems to be above the crossover point
                return array.AsSpan().ToArray();

            return (T[])array.Clone();
        }

        internal static CreationType Create<T, Nodes, CreationType>(T[] array, ref Nodes nodes)
            where Nodes : INodes
        {
            var sa = new Optimizations.SourceArray<T> { Array = array, Start = 0, Count = array.Length };
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceArray<T>, CreationType>(in sa, out var creation))
                return creation;

            var enumerator = new ArrayFastEnumerator<T>(array, 0, array.Length);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(TIn[] array, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ProcessArray<TIn, FEnumerator>(array, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ProcessArray<TIn, FEnumerator>(TIn[] array, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            if (array == null)
                throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

            if (array.Length < 20 // magic number! skip small, as cost of trying for batch outweighs benefit (and no "perfect" value)
             || BatchProcessResult.Unavailable == fenum.TryProcessBatch<TIn[], GetSpan<TIn[], TIn>>(array, in Optimizations.UseSpan<TIn>.FromArray))
            {
                // Used to share Span version, but getting span isn't free (albeit cheap), and Loop logic is trivial anyway
                Loop<TIn, FEnumerator>(array, ref fenum);
            }
        }

        internal static void Loop<TIn, FEnumerator>(TIn[] array, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = 0; i < array.Length; ++i)
            {
                if (!fenum.ProcessNext(array[i]))
                    break;
            }
        }
    }
}
