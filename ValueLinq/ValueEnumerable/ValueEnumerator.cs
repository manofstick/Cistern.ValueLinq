using System;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public struct ValueEnumerator<T>
        : IDisposable
    {
        private FastEnumerator<T> _enumerator;
        private T _current;

        internal ValueEnumerator(FastEnumerator<T> enumerator) => (_enumerator, _current) = (enumerator, default);

        public T Current => _current;

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.TryGetNext(out _current);

        internal FastEnumerator<T> FastEnumerator { get => _enumerator; } 
    }

    struct FastEnumeratorToValueEnumeratorNode
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> _) => throw new InvalidOperationException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
                => (CreationType)(object)(new ValueEnumerator<EnumeratorElement>(new FastEnumerator<Enumerator, EnumeratorElement>(enumerator)));

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) => throw new NotImplementedException();
    }
}
