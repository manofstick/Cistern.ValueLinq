namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

// the result-selector overloads of GroupBy: the lookup is built as GroupBy builds it, and the result selector is run
// over each grouping as it is enumerated or pushed, so the node is not the lookup and stays hand-written
public static partial class GroupByResult
{
    [Answerer]
    public partial struct Node<TSource, TKey, TResult, NodeT>
        : INode<TResult>
        , Optimizations.IOptimizationCount
        where NodeT : INode<TSource>
    {
        internal NodeT _nodeT;
        internal Func<TSource, TKey> _keySelector;
        internal Func<TKey, IEnumerable<TSource>, TResult> _resultSelector;
        internal IEqualityComparer<TKey>? _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            _nodeT = nodeT;
            _keySelector = keySelector;
            _resultSelector = resultSelector;
            _comparer = comparer;
        }

        TPushResult INode<TResult>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var lookup = _nodeT.CreateViaPush<GroupBy.Lookup<TKey, TSource>, ToLookup<TSource, TKey>, Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>>(in options, new Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>(new ToLookup<TSource, TKey>.Args(_comparer, _keySelector)));
            return FastEnumerate<TKey, TSource, TResult, TPushResult, TPushEnumerator>(lookup, _resultSelector, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
        }

        // a result per grouping, so the count is the lookup's, and the result selector is not run, as System.Linq's is not
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            var lookup = _nodeT.CreateViaPush<GroupBy.Lookup<TKey, TSource>, ToLookup<TSource, TKey>, Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>(new ToLookup<TSource, TKey>.Args(_comparer, _keySelector)));
            response = lookup.Count;

            return true;
        }
    }

    [Puller]
    public partial struct Pull<TSource, TKey, TResult, NodeT>
        where NodeT : INode<TSource>
    {
        private LookupResultEnumerator<TKey, TSource, TResult> _enumerator;

        // why start with _next? to get same result as System.Linq...
        private Pull(Func<TKey, IEnumerable<TSource>, TResult> resultSelector, GroupBy.Lookup<TKey, TSource> lookup) => _enumerator = new(resultSelector, lookup._lastGrouping?._next);

        private static Pull<TSource, TKey, TResult, NodeT> Create(in Node<TSource, TKey, TResult, NodeT> node)
            => new(node._resultSelector, node._nodeT.CreateViaPush<GroupBy.Lookup<TKey, TSource>, ToLookup<TSource, TKey>, Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>(new ToLookup<TSource, TKey>.Args(node._comparer, node._keySelector))));

        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext() => _enumerator.MoveNext();
        public TResult Current => _enumerator.Current;
    }

    [Answerer]
    public partial struct Node<TSource, TKey, TElement, TResult, NodeT>
        : INode<TResult>
        , Optimizations.IOptimizationCount
        where NodeT : INode<TSource>
    {
        internal NodeT _nodeT;
        internal Func<TSource, TKey> _keySelector;
        internal Func<TSource, TElement> _elementSelector;
        internal Func<TKey, IEnumerable<TElement>, TResult> _resultSelector;
        internal IEqualityComparer<TKey>? _comparer;

        public void GetCountInformation(out CountInformation info) => info = new CountInformation();

        public Node(in NodeT nodeT, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null)
                throw new ArgumentNullException(nameof(elementSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            _nodeT = nodeT;
            _keySelector = keySelector;
            _elementSelector = elementSelector;
            _resultSelector = resultSelector;
            _comparer = comparer;
        }

        TPushResult INode<TResult>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var lookup = _nodeT.CreateViaPush<GroupBy.Lookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>, Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>>(in options, new Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>(new ToLookup<TSource, TKey, TElement>.Args(_comparer, _keySelector, _elementSelector)));
            return FastEnumerate<TKey, TElement, TResult, TPushResult, TPushEnumerator>(lookup, _resultSelector, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
        }

        // a result per grouping, so the count is the lookup's, and the result selector is not run, as System.Linq's is not
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                response = default;

                return false;
            }

            var lookup = _nodeT.CreateViaPush<GroupBy.Lookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>, Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>(new ToLookup<TSource, TKey, TElement>.Args(_comparer, _keySelector, _elementSelector)));
            response = lookup.Count;

            return true;
        }
    }

    [Puller]
    public partial struct Pull<TSource, TKey, TElement, TResult, NodeT>
        where NodeT : INode<TSource>
    {
        private LookupResultEnumerator<TKey, TElement, TResult> _enumerator;

        // why start with _next? to get same result as System.Linq...
        private Pull(Func<TKey, IEnumerable<TElement>, TResult> resultSelector, GroupBy.Lookup<TKey, TElement> lookup) => _enumerator = new(resultSelector, lookup._lastGrouping?._next);

        private static Pull<TSource, TKey, TElement, TResult, NodeT> Create(in Node<TSource, TKey, TElement, TResult, NodeT> node)
            => new(node._resultSelector, node._nodeT.CreateViaPush<GroupBy.Lookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>, Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>(new ToLookup<TSource, TKey, TElement>.Args(node._comparer, node._keySelector, node._elementSelector))));

        public void Dispose() => _enumerator.Dispose();
        public bool MoveNext() => _enumerator.MoveNext();
        public TResult Current => _enumerator.Current;
    }

    // the lookup's push with the result selector over each grouping
    private static TResultx FastEnumerate<TKey, TElement, TResult, TResultx, TPushEnumerator>(GroupBy.Lookup<TKey, TElement> lookup, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TResult>
    {
        try
        {
            Loop(lookup, resultSelector, ref fenum);
            return fenum.GetResult<TResultx>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void Loop<TKey, TElement, TResult, TPushEnumerator>(GroupBy.Lookup<TKey, TElement> lookup, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TResult>
    {
        var last = lookup._lastGrouping?._next;
        var current = last;
        if (current != null)
        {
            do
            {
                if (!fenum.ProcessNext(resultSelector(current.Key, current)))
                    break;
                current = current._next;
            } while (current != last);
        }
    }

    [Puller]
    internal partial struct LookupResultEnumerator<TKey, TElement, TResult>
    {
        Func<TKey, IEnumerable<TElement>, TResult> _resultSelector;
        private GroupBy.Grouping<TKey, TElement>? _lastGrouping;
        private GroupBy.Grouping<TKey, TElement>? _g;

        public LookupResultEnumerator(Func<TKey, IEnumerable<TElement>, TResult> resultSelector, GroupBy.Grouping<TKey, TElement>? lastGrouping)
        {
            _resultSelector = resultSelector;
            _lastGrouping = lastGrouping;
            _g = lastGrouping;
        }

        private GroupBy.Grouping<TKey, TElement>? _current;

        public bool MoveNext()
        {
            if (_g == null)
                return false;

            _current = _g;
            _g = _g._next;
            if (_g == _lastGrouping)
                _g = null;

            return true;
        }

        public TResult Current => _resultSelector(_current!.Key, _current);

        static TBuilt IPullEnumerator<TResult>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
            => throw new InvalidOperationException();

        public void Dispose() => _g = _lastGrouping = null;
    }
}
