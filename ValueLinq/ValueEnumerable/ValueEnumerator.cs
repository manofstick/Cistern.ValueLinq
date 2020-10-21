using System;

namespace Cistern.ValueLinq.ValueEnumerable
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
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> _) => throw new InvalidOperationException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
                => (CreationType)(object)(new ValueEnumerator<EnumeratorElement>(enumerator));

        T INode.CheckForOptimization<T>() => null;
    }
}
