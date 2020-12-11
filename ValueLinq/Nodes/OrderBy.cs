using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    public interface IKeySelectors<T>
    {
        int[] SortByIndexes<Comparer>(T[] elements, in Comparer comparer)
            where Comparer : IComparer<int>;
    }

    public struct KeySelectorsRoot<T>
        : IKeySelectors<T>
    {
        public int[] SortByIndexes<Comparer>(T[] elements, in Comparer comparer) where Comparer : IComparer<int>
        {
            var indexes = new int[elements.Length]; // TODO: ArrayPool
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;

            Array.Sort(indexes, comparer);

            return indexes;
        }
    }

    public struct KeySelectors<T, TKey, PriorKeySelector>
        : IKeySelectors<T>
        where PriorKeySelector : IKeySelectors<T>
    {
        PriorKeySelector _priorKeySelector;

        Func<T, TKey> _keySelector;
        IComparer<TKey> _comparer;
        bool _descending;

        public KeySelectors(PriorKeySelector priorKeySelector, Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending) =>
            (_priorKeySelector, _keySelector, _comparer, _descending) = (priorKeySelector, keySelector, comparer, descending);

        public int[] SortByIndexes<Comparer>(T[] elements, in Comparer comparer) where Comparer : IComparer<int>
        {
            TKey[] keys = new TKey[elements.Length]; // TODO: ArrayPool
            for (var i = 0; i < keys.Length; ++i)
                keys[i] = _keySelector(elements[i]);

            if (_descending)
            {
                if (_comparer == Comparer<TKey>.Default)
                    return _priorKeySelector.SortByIndexes(elements, new KeySortWithDefaultComparerDescending<TKey, Comparer>(comparer, keys));
                else
                    return _priorKeySelector.SortByIndexes(elements, new KeySortWithComparerDescending<TKey, Comparer>(comparer, keys, _comparer));
            }
            else
            {
                if (_comparer == Comparer<TKey>.Default)
                    return _priorKeySelector.SortByIndexes(elements, new KeySortWithDefaultComparer<TKey, Comparer>(comparer, keys, _comparer));
                else
                    return _priorKeySelector.SortByIndexes(elements, new KeySortWithComparer<TKey, Comparer>(comparer, keys, _comparer));
            }
        }
    }

    struct KeySortWithComparer<TKey, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        Lower _lower;

        TKey[] _keys;
        IComparer<TKey> _comparer;

        public KeySortWithComparer(Lower lower, TKey[] keys, IComparer<TKey> comparer)
            => (_lower, _keys, _comparer) = (lower, keys, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithDefaultComparer<TKey, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        Lower _lower;

        TKey[] _keys;

        public KeySortWithDefaultComparer(Lower lower, TKey[] keys, IComparer<TKey> comparer)
            => (_lower, _keys) = (lower, keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            Comparer<TKey>.Default.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithComparerDescending<TKey, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        Lower _lower;

        TKey[] _keys;
        IComparer<TKey> _comparer;

        public KeySortWithComparerDescending(Lower lower, TKey[] keys, IComparer<TKey> comparer)
            => (_lower, _keys, _comparer) = (lower, keys, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithDefaultComparerDescending<TKey, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        Lower _lower;

        TKey[] _keys;

        public KeySortWithDefaultComparerDescending(Lower lower, TKey[] keys)
            => (_lower, _keys) = (lower, keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            Comparer<TKey>.Default.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct IntSort
        : IComparer<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(int x, int y) => x.CompareTo(y);
    }

    public struct OrderByNode<T, KeySelectors, NodeT>
        : INode<T>
        where NodeT : INode<T>
        where KeySelectors : IKeySelectors<T>
    {
        internal NodeT _nodeT;
        internal KeySelectors _keySelectors;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        public OrderByNode(in NodeT nodeT, KeySelectors keySelectors) =>
            (_nodeT, _keySelectors) = (nodeT, keySelectors);

        private (T[], int[]) GetOrdered()
        {
            var array = NodeImpl.ToArray<T, NodeT>(in _nodeT, null, null); // TODO: ArrayPool
            var sortedIndexes = _keySelectors.SortByIndexes(array, new IntSort());

            return (array, sortedIndexes);
        }

        private T[] GetOrderedAsArray()
        {
            var (array, sortedIndexes) = GetOrdered();

            for (var i=0; i < sortedIndexes.Length; ++i)
            {
                var swapIdx = sortedIndexes[i];
                while (swapIdx < i)
                {
                    swapIdx = sortedIndexes[swapIdx];
                }
                if (swapIdx != i)
                {
                    var tmp = array[i];
                    array[i] = array[swapIdx];
                    array[swapIdx] = tmp;
                }
            }

            return array;
/*
            // TODO: just temp
            var sorted = new T[array.Length];
            for (var i = 0; i < sortedIndexes.Length; ++i)
                sorted[i] = array[sortedIndexes[i]];

            return sorted;
*/
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var ordered = GetOrderedAsArray();
            return ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(ordered, ref nodes);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)GetOrderedAsArray();
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => OrderByNode.FastEnumerate<T, TResult, FEnumerator>(GetOrdered(), fenum);
    }

    static class OrderByNode
    {
        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(in (TIn[], int[]) sortedInfo, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            // TODO: Possibly it is worth coverting to a "normal" array here and passing off to ArrayNode.FastEnumerate
            // because of the additional optimizations that are available for arrays
            try
            {
                ProcessSortedInfo<TIn, FEnumerator>(in sortedInfo, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void ProcessSortedInfo<TIn, FEnumerator>(in (TIn[], int[]) sortedInfo, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            var data = sortedInfo.Item1;
            var ordering = sortedInfo.Item2;
            for (var orderingIdx = 0; orderingIdx < ordering.Length; ++orderingIdx)
            {
                if (!fenum.ProcessNext(data[ordering[orderingIdx]]))
                    break;
            }
        }
    }
}
