using System;

namespace Cistern.ValueLinq.Nodes
{
    struct TakeWhileIdxNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, int, bool> _predicate;
        bool _taking;
        int _idx;

        public TakeWhileIdxNodeEnumerator(in TInEnumerator enumerator, Func<TIn, int, bool> predicate) => (_enumerator, _predicate, _taking, _idx) = (enumerator, predicate, true, 0);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            if (!_taking)
            {
                current = default;
                return false;
            }

            if (_enumerator.TryGetNext(out current) && _predicate(current, _idx++))
                return true;

            _taking = false;
            _enumerator.Dispose();
            return false;
        }
    }

    public struct TakeWhileIdxNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, int, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public TakeWhileIdxNode(in NodeT nodeT, Func<T, int, bool> predicate) => (_nodeT, _predicate) = (nodeT, predicate);

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new TakeWhileIdxNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, int, bool>)(object)_predicate);
            return tail.CreateObject<CreationType, EnumeratorElement, TakeWhileIdxNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateViaPull<TResult, TakeWhileIdxFoward<T, FEnumerator>>(new TakeWhileIdxFoward<T, FEnumerator>(fenum, _predicate));
    }

    struct TakeWhileIdxFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, int, bool> _predicate;
        bool _taking;
        int _idx;

        public TakeWhileIdxFoward(in Next prior, Func<T, int, bool> predicate) => (_next, _predicate, _taking, _idx) = (prior, predicate, true, 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_taking && _predicate(input, _idx++))
                return _next.ProcessNext(input);

            _taking = false;
            return false;
        }
    }
}
