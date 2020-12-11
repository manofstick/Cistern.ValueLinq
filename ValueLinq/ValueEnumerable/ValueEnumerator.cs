using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
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
    }

    struct FastEnumeratorToValueEnumeratorNode
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> _)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            if (typeof(Enumerator) == typeof(Containers.EmptyFastEnumerator<EnumeratorElement>))
                return (CreationType)(object)InstanceOfEmptyFastEnumerator<EnumeratorElement>.Instance;

            return (CreationType)(object)new FastEnumerator<Enumerator, EnumeratorElement>(enumerator);
        }
        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
