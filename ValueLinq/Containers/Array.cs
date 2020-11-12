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
                return EmptyNode.Create<T, Head, Tail, CreationType>(ref nodes);

            return ArrayNode.Create<T, Head, Tail, CreationType>(_array, ref nodes);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(_array, fenum);
    }

    static class ArrayNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(T[] array, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(TIn[] array, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                Loop(array, ref fenum);

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TIn, FEnumerator>(TIn[] array, ref FEnumerator fenum)
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
