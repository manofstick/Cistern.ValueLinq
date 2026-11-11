namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public static partial class GroupJoin
{
    [Operator]
    public partial struct Node<TOuter, TInner, TKey, TResult, NodeT>
        : INode<TResult>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationCopyTo<TResult>
        where NodeT : INode<TOuter>
    {
        internal NodeT _nodeT;
        [Propagate(ByRef = true)] internal LeftJoin.LookupSource<TInner, TKey, SystemIEnumerable.Node<TInner>> _inner;
        [Propagate] internal Func<TOuter, TKey> _outerKeySelector;
        [Propagate] internal Func<TOuter, IEnumerable<TInner>, TResult> _resultSelector;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        // one result per outer element, but System.Linq reads inner and runs both selectors to count, so only a count told
        // to skip them is outer's; an outer known to be empty runs neither
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (!request.IgnorePotentialSideEffects)
            {
                if (NodeImpl.TryGetNonEnumeratedCount<TOuter, NodeT>(in _nodeT, out var outerCount)
                    && outerCount == 0)
                {
                    response = 0;

                    return true;
                }

                response = default;

                return false;
            }

            response = NodeImpl.Count<TOuter, NodeT>(in _nodeT, ignorePotentialSideEffects: true);

            return true;
        }

        // POTENTIAL OPTIMIZATION: CopyTo over an outer answering it without a span (a Concat, an AppendPrepend), outer copied into a pooled buffer and joined from there
        // inner is read only for a first outer element, as System.Linq reads it
        private bool CopyTo(in Optimizations.CopyTo<TResult> request, out int response)
        {
            if (!Optimizations.AsSpanSource.TryGet<TOuter, NodeT>(ref _nodeT, out var source))
            {
                response = default;

                return false;
            }

            var outers = source.GetSpan();
            response = outers.Length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span;
            if (outers is [_, ..])
            {
                var lookup = _inner.CreateLookup(default);

                for (var i = 0; i < outers.Length; ++i)
                {
                    var outer = outers[i];
                    var key = _outerKeySelector(outer);
                    var matches = Matches(lookup, key);

                    destination[i] = _resultSelector(outer, matches);
                }
            }

            return true;
        }
    }

    partial struct Push<TOuter, TInner, TKey, TResult, Next>
    {
        private ChainOptions _options;
        private GroupBy.Lookup<TKey, TInner>? _lookup;

        // as Select.Push.AllowBatchesWhen
        static BatchKinds IPushEnumerator<TOuter>.AllowBatchesWhen =>
            (typeof(Next).IsAssignableTo(typeof(IInternal__Aggregator__NotForManualUse))
                ? BatchKinds.Container | BatchKinds.Forwarder
                : BatchKinds.Container)
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<TOuter>.ProcessBatch(scoped ref Batch<TOuter> batch)
        {
            var span = batch.GetSpan();
            if (span is [])
                return true;

            return ProcessSpan(span);
        }

        private bool ProcessSpan(ReadOnlySpan<TOuter> span)
        {
            _lookup ??= _inner.CreateLookup(in _options);

            // the downstream runs the span from a local copy: Docs/BatchFromLocalCopy.md, "The fix" and "Constraints"
            var next = _next;
            var lookup = _lookup;
            var outerKeySelector = _outerKeySelector;
            var resultSelector = _resultSelector;
            var more = true;
            foreach (var outer in span)
            {
                var key = outerKeySelector(outer);
                var matches = Matches(lookup, key);
                var selected = resultSelector(outer, matches);
                if (!next.ProcessNext(selected))
                {
                    more = false;
                    break;
                }
            }
            _next = next;

            return more;
        }

        public bool ProcessNext(TOuter input)
        {
            _lookup ??= _inner.CreateLookup(in _options);

            var key = _outerKeySelector(input);
            var matches = Matches(_lookup, key);
            var result = _resultSelector(input, matches);

            var proceed = _next.ProcessNext(result);

            return proceed;
        }
    }

    public partial struct Pull<TOuter, TInner, TKey, TResult, NodeT, TEnumerator>
    {
        private GroupBy.Lookup<TKey, TInner>? _lookup;
        [AllowNull] private TOuter _outer;
        [AllowNull] private IEnumerable<TInner> _matches;

        // outer's element is read before inner, as the push reads it, so a selector below the join runs where System.Linq's runs it
        public bool MoveNext()
        {
            if (!_enumerator.MoveNext())
                return false;

            _outer = _enumerator.Current;

            _lookup ??= _inner.CreateLookup(default);

            var key = _outerKeySelector(_outer);
            _matches = Matches(_lookup, key);

            return true;
        }

        public TResult Current
        {
            get
            {
                var result = _resultSelector(_outer, _matches);

                return result;
            }
        }
    }

    // an unmatched key is handed System.Linq's own empty sequence, the one shared empty array
    private static IEnumerable<TInner> Matches<TKey, TInner>(GroupBy.Lookup<TKey, TInner> lookup, TKey key)
    {
        IEnumerable<TInner> matches =
            lookup.GetGrouping(key, create: false) switch
            {
                {} grouping => grouping,
                null => Array.Empty<TInner>(),
            };

        return matches;
    }

    // System.Linq's GroupJoinGrouping: a grouping of its own for each outer element, keyed by it, over its matches
    internal sealed class Grouping<TOuter, TInner>(TOuter key, IEnumerable<TInner> elements)
        : System.Linq.IGrouping<TOuter, TInner>
    {
        public TOuter Key => key;

        public IEnumerator<TInner> GetEnumerator() => elements.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

        internal static System.Linq.IGrouping<TOuter, TInner> Create(TOuter key, IEnumerable<TInner> elements) => new Grouping<TOuter, TInner>(key, elements);
    }
}
