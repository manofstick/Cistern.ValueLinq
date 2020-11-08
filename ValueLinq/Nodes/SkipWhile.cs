using System;

namespace Cistern.ValueLinq.Nodes
{
    struct SkipWhileNodeEnumerator<TIn, TInEnumerator>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, bool> _predicate;
        bool _skipping;

        public SkipWhileNodeEnumerator(in TInEnumerator enumerator, Func<TIn, bool> predicate) => (_enumerator, _predicate, _skipping) = (enumerator, predicate, true);

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while(_skipping)
            {
                if (!_enumerator.TryGetNext(out current))
                    goto dispose;

                if (!_predicate(current))
                {
                    _skipping = false;
                    return true;
                }
            }

            if (_enumerator.TryGetNext(out current))
                return true;

            dispose:
            _enumerator.Dispose();
            return false;
        }
    }

    public struct SkipWhileNode<T, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public SkipWhileNode(in NodeT nodeT, Func<T, bool> predicate) => (_nodeT, _predicate) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SkipWhileNodeEnumerator<EnumeratorElement, Enumerator>(in enumerator, (Func<EnumeratorElement, bool>)(object)_predicate);
            return tail.CreateObject<CreationType, EnumeratorElement, SkipWhileNodeEnumerator<EnumeratorElement, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TIn, TResult, SkipWhileFoward<TIn, FEnumerator>>(new SkipWhileFoward<TIn, FEnumerator>(fenum, (Func<TIn, bool>)(object)_predicate));
    }

    struct SkipWhileFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;
        Func<T, bool> _predicate;
        bool _skipping;

        public SkipWhileFoward(in Next prior, Func<T, bool> predicate) => (_next, _predicate, _skipping) = (prior, predicate, true);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input)
        {
            if (_skipping)
            {
                if (_predicate(input))
                    return true;
                _skipping = false;
            }
            return _next.ProcessNext(input);
        }
    }
}
