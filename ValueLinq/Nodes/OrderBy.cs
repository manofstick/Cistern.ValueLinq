namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Utils;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Maths;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

public static partial class OrderBy
{
    [Answerer]
    public partial struct Node<TElement, KeySelectors, NodeT>
        : INode<TElement>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<TElement>
        , Optimizations.IOptimizationToArray<TElement>
        , Optimizations.IOptimizationCopyTo<TElement>
        , Optimizations.IOptimizationSkipTake<TElement>
        , Optimizations.IOptimizationAny
        , Optimizations.IOptimizationContains<TElement>
        , Optimizations.IOptimizationLastMatch<TElement>
        , IBuffered<TElement>
        where NodeT : INode<TElement>
        where KeySelectors : IKeySelectors<TElement>
    {
        internal NodeT _nodeT;
        internal KeySelectors _keySelectors;

        public Node(in NodeT nodeT, KeySelectors keySelectors)
        {
            _nodeT = nodeT;
            _keySelectors = keySelectors;
        }

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        internal SystemArray.Node<TElement> Materialize(in ChainOptions options) => new(GetOrderedArray());

        // the elements are read where the source holds them when it has a span, and gathered from there into the result;
        // otherwise from a copy of the source, which is sorted in place. A key selector that writes into a source list
        // is therefore seen by the gather, where System.Linq's reads its own copy
        internal TElement[] GetOrderedArray()
        {
            if (!SortsElementsThemselves()
             && Optimizations.AsSpanSource.TryGet<TElement, NodeT>(ref _nodeT, out var source))
            {
                var elements = source.GetSpan();
                return Gather(elements, 0, elements.Length);
            }

            var array = NodeImpl.ToArrayByRef<TElement, NodeT>(ref _nodeT, null, null, false);
            SortInPlace(array);
            return array;
        }

        // the dry run is the source's; the copy is gathered into the destination as GetOrderedArray gathers into its
        // array, or the source copied in and sorted there
        private bool CopyTo(in Optimizations.CopyTo<TElement> request, out int response)
        {
            if (request.IsDryRun)
                return Optimizations.CopyTo.TryGetCount<TElement, NodeT>(ref _nodeT, out response);

            if (!SortsElementsThemselves()
             && Optimizations.AsSpanSource.TryGet<TElement, NodeT>(ref _nodeT, out var source))
            {
                var elements = source.GetSpan();
                GatherInto(elements, 0, request.Span.Slice(0, elements.Length));
                response = elements.Length;
                return true;
            }

            response = Optimizations.CopyTo.Copy<TElement, NodeT>(ref _nodeT, in request);
            SortInPlace(request.Span.Slice(0, response));
            return true;
        }

        // only the window's elements are put in order, as System.Linq's partial sort does, and only they are gathered
        private ReadOnlyMemory<TElement> GetOrderedWindow(long skip, long take)
        {
            var elements = Elements(out var copy);
            var start = (int)Math.Min(skip, elements.Length);
            var length = (int)Math.Min(take, elements.Length - start);
            var window = length > 0 ? Gather(elements, start, length) : ReadOnlyMemory<TElement>.Empty;
            copy.Return();
            return window;
        }

        // the elements are only read, where the source holds them or from a copy rented for the call, which the caller
        // returns
        private ReadOnlySpan<TElement> Elements(out PooledArray<TElement> copy)
        {
            if (Optimizations.AsSpanSource.TryGet<TElement, NodeT>(ref _nodeT, out var source))
            {
                copy = default;
                return source.GetSpan();
            }

            copy = NodeImpl.ToPooledArrayByRef<TElement, NodeT>(ref _nodeT, null, false);
            return copy.Span;
        }

        // equal elements of an implicitly stable type are bit-identical, so an unstable sort of the elements themselves
        // gives the stable order, as System.Linq's Order sorts them; a window keeps the partial sort
        private bool SortsElementsThemselves() =>
            _keySelectors.IsIdentity(out _) && OrderByImpl.IsImplicitlyStable<TElement>();

        // a single element is in order already, with no key computed for it, as in System.Linq's ToArray and ToList
        private void SortInPlace(Span<TElement> elements)
        {
            if (elements.Length <= 1)
                return;

            if (_keySelectors.IsIdentity(out var descending) && OrderByImpl.IsImplicitlyStable<TElement>())
            {
                elements.Sort();
                if (descending)
                    elements.Reverse();
                return;
            }

            int[]? rented = null;
            Span<int> indexes =
                elements.Length < OrderByImpl.StackIndexes
                    ? stackalloc int[elements.Length]
                    : OrderByImpl.RentIndexes(elements.Length, out rented);
            SortIndexes(elements, indexes, 0, elements.Length);
            OrderByImpl.PermuteByIndex(elements, indexes);
            OrderByImpl.ReturnIndexes(rented);
        }

        // the elements at [start, start + length) of the ordering, in an array of their own; the elements are only read
        private TElement[] Gather(ReadOnlySpan<TElement> elements, int start, int length)
        {
            if (length == 0)
                return Array.Empty<TElement>();

            // every slot is written by the gather
            var gathered = GC.AllocateUninitializedArray<TElement>(length);
            GatherInto(elements, start, gathered);
            return gathered;
        }

        // the elements at [start, start + destination.Length) of the ordering, into the destination; a single element is
        // copied with no key computed for it, as SortInPlace leaves one
        private void GatherInto(ReadOnlySpan<TElement> elements, int start, Span<TElement> destination)
        {
            if (elements.Length <= 1)
            {
                elements.Slice(start, destination.Length).CopyTo(destination);
                return;
            }

            int[]? rented = null;
            Span<int> indexes =
                elements.Length < OrderByImpl.StackIndexes
                    ? stackalloc int[elements.Length]
                    : OrderByImpl.RentIndexes(elements.Length, out rented);
            SortIndexes(elements, indexes, start, destination.Length);
            OrderByImpl.Gather(elements, indexes, start, destination);
            OrderByImpl.ReturnIndexes(rented);
        }

        private TElement ElementAt(ReadOnlySpan<TElement> elements, int offset)
        {
            if (elements.Length <= 1)
                return elements[offset];

            int[]? rented = null;
            Span<int> indexes =
                elements.Length < OrderByImpl.StackIndexes
                    ? stackalloc int[elements.Length]
                    : OrderByImpl.RentIndexes(elements.Length, out rented);
            SortIndexes(elements, indexes, offset, 1);
            var element = elements[indexes[offset]];
            OrderByImpl.ReturnIndexes(rented);
            return element;
        }

        // a sort that throws (a key selector or comparer running user code) leaves the rented buffers unreturned, which
        // costs the pool an array and nothing else
        private void SortIndexes(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length)
        {
            OrderByImpl.InitIndexes(indexes);
            _keySelectors.SortIndexes(elements, indexes, start, length, new IntSort());
        }

        private (bool, TElement) Select(bool last)
        {
            // an integer ordered by itself under the default comparer: equal elements cannot be told apart, so the ends
            // are the least and greatest values, found a vector at a time over the source's span
            if (_keySelectors.IsIdentity(out var descending)
             && IsIntegral()
             && Optimizations.AsSpanSource.TryGet<TElement, NodeT>(ref _nodeT, out var source))
            {
                return Extremum(in source, least: last == descending);
            }

            // an opaque source at a single level is selected from in one loop over its enumerator, rather than pushed
            // into OrderedSelect, whose state goes through memory on every element
            if (typeof(NodeT) == typeof(SystemIEnumerable.Node<TElement>))
            {
                var enumerable = ((SystemIEnumerable.Node<TElement>)(object)_nodeT)._enumerable;
                if (SystemIEnumerable.IsOpaque(enumerable) && _keySelectors.TrySelect(enumerable, last, out var selected))
                    return selected;
            }

            return Select(in _nodeT, last);
        }

        private (bool, TElement) Select<Source>(in Source source, bool last)
            where Source : INode<TElement>
            => source.CreateViaPush<(bool, TElement), OrderedSelect<TElement, KeySelectors>, Construct<OrderedSelect<TElement, KeySelectors>, OrderedSelect<TElement, KeySelectors>.Args>>(default(ChainOptions), new Construct<OrderedSelect<TElement, KeySelectors>, OrderedSelect<TElement, KeySelectors>.Args>(new OrderedSelect<TElement, KeySelectors>.Args(_keySelectors, last)));

        private static bool IsIntegral() =>
            typeof(TElement) == typeof(int)
         || typeof(TElement) == typeof(long)
         || typeof(TElement) == typeof(uint)
         || typeof(TElement) == typeof(ulong)
         || typeof(TElement) == typeof(short)
         || typeof(TElement) == typeof(ushort)
         || typeof(TElement) == typeof(byte)
         || typeof(TElement) == typeof(sbyte);

        private static (bool, TElement) Extremum(in Optimizations.SpanSource<TElement> source, bool least)
        {
            if (typeof(TElement) == typeof(int))
                return Extremum<int>(in source, least);
            if (typeof(TElement) == typeof(long))
                return Extremum<long>(in source, least);
            if (typeof(TElement) == typeof(uint))
                return Extremum<uint>(in source, least);
            if (typeof(TElement) == typeof(ulong))
                return Extremum<ulong>(in source, least);
            if (typeof(TElement) == typeof(short))
                return Extremum<short>(in source, least);
            if (typeof(TElement) == typeof(ushort))
                return Extremum<ushort>(in source, least);
            if (typeof(TElement) == typeof(byte))
                return Extremum<byte>(in source, least);
            return Extremum<sbyte>(in source, least);
        }

        private static (bool, TElement) Extremum<T>(in Optimizations.SpanSource<TElement> source, bool least)
            where T : struct, INumber<T>
        {
            var span = ((Optimizations.SpanSource<T>)(object)source).GetSpan();

            var value = default(T);
            var noData = true;
            if (least)
                SIMD.Min(span, ref value, ref noData);
            else
                SIMD.Max(span, ref value, ref noData);

            return noData ? (false, default!) : (true, (TElement)(object)value);
        }

        // ordering changes none of these, so the source answers in its place and nothing is sorted; the last match is
        // a selection over the source's matches

        private bool Any(in Optimizations.Any request, out bool response) =>
            BufferedWindow.Any<TElement, NodeT>(in _nodeT, out response);

        private bool Contains(in Optimizations.Contains<TElement> request, out bool response) =>
            TryContains(0, long.MaxValue, request.Value, out response);

        private bool LastMatch(in Optimizations.LastMatch<TElement> request, out (bool, TElement) response)
        {
            response = Select(NodeImpl.Where<TElement, NodeT>(in _nodeT, request.Predicate), last: true);
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response) =>
            BufferedWindow.Count<TElement, NodeT>(in _nodeT, in request, out response);

        // the ends of an ordering are a selection, not a sort: the first is the earliest element with the least
        // key, the last the latest with the greatest; any other index is one quickselect over the buffered source, as
        // System.Linq's. Reverse is not answered, as it could only be by sorting here, which a declined request would
        // then repeat; a Reverse above asks for the mirrored index instead.
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, TElement) response)
        {
            var index = request.Index;
            if (index.Value == (index.IsFromEnd ? 1 : 0))
            {
                response = Select(last: index.IsFromEnd);
                return true;
            }

            var elements = Elements(out var copy);
            var offset = request.OffsetIn(elements.Length);
            response = offset < 0 ? (false, default!) : (true, ElementAt(elements, offset));
            copy.Return();
            return true;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out TElement[] response)
        {
            response = GetOrderedArray();
            return true;
        }

        ReadOnlyMemory<TElement> IBuffered<TElement>.GetWindow(long skip, long take) => GetOrderedWindow(skip, take);

        bool IBuffered<TElement>.TryContains(long skip, long take, TElement value, out bool found) => TryContains(skip, take, value, out found);

        // the whole ordering holds its source's elements, so the source answers in its place; any other window's
        // elements are found by the selection that running the window does, so it is declined
        private bool TryContains(long skip, long take, TElement value, out bool found)
        {
            if (BufferedWindow.CoversAll<TElement, NodeT>(in _nodeT, skip, take))
            {
                found = NodeImpl.Contains<TElement, NodeT>(_nodeT, value);
                return true;
            }

            found = false;
            return false;
        }

        // a window over the ordering, its elements ordered only when the window is run
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<TElement> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new BufferedWindow.Node<TElement, Node<TElement, KeySelectors, NodeT>>(in this, request.Skip, request.Take));
            return true;
        }

        // the ordering lives only as long as the push, so it is buffered in a rented array, built as GetOrderedArray builds
        // its own
        TPushResult INode<TElement>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            PooledArray<TElement> sorted;
            if (!SortsElementsThemselves()
             && Optimizations.AsSpanSource.TryGet<TElement, NodeT>(ref _nodeT, out var source))
            {
                var elements = source.GetSpan();
                sorted = elements.Length == 0
                    ? PooledArray<TElement>.Owned(Array.Empty<TElement>())
                    : PooledArray<TElement>.Rent(elements.Length, null, false);
                GatherInto(elements, 0, sorted.Span);
            }
            else
            {
                sorted = NodeImpl.ToPooledArrayByRef<TElement, NodeT>(ref _nodeT, null, false);
                SortInPlace(sorted.Span);
            }

            return SystemArray.ExecutePush<TElement, TPushResult, TPushEnumerator>(sorted, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
        }
    }

    // sorted on the first MoveNext, as System.Linq's OrderBy sorts, so a string key compares under the culture current
    // at that call
    [Puller]
    public partial struct Pull<TElement, KeySelectors, NodeT>
        : Optimizations.IPullOptimizationMergeSkipTake<TElement>
        where NodeT : INode<TElement>
        where KeySelectors : IKeySelectors<TElement>
    {
        private enum State : byte
        {
            Unsorted,
            Sorted,
            Disposed,
        }

        private Node<TElement, KeySelectors, NodeT> _node;
        private SystemArray.Pull<TElement> _enumerator;
        private State _state;

        private Pull(in Node<TElement, KeySelectors, NodeT> node)
        {
            _node = node;
            _enumerator = default;
            _state = State.Unsorted;
        }

        private static Pull<TElement, KeySelectors, NodeT> Create(in Node<TElement, KeySelectors, NodeT> node) => new(in node);

        static bool IPullEnumerator<TElement>.RebuildChanges => false;

        static TBuilt IPullEnumerator<TElement>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder) =>
            ChainBuilder.Identity<TElement, Node<TElement, KeySelectors, NodeT>, Pull<TElement, KeySelectors, NodeT>, TOwnNode, TBuilt, TBuilder>(in node, in builder);

        private static bool MergeSkipTake<TOwnNode, TBuilt, TBuilder>(in Optimizations.MergeSkipTake request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
            where TBuilder : IChainBuilder<TElement, TBuilt>, allows ref struct
        {
            built = BufferedWindow.Window<TElement, Node<TElement, KeySelectors, NodeT>, TOwnNode, TBuilt, TBuilder>(in node, request.Skip, request.Take, in builder);
            return true;
        }

        public bool MoveNext()
        {
            if (_state == State.Unsorted)
            {
                var sorted = _node.Materialize(default);

                _enumerator = Helper.CreatePull<SystemArray.Node<TElement>, SystemArray.Pull<TElement>>(in sorted);
                _state = State.Sorted;
            }

            var moved =
                _state == State.Sorted
                && _enumerator.MoveNext();

            return moved;
        }

        public TElement Current => _enumerator.Current;

        public void Dispose() => (_node, _enumerator, _state) = (default, default, State.Disposed);
    }

    internal static class Identity<TElement>
    {
        internal static readonly Func<TElement, TElement> KeySelector = static element => element;
    }

    public interface IKeySelectors<TElement>
    {
        // indexes, 0 to elements.Length - 1 on entry, are ordered so indexes[start..start + length] index the window's
        // elements in order, the others in no order; the whole span sorts them all. The elements are only read
        void SortIndexes<Comparer>(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length, in Comparer comparer)
            where Comparer : IComparer<int>;

        // Selection without sorting: an element is compared with the current best, the primary key first, each level
        // keeping the best's key so an element's key at a level is computed at most once, and only when every level
        // above it ties.
        int CompareWithBest(TElement element, TElement best);
        void PromoteLast();
        void ResetBest();

        // a span selected into best in one loop, true; false from a chain of levels, which selects through CompareWithBest
        bool TrySelect(ReadOnlySpan<TElement> elements, bool last, ref TElement? best, ref bool found);

        // an opaque source selected from in one loop over its enumerator, true; false, having read nothing, from a
        // chain of levels
        bool TrySelect(IEnumerable<TElement> source, bool last, out (bool, TElement) selected);

        // the elements are their own keys under the default comparer, at a single level (Order, OrderDescending)
        bool IsIdentity(out bool descending);
    }

    public readonly struct KeySelectorsRoot<TElement>
        : IKeySelectors<TElement>
    {
        public bool IsIdentity(out bool descending)
        {
            descending = false;
            return false;
        }

        public int CompareWithBest(TElement element, TElement best) => 0;
        public void PromoteLast() { }
        public void ResetBest() { }
        public bool TrySelect(ReadOnlySpan<TElement> elements, bool last, ref TElement? best, ref bool found) => false;

        public bool TrySelect(IEnumerable<TElement> source, bool last, out (bool, TElement) selected)
        {
            selected = default;
            return false;
        }

        // the index map is sorted alone, not alongside the elements: Span<TKey>.Sort(Span<TValue>, TComparer) returns a
        // mis-ordered result on .NET 11 RC2 (11.0.0-rc.2.26504.105) when TComparer is a struct
        public void SortIndexes<Comparer>(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length, in Comparer comparer)
            where Comparer : IComparer<int>
        {
            if (length < indexes.Length)
                OrderByImpl.PartialSort(indexes, start, length, comparer);
            else
                indexes.Sort(comparer);
        }
    }

    static class OrderByImpl
    {
        // rented, at least elements.Length long; ReturnKeys once the sort is done with it
        internal static TKey[] ExtractKeys<TElement, TKey>(ReadOnlySpan<TElement> elements, Func<TElement, TKey> keySelector)
        {
            var keys = ArrayPool<TKey>.Shared.Rent(elements.Length);
            for (var i = 0; i < elements.Length; ++i)
                keys[i] = keySelector(elements[i]);
            return keys;
        }

        internal static void ReturnKeys<TKey>(TKey[] keys) =>
            ArrayPool<TKey>.Shared.Return(keys, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<TKey>());

        internal static void DescendingDefaultComparer<TElement, TKey, PriorKeySelector, Comparer>(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length, TKey[] keys, ref PriorKeySelector prior, in Comparer comparer)
            where Comparer : IComparer<int>
            where PriorKeySelector : IKeySelectors<TElement>
        {
                 if (typeof(TKey) == typeof(double))   prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<double,   Comparer>(comparer, (double[])  (object)keys));
            else if (typeof(TKey) == typeof(float))    prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<float,    Comparer>(comparer, (float[])   (object)keys));
            else if (typeof(TKey) == typeof(decimal))  prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<decimal,  Comparer>(comparer, (decimal[]) (object)keys));
            else if (typeof(TKey) == typeof(long))     prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<long,     Comparer>(comparer, (long[])    (object)keys));
            else if (typeof(TKey) == typeof(int))      prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<int,      Comparer>(comparer, (int[])     (object)keys));
            else if (typeof(TKey) == typeof(DateTime)) prior.SortIndexes(elements, indexes, start, length, new GenericSortDescending<DateTime, Comparer>(comparer, (DateTime[])(object)keys));
            else if (typeof(TKey) == typeof(string))   prior.SortIndexes(elements, indexes, start, length, new StringSortDescending<Comparer>(comparer, (string[])(object)keys, StringComparer.CurrentCulture));
            else                                       prior.SortIndexes(elements, indexes, start, length, new KeySortWithDefaultComparerDescending<TKey, Comparer>(comparer, keys));
        }

        internal static void AscendingDefaultComparer<TElement, TKey, PriorKeySelector, Comparer>(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length, TKey[] keys, ref PriorKeySelector prior, in Comparer comparer)
            where Comparer : IComparer<int>
            where PriorKeySelector : IKeySelectors<TElement>
        {
                 if (typeof(TKey) == typeof(double))   prior.SortIndexes(elements, indexes, start, length, new GenericSort<double,   Comparer>(comparer, (double[])  (object)keys));
            else if (typeof(TKey) == typeof(float))    prior.SortIndexes(elements, indexes, start, length, new GenericSort<float,    Comparer>(comparer, (float[])   (object)keys));
            else if (typeof(TKey) == typeof(decimal))  prior.SortIndexes(elements, indexes, start, length, new GenericSort<decimal,  Comparer>(comparer, (decimal[]) (object)keys));
            else if (typeof(TKey) == typeof(long))     prior.SortIndexes(elements, indexes, start, length, new GenericSort<long,     Comparer>(comparer, (long[])    (object)keys));
            else if (typeof(TKey) == typeof(int))      prior.SortIndexes(elements, indexes, start, length, new GenericSort<int,      Comparer>(comparer, (int[])     (object)keys));
            else if (typeof(TKey) == typeof(DateTime)) prior.SortIndexes(elements, indexes, start, length, new GenericSort<DateTime, Comparer>(comparer, (DateTime[])(object)keys));
            else if (typeof(TKey) == typeof(string))   prior.SortIndexes(elements, indexes, start, length, new StringSort<Comparer>(comparer, (string[])(object)keys, StringComparer.CurrentCulture));
            else                                       prior.SortIndexes(elements, indexes, start, length, new KeySortWithDefaultComparer<TKey, Comparer>(comparer, keys));
        }

        internal static void InitIndexes(Span<int> indexes)
        {
            for (var i = 0; i < indexes.Length; ++i)
                indexes[i] = i;
        }

        // the comparer orders every pair of indexes (ties fall to the index), so the order of the window's elements is
        // the one a full sort gives; one element is a selection, more a partial quicksort, O(n + k log k) on average
        internal static void PartialSort<Comparer>(Span<int> indexes, int start, int length, Comparer comparer)
            where Comparer : IComparer<int>
        {
            if (length == 1)
                SelectAt(indexes, start, comparer);
            else
                PartialQuickSort(indexes, 0, indexes.Length - 1, start, start + length - 1, comparer);
        }

        // ported from System.Linq's EnumerableSorter.PartialQuickSort (dotnet/runtime, MIT; THIRD-PARTY-NOTICES.TXT): a
        // partition is recursed into only where it overlaps [minIdx, maxIdx], so that range ends sorted and the rest
        // partitioned around it
        private static void PartialQuickSort<Comparer>(Span<int> indexes, int left, int right, int minIdx, int maxIdx, Comparer comparer)
            where Comparer : IComparer<int>
        {
            do
            {
                var i = left;
                var j = right;
                var pivot = indexes[i + ((j - i) >> 1)];
                do
                {
                    while (i < indexes.Length && comparer.Compare(pivot, indexes[i]) > 0)
                        ++i;
                    while (j >= 0 && comparer.Compare(pivot, indexes[j]) < 0)
                        --j;

                    if (i > j)
                        break;

                    if (i < j)
                        (indexes[i], indexes[j]) = (indexes[j], indexes[i]);

                    ++i;
                    --j;
                }
                while (i <= j);

                if (minIdx >= i)
                    left = i + 1;
                else if (maxIdx <= j)
                    right = j - 1;

                if (j - left <= right - i)
                {
                    if (left < j)
                        PartialQuickSort(indexes, left, j, minIdx, maxIdx, comparer);

                    left = i;
                }
                else
                {
                    if (i < right)
                        PartialQuickSort(indexes, i, right, minIdx, maxIdx, comparer);

                    right = j;
                }
            }
            while (left < right);
        }

        // quickselect: indexes[n] becomes the index a sort puts there, those before it order before it, those after after
        private static void SelectAt<Comparer>(Span<int> indexes, int n, Comparer comparer)
            where Comparer : IComparer<int>
        {
            var low = 0;
            var high = indexes.Length - 1;
            while (low < high)
            {
                var pivot = indexes[low + (high - low) / 2];
                var i = low;
                var j = high;
                while (i <= j)
                {
                    while (comparer.Compare(indexes[i], pivot) < 0)
                        ++i;
                    while (comparer.Compare(indexes[j], pivot) > 0)
                        --j;
                    if (i <= j)
                    {
                        (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
                        ++i;
                        --j;
                    }
                }

                if (n <= j)
                    high = j;
                else if (n >= i)
                    low = i;
                else
                    return;
            }
        }

        // MAGIC! 200 is that reasonable?
        internal const int StackIndexes = 200;

        internal static Span<int> RentIndexes(int length, out int[]? rented) =>
            (rented = ArrayPool<int>.Shared.Rent(length)).AsSpan(0, length);

        internal static void ReturnIndexes(int[]? rented)
        {
            if (rented is not null)
                ArrayPool<int>.Shared.Return(rented);
        }

        internal static void Gather<TElement>(ReadOnlySpan<TElement> elements, ReadOnlySpan<int> indexes, int start, Span<TElement> destination)
        {
            for (var i = 0; i < destination.Length; ++i)
                destination[i] = elements[indexes[start + i]];
        }

        // the elements put in the order of the sorted index map
        internal static void PermuteByIndex<TElement>(Span<TElement> elements, ReadOnlySpan<int> indexes)
        {
            // I haven't worked out the algorithmic complexity of OrderByImpl.OrderByIndex, so I'm only using it
            // when collections aren't too large...
            if (elements.Length < 500) // MAGIC! but is 500 reasonable?
            {
                OrderByIndex(elements, indexes);
                return;
            }

            // the gather is O(n) where OrderByIndex's in-place permutation is O(n^2) in its worst case
            var pool = ArrayPool<TElement>.Shared;
            var copy = pool.Rent(elements.Length);
            elements.CopyTo(copy);
            for (var i = 0; i < elements.Length; ++i)
                elements[i] = copy[indexes[i]];
            pool.Return(copy, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<TElement>());
        }

        // System.Linq's TypeIsImplicitlyStable; every enum's underlying type is one of these
        internal static bool IsImplicitlyStable<TElement>() =>
            typeof(TElement) == typeof(int)
         || typeof(TElement) == typeof(long)
         || typeof(TElement) == typeof(uint)
         || typeof(TElement) == typeof(ulong)
         || typeof(TElement) == typeof(short)
         || typeof(TElement) == typeof(ushort)
         || typeof(TElement) == typeof(byte)
         || typeof(TElement) == typeof(sbyte)
         || typeof(TElement) == typeof(nint)
         || typeof(TElement) == typeof(nuint)
         || typeof(TElement) == typeof(Int128)
         || typeof(TElement) == typeof(UInt128)
         || typeof(TElement) == typeof(char)
         || typeof(TElement) == typeof(bool)
         || typeof(TElement).IsEnum;

        internal static void OrderByIndex<TElement>(Span<TElement> elements, ReadOnlySpan<int> indexes)
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

    public struct KeySelectors<TElement, TKey, PriorKeySelector>
        : IKeySelectors<TElement>
        where PriorKeySelector : IKeySelectors<TElement>
    {
        PriorKeySelector _priorKeySelector;

        Func<TElement, TKey> _keySelector;
        IComparer<TKey> _comparer;
        bool _descending;

        TKey? _bestKey;
        bool _hasBestKey;
        TKey? _lastKey;
        bool _hasLastKey;

        public KeySelectors(PriorKeySelector priorKeySelector, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            _priorKeySelector = priorKeySelector;
            _keySelector = keySelector;
            _comparer = comparer;
            _descending = descending;
            _bestKey = default;
            _hasBestKey = false;
            _lastKey = default;
            _hasLastKey = false;
        }

        public bool IsIdentity(out bool descending)
        {
            descending = _descending;
            return typeof(PriorKeySelector) == typeof(KeySelectorsRoot<TElement>)
                && (object)_keySelector == Identity<TElement>.KeySelector
                && _comparer == Comparer<TKey>.Default;
        }

        public int CompareWithBest(TElement element, TElement best)
        {
            _hasLastKey = false;

            var prior = _priorKeySelector.CompareWithBest(element, best);
            if (prior != 0)
                return prior;

            if (!_hasBestKey)
            {
                _bestKey = _keySelector(best);
                _hasBestKey = true;
            }

            _lastKey = _keySelector(element);
            _hasLastKey = true;

            // the same comparison the sort makes, descending by swapping the operands
            return _descending
                ? _comparer.Compare(_bestKey!, _lastKey)
                : _comparer.Compare(_lastKey, _bestKey!);
        }

        // the element last compared becomes the best; a level that never computed its key recomputes it when next asked
        public void PromoteLast()
        {
            _priorKeySelector.PromoteLast();
            _bestKey = _lastKey;
            _hasBestKey = _hasLastKey;
        }

        public void ResetBest()
        {
            _priorKeySelector.ResetBest();
            _hasBestKey = false;
            _hasLastKey = false;
        }

        // at a single level, the loop holds the best and its key in locals and writes no field, so the JIT checks the
        // delegate and the comparer once before it rather than once per element; CompareWithBest's comparisons, and
        // like it the first element's key computed only once a second element arrives
        public bool TrySelect(ReadOnlySpan<TElement> elements, bool last, ref TElement? best, ref bool found)
        {
            if (typeof(PriorKeySelector) != typeof(KeySelectorsRoot<TElement>))
                return false;

            if (!found && !elements.IsEmpty)
            {
                (found, best) = (true, elements[0]);
                elements = elements[1..];
            }

            if (elements.IsEmpty)
                return true;

            if (!_hasBestKey)
            {
                _bestKey = _keySelector(best!);
                _hasBestKey = true;
            }

            _bestKey =
                _comparer == Comparer<TKey>.Default
                    ? Select(elements, last, new DefaultKeyComparer<TKey>(), ref best)
                    : Select(elements, last, new KeyComparer<TKey>(_comparer), ref best);
            return true;
        }

        private TKey Select<TComparer>(ReadOnlySpan<TElement> elements, bool last, TComparer comparer, ref TElement? best)
            where TComparer : struct, IComparer<TKey>
            => (last, _descending) switch
            {
                (false, false) => Select<FirstAscending, TComparer>(elements, _keySelector, comparer, ref best, _bestKey!),
                (false, true)  => Select<FirstDescending, TComparer>(elements, _keySelector, comparer, ref best, _bestKey!),
                (true, false)  => Select<LastAscending, TComparer>(elements, _keySelector, comparer, ref best, _bestKey!),
                (true, true)   => Select<LastDescending, TComparer>(elements, _keySelector, comparer, ref best, _bestKey!),
            };

        private static TKey Select<TEnd, TComparer>(ReadOnlySpan<TElement> elements, Func<TElement, TKey> keySelector, TComparer comparer, ref TElement? best, TKey bestKey)
            where TEnd : struct, ISelectedEnd
            where TComparer : struct, IComparer<TKey>
        {
            var selected = best;
            foreach (var element in elements)
            {
                var key = keySelector(element);
                var order = TEnd.Descending ? comparer.Compare(bestKey, key) : comparer.Compare(key, bestKey);
                if (TEnd.Last ? order >= 0 : order < 0)
                    (selected, bestKey) = (element, key);
            }

            best = selected;
            return bestKey;
        }

        public bool TrySelect(IEnumerable<TElement> source, bool last, out (bool, TElement) selected)
        {
            if (typeof(PriorKeySelector) != typeof(KeySelectorsRoot<TElement>))
            {
                selected = default;
                return false;
            }

            selected = SelectOpaque(source, last);
            return true;
        }

        // out of line, as First and Last are inlined into their callers; the identity key (Order) is the element
        // itself, with no delegate call
        [MethodImpl(MethodImplOptions.NoInlining)]
        private (bool, TElement) SelectOpaque(IEnumerable<TElement> source, bool last) =>
            typeof(TKey) == typeof(TElement) && (object)_keySelector == Identity<TElement>.KeySelector
                ? SelectOpaque(source, last, new IdentityAsKey<TElement, TKey>())
                : SelectOpaque(source, last, new FuncToIFunc<TElement, TKey>(_keySelector));

        private (bool, TElement) SelectOpaque<TKeyOf>(IEnumerable<TElement> source, bool last, TKeyOf keyOf)
            where TKeyOf : struct, IFunc<TElement, TKey>
            => _comparer == Comparer<TKey>.Default
                ? SelectOpaque(source, last, keyOf, new DefaultKeyComparer<TKey>())
                : SelectOpaque(source, last, keyOf, new KeyComparer<TKey>(_comparer));

        private (bool, TElement) SelectOpaque<TKeyOf, TComparer>(IEnumerable<TElement> source, bool last, TKeyOf keyOf, TComparer comparer)
            where TKeyOf : struct, IFunc<TElement, TKey>
            where TComparer : struct, IComparer<TKey>
            => (last, _descending) switch
            {
                (false, false) => SelectOpaque<FirstAscending, TComparer, TKeyOf>(source, keyOf, comparer),
                (false, true)  => SelectOpaque<FirstDescending, TComparer, TKeyOf>(source, keyOf, comparer),
                (true, false)  => SelectOpaque<LastAscending, TComparer, TKeyOf>(source, keyOf, comparer),
                (true, true)   => SelectOpaque<LastDescending, TComparer, TKeyOf>(source, keyOf, comparer),
            };

        // the span loop's comparisons, with the best and its key in locals; the first element's key is computed only
        // once a second element arrives, as the span loop's is
        private static (bool, TElement) SelectOpaque<TEnd, TComparer, TKeyOf>(IEnumerable<TElement> source, TKeyOf keyOf, TComparer comparer)
            where TEnd : struct, ISelectedEnd
            where TComparer : struct, IComparer<TKey>
            where TKeyOf : struct, IFunc<TElement, TKey>
        {
            using var e = source.GetEnumerator();
            if (!e.MoveNext())
                return (false, default!);

            var best = e.Current;
            if (!e.MoveNext())
                return (true, best);

            var bestKey = keyOf.Invoke(best);
            do
            {
                var element = e.Current;
                var key = keyOf.Invoke(element);
                var order = TEnd.Descending ? comparer.Compare(bestKey, key) : comparer.Compare(key, bestKey);
                if (TEnd.Last ? order >= 0 : order < 0)
                    (best, bestKey) = (element, key);
            }
            while (e.MoveNext());

            return (true, best);
        }

        public void SortIndexes<Comparer>(ReadOnlySpan<TElement> elements, Span<int> indexes, int start, int length, in Comparer comparer)
            where Comparer : IComparer<int>
        {
            TKey[] keys = OrderByImpl.ExtractKeys(elements, _keySelector);

            if (_descending)
            {
                if (_comparer == Comparer<TKey>.Default)
                    OrderByImpl.DescendingDefaultComparer<TElement, TKey, PriorKeySelector, Comparer>(elements, indexes, start, length, keys, ref _priorKeySelector, in comparer);
                else
                    _priorKeySelector.SortIndexes(elements, indexes, start, length, new KeySortWithComparerDescending<TKey, Comparer>(comparer, keys, _comparer));
            }
            else
            {
                if (_comparer == Comparer<TKey>.Default)
                    OrderByImpl.AscendingDefaultComparer<TElement, TKey, PriorKeySelector, Comparer>(elements, indexes, start, length, keys, ref _priorKeySelector, in comparer);
                else
                    _priorKeySelector.SortIndexes(elements, indexes, start, length, new KeySortWithComparer<TKey, Comparer>(comparer, keys, _comparer));
            }

            // as the index map, left unreturned by a key selector or comparer that throws
            OrderByImpl.ReturnKeys(keys);
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
        {
            _lower = lower;
            _keys = keys;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct StringSortDescending<TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
    {
        private readonly StringComparer _comparer;

        TLowerComparer _lower;

        string[] _keys;

        public StringSortDescending(in TLowerComparer lower, string[] keys, StringComparer comparer)
        {
            _lower = lower;
            _keys = keys;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct GenericSort<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
        where TKey : IComparable<TKey>
    {
        TLowerComparer _lower;

        TKey[] _keys;

        public GenericSort(in TLowerComparer lower, TKey[] keys)
        {
            _lower = lower;
            _keys = keys;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _keys[x].CompareTo(_keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct GenericSortDescending<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
        where TKey : IComparable<TKey>
    {
        TLowerComparer _lower;

        TKey[] _keys;

        public GenericSortDescending(in TLowerComparer lower, TKey[] keys)
        {
            _lower = lower;
            _keys = keys;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _keys[y].CompareTo(_keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithComparer<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
    {
        TLowerComparer _lower;

        TKey[] _keys;
        IComparer<TKey> _comparer;

        public KeySortWithComparer(in TLowerComparer lower, TKey[] keys, IComparer<TKey> comparer)
        {
            _lower = lower;
            _keys = keys;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithDefaultComparer<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
    {
        TLowerComparer _lower;

        TKey[] _keys;

        public KeySortWithDefaultComparer(in TLowerComparer lower, TKey[] keys)
        {
            _lower = lower;
            _keys = keys;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            Comparer<TKey>.Default.Compare(_keys[x], _keys[y]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithComparerDescending<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
    {
        TLowerComparer _lower;

        TKey[] _keys;
        IComparer<TKey> _comparer;

        public KeySortWithComparerDescending(in TLowerComparer lower, TKey[] keys, IComparer<TKey> comparer)
        {
            _lower = lower;
            _keys = keys;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            _comparer.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    struct KeySortWithDefaultComparerDescending<TKey, TLowerComparer>
        : IComparer<int>
        where TLowerComparer : IComparer<int>
    {
        TLowerComparer _lower;

        TKey[] _keys;

        public KeySortWithDefaultComparerDescending(in TLowerComparer lower, TKey[] keys)
        {
            _lower = lower;
            _keys = keys;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IComparer<int>.Compare(int x, int y) =>
            Comparer<TKey>.Default.Compare(_keys[y], _keys[x]) switch
            {
                0 => _lower.Compare(x, y),
                var c => c
            };
    }

    // the first or last element of an ordering in one pass: a later element replaces the best for First only when it
    // orders strictly before it (the sort is stable, so the earliest of equals comes first), for Last when it does not
    // order before it (the latest of equals comes last)
    [Aggregator]
    partial struct OrderedSelect<TElement, KeySelectors>
        : IPushEnumerator<TElement>
        where KeySelectors : IKeySelectors<TElement>
    {
        private KeySelectors _keySelectors;
        private readonly bool _last;
        private TElement? _best;
        private bool _found;

        public OrderedSelect(in KeySelectors keySelectors, bool last)
        {
            _keySelectors = keySelectors;
            _last = last;
            _best = default;
            _found = false;
            _keySelectors.ResetBest();
        }

        // the span runs from a local copy: Docs/BatchFromLocalCopy.md, "The fix"
        static BatchKinds IPushEnumerator<TElement>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<TElement>.ProcessBatch(scoped ref Batch<TElement> batch)
        {
            var span = batch.GetSpan();
            if (_keySelectors.TrySelect(span, _last, ref _best, ref _found))
                return true;

            var selection = this;
            foreach (var input in span)
                selection.ProcessNext(input);
            this = selection;
            return true;
        }

        public TPushResult GetResult<TPushResult>() => Aggregation.Impl.ResultCast<(bool, TElement), TPushResult>((_found, _best!));

        public bool ProcessNext(TElement input)
        {
            if (!_found)
            {
                (_found, _best) = (true, input);
                return true;
            }

            var order = _keySelectors.CompareWithBest(input, _best!);
            if (_last ? order >= 0 : order < 0)
            {
                _best = input;
                _keySelectors.PromoteLast();
            }
            return true;
        }
    }

    // the end a selection keeps, as a type, so each of its loops is compiled for one end and folds the tests away
    interface ISelectedEnd
    {
        static abstract bool Last { get; }
        static abstract bool Descending { get; }
    }

    readonly struct FirstAscending : ISelectedEnd
    {
        public static bool Last => false;
        public static bool Descending => false;
    }

    readonly struct FirstDescending : ISelectedEnd
    {
        public static bool Last => false;
        public static bool Descending => true;
    }

    readonly struct LastAscending : ISelectedEnd
    {
        public static bool Last => true;
        public static bool Descending => false;
    }

    readonly struct LastDescending : ISelectedEnd
    {
        public static bool Last => true;
        public static bool Descending => true;
    }

    // the default comparer called directly, so a value type's comparison is inlined with no guard
    readonly struct DefaultKeyComparer<TKey>
        : IComparer<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(TKey? x, TKey? y) => Comparer<TKey>.Default.Compare(x!, y!);
    }

    // the element as its own key, where TKey is TElement
    readonly struct IdentityAsKey<TElement, TKey>
        : IFunc<TElement, TKey>
    {
        public TKey Invoke(TElement element) => (TKey)(object)element!;
    }

    readonly struct KeyComparer<TKey>(IComparer<TKey> comparer)
        : IComparer<TKey>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(TKey? x, TKey? y) => comparer.Compare(x, y);
    }

    readonly struct IntSort
        : IComparer<int>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(int x, int y) => x.CompareTo(y);
    }
}
