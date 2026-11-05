namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

public static partial class Join
{
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

    partial struct Push<TOuter, TInner, TKey, TResult, Next, TSelector>
    {
        private ChainOptions _options;
        private Table<TKey, TInner>? _table;

        // POTENTIAL OPTIMIZATION: a ProcessBatch running the outer span against a local copy of the downstream, as Select.Push.ProcessBatch does, each outer element's matches pushed into the copy
        public bool ProcessNext(TOuter input)
        {
            // inner is read once outer has an element, and an empty table ends the join before any outer key is taken, as System.Linq's does
            var table = _table ??= CreateTable<TInner, TKey, SystemIEnumerable.Node<TInner>>(in _inner, in _options, _innerKeySelector, _comparer, keepsNullKeys: false);

            if (table._groupCount == 0)
                return false;

            var key = _outerKeySelector(input);
            if (table.TryGetGroup(key, out var group))
            {
                for (var element = table._groups[group]._first; element >= 0; element = table._next[element])
                {
                    var result = _selector.Invoke(input, table._elements[element]);
                    if (!_next.ProcessNext(result))
                        return false;
                }
            }

            return true;
        }
    }

    public partial struct Pull<TOuter, TInner, TKey, TResult, NodeT, TEnumerator, TSelector>
    {
        private Table<TKey, TInner>? _table;
        private int _element; // the current match, -1 once the outer element's matches are spent
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

            return MoveToNextMatch();
        }

        public TResult Current
        {
            get
            {
                var element = _table!._elements[_element];

                var result = _selector.Invoke(_outer, element);

                return result;
            }
        }

        // outer's element is read before inner, as the push reads it, so a selector below the join runs where System.Linq's runs it
        private bool MoveToNextMatch()
        {
            _element = -1;

            while (_table is not { _groupCount: 0 }
                && _enumerator.MoveNext())
            {
                var outer = _enumerator.Current;

                var table = _table ??= CreateTable<TInner, TKey, SystemIEnumerable.Node<TInner>>(in _inner, default, _innerKeySelector, _comparer, keepsNullKeys: false);

                if (table._groupCount == 0)
                    return false;

                var key = _outerKeySelector(outer);
                if (table.TryGetGroup(key, out var group))
                {
                    _outer = outer;
                    _element = table._groups[group]._first;

                    return true;
                }
            }

            return false;
        }
    }

    internal static GroupBy.Lookup<TKey, TInner> CreateLookup<TInner, TKey, InnerNode>(in InnerNode inner, in ChainOptions options, Func<TInner, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        where InnerNode : INode<TInner>
    {
        var args = new ToJoinLookup<TInner, TKey>.Args(comparer, keySelector);
        var construct = new Construct<ToJoinLookup<TInner, TKey>, ToJoinLookup<TInner, TKey>.Args>(args);

        var lookup = inner.CreateViaPush<GroupBy.Lookup<TKey, TInner>, ToJoinLookup<TInner, TKey>, Construct<ToJoinLookup<TInner, TKey>, ToJoinLookup<TInner, TKey>.Args>>(in options, in construct);

        return lookup;
    }

    internal static Table<TKey, TInner> CreateTable<TInner, TKey, InnerNode>(in InnerNode inner, in ChainOptions options, Func<TInner, TKey> keySelector, IEqualityComparer<TKey>? comparer, bool keepsNullKeys)
        where InnerNode : INode<TInner>
    {
        var args = new ToJoinTable<TInner, TKey>.Args(comparer, keySelector, keepsNullKeys);
        var construct = new Construct<ToJoinTable<TInner, TKey>, ToJoinTable<TInner, TKey>.Args>(args);

        var table = inner.CreateViaPush<Table<TKey, TInner>, ToJoinTable<TInner, TKey>, Construct<ToJoinTable<TInner, TKey>, ToJoinTable<TInner, TKey>.Args>>(in options, in construct);

        return table;
    }

    // a join's inner sequence, which only the join reads, so it holds no object per key: the elements in the order they
    // arrived, each group a chain through them in that order, and the groups numbered in the order their keys first
    // arrived, the order a lookup keeps its groupings in. An inner element with a null key is held only for a join that
    // pushes the unmatched ones
    internal sealed class Table<TKey, TInner>
    {
        internal struct Group
        {
            [AllowNull] internal TKey _key;
            internal int _hashCode;
            internal int _hashNext; // the next group of the same bucket, -1 at its end
            internal int _first;
            internal int _last;
            internal int _count;
        }

        private readonly IEqualityComparer<TKey>? _comparer; // null for a value type's default comparer, so it devirtualises
        private readonly bool _keepsNullKeys;
        private int[] _buckets = new int[7]; // 1-based group indexes, 0 for an empty bucket

        internal TInner[] _elements = [];
        internal int[] _next = []; // the next element of the same group, -1 at its end
        internal Group[] _groups = [];
        internal int _elementCount;
        internal int _groupCount;

        internal Table(IEqualityComparer<TKey>? comparer, bool keepsNullKeys)
        {
            if (!typeof(TKey).IsValueType)
                _comparer = comparer ?? EqualityComparer<TKey>.Default;
            else if (comparer is not null && comparer != EqualityComparer<TKey>.Default)
                _comparer = comparer;

            _keepsNullKeys = keepsNullKeys;
        }

        // a null key is looked up as the lookup looks it up, at hash 0 through the comparer
        internal bool TryGetGroup(TKey key, out int group)
        {
            group = Find(key, HashCode(key));

            return group >= 0;
        }

        internal void Add(TKey key, TInner element)
        {
            if (key is null
                && !_keepsNullKeys)
            {
                return;
            }

            var hashCode = HashCode(key);

            var group = Find(key, hashCode);
            if (group < 0)
            {
                group = NewGroup(key, hashCode);
                Link(group);
            }

            if (_elementCount == _elements.Length)
                Grow(_elementCount + 1);

            var index = _elementCount++;
            _elements[index] = element;
            _next[index] = -1;

            ref var g = ref _groups[group];
            if (g._count == 0)
                g._first = index;
            else
                _next[g._last] = index;

            g._last = index;
            ++g._count;
        }

        internal void EnsureCapacity(int count)
        {
            if (count > _elements.Length)
                Grow(count);
        }

        private void Grow(int minimum)
        {
            var size = Math.Max(Math.Max(minimum, _elements.Length * 2), 4);

            Array.Resize(ref _elements, size);
            Array.Resize(ref _next, size);
        }

        private int HashCode(TKey key) =>
            key is null ? 0
            : typeof(TKey).IsValueType && _comparer is null ? EqualityComparer<TKey>.Default.GetHashCode(key) & 0x7FFFFFFF
            : _comparer!.GetHashCode(key) & 0x7FFFFFFF;

        private int Find(TKey key, int hashCode)
        {
            var groups = _groups;
            for (var group = _buckets[hashCode % _buckets.Length] - 1; group >= 0; group = groups[group]._hashNext)
            {
                ref var candidate = ref groups[group];
                if (candidate._hashCode == hashCode
                    && (typeof(TKey).IsValueType && _comparer is null
                        ? EqualityComparer<TKey>.Default.Equals(candidate._key, key)
                        : _comparer!.Equals(candidate._key, key)))
                {
                    return group;
                }
            }

            return -1;
        }

        private int NewGroup(TKey key, int hashCode)
        {
            if (_groupCount == _groups.Length)
                Array.Resize(ref _groups, Math.Max(_groups.Length * 2, 4));

            var group = _groupCount++;
            ref var created = ref _groups[group];
            created._key = key;
            created._hashCode = hashCode;
            created._hashNext = -1;

            return group;
        }

        // the buckets grow with the groups, as the lookup's do
        private void Link(int group)
        {
            if (_groupCount > _buckets.Length)
            {
                _buckets = new int[checked(_groupCount * 2 + 1)];
                for (var linked = 0; linked < _groupCount; ++linked)
                    LinkInto(linked);

                return;
            }

            LinkInto(group);
        }

        private void LinkInto(int group)
        {
            ref var linked = ref _groups[group];
            ref var bucket = ref _buckets[linked._hashCode % _buckets.Length];
            linked._hashNext = bucket - 1;
            bucket = group + 1;
        }
    }
}
