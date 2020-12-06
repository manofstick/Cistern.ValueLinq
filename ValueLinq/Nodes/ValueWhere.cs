using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ValueWhereNodeEnumerator<T, TInEnumerator, AlsoT, Predicate>
        : IFastEnumerator<T>
        where TInEnumerator : IFastEnumerator<T>
        where Predicate : IFuncBase<AlsoT, bool>
    {
        private TInEnumerator _enumerator;
        private Predicate _filter;

        public ValueWhereNodeEnumerator(in TInEnumerator enumerator, Predicate filter) => (_enumerator, _filter) = (enumerator, filter);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
        {
            while (_enumerator.TryGetNext(out current))
            {
                bool filtered;

                     if (_filter is IFunc<T, bool>)   filtered = ((IFunc<T, bool>)  _filter).Invoke(current);
                else if (_filter is IInFunc<T, bool>) filtered = ((IInFunc<T, bool>)_filter).Invoke(in current);
                else throw new NotImplementedException();

                if (filtered)
                    return true;
            }
            return false;
        }
    }

    public struct ValueWhereNode<T, NodeT, Predicate>
        : INode<T>
        where NodeT : INode<T>
        where Predicate : IFuncBase<T, bool>
    {
        private NodeT _nodeT;
        private Predicate _filter;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public ValueWhereNode(in NodeT nodeT, Predicate predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>(in enumerator, _filter);
            return tail.CreateObject<CreationType, EnumeratorElement, ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>>(ref nextEnumerator);
        }

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateViaPull<TResult, ValueWhereFoward<T, FEnumerator, Predicate>>(new ValueWhereFoward<T, FEnumerator, Predicate>(fenum, _filter));
    }

    struct ValueWhereFoward<T, Next, Predicate>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
        where Predicate : IFuncBase<T, bool>
    {
        Next _next;
        Predicate _predicate;

        public ValueWhereFoward(in Next prior, Predicate predicate) => (_next, _predicate) = (prior, predicate);

        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            bool filtered;

                 if (_predicate is IFunc<T, bool>)   filtered = ((IFunc<T, bool>)  _predicate).Invoke(input);
            else if (_predicate is IInFunc<T, bool>) filtered = ((IInFunc<T, bool>)_predicate).Invoke(in input);
            else throw new NotImplementedException();

            if (filtered)
                return _next.ProcessNext(input);
            return true;
        }

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;

                var span = getSpan(obj);

                if (_predicate is IFunc<T, bool>)   return ProcessBatch(span);
                if (_predicate is IInFunc<T, bool>) return ProcessBatchRef(span);

                throw new NotImplementedException();
            }
            return BatchProcessResult.Unavailable;
        }

        private BatchProcessResult ProcessBatch(ReadOnlySpan<T> data)
        {
            for (var i=0; i < data.Length; ++i)
            {
                if (((IFunc<T, bool>)_predicate).Invoke(data[i]))
                    if (!_next.ProcessNext(data[i]))
                        return BatchProcessResult.SuccessAndHalt;
            }
            return BatchProcessResult.SuccessAndContinue;
        }

        private BatchProcessResult ProcessBatchRef(ReadOnlySpan<T> data)
        {
            for (var i = 0; i < data.Length; ++i)
            {
                if (((IInFunc<T, bool>)_predicate).Invoke(in data[i]))
                    if (!_next.ProcessNext(data[i]))
                        return BatchProcessResult.SuccessAndHalt;
            }
            return BatchProcessResult.SuccessAndContinue;
        }
    }
}
