using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Nodes
{
    // TODo: Add the Default Equality version

    struct ExceptNodeEnumerator<T, TInEnumerator>
        : IFastEnumerator<T>
        where TInEnumerator : IFastEnumerator<T>
    {
        private TInEnumerator _enumerator;
        private Set<T> _set;

        public ExceptNodeEnumerator(in TInEnumerator enumerator, Set<T> set)
            => (_enumerator, _set) = (enumerator, set);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out T current)
        {
            for (;;)
            {
                if (!_enumerator.TryGetNext(out current))
                    return false;

                if (_set.Add(current))
                    return true;
            }
        }
    }

    public struct ExceptNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private IEnumerable<T> _second;
        private IEqualityComparer<T> _comparer;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public ExceptNode(in NodeT nodeT, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            (_nodeT, _second, _comparer) = (nodeT, second, comparer);
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var set = ExceptImpl.CreateSet(_comparer, _second);
                
            var nextEnumerator = new ExceptNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Set<EnumeratorElement>)(object)set);
            return tail.CreateObject<CreationType, EnumeratorElement, ExceptNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            var set = ExceptImpl.CreateSet(_comparer, _second);

            return _nodeT.CreateViaPush<TResult, ExceptFoward<T, FEnumerator>>(new ExceptFoward<T, FEnumerator>(fenum, set));
        }
    }

    static class ExceptImpl
    {
        internal static Set<T> CreateSet<T>(IEqualityComparer<T> comparer, IEnumerable<T> items)
        {
            var set = new Set<T>(comparer);

            foreach (var item in items)
                set.Add(item);

            return set;
        }
    }

    struct ExceptFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        internal Next _next;

        Set<T> _set;

        public ExceptFoward(in Next prior, Set<T> set)
            => (_next, _set) = (prior, set);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
        public bool ProcessNext(T input)
        {
            if (_set.Add(input))
                _next.ProcessNext(input);

            return true;
        }
    }
}
