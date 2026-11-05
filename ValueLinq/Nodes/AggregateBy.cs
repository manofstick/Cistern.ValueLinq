namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;

public static partial class AggregateBy
{
    [Answerer]
    public partial struct Node<TSource, TKey, TAccumulate, NodeT>
        : INode<KeyValuePair<TKey, TAccumulate>>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationToArray<KeyValuePair<TKey, TAccumulate>>
        where NodeT : INode<TSource>
        where TKey : notnull
    {
        private NodeT _nodeT;
        private readonly AggregateByKey<TSource, TKey, TAccumulate>.Args _args;

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _nodeT = nodeT;
            _args = new AggregateByKey<TSource, TKey, TAccumulate>.Args(keySelector, seed, null, func, comparer);
        }

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            if (seedSelector == null)
                throw new ArgumentNullException(nameof(seedSelector));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            _nodeT = nodeT;
            _args = new AggregateByKey<TSource, TKey, TAccumulate>.Args(keySelector, default!, seedSelector, func, comparer);
        }

        // there are no more keys than elements, and none of an empty source
        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        internal InsertionMap<TKey, TAccumulate> AggregateKeys(in ChainOptions options)
        {
            var accumulates = _nodeT.CreateViaPush<InsertionMap<TKey, TAccumulate>, AggregateByKey<TSource, TKey, TAccumulate>, Construct<AggregateByKey<TSource, TKey, TAccumulate>, AggregateByKey<TSource, TKey, TAccumulate>.Args>>(in options, new Construct<AggregateByKey<TSource, TKey, TAccumulate>, AggregateByKey<TSource, TKey, TAccumulate>.Args>(_args));

            return accumulates;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            var accumulates = AggregateKeys(default);
            response = accumulates.Count;
            accumulates.Release();

            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out KeyValuePair<TKey, TAccumulate>[] response)
        {
            response = AggregateKeys(default).ToArrayAndRelease();

            return true;
        }

        // the enumerator releases the map once the push has run, however it ended
        TPushResult INode<KeyValuePair<TKey, TAccumulate>>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var accumulates = AggregateKeys(in options);

            var result = SystemEnumerableGeneric.ExecutePush<KeyValuePair<TKey, TAccumulate>, TPushResult, TPushEnumerator, InsertionMap<TKey, TAccumulate>.Enumerator>(accumulates.GetEnumerator(), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

            return result;
        }
    }

    // the source is aggregated on the first MoveNext, so a source that throws does so there, as System.Linq's does
    [Puller]
    public partial struct Pull<TSource, TKey, TAccumulate, NodeT>
        where NodeT : INode<TSource>
        where TKey : notnull
    {
        private enum State : byte
        {
            Unaggregated,
            Aggregated,
            Disposed,
        }

        private Node<TSource, TKey, TAccumulate, NodeT> _node;
        private InsertionMap<TKey, TAccumulate>.Enumerator _enumerator;
        private State _state;

        private Pull(in Node<TSource, TKey, TAccumulate, NodeT> node)
        {
            _node = node;
            _enumerator = default;
            _state = State.Unaggregated;
        }

        private static Pull<TSource, TKey, TAccumulate, NodeT> Create(in Node<TSource, TKey, TAccumulate, NodeT> node) => new(in node);

        public bool MoveNext()
        {
            if (_state == State.Unaggregated)
            {
                _enumerator = _node.AggregateKeys(default).GetEnumerator();
                _state = State.Aggregated;
            }

            var moved =
                _state == State.Aggregated
                && _enumerator.MoveNext();

            return moved;
        }

        public KeyValuePair<TKey, TAccumulate> Current => _enumerator.Current;

        public void Dispose()
        {
            _enumerator.Dispose();
            (_node, _enumerator, _state) = (default, default, State.Disposed);
        }
    }
}
