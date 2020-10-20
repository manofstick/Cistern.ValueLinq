using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    class FastEnumeratorToEnumerator<T, TEnumerator>
        : IEnumerator<T>
        where TEnumerator : IFastEnumerator<T>
    {
        private TEnumerator _enumerator;
        private T _current;

        public FastEnumeratorToEnumerator(in TEnumerator enumerator) => (_enumerator, _current) = (enumerator, default);

        public T Current => _current;

        object System.Collections.IEnumerator.Current => _current;

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.TryGetNext(out _current);

        public void Reset() => throw new NotImplementedException();
    }

    struct FastEnumeratorToEnumeratorNode
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> _) => throw new InvalidOperationException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
                => (CreationType)(object)(new FastEnumeratorToEnumerator<EnumeratorElement, Enumerator>(in enumerator));
    }
}
