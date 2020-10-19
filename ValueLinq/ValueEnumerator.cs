using System;

namespace Cistern.ValueLinq
{
    public struct ValueEnumerator<T>
    {
        private IFastEnumerator<T> _enumerator;
        private T _current;

        internal ValueEnumerator(IFastEnumerator<T> enumerator) => (_enumerator, _current) = (enumerator, default);

        public T Current => _current;

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.TryGetNext(out _current);
    }

    struct FastEnumeratorToValueEnumeratorNode
        : INode
    {
        public CreationType CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
                => throw new InvalidOperationException();

        public CreationType CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, in Enumerator enumerator)
            where Enumerator : IFastEnumerator<EnumeratorElement>
            where Tail : INodes
                => (CreationType)(object)(new ValueEnumerator<EnumeratorElement>(enumerator));
    }
}
