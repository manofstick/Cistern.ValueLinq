using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ValueSelectNodeEnumerator<T, U, TInEnumerator, AlsoT, Func>
        : IFastEnumerator<U>
        where TInEnumerator : IFastEnumerator<T>
        where Func : IFunc<AlsoT, U>
    {
        private TInEnumerator _enumerator;
        private Func _map;

        public ValueSelectNodeEnumerator(in TInEnumerator enumerator, Func map) => (_enumerator, _map) = (enumerator, map);

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                null => null,
                (_, var size) => (false, size)
            };

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out U current)
        {
            if (_enumerator.TryGetNext(out var currentIn))
            {
                current = _map.Invoke((AlsoT)(object)currentIn);
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ValueSelectNode<T, U, NodeT, Func>
        : INode
        where NodeT : INode
        where Func : IFunc<T, U>
    {
        private NodeT _nodeT;
        private Func _map;

        public int? MaximumLength => _nodeT.MaximumLength;

        public ValueSelectNode(in NodeT nodeT, Func selector) => (_nodeT, _map) = (nodeT, selector);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new ValueSelectNodeEnumerator<EnumeratorElement, U, Enumerator, T, Func>(in enumerator, _map);
            return tail.CreateObject<CreationType, U, ValueSelectNodeEnumerator<EnumeratorElement, U, Enumerator, T, Func>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TNext, TResult, Next>(in Next fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<T, TResult, ValueSelectFoward<T, TNext, Next, U, Func>>(new ValueSelectFoward<T, TNext, Next, U, Func>(fenum, _map));
    }

    struct ValueSelectFoward<T, U, Next, AlsoU, Func>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<U>
        where Func : IFunc<T, AlsoU>
    {
        Next _next;
        Func _selector;

        public ValueSelectFoward(in Next prior, Func predicate) => (_next, _selector) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input) => _next.ProcessNext((U)(object)_selector.Invoke(input));
    }
}
