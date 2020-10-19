using System;
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
            if (!_enumerator.MoveNext())
            {
                current = default;
                return false;
            }
            current = _enumerator.Current;
            return true;
        }
    }

    public struct EnumerableNode<T>
        : IValueEnumerable<T>
    {
        private readonly IEnumerable<T> _enumerable;

        public EnumerableNode(IEnumerable<T> enumerable) => _enumerable = enumerable;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            if (_enumerable is List<T> list)
            {
                return AsList<Head, Tail, CreationType>(list, ref nodes);
            }
            else if (_enumerable is T[] array)
            {
                return AsArray<Head, Tail, CreationType>(array, ref nodes);
            }
            else
            {
                return AsEnumerator<Head, Tail, CreationType>(_enumerable.GetEnumerator(), ref nodes);
            }
        }

        private static CreationType AsArray<Head, Tail, CreationType>(T[] array, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ArrayFastEnumerator<T>(array);
            return nodes.CreateObject<CreationType, T, ArrayFastEnumerator<T>>(in enumerator);
        }

        private static CreationType AsEnumerator<Head, Tail, CreationType>(IEnumerator<T> enumerator, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var e = new EnumerableFastEnumerator<T>(enumerator);
            return nodes.CreateObject<CreationType, T, EnumerableFastEnumerator<T>>(in e);
        }

        private static CreationType AsList<Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(in enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
            => throw new InvalidOperationException();

        public IEnumerator<T> GetEnumerator() => _enumerable.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _enumerable.GetEnumerator();
    }
}
