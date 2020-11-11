using System;
using System.Threading;

namespace Cistern.ValueLinq.Containers
{
    struct ConcatFastEnumerator<T, Finish>
        : IFastEnumerator<T>
        where Finish : INode
    {
        private FastEnumerator<T> _current;
        private Finish _finish;
        int _state;

        public ConcatFastEnumerator(FastEnumerator<T> start, in Finish finish) => (_current, _finish, _state) = (start, finish, 0);

        public void Dispose()
        {
            if (_state < 2)
                _current.Dispose();
        }

        public bool TryGetNext(out T current)
        {
            tryAgain:
            if (_current.TryGetNext(out current))
                return true;

            if (TryGetNextFalse())
                goto tryAgain;

            return false;
        }

        private bool TryGetNextFalse()
        {
            switch (_state)
            {
                case 0:
                    _current.Dispose();
                    _current = Nodes<T>.CreateValueEnumerator(_finish).FastEnumerator;
                    _state = 1;
                    return true;

                case 1:
                    _current.Dispose();
                    _state = 2;
                    return false;

                default:
                    return false;
            }
        }
    }

    public struct ConcatNode<T, Start, Finish>
        : INode<T>
        where Start : INode<T>
        where Finish : INode<T>
    {
        public void GetCountInformation(out CountInformation info) => info = _countInfo;

        private Start _start;
        private Finish _finish;
        CountInformation _countInfo;

        public ConcatNode(in Start start, in Finish finish)
        {
            start.GetCountInformation(out _countInfo);
            finish.GetCountInformation(out var rhs);

            _countInfo.MaximumLength += rhs.MaximumLength;
            _countInfo.ActualLengthIsMaximumLength &= rhs.ActualLengthIsMaximumLength;
            _countInfo.IsImmutable &= rhs.IsImmutable;
            _countInfo.IsStale = !_countInfo.IsImmutable || !rhs.IsImmutable;

#if PROPERLY_RIPPLE_POTENTIAL_SIDE_EFFECTS
            _countInfo.PotentialSideEffects |= rhs.PotentialSideEffects;
#else
            _countInfo.PotentialSideEffects = false;
#endif

            (_start, _finish) = (start, finish);
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
                ConcatNode.Create<T, Start, Finish, Head, Tail, CreationType>(in _start, in _finish, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                var countInfo = (Optimizations.Count)(object)request;
                result = (TResult)(object)Count(countInfo.IgnorePotentialSideEffects);
                return true;
            }

            result = default;
            return false;
        }

        private readonly int Count(bool ignorePotentialSideEffects)
        {
            checked { return Enumerable.Count<T, Start>(_start, ignorePotentialSideEffects) + Enumerable.Count<T, Finish>(_finish, ignorePotentialSideEffects); }
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => _start.CreateObjectViaFastEnumerator<TResult, ConcatStartFoward<T, Finish, FEnumerator>>(new ConcatStartFoward<T, Finish, FEnumerator>(fenum, _finish));
    }

    static class ConcatNode
    {
        public static CreationType Create<T, Start, Finish, Head, Tail, CreationType>(in Start start, in Finish finish, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Start : INode
            where Finish : INode
        {
            var startEnumerator = Nodes<T>.CreateValueEnumerator(start);

            var e = new ConcatFastEnumerator<T, Finish>(startEnumerator.FastEnumerator, finish);
            return nodes.CreateObject<CreationType, T, ConcatFastEnumerator<T, Finish>>(ref e);
        }
    }

    struct ConcatFinishFoward<T, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
    {
        Next _next;

        public ConcatFinishFoward(in Next prior) => (_next) = (prior);

        public void Dispose() => _next.Dispose();

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }

    struct ConcatStartFoward<T, Finish, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
        where Finish : INode<T>
    {
        Next _next;
        Finish _finish;
        bool _disposedOrOntoFinish;

        public ConcatStartFoward(in Next prior, in Finish finish) => (_next, _finish, _disposedOrOntoFinish) = (prior, finish, false);

        public void Dispose()
        {
            if (!_disposedOrOntoFinish)
            {
                _disposedOrOntoFinish = true;
                _next.Dispose();
            }
        }

        public TResult GetResult<TResult>()
        {
            _disposedOrOntoFinish = true;
            return _finish.CreateObjectViaFastEnumerator<TResult, ConcatFinishFoward<T, Next>>(new ConcatFinishFoward<T, Next>(in _next));
        }

        public bool ProcessNext(T input) => _next.ProcessNext(input);
    }
}
