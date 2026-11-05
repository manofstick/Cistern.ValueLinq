namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static partial class Where
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationFilteredSpanSource<T>
        , Optimizations.IOptimizationMaterialiseList<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, bool> _predicate;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = Where.ToArray(source.GetSpan(), _predicate, nullIfUnchanged: false)!;
                return true;
            }

            response = default;
            return false;
        }

        private bool MaterialiseList(in Optimizations.MaterialiseList request, [MaybeNullWhen(false)] out List<T> response)
        {
            if (Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = Where.ToList(source.GetSpan(), _predicate);
                return true;
            }

            response = default;
            return false;
        }

        // over a span, an index from the start is a scan counting matches, which runs the predicate as the push would;
        // the last is a scan backwards from the end that stops at the first match, the predicate run on the tail alone
        // and in reverse order, as System.Linq's Where answers Last over an array or list
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            if ((index.IsFromEnd && index.Value != 1)
                || !Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = default;
                return false;
            }

            var span = source.GetSpan();
            var predicate = _predicate;
            if (index.IsFromEnd)
            {
                for (var i = span.Length - 1; i >= 0; --i)
                {
                    if (predicate(span[i]))
                    {
                        response = (true, span[i]);
                        return true;
                    }
                }
            }
            else
            {
                var remaining = index.Value;
                foreach (var item in span)
                {
                    if (predicate(item) && remaining-- == 0)
                    {
                        response = (true, item);
                        return true;
                    }
                }
            }

            response = (false, default!);
            return true;
        }

        // the source's span and the predicate, for a Select above to filter and project in one pass; nothing is run
        private bool FilteredSpanSource(in Optimizations.FilteredSpanSource request, out (Optimizations.SpanSource<T> Source, Func<T, bool> Predicate) response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                response = default;
                return false;
            }

            response = (source, _predicate);
            return true;
        }
    }

    // Masks up to this many are kept on the stack (1 KB, covering 8192 elements); beyond it they are rented.
    private const int StackMaskCount = 256;

    /// <summary>
    /// Filters an ImmutableArray, handing back the source itself when every element is accepted, as nothing can
    /// observe the difference.
    /// </summary>
    internal static ImmutableArray<T> ToImmutableArray<T>(ImmutableArray<T> source, Func<T, bool> predicate)
    {
        var filtered = ToArray(source.AsSpan(), predicate, nullIfUnchanged: true);
        return filtered == null ? source : ImmutableCollectionsMarshal.AsImmutableArray(filtered);
    }

    /// <summary>
    /// Filters a span into an exactly sized array, as FSharp.Core's Array.filter does: the first pass runs the
    /// predicate once per element, in order, recording each result as a bit in a mask per 32 elements and counting
    /// the survivors; the second allocates the result and copies the survivors the masks select. The only temporary
    /// storage is a bit per element, so nothing is allocated but the result unless the source is long enough to
    /// need rented masks. With <paramref name="nullIfUnchanged"/>, null stands for "every element accepted", for a
    /// caller that can hand back its source rather than a copy of it.
    /// </summary>
    internal static T[]? ToArray<T>(ReadOnlySpan<T> source, Func<T, bool> predicate, bool nullIfUnchanged)
    {
        var maskCount = (source.Length + 31) >> 5;
        uint[]? rented = null;
        Span<uint> masks = maskCount <= StackMaskCount
            ? stackalloc uint[StackMaskCount]
            : (rented = System.Buffers.ArrayPool<uint>.Shared.Rent(maskCount));

        try
        {
            int count = CollectMasks(source, predicate, masks);

            if (count == source.Length && nullIfUnchanged)
                return null;

            if (count == 0)
                return Array.Empty<T>();

            if (count == source.Length)
                return source.ToArray();

            // every slot is written by the fill
            var result = GC.AllocateUninitializedArray<T>(count);
            Fill(source, masks[..maskCount], result);
            return result;
        }
        finally
        {
            if (rented != null)
                System.Buffers.ArrayPool<uint>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Filters a span into an exactly sized list in the two passes of <see cref="ToArray{T}"/>, the second writing the
    /// survivors straight into the list's storage.
    /// </summary>
    internal static List<T> ToList<T>(ReadOnlySpan<T> source, Func<T, bool> predicate)
    {
        var maskCount = (source.Length + 31) >> 5;
        uint[]? rented = null;
        Span<uint> masks = maskCount <= StackMaskCount
            ? stackalloc uint[StackMaskCount]
            : (rented = System.Buffers.ArrayPool<uint>.Shared.Rent(maskCount));

        try
        {
            int count = CollectMasks(source, predicate, masks);

            var list = new List<T>(count);
            CollectionsMarshal.SetCount(list, count);
            var result = CollectionsMarshal.AsSpan(list);

            if (count == source.Length)
                source.CopyTo(result);
            else if (count != 0)
                Fill(source, masks[..maskCount], result);

            return list;
        }
        finally
        {
            if (rented != null)
                System.Buffers.ArrayPool<uint>.Shared.Return(rented);
        }
    }

    // the second pass: the survivors the masks select, copied in order into a destination exactly their number long
    private static void Fill<T>(ReadOnlySpan<T> source, ReadOnlySpan<uint> masks, Span<T> result)
    {
        var dst = 0;
        var m = 0;
        foreach (var mask in masks)
        {
            var start = m << 5;
            ++m;

            if (mask == uint.MaxValue)
            {
                source.Slice(start, 32).CopyTo(result.Slice(dst, 32));
                dst += 32;
                continue;
            }

            var msk = mask;
            while (msk != 0)
            {
                result[dst++] = source[start + System.Numerics.BitOperations.TrailingZeroCount(msk)];
                msk &= msk - 1;
            }
        }
    }

    /// <summary>
    /// Filters and projects a span into an exactly sized array in the two passes of <see cref="ToArray{T}"/>: the
    /// predicate runs once per element, in order, then the selector once per survivor, in order. Every predicate call
    /// therefore comes before any selector call, where a fused loop interleaves them.
    /// </summary>
    internal static U[] ToArray<T, U>(ReadOnlySpan<T> source, Func<T, bool> predicate, Func<T, U> selector)
    {
        var maskCount = (source.Length + 31) >> 5;
        uint[]? rented = null;
        Span<uint> masks = maskCount <= StackMaskCount
            ? stackalloc uint[StackMaskCount]
            : (rented = System.Buffers.ArrayPool<uint>.Shared.Rent(maskCount));

        try
        {
            int count = CollectMasks(source, predicate, masks);

            if (count == 0)
                return Array.Empty<U>();

            return getResult(source, count, masks[..maskCount], selector);
        }
        finally
        {
            if (rented != null)
                System.Buffers.ArrayPool<uint>.Shared.Return(rented);
        }

        static U[] getResult(ReadOnlySpan<T> source, int count, ReadOnlySpan<uint> masks, Func<T, U> selector)
        {
            // every slot is written below
            var result = GC.AllocateUninitializedArray<U>(count);
            var dst = 0;
            var m = 0;
            foreach (var mask in masks)
            {
                var start = m << 5;
                ++m;

                if (mask == uint.MaxValue)
                {
                    for (var i = 0; i < 32; ++i)
                        result[dst + i] = selector(source[start + i]);
                    dst += 32;
                    continue;
                }

                var msk = mask;
                while (msk != 0)
                {
                    result[dst++] = selector(source[start + System.Numerics.BitOperations.TrailingZeroCount(msk)]);
                    msk &= msk - 1;
                }
            }
            return result;
        }
    }

    // the first pass: the predicate run once per element, in order, its results recorded as a bit per element in a mask
    // per 32 elements; the number of survivors
    private static int CollectMasks<T>(ReadOnlySpan<T> source, Func<T, bool> predicate, Span<uint> masks)
    {
        var maskCount = (source.Length + 31) >> 5;
        var count = 0;
        for (var m = 0; m < maskCount; ++m)
        {
            var start = m << 5;
            var end = Math.Min(start + 32, source.Length);
            var mask = 0u;
            for (var i = start; i < end; ++i)
            {
                if (predicate(source[i]))
                    mask |= 1u << (i - start);
            }
            masks[m] = mask;
            count += System.Numerics.BitOperations.PopCount(mask);
        }
        return count;
    }

    partial struct Push<T, Next>
    {
        // a cold batch only over a downstream that reads every element, as one does that takes cold
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            BatchKinds.Container
            | BatchKinds.Forwarder
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => ProcessElements(batch.GetSpan());

        // element by element, from a local copy: Docs/BatchFromLocalCopy.md, "The fix"; out of line, as inlined into a
        // large caller it spends the inline budget and leaves the downstream's ProcessNext a call per survivor
        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool ProcessElements(ReadOnlySpan<T> span)
        {
            var next = _next;
            var predicate = _predicate;
            var more = true;
            foreach (var item in span)
            {
                if (predicate(item) && !next.ProcessNext(item))
                {
                    more = false;
                    break;
                }
            }
            _next = next;
            return more;
        }

        public bool ProcessNext(T input)
        {
            if (_predicate(input))
                return _next.ProcessNext(input);
            return true;
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                var current = _enumerator.Current;
                if (_predicate(current))
                {
                    _current = current;
                    return true;
                }
            }
            return false;
        }

        public T Current => _current;
    }
}
