using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ValueSelectNodeEnumerator<T, U, TInEnumerator, AlsoT, Func>
        : IFastEnumerator<U>
        where TInEnumerator : IFastEnumerator<T>
        where Func : IFuncBase<AlsoT, U>
    {
        private TInEnumerator _enumerator;
        private Func _map;

        public ValueSelectNodeEnumerator(in TInEnumerator enumerator, Func map) => (_enumerator, _map) = (enumerator, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out U current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                     if (_map is IFunc<T, U>)   current = ((IFunc<T, U>)  _map).Invoke(currentIn);
                else if (_map is IInFunc<T, U>) current = ((IInFunc<T, U>)_map).Invoke(in currentIn);
                else throw new NotImplementedException();

                return true;
            }

            current = default;
            return false;
        }
    }

    public struct ValueSelectNode<T, U, NodeT, Func>
        : INode<U>
        where NodeT : INode<T>
        where Func : IFuncBase<T, U>
    {
        private NodeT _nodeT;
        private Func _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public ValueSelectNode(in NodeT nodeT, Func selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new ValueSelectNodeEnumerator<EnumeratorElement, U, Enumerator, T, Func>(in enumerator, _map);
            return tail.CreateObject<CreationType, U, ValueSelectNodeEnumerator<EnumeratorElement, U, Enumerator, T, Func>>(ref nextEnumerator);
        }

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<U>.CreateViaPull<TResult, Next>(in Next fenum) =>
            _nodeT.CreateViaPull<TResult, ValueSelectFoward<T, U, Next, Func>>(new ValueSelectFoward<T, U, Next, Func>(fenum, _map));
    }

    struct ValueSelectFoward<T, U, Next, Func>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
        where Func : IFuncBase<T, U>
    {
        Next _next;
        Func _selector;

        public ValueSelectFoward(in Next prior, Func predicate) => (_next, _selector) = (prior, predicate);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            U u;

                 if (_selector is IFunc<T, U>)   u = ((IFunc<T, U>)  _selector).Invoke(input);
            else if (_selector is IInFunc<T, U>) u = ((IInFunc<T, U>)_selector).Invoke(in input);
            else throw new NotImplementedException();

            return _next.ProcessNext(u);
        }
    }
}
