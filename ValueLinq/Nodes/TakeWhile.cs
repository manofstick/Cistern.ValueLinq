using System;

namespace Cistern.ValueLinq.Nodes
{
    struct TakeWhileNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _predicate;
        bool _taking;

        public TakeWhileNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> predicate) => (_enumerator, _predicate, _taking) = (enumerator, predicate, true);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            if (!_taking)
            {
                current = default;
                return false;
            }

            if (_enumerator.TryGetNext(out current) && _predicate(current))
                return true;

            _taking = false;
            _enumerator.Dispose();
            return false;
        }
    }

    public struct TakeWhileNode<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private Func<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public TakeWhileNode(in NodeT nodeT, Func<T, bool> predicate) => (_nodeT, _predicate) = (nodeT, predicate);

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new TakeWhileNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_predicate);
            return tail.CreateObject<CreationType, EnumeratorElement, TakeWhileNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateViaPull<TResult, TakeWhileFoward<T, FEnumerator>>(new TakeWhileFoward<T, FEnumerator>(fenum, _predicate));
    }

    struct TakeWhileFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, bool> _predicate;
        bool _taking;

        public TakeWhileFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate, _taking) = (prior, predicate, true);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_taking && _predicate(input))
                return _next.ProcessNext(input);

            _taking = false;
            return false;
        }
    }
}
