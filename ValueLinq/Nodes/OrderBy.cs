using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    public interface IKeySelectors<T>
    {
        void SortElements<Comparer>(Span<T> elements, in Comparer comparer)
            where Comparer : IComparer<int>;
    }

    public struct KeySelectorsRoot<T>
        : IKeySelectors<T>
    {
        public void SortElements<Comparer>(Span<T> elements, in Comparer comparer) 
            where Comparer : IComparer<int>
        {
            Span<int> indexes =
                elements.Length < 200 // MAGIC! 200 is that reasonable?
                    ? stackalloc int[elements.Length]
                    : new int[elements.Length]; // TODO: ArrayPool
            OrderByImpl.InitIndexes(indexes);

            // I haven't worked out the algorithmic complexity of OrderByImpl.OrderByIndex, so I'm only using it
            // when collections aren't too large...
            if (elements.Length < 500) // MAGIC! but is 500 reasonable?
            {
                indexes.Sort(comparer);
                OrderByImpl.OrderByIndex(elements, indexes);
            }
            else
            {
                indexes.Sort(elements, comparer);
            }
        }
    }

    static class OrderByImpl
    {
        internal static TKey[] ExtractKeys<T, TKey>(Span<T> elements, Func<T, TKey> _keySelector)
        {
            TKey[] keys = new TKey[elements.Length]; // TODO: ArrayPool
            for (var i = 0; i < keys.Length; ++i)
                keys[i] = _keySelector(elements[i]);
            return keys;
        }

        internal static void DescendingDefaultComparer<T, TKey, PriorKeySelector, Comparer>(Span<T> elements, TKey[] keys, ref PriorKeySelector prior, in Comparer comparer)
            where Comparer : IComparer<int>
            where PriorKeySelector : IKeySelectors<T>
        {
                 if (typeof(TKey) == typeof(double))   prior.SortElements(elements, new GenericSortDescending<double,   Comparer>(comparer, (double[])  (object)keys));
            else if (typeof(TKey) == typeof(float))    prior.SortElements(elements, new GenericSortDescending<float,    Comparer>(comparer, (float[])   (object)keys));
            else if (typeof(TKey) == typeof(decimal))  prior.SortElements(elements, new GenericSortDescending<decimal,  Comparer>(comparer, (decimal[]) (object)keys));
            else if (typeof(TKey) == typeof(long))     prior.SortElements(elements, new GenericSortDescending<long,     Comparer>(comparer, (long[])    (object)keys));
            else if (typeof(TKey) == typeof(int))      prior.SortElements(elements, new GenericSortDescending<int,      Comparer>(comparer, (int[])     (object)keys));
            else if (typeof(TKey) == typeof(DateTime)) prior.SortElements(elements, new GenericSortDescending<DateTime, Comparer>(comparer, (DateTime[])(object)keys));
            else if (typeof(TKey) == typeof(string))   prior.SortElements(elements, new StringSortDescending<Comparer>(comparer, (string[])(object)keys, StringComparer.CurrentCulture));
            else                                       prior.SortElements(elements, new KeySortWithDefaultComparerDescending<TKey, Comparer>(comparer, keys));
        }

        internal static void AscendingDefaultComparer<T, TKey, PriorKeySelector, Comparer>(Span<T> elements, TKey[] keys, ref PriorKeySelector prior, in Comparer comparer)
            where Comparer : IComparer<int>
            where PriorKeySelector : IKeySelectors<T>
        {
                 if (typeof(TKey) == typeof(double))   prior.SortElements(elements, new GenericSort<double,   Comparer>(comparer, (double[])  (object)keys));
            else if (typeof(TKey) == typeof(float))    prior.SortElements(elements, new GenericSort<float,    Comparer>(comparer, (float[])   (object)keys));
            else if (typeof(TKey) == typeof(decimal))  prior.SortElements(elements, new GenericSort<decimal,  Comparer>(comparer, (decimal[]) (object)keys));
            else if (typeof(TKey) == typeof(long))     prior.SortElements(elements, new GenericSort<long,     Comparer>(comparer, (long[])    (object)keys));
            else if (typeof(TKey) == typeof(int))      prior.SortElements(elements, new GenericSort<int,      Comparer>(comparer, (int[])     (object)keys));
            else if (typeof(TKey) == typeof(DateTime)) prior.SortElements(elements, new GenericSort<DateTime, Comparer>(comparer, (DateTime[])(object)keys));
            else if (typeof(TKey) == typeof(string))   prior.SortElements(elements, new StringSort<Comparer>(comparer, (string[])(object)keys, StringComparer.CurrentCulture));
            else                                       prior.SortElements(elements, new KeySortWithDefaultComparer<TKey, Comparer>(comparer, keys));
        }

        internal static void InitIndexes(Span<int> indexes)
        {
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;
        }

        internal static void OrderByIndex<T>(Span<T> elements, ReadOnlySpan<int> indexes)
        {
            // hmmm. I should work out O(upper bound) for this...
            for (var i = 0; i < indexes.Length; ++i)
            {
                var swapIdx = indexes[i];
                while (swapIdx < i)
                {
                    swapIdx = indexes[swapIdx];
                }
                if (swapIdx != i)
                {
                    var tmp = elements[i];
                    elements[i] = elements[swapIdx];
                    elements[swapIdx] = tmp;
                }
            }
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

        public void SortElements<Comparer>(Span<T> elements, in Comparer comparer)
            where Comparer : IComparer<int>
        {
            TKey[] keys = OrderByImpl.ExtractKeys(elements, _keySelector);

            if (_descending)
            {
                if (_comparer == Comparer<TKey>.Default)
                    OrderByImpl.DescendingDefaultComparer<T, TKey, PriorKeySelector, Comparer>(elements, keys, ref _priorKeySelector, in comparer);
                else
                    _priorKeySelector.SortElements(elements, new KeySortWithComparerDescending<TKey, Comparer>(comparer, keys, _comparer));
            }
            else
            {
                if (_comparer == Comparer<TKey>.Default)
                    OrderByImpl.AscendingDefaultComparer<T, TKey, PriorKeySelector, Comparer>(elements, keys, ref _priorKeySelector, in comparer);
                else
                    _priorKeySelector.SortElements(elements, new KeySortWithComparer<TKey, Comparer>(comparer, keys, _comparer));
            }
        }
    }

    struct StringSort<Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        private readonly StringComparer _comparer;

        Lower _lower;

        string[] _keys;

        public StringSort(in Lower lower, string[] keys, StringComparer comparer)
            => (_lower, _keys, _comparer) = (lower, keys, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct StringSortDescending<Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        private readonly StringComparer _comparer;

        Lower _lower;

        string[] _keys;

        public StringSortDescending(in Lower lower, string[] keys, StringComparer comparer)
            => (_lower, _keys, _comparer) = (lower, keys, comparer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct GenericSort<Type, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
        where Type : IComparable<Type>
    {
        Lower _lower;

        Type[] _keys;

        public GenericSort(in Lower lower, Type[] keys)
            => (_lower, _keys) = (lower, keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _keys[x].CompareTo(_keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct GenericSortDescending<Type, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
        where Type : IComparable<Type>
    {
        Lower _lower;

        Type[] _keys;

        public GenericSortDescending(in Lower lower, Type[] keys)
            => (_lower, _keys) = (lower, keys);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _keys[y].CompareTo(_keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithComparer<TKey, Lower>
        : IComparer<int>
        where Lower : IComparer<int>
    {
        Lower _lower;

        TKey[] _keys;
        IComparer<TKey> _comparer;

        public KeySortWithComparer(in Lower lower, TKey[] keys, IComparer<TKey> comparer)
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

        public KeySortWithDefaultComparer(in Lower lower, TKey[] keys)
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

        public KeySortWithComparerDescending(in Lower lower, TKey[] keys, IComparer<TKey> comparer)
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

        public KeySortWithDefaultComparerDescending(in Lower lower, TKey[] keys)
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

        private T[] GetOrderedArray()
        {
            var array = NodeImpl.ToArray<T, NodeT>(in _nodeT, null, null); // TODO: ArrayPool
            if (array.Length > 1)
                _keySelectors.SortElements(array, new IntSort());
            return array;
        }

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => ArrayNode.Create<T, Nodes<Head, Tail>, CreationType>(GetOrderedArray(), ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new ReversedMemoryNode<T>(GetOrderedArray()));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)GetOrderedArray();
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Skip(new ReadOnlyMemory<T>(GetOrderedArray()), skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var skip = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                MemoryNode.Take(GetOrderedArray(), skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => ArrayNode.FastEnumerate<T, TResult, FEnumerator>(GetOrderedArray(), fenum);
    }
}
