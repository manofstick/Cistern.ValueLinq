﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    struct EnumerableFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public EnumerableFastEnumerator(IEnumerator<T> enumerator) => _enumerator = enumerator;

        public int? InitialSize => null;

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (_enumerator.MoveNext())
            {
                current = _enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct EnumerableNode<T>
        : IValueEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public EnumerableNode(IEnumerable<T> enumerable) => _enumerable = enumerable;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            _enumerable switch
            {
                List<T> list  => AsList<Head, Tail, CreationType>(list, ref nodes),
                T[]     array => AsArray<Head, Tail, CreationType>(array, ref nodes),
                _             => AsEnumerator<Head, Tail, CreationType>(_enumerable.GetEnumerator(), ref nodes)
            };

        private static CreationType AsArray<Head, Tail, CreationType>(T[] array, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(ref enumerator);
        }

        private static CreationType AsEnumerator<Head, Tail, CreationType>(IEnumerator<T> enumerator, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var e = new EnumerableFastEnumerator<T>(enumerator);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(ref e);
        }

        private static CreationType AsList<Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _enumerable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _enumerable.GetEnumerator();
    }
}
