namespace Cistern.ValueLinq.Nodes;

using System;

public static partial class Where_In
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal InFunc<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }
    }

    partial struct Push<T, Next>
    {
        public bool ProcessNext(T input)
        {
            if (_predicate(in input))
                return _next.ProcessNext(input);
            return true;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate(in current))
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
