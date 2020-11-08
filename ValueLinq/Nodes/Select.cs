using System;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct SelectNodeEnumerator<TIn, TOut, TInEnumerator>
        : IFastEnumerator<TOut>
        where TInEnumerator : IFastEnumerator<TIn>
    {
        private TInEnumerator _enumerator;
        private Func<TIn, TOut> _map;

        public SelectNodeEnumerator(in TInEnumerator enumerator, Func<TIn, TOut> map) => (_enumerator, _map) = (enumerator, map);

        public (bool, int)? InitialSize => 
            _enumerator.InitialSize switch
            {
                null => null,
                (_, var size) => (false, size)
            };

        public void Dispose() => _enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out TOut current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map(currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct SelectNode<T, U, NodeT>
        : INode
        where NodeT : INode
    {
        private NodeT _nodeT;
        private Func<T, U> _map;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        public SelectNode(in NodeT nodeT, Func<T, U> selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new SelectNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, (Func<EnumeratorElement, U>)(object)_map);
            return tail.CreateObject<CreationType, U, SelectNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<T, TResult, SelectFoward<T, TIn, FEnumerator>>(new SelectFoward<T, TIn, FEnumerator>(fenum, (Func<T, TIn>)(object)_map));
    }

    struct SelectFoward<T, U, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
    {
        Next _next;
        Func<T, U> _selector;

        public SelectFoward(in Next prior, Func<T, U> predicate) => (_next, _selector) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(input));
    }

}
