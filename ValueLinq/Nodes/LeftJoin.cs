namespace Cistern.ValueLinq.Nodes;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

public static partial class LeftJoin
{
    [Operator]
    public partial struct Node<TOuter, TInner, TKey, TResult, NodeT, InnerNode, TSelector>
        : INode<TResult>
        where NodeT : INode<TOuter>
        where InnerNode : INode<TInner>
        where TSelector : struct, Zip.ISelector<TOuter, TInner, TResult>
    {
        internal NodeT _nodeT;
        [Propagate(ByRef = true)] internal LookupSource<TInner, TKey, InnerNode> _inner;
        [Propagate] internal Func<TOuter, TKey> _outerKeySelector;
        [Propagate] internal TSelector _selector;

        // an outer element can match any number of inner ones, so only an empty outer sequence bounds the length
        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);

            if (info.MaximumLength != 0)
            {
                info.MaximumLength = null;
                info.ActualLengthIsMaximumLength = false;
            }
        }
    }

    partial struct Push<TOuter, TInner, TKey, TResult, Next, InnerNode, TSelector>
    {
        private ChainOptions _options;
        private Join.Table<TKey, TInner>? _table;

        // POTENTIAL OPTIMIZATION: a ProcessBatch running the outer span against a local copy of the downstream, as Select.Push.ProcessBatch does, each outer element's results pushed into the copy
        public bool ProcessNext(TOuter input)
        {
            var table = _table ??= _inner.CreateTable(in _options);

            var key = _outerKeySelector(input);

            if (table.TryGetGroup(key, out var group))
            {
                for (var element = table._groups[group]._first; element >= 0; element = table._next[element])
                {
                    var result = _selector.Invoke(input, table._elements[element]);
                    if (!_next.ProcessNext(result))
                        return false;
                }

                return true;
            }
            else
            {
                var unmatched = _selector.Invoke(input, default!);

                var proceed = _next.ProcessNext(unmatched);

                return proceed;
            }
        }
    }

    public partial struct Pull<TOuter, TInner, TKey, TResult, NodeT, TEnumerator, InnerNode, TSelector>
    {
        private Join.Table<TKey, TInner>? _table;
        private int _element; // the current match, -1 for an outer element that matched nothing
        [AllowNull] private TOuter _outer;

        public bool MoveNext()
        {
            if (_table is {} table
                && _element >= 0)
            {
                _element = table._next[_element];
                if (_element >= 0)
                    return true;
            }

            return MoveToNextOuter();
        }

        public TResult Current
        {
            get
            {
                var element =
                    _element >= 0
                        ? _table!._elements[_element]
                        : default!;

                var result = _selector.Invoke(_outer, element);

                return result;
            }
        }

        // outer's element is read before inner, as the push reads it, so a selector below the join runs where System.Linq's runs it
        private bool MoveToNextOuter()
        {
            _element = -1;

            if (!_enumerator.MoveNext())
                return false;

            _outer = _enumerator.Current;

            var table = _table ??= _inner.CreateTable(default);

            var key = _outerKeySelector(_outer);
            if (table.TryGetGroup(key, out var group))
                _element = table._groups[group]._first;

            return true;
        }
    }

    // a struct around the looked-up node, since an [Operator] node holds exactly one node field, its source
    public readonly struct LookupSource<TInner, TKey, InnerNode>(in InnerNode node, Func<TInner, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        where InnerNode : INode<TInner>
    {
        private readonly InnerNode _node = node;
        private readonly Func<TInner, TKey> _keySelector = keySelector;
        private readonly IEqualityComparer<TKey>? _comparer = comparer;

        internal GroupBy.Lookup<TKey, TInner> CreateLookup(in ChainOptions options)
        {
            var lookup = Join.CreateLookup<TInner, TKey, InnerNode>(in _node, in options, _keySelector, _comparer);

            return lookup;
        }

        internal Join.Table<TKey, TInner> CreateTable(in ChainOptions options)
        {
            var table = Join.CreateTable<TInner, TKey, InnerNode>(in _node, in options, _keySelector, _comparer, keepsNullKeys: false);

            return table;
        }
    }
}
