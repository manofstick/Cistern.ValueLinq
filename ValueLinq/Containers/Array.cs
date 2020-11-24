using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ArrayFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly T[] _array;
        private int _idx;

        public ArrayFastEnumerator(T[] array) => (_array, _idx) = (array, -1);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
        {
            if (++_idx < _array.Length)
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

        public ArrayFastWhereEnumerator(T[] array, Func<T, bool> predicate) => (_array, _predicate, _idx) = (array, predicate, -1);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
        {
            while (++_idx < _array.Length)
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

        public ArrayFastSelectEnumerator(T[] array, Func<T, U> map) => (_array, _map, _idx) = (array, map, -1);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
        {
            if (++_idx < _array.Length)
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

        public ArrayFastWhereSelectEnumerator(T[] array, Func<T, bool> predicate, Func<T, U> map) => (_array, _predicate, _map, _idx) = (array, predicate, map, -1);

        public void Dispose() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
        {
            while (++_idx < _array.Length)
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

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                result = (TResult)(object)new ReversedArrayNode<T>(_array);
                return true;
            }

            result = default;
            return false;
        }


        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(_array, fenum);
    }

    public struct ReversedArrayNode<T>
        : INode<T>
    {
        private readonly T[] _array;

        public ReversedArrayNode(T[] array) => _array = array;

#region "This node is only used in forward context, so most of interface is not supported"
        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException();
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotSupportedException();
        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result) => throw new NotSupportedException();
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
#endregion

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => ArrayNode.FastReverseEnumerate<T, TResult, FEnumerator>(_array, fenum);
    }


    static class ArrayNode
    {
        public static CreationType Create<T, Nodes, CreationType>(T[] array, ref Nodes nodes)
            where Nodes : INodes
        {
            if (nodes.TryObjectAscentOptimization<Optimizations.SourceArray<T>, CreationType>(0, new Optimizations.SourceArray<T> { Array = array }, out var creation))
                return creation;

            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(0, ref enumerator);
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

        internal static TResult FastReverseEnumerate<TIn, TResult, FEnumerator>(TIn[] array, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ReverseLoop<TIn, FEnumerator>(array, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ReverseLoop<TIn, FEnumerator>(TIn[] array, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = array.Length-1; i >= 0; --i)
            {
                if (!fenum.ProcessNext(array[i]))
                    break;
            }
        }
    }
}
