namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// System.Linq's order: the inner table is built, null keys included, before outer is read; each outer element is
// joined to its group or to nothing; then the inner groups no outer element matched follow, in the order their keys
// first arrived, as System.Linq's lookup orders them. A side that is empty (an empty array) is joined to nothing without
// a table or a key selector.
public static partial class FullJoin
{
    internal const byte NeitherEmpty = 0;
    internal const byte OuterEmpty = 1;
    internal const byte InnerEmpty = 2;

    [Operator]
    public partial struct Node<TOuter, TInner, TKey, TResult, NodeT, TSelector>
        : INode<TResult>
        where NodeT : INode<TOuter>
        where TSelector : struct, Zip.ISelector<TOuter, TInner, TResult>
    {
        internal NodeT _nodeT;
        [Propagate] internal SystemIEnumerable.Node<TInner> _inner;
        [Propagate] internal Func<TOuter, TKey> _outerKeySelector;
        [Propagate] internal Func<TInner, TKey> _innerKeySelector;
        [Propagate] internal IEqualityComparer<TKey>? _comparer;
        [Propagate] internal TSelector _selector;
        [Propagate] internal byte _emptySide;

        // the unmatched inner elements follow outer's, so not even an empty outer sequence bounds the length
        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);

            info.MaximumLength = null;
            info.ActualLengthIsMaximumLength = false;
        }
    }

    partial struct Push<TOuter, TInner, TKey, TResult, Next, TSelector>
    {
        private ChainOptions _options;
        private Join.Table<TKey, TInner>? _table;
        private bool[]? _matched;
        private int _matchedCount;
        private bool _halted;
        private bool _begun;

        // the table is built when the chain is initialised, before its source is opened; once, as a chain handed on
        // already initialised is initialised again. Out of line, or the outer push is inlined into its caller with it,
        // where the outer loop's selectors are not devirtualised
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Begin()
        {
            if (_begun || _emptySide != NeitherEmpty)
                return;

            _begun = true;

            _table = CreateTable(in _inner, in _options, _innerKeySelector, _comparer);
            if (_table._groupCount != 0)
                _matched = new bool[_table._groupCount];
        }

        public bool ProcessNext(TOuter input)
        {
            if (_table is not {} table
                || _outerKeySelector(input) is not {} key
                || !table.TryGetGroup(key, out var group))
            {
                return Forward(_selector.Invoke(input, default!));
            }

            ref var matched = ref _matched![group];
            if (!matched)
            {
                matched = true;
                ++_matchedCount;
            }

            for (var element = table._groups[group]._first; element >= 0; element = table._next[element])
            {
                if (!Forward(_selector.Invoke(input, table._elements[element])))
                    return false;
            }

            return true;
        }

        // the inner elements no outer element matched follow, unless the downstream has already halted
        public TPushResult GetResult<TPushResult>()
        {
            if (!_halted)
            {
                if (_emptySide == OuterEmpty)
                {
                    foreach (var element in _inner._enumerable)
                    {
                        if (!_next.ProcessNext(_selector.Invoke(default!, element)))
                            break;
                    }
                }
                else if (_table is { _groupCount: > 0 } table
                    && _matchedCount < table._groupCount)
                {
                    PushUnmatched(table);
                }
            }

            return _next.GetResult<TPushResult>();
        }

        private void PushUnmatched(Join.Table<TKey, TInner> table)
        {
            var matched = _matched!;
            for (var group = 0; group < table._groupCount; ++group)
            {
                if (matched[group])
                    continue;

                for (var element = table._groups[group]._first; element >= 0; element = table._next[element])
                {
                    if (!_next.ProcessNext(_selector.Invoke(default!, table._elements[element])))
                        return;
                }
            }
        }

        private bool Forward(TResult result)
        {
            if (_next.ProcessNext(result))
                return true;

            _halted = true;

            return false;
        }
    }

    public partial struct Pull<TOuter, TInner, TKey, TResult, NodeT, TEnumerator, TSelector>
    {
        const int NotStarted = 0;
        const int InOuter = 1;
        const int InUnmatched = 2;
        const int InInner = 3;
        const int Done = 4;

        private int _state;
        private Join.Table<TKey, TInner>? _table;
        private bool[]? _matched;
        private int _matchedCount;
        private int _group;
        private int _element; // the current inner element, -1 for an outer element that matched nothing
        private IEnumerator<TInner>? _innerEnumerator;
        [AllowNull] private TOuter _outer;

        public bool MoveNext()
        {
            switch (_state)
            {
                case NotStarted:
                    if (_emptySide == OuterEmpty)
                    {
                        _innerEnumerator = _inner._enumerable.GetEnumerator();
                        _state = InInner;

                        return MoveNextInner();
                    }

                    if (_emptySide == NeitherEmpty)
                    {
                        _table = CreateTable(in _inner, default, _innerKeySelector, _comparer);
                        if (_table._groupCount != 0)
                            _matched = new bool[_table._groupCount];
                    }

                    _state = InOuter;

                    return MoveToNextOuter();

                case InOuter:
                    if (_element >= 0)
                    {
                        _element = _table!._next[_element];
                        if (_element >= 0)
                            return true;
                    }

                    return MoveToNextOuter();

                case InUnmatched:
                    _element = _table!._next[_element];

                    return _element >= 0
                        || MoveToNextUnmatched();

                case InInner:
                    return MoveNextInner();

                default:
                    return false;
            }
        }

        public TResult Current
        {
            get
            {
                var result =
                    _state switch
                    {
                        InOuter => _selector.Invoke(_outer, _element >= 0 ? _table!._elements[_element] : default!),
                        InUnmatched => _selector.Invoke(default!, _table!._elements[_element]),
                        _ => _selector.Invoke(default!, _innerEnumerator!.Current),
                    };

                return result;
            }
        }

        public void Dispose()
        {
            _enumerator.Dispose();
            _innerEnumerator?.Dispose();
        }

        private bool MoveToNextOuter()
        {
            if (!_enumerator.MoveNext())
            {
                if (_table is { _groupCount: > 0 } table
                    && _matchedCount < table._groupCount)
                {
                    _state = InUnmatched;
                    _group = -1;

                    return MoveToNextUnmatched();
                }

                _state = Done;

                return false;
            }

            _outer = _enumerator.Current;
            _element = -1;
            if (_table is {} t
                && _outerKeySelector(_outer) is {} key
                && t.TryGetGroup(key, out var group))
            {
                ref var matched = ref _matched![group];
                if (!matched)
                {
                    matched = true;
                    ++_matchedCount;
                }

                _element = t._groups[group]._first;
            }

            return true;
        }

        // the groups no outer element matched, in the order their keys first arrived
        private bool MoveToNextUnmatched()
        {
            var table = _table!;
            while (++_group < table._groupCount)
            {
                if (!_matched![_group])
                {
                    _element = table._groups[_group]._first;

                    return true;
                }
            }

            _state = Done;

            return false;
        }

        private bool MoveNextInner()
        {
            if (_innerEnumerator!.MoveNext())
                return true;

            _state = Done;

            return false;
        }
    }

    // an outer key that is null matches nothing, though the table holds the inner elements with one, to be joined to nothing
    private static Join.Table<TKey, TInner> CreateTable<TInner, TKey>(in SystemIEnumerable.Node<TInner> inner, in ChainOptions options, Func<TInner, TKey> keySelector, IEqualityComparer<TKey>? comparer) =>
        Join.CreateTable<TInner, TKey, SystemIEnumerable.Node<TInner>>(in inner, in options, keySelector, comparer, keepsNullKeys: true);

    public readonly struct PairSelector<TOuter, TInner>
        : Zip.ISelector<TOuter, TInner, (TOuter? Outer, TInner? Inner)>
    {
        static bool Zip.ISelector<TOuter, TInner, (TOuter? Outer, TInner? Inner)>.PotentialSideEffects => false;

        (TOuter? Outer, TInner? Inner) Zip.ISelector<TOuter, TInner, (TOuter? Outer, TInner? Inner)>.Invoke(TOuter outer, TInner inner) => (outer, inner);
    }
}
