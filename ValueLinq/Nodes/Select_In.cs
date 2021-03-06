﻿using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct Select_InNodeEnumerator<TIn, TOut, TInEnumerator>
        : IPullEnumerator<TOut>
        where TInEnumerator : IPullEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private InFunc<TIn, TOut> _map;

        public Select_InNodeEnumerator(in TInEnumerator enumerator, InFunc<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(in currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct Select_InNode<T, U, NodeT>
        : INode<U>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;
        private InFunc<T, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public Select_InNode(in NodeT nodeT, InFunc<T, U> map) => (_nodeT, _map) = (nodeT, map);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new Select_InNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (InFunc<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, Select_InNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<U>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) =>
            _nodeT.CreateViaPush<TResult, Select_InFoward<T, U, TPushEnumerator>>(new Select_InFoward<T, U, TPushEnumerator>(fenum, _map));
    }

    struct Select_InFoward<T, U, Next>
        : IPushEnumerator<T>
        where Next : IPushEnumerator<U>
    {
        Next _next;
        InFunc<T, U> _selector;

        public Select_InFoward(in Next prior, InFunc<T, U> predicate) => (_next, _selector) = (prior, predicate);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(in input));
    }

}
