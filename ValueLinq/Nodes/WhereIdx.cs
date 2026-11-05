namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

public static partial class WhereIdx
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, int, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }
    }

    partial struct Push<T, Next>
    {
        int _idx;

        public bool ProcessNext(T input)
        {
            if (_predicate(input, _idx++))
                return _next.ProcessNext(input);
            return true;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        int _idx;

        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate(current, _idx++))
                {
                    _current = current;
                    return true;
                }
            }
            return false;
        }

        public T Current => _current;
    }
}
