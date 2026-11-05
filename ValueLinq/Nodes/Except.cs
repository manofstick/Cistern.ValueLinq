namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

// TODo: Add the Default Equality version

public static partial class Except
{
    [Operator]
    public partial struct Node<TElement, TKey, NodeT, TKeySelector>
        : INode<TElement>
        , Optimizations.IOptimizationElementAt<TElement>
        , Optimizations.IOptimizationAny
        , Optimizations.IOptimizationContains<TElement>
        where NodeT : INode<TElement>
        where TKeySelector : struct, IFunc<TElement, TKey>
    {
        internal NodeT _nodeT;
        [Propagate] internal IEnumerable<TKey> _second;
        [Propagate] internal IEqualityComparer<TKey>? _comparer;
        [Propagate] internal TKeySelector _keySelector;

        // Distinct, and Union over the Concat of its parts: each element its own key and nothing excluded. Such a node's
        // first element is its source's first, and it holds an element, or a value, exactly when its source does, so
        // these are answered by the source with no set built, as System.Linq's Distinct and Union answer them; DistinctBy
        // is not, as System.Linq runs its key selector on the first element
        private readonly bool ExcludesNothing =>
            typeof(TKeySelector) == typeof(IdentityKey<TElement>)
            && _second is TKey[] { Length: 0 };

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, TElement) response)
        {
            var index = request.Index;
            if (!ExcludesNothing || index.IsFromEnd || index.Value != 0)
            {
                response = default;
                return false;
            }

            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, TElement)>(in request, out response))
                response = _nodeT.CreateViaPush<(bool, TElement), Aggregation.FoundElementAt<TElement>, Construct<Aggregation.FoundElementAt<TElement>, Aggregation.FoundElementAt<TElement>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<TElement>, Aggregation.FoundElementAt<TElement>.Args>(new Aggregation.FoundElementAt<TElement>.Args(index)));
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            if (!ExcludesNothing)
            {
                response = default;
                return false;
            }

            response = NodeImpl.Any<TElement, NodeT>(_nodeT);
            return true;
        }

        // only with no comparer given, as System.Linq's: an explicit comparer, even the default one, keeps the set
        private bool Contains(in Optimizations.Contains<TElement> request, out bool response)
        {
            if (!ExcludesNothing || _comparer is not null)
            {
                response = default;
                return false;
            }

            response = NodeImpl.Contains<TElement, NodeT>(_nodeT, request.Value);
            return true;
        }

        // POTENTIAL OPTIMIZATION: ToArray (and CopyTo) for the identity key (Distinct, Union), the push's set copied out at its exact size by HashSet<T>.CopyTo, which keeps first-seen order as nothing is removed from it; System.Linq's Distinct and Union build their arrays from the set so

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        // the source by value: taken by in, it is left unpromoted, so the node is built in a temp and copied whole with
        // one wide load over the narrower stores that built it
        public Node(NodeT nodeT, IEnumerable<TKey> second, IEqualityComparer<TKey>? comparer, TKeySelector keySelector)
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
                _set.Add(key)
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
                if (_set.Add(_keySelector.Invoke(current)))
                {
                    _current = current;
                    return true;
                }
            }
            return false;
        }

        public TElement Current => _current;
    }

    public readonly struct IdentityKey<T>
        : IFunc<T, T>
    {
        public T Invoke(T t) => t;
    }
}
