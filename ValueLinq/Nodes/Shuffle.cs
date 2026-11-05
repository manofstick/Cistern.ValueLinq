namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static partial class Shuffle
{
    // Enumerable.Shuffle's contract (learn.microsoft.com/dotnet/api/system.linq.enumerable.shuffle): a sequence whose
    // elements are the source's in randomized order, from a non-cryptographically-secure generator. As in the runtime's
    // ShuffleIterator, the source is buffered and permuted afresh each time it is enumerated or pushed, Count is the
    // source's count with nothing permuted, and a Take of a permutation is a uniform sample of the source.
    [Materialized]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationAny
        , Optimizations.IOptimizationContains<T>
        , IBuffered<T>
        where NodeT : INode<T>
    {
        private NodeT _nodeT;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
        }

        internal SystemArray.Node<T> Materialize(in ChainOptions options) => new(GetShuffledArray());

        // the array is this node's own (a source that holds an array answers ToArray with a copy), so the shuffle is
        // in place
        internal T[] GetShuffledArray()
        {
            var array = NodeImpl.ToArrayByRef<T, NodeT>(ref _nodeT, null, null, false);
            Random.Shared.Shuffle(array);
            return array;
        }

        private bool Count(in Optimizations.Count request, out int response) =>
            BufferedWindow.Count<T, NodeT>(in _nodeT, in request, out response);

        // the shuffled array is the result itself rather than a copy of it
        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = GetShuffledArray();
            return true;
        }

        // the dry run is the source's; the source is copied into the destination and shuffled there, as GetShuffledArray
        // shuffles its own array
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (request.IsDryRun)
                return Optimizations.CopyTo.TryGetCount<T, NodeT>(ref _nodeT, out response);

            response = Optimizations.CopyTo.Copy<T, NodeT>(ref _nodeT, in request);
            Random.Shared.Shuffle(request.Span.Slice(0, response));
            return true;
        }

        // A window of a uniform permutation is a uniform sample of its length in uniform order, wherever it starts. Over a
        // source that copies itself out, a window shorter than it is that many Fisher-Yates steps over a copy, one draw
        // each; over any other, a leading window is a reservoir sample, shuffled, holding only the window's elements, as
        // System.Linq's Shuffle().Take(k). Any other window is a slice of the whole permutation.
        ReadOnlyMemory<T> IBuffered<T>.GetWindow(long skip, long take)
        {
            if (Optimizations.CopyTo.TryGetCount<T, NodeT>(ref _nodeT, out var count))
            {
                var start = (int)Math.Min(skip, count);
                var length = (int)Math.Min(take, count - start);
                if (length < count)
                    return Sample(count, length);
            }
            else if (skip == 0
             && take <= int.MaxValue
             && !(NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out count) && take >= count))
            {
                return _nodeT.CreateViaPush<ReadOnlyMemory<T>, Reservoir<T>, Construct<Reservoir<T>, Reservoir<T>.Args>>(default(ChainOptions), new Construct<Reservoir<T>, Reservoir<T>.Args>(new Reservoir<T>.Args((int)take)));
            }

            return BufferedWindow.Slice(GetShuffledArray(), skip, take);
        }

        // the first length steps of a Fisher-Yates shuffle (Durstenfeld's in-place form, "Algorithm 235: Random
        // permutation", Communications of the ACM 7(7), 1964; Knuth, TAOCP vol. 2, 3.4.2, Algorithm P) over a rented copy
        // of the source: step i swaps in an element drawn uniformly from those not yet picked, so the picks are a uniform
        // sample in uniform order
        private T[] Sample(int count, int length)
        {
            if (length == 0)
                return Array.Empty<T>();

            var pool = ArrayPool<T>.Shared;
            var copy = pool.Rent(count);
            var elements = copy.AsSpan(0, Optimizations.CopyTo.Copy<T, NodeT>(ref _nodeT, new Optimizations.CopyTo<T>(copy)));

            // every slot is written by the loop
            var sample = GC.AllocateUninitializedArray<T>(length);
            for (var i = 0; i < sample.Length; ++i)
            {
                var j = Random.Shared.Next(i, elements.Length);
                sample[i] = elements[j];
                elements[j] = elements[i];
            }

            pool.Return(copy, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
            return sample;
        }

        bool IBuffered<T>.TryContains(long skip, long take, T value, out bool found) => TryContains(skip, take, value, out found);

        // the whole permutation holds its source's elements, so the source answers in its place. Any other window is
        // System.Linq's answer for a sample: a random boolean, true with the probability that a uniform sample of the
        // window's length holds a match, the matches counted in the source with nothing drawn. It is tied to no
        // enumeration of the query, so a Contains and a ToArray of the same query may disagree.
        private bool TryContains(long skip, long take, T value, out bool found)
        {
            if (BufferedWindow.CoversAll<T, NodeT>(in _nodeT, skip, take))
            {
                found = NodeImpl.Contains<T, NodeT>(_nodeT, value);
                return true;
            }

            var (length, matches) = _nodeT.CreateViaPush<(long, long), CountMatches<T>, Construct<CountMatches<T>, CountMatches<T>.Args>>(default(ChainOptions), new Construct<CountMatches<T>, CountMatches<T>.Args>(new CountMatches<T>.Args(value)));
            var size = Math.Min(take, Math.Max(0, length - skip));

            if (matches == 0 || size == 0)
            {
                found = false;
                return true;
            }

            // a sample larger than the non-matches cannot miss every match
            if (size > length - matches)
            {
                found = true;
                return true;
            }

            // the chance no match is drawn: each pick in turn is one of the non-matches left
            var none = 1.0;
            for (var i = 0L; i < size && none > 0; ++i)
                none *= (double)(length - matches - i) / (length - i);

            found = Random.Shared.NextDouble() >= none;
            return true;
        }

        // any index of a permutation holds a uniformly random element, so it is one draw, as System.Linq's: the index is
        // only checked against the length, and each call draws afresh (First and Last may give the same element). A
        // source with a count is asked for a random index; any other is run once, keeping one element by reservoir.
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            if (NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out var count))
            {
                if (request.OffsetIn(count) < 0)
                {
                    response = (false, default!);
                    return true;
                }

                var pick = Random.Shared.Next(count);
                if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(pick), out response))
                    response = _nodeT.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(pick)));
                return true;
            }

            var (seen, element) = _nodeT.CreateViaPush<(long, T), RandomElement<T>, Construct<RandomElement<T>, RandomElement<T>.Args>>(default(ChainOptions), new Construct<RandomElement<T>, RandomElement<T>.Args>(default));
            response = seen <= int.MaxValue && request.OffsetIn((int)seen) >= 0 ? (true, element) : (false, default!);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response) =>
            BufferedWindow.Any<T, NodeT>(in _nodeT, out response);

        private bool Contains(in Optimizations.Contains<T> request, out bool response) =>
            TryContains(0, long.MaxValue, request.Value, out response);

        // a slice of a uniform permutation is a uniform selection, so a window over the permutation, shuffled whole
        // only when the window is run
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            response.SetNode(new BufferedWindow.Node<T, Node<T, NodeT>>(in this, request.Skip, request.Take));
            return true;
        }
    }

    public partial struct Pull<T, NodeT>
        : Optimizations.IPullOptimizationMergeSkipTake<T>
    {
        private SystemArray.Pull<T> _enumerator;

        private static bool MergeSkipTake<TOwnNode, TBuilt, TBuilder>(in Optimizations.MergeSkipTake request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
            where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
        {
            built = BufferedWindow.Window<T, Node<T, NodeT>, TOwnNode, TBuilt, TBuilder>(in node, request.Skip, request.Take, in builder);
            return true;
        }
    }

    // Algorithm L (Li, "Reservoir-Sampling Algorithms of Time Complexity O(n(1 + log(N/n)))", 1994): the first elements
    // fill the reservoir; after that the index of the next element to replace a random slot is drawn ahead, as a
    // geometric skip, so the elements between are passed over with nothing drawn. It keeps each element with the same
    // probabilities as Algorithm R's draw per element, so the reservoir, shuffled, is a uniform sample in uniform order.
    [Aggregator]
    partial struct Reservoir<T>
        : IPushEnumerator<T>
    {
        private readonly int _size;
        private T[] _items;
        private long _seen;

        // the index of the next element taken, once the reservoir is full; and log W, Li's running maximum of the
        // elements' random keys, held as a logarithm so that 1 - W is exact however close W comes to 1
        private long _next;
        private double _logW;

        public Reservoir(int size)
        {
            _size = size;
            _items = new T[Math.Min(size, 16)];
            _seen = 0;
            _next = long.MaxValue;
            _logW = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        // once full, only the elements taken are read: the batch is stepped to each in turn
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var i = 0;
            while (i < span.Length && _seen < _size)
                Fill(span[i++]);

            while (_next - _seen < span.Length - i)
            {
                i += (int)(_next - _seen);
                _seen = _next;
                Take(span[i++]);
                ++_seen;
            }

            _seen += span.Length - i;
            return true;
        }

        public bool ProcessNext(T input)
        {
            if (_seen < _size)
                Fill(input);
            else
            {
                if (_seen == _next)
                    Take(input);
                ++_seen;
            }
            return true;
        }

        private void Fill(T item)
        {
            if (_seen == _items.Length)
                _items = Grow(_items, _size);
            _items[_seen] = item;

            if (++_seen == _size)
            {
                _logW = LogUniform() / _size;
                _next = NextIndex(_seen - 1, _logW);
            }
        }

        // the element at index _seen replaces a uniformly random slot; W shrinks by a further key, and the next index
        // is drawn past this one
        private void Take(T item)
        {
            _items[Random.Shared.Next(_size)] = item;
            _logW += LogUniform() / _size;
            _next = NextIndex(_seen, _logW);
        }

        // the next index taken after index: Li's skip, floor(log U / log(1 - W)), saturating at long.MaxValue
        private static long NextIndex(long index, double logW)
        {
            var skip = Math.Floor(LogUniform() / Math.Log(-double.ExpM1(logW)));
            return skip < long.MaxValue - index - 1 ? index + 1 + (long)skip : long.MaxValue;
        }

        // the log of a uniform draw in the open interval (0, 1), so it is finite and negative
        private static double LogUniform() =>
            Math.Log((Random.Shared.NextInt64((1L << 53) - 1) + 1) * (1.0 / (1L << 53)));

        // out of line: the reservoir grows at most log2(size / 16) times
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T[] Grow(T[] items, int size)
        {
            Array.Resize(ref items, (int)Math.Min((long)items.Length * 2, size));
            return items;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>()
        {
            var sample = _items.AsMemory(0, (int)Math.Min(_seen, _size));
            Random.Shared.Shuffle(sample.Span);
            return Aggregation.Impl.ResultCast<ReadOnlyMemory<T>, TPushResult>(sample);
        }
    }

    // the elements and those equal to the value, counted as (length, matches); a span is counted by MemoryExtensions,
    // vectorised for the default comparer
    [Aggregator]
    partial struct CountMatches<T>
        : IPushEnumerator<T>
    {
        private readonly T _value;
        private long _length;
        private long _matches;

        public CountMatches(T value)
        {
            _value = value;
            _length = 0;
            _matches = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            _length += span.Length;
            _matches += span.Count(_value, null);
            return true;
        }

        public bool ProcessNext(T input)
        {
            ++_length;
            if (EqualityComparer<T>.Default.Equals(_value, input))
                ++_matches;
            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Aggregation.Impl.ResultCast<(long, long), TPushResult>((_length, _matches));
    }

    // a reservoir of one: the element seen n-th replaces the kept one with probability 1 / n, so the one kept is
    // uniformly random; answered as (count seen, element)
    [Aggregator]
    partial struct RandomElement<T>
        : IPushEnumerator<T>
    {
        private long _seen;
        private T? _element;

        public RandomElement()
        {
            _seen = 0;
            _element = default;
        }

        // the span runs from a local copy: Docs/BatchFromLocalCopy.md, "The fix"
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdSpan;
        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            var span = batch.GetSpan();
            var selection = this;
            foreach (var item in span)
                selection.ProcessNext(item);
            this = selection;
            return true;
        }

        public bool ProcessNext(T input)
        {
            if (Random.Shared.NextInt64(++_seen) == 0)
                _element = input;
            return true;
        }

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Aggregation.Impl.ResultCast<(long, T), TPushResult>((_seen, _element!));
    }
}
