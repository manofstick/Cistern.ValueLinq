using Cistern.ValueLinq.Aggregation;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    sealed class FastEnumeratorToEnumerator<T, TEnumerator>
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

        public void Reset() => throw new NotSupportedException();
    }

    struct FastEnumeratorToEnumeratorNode
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> _) 
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
                => (CreationType)(object)(new FastEnumeratorToEnumerator<EnumeratorElement, Enumerator>(in enumerator));

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
