namespace Cistern.ValueLinq.Nodes;

using System;
using System.Runtime.CompilerServices;

public static partial class ValueSelect
{
    [Operator]
    public partial struct Node<T, U, NodeT, Func>
        : INode<U>
        , Optimizations.IOptimizationCount
        where NodeT : INode<T>
        where Func : IFunc<T, U>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func _selector;

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

    partial struct Push<T, U, Next, Func>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input) => _next.ProcessNext(_selector.Invoke(input));
    }

    public partial struct Pull<T, U, NodeT, TInEnumerator, Func>
    {
        public bool MoveNext() => _enumerator.MoveNext();

        public U Current => _selector.Invoke(_enumerator.Current);
    }
}
