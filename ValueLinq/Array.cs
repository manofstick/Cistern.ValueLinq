﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.ValueLinq
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
        : IValueEnumerable<T>
    {
        private readonly T[] _array;

        public ArrayNode(T[] array) => _array = array;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new ArrayFastEnumerator<T>(_array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(in enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
            => throw new InvalidOperationException();

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in this); 
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
    }
}
