namespace Cistern.ValueLinq.Nodes;

using System;
using System.Collections.Generic;

public static partial class Intersect
{
    [Operator]
    public partial struct Node<TElement, TKey, NodeT, TKeySelector>
        : INode<TElement>
        where NodeT : INode<TElement>
        where TKeySelector : struct, IFunc<TElement, TKey>
    {
        internal NodeT _nodeT;
        [Propagate] internal IEnumerable<TKey> _second;
        [Propagate] internal IEqualityComparer<TKey>? _comparer;
        [Propagate] internal TKeySelector _keySelector;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public Node(in NodeT nodeT, IEnumerable<TKey> second, IEqualityComparer<TKey>? comparer, TKeySelector keySelector)
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            _nodeT = nodeT;
            _second = second;
            _comparer = comparer;
            _keySelector = keySelector;
        }
    }

    partial struct Push<TElement, TKey, Next, TKeySelector>
    {
        HashSet<TKey> _set;

        // the set is built when the chain is initialised, before its source is opened; once, as a chain handed on already
        // initialised is initialised again
        private void Begin()
        {
            if (_set is null)
                _set = new HashSet<TKey>(_second, _comparer);
        }

        // POTENTIAL OPTIMIZATION: a ProcessBatch running the span against a local copy of the downstream, as Where.Push.ProcessElements does
        public bool ProcessNext(TElement input)
        {
            var key = _keySelector.Invoke(input);

            var proceed =
                _set.Remove(key)
                    ? _next.ProcessNext(input)
                    : true;

            return proceed;
        }
    }

    public partial struct Pull<TElement, TKey, NodeT, TEnumerator, TKeySelector>
    {
        private HashSet<TKey>? _set;
        [AllowNull] private TElement _current;

        public bool MoveNext()
        {
            // second is read on the first MoveNext, before the source is pulled, as System.Linq reads it
            _set ??= new HashSet<TKey>(_second, _comparer);

            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                var key = _keySelector.Invoke(current);

                if (_set.Remove(key))
                {
                    _current = current;

                    return true;
                }
            }

            return false;
        }

        public TElement Current => _current;
    }
}
