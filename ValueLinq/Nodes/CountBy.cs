namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;

public static partial class CountBy
{
    [Answerer]
    public partial struct Node<TSource, TKey, NodeT>
        : INode<KeyValuePair<TKey, int>>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationToArray<KeyValuePair<TKey, int>>
        where NodeT : INode<TSource>
        where TKey : notnull
    {
        private NodeT _nodeT;
        private readonly Func<TSource, TKey> _keySelector;
        private readonly IEqualityComparer<TKey>? _comparer;

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            _nodeT = nodeT;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        // there are no more keys than elements, and none of an empty source
        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        internal InsertionMap<TKey, int> CountKeys(in ChainOptions options)
        {
            var args = new CountByKey<TSource, TKey>.Args(_keySelector, _comparer);

            var counts = _nodeT.CreateViaPush<InsertionMap<TKey, int>, CountByKey<TSource, TKey>, Construct<CountByKey<TSource, TKey>, CountByKey<TSource, TKey>.Args>>(in options, new Construct<CountByKey<TSource, TKey>, CountByKey<TSource, TKey>.Args>(args));

            return counts;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            var counts = CountKeys(default);
            response = counts.Count;
            counts.Release();

            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out KeyValuePair<TKey, int>[] response)
        {
            response = CountKeys(default).ToArrayAndRelease();

            return true;
        }

        // the enumerator releases the map once the push has run, however it ended
        TPushResult INode<KeyValuePair<TKey, int>>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var counts = CountKeys(in options);

            var result = SystemEnumerableGeneric.ExecutePush<KeyValuePair<TKey, int>, TPushResult, TPushEnumerator, InsertionMap<TKey, int>.Enumerator>(counts.GetEnumerator(), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

            return result;
        }
    }

    // the source is counted on the first MoveNext, so a source that throws does so there, as System.Linq's does
    [Puller]
    public partial struct Pull<TSource, TKey, NodeT>
        where NodeT : INode<TSource>
        where TKey : notnull
    {
        private enum State : byte
        {
            Uncounted,
            Counted,
            Disposed,
        }

        private Node<TSource, TKey, NodeT> _node;
        private InsertionMap<TKey, int>.Enumerator _enumerator;
        private State _state;

        private Pull(in Node<TSource, TKey, NodeT> node)
        {
            _node = node;
            _enumerator = default;
            _state = State.Uncounted;
        }

        private static Pull<TSource, TKey, NodeT> Create(in Node<TSource, TKey, NodeT> node) => new(in node);

        public bool MoveNext()
        {
            if (_state == State.Uncounted)
            {
                _enumerator = _node.CountKeys(default).GetEnumerator();
                _state = State.Counted;
            }

            var moved =
                _state == State.Counted
                && _enumerator.MoveNext();

            return moved;
        }

        public KeyValuePair<TKey, int> Current => _enumerator.Current;

        public void Dispose()
        {
            _enumerator.Dispose();
            (_node, _enumerator, _state) = (default, default, State.Disposed);
        }
    }
}
