using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectIdxNodeEnumerator<TIn, TOut, TInEnumerator>
        : IPullEnumerator<TOut>
        where TInEnumerator : IPullEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, int, TOut> _map;
        private int _i;

        public SelectIdxNodeEnumerator(in TInEnumerator enumerator, Func<TIn, int, TOut> map) => (_enumerator, _map, _i) = (enumerator, map, 0);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(currentIn, _i++);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct SelectIdxNode<T, U, NodeT>
        : INode<U>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, int, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public SelectIdxNode(in NodeT nodeT, Func<T, int, U> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectIdxNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, int, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectIdxNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false;}

        TResult INode<U>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => _nodeT.CreateViaPush<TResult, SelectIdxFoward<T, U, TPushEnumerator>>(new SelectIdxFoward<T, U, TPushEnumerator>(fenum, _map));
    }

    struct SelectIdxFoward<T, U, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<U>
    {
        Next _next;
        Func<T, int, U> _selector;
        int _idx;

        public SelectIdxFoward(in Next prior, Func<T, int, U> predicate) => (_next, _selector, _idx) = (prior, predicate, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(input, _idx++));
    }

}
