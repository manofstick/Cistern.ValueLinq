namespace Cistern.ValueLinq.Nodes;

using System;

public static partial class SelectIdx
{
    [Operator]
    public partial struct Node<T, U, NodeT>
        : INode<U>
        , Optimizations.IOptimizationCount
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, int, U> _selector;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        // only the dry run, as Select answers it
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (!request.DryRun)
            {
                response = default;

                return false;
            }

            return NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out response);
        }
    }

    partial struct Push<T, U, Next>
    {
        private int _idx;

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(input, _idx++));
    }

    public partial struct Pull<T, U, NodeT, TInEnumerator>
    {
        private int _i;

        partial void Start() => _i = -1;

        public bool MoveNext()
        {
            if (_enumerator.MoveNext())
            {
                ++_i;
                return true;
            }
            return false;
        }

        public U Current => _selector(_enumerator.Current, _i);
    }
}
