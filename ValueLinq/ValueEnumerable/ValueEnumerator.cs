using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using System;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public struct ValueEnumerator<T>
        : IDisposable
    {
        private PullEnumerator<T> _enumerator;
        private T _current;

        internal ValueEnumerator(PullEnumerator<T> enumerator) => (_enumerator, _current) = (enumerator, default);

        public T Current => _current;

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.TryGetNext(out _current);
    }

    struct PullEnumeratorToValueEnumeratorNode
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes _)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            if (typeof(Enumerator) == typeof(Containers.EmptyPullEnumerator<EnumeratorElement>))
                return (CreationType)(object)InstanceOfEmptyPullEnumerator<EnumeratorElement>.Instance;

            return (CreationType)(object)new PullEnumerator<Enumerator, EnumeratorElement>(enumerator);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
