using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ValueWhereNodeEnumerator<TIn, TInEnumerator, AlsoT, Predicate>
        : IFastEnumerator<TIn>
        where TInEnumerator : IFastEnumerator<TIn>
        where Predicate : IFunc<AlsoT, bool>
    {
        private TInEnumerator _enumerator;
        private Predicate _filter;

        public ValueWhereNodeEnumerator(in TInEnumerator enumerator, Predicate filter) => (_enumerator, _filter) = (enumerator, filter);

        public (bool, int)? InitialSize =>
            _enumerator.InitialSize switch
            {
                (var flag, 0) => (flag, 0),
                _ => null
            };

        public void Dispose() => _enumerator.Dispose();

        public bool TryGetNext(out TIn current)
        {
            while (_enumerator.TryGetNext(out current))
            {
                if (_filter.Invoke((AlsoT)(object)current))
                    return true;
            }
            return false;
        }
    }

    public struct ValueWhereNode<T, NodeT, Predicate>
        : INode
        where NodeT : INode
        where Predicate : IFunc<T, bool>
    {
        private NodeT _nodeT;
        private Predicate _filter;

        public void GetCountInformation(out int? maximumLength)
        {
            _nodeT.GetCountInformation(out maximumLength);
        }

        public ValueWhereNode(in NodeT nodeT, Predicate predicate) => (_nodeT, _filter) = (nodeT, predicate);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var nextEnumerator = new ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>(in enumerator, _filter);
            return tail.CreateObject<CreationType, EnumeratorElement, ValueWhereNodeEnumerator<EnumeratorElement, Enumerator, T, Predicate>>(ref nextEnumerator);
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) =>
            _nodeT.CreateObjectViaFastEnumerator<TIn, TResult, ValueWhereFoward<TIn, FEnumerator, T, Predicate>>(new ValueWhereFoward<TIn, FEnumerator, T, Predicate>(fenum, _filter));
    }

    struct ValueWhereFoward<T, Next, AlsoT, Predicate>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<T>
        where Predicate : IFunc<AlsoT, bool>
    {
        Next _next;
        Predicate _predicate;

        public ValueWhereFoward(in Next prior, Predicate predicate) => (_next, _predicate) = (prior, predicate);

        public TResult GetResult<TResult>() => _next.GetResult<TResult>();

        public void Init(int? size) => _next.Init(null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
        {
            if (_predicate.Invoke((AlsoT)(object)input))
                return _next.ProcessNext(input);
            return true;
        }
    }
}
