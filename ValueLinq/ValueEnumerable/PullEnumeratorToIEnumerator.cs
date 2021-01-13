using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    sealed class PullEnumeratorToIEnumerator<TElement, TEnumerator>
        : IEnumerator<TElement>
        where TEnumerator : struct, IPullEnumerator<TElement>
    {
        private TEnumerator _enumerator;
        private TElement _current;

        public PullEnumeratorToIEnumerator(in TEnumerator enumerator) => (_enumerator, _current) = (enumerator, default);

        public TElement Current => _current;

        object System.Collections.IEnumerator.Current => _current;

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.TryGetNext(out _current);

        public void Reset() => throw new NotSupportedException();
    }

    struct FastEnumeratorToIEnumeratorNode
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes _) 
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            if (typeof(Enumerator) == typeof(Containers.EmptyPullEnumerator<EnumeratorElement>))
                return (CreationType)(object)InstanceOfEmpty<EnumeratorElement>.AsEnumerator;

            return (CreationType)(object)(new PullEnumeratorToIEnumerator<EnumeratorElement, Enumerator>(in enumerator));
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
