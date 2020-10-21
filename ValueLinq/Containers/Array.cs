using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ArrayFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly T[] _array;
        private int _idx;

        public ArrayFastEnumerator(T[] array) => (_array, _idx) = (array, -1);

        public int? InitialSize => _array.Length;

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _idx + 1;
            if (idx < _array.Length)
            {
                current = _array[idx];
                _idx = idx;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ArrayNode<T>
        : INode
        , IOptimizedCreateCollectionInner<T>
    {
        private readonly T[] _array;

        public ArrayNode(T[] array) => _array = array;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new ArrayFastEnumerator<T>(_array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        TOptimization INode.CheckForOptimization<TOptimization>() => null;

        List<U> IOptimizedCreateCollectionInner<T>.ToList<U>(Func<T, U> map) => EnumerableNode.ToList(_array, map);
        List<T> IOptimizedCreateCollectionInner<T>.ToList(Func<T, bool> filter) => EnumerableNode.ToList(_array, filter);
    }
}
