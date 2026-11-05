using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Utils
{
    // The elements of a sequence of unknown length, gathered as System.Linq's SegmentedArrayBuilder gathers them: a
    // scratch buffer first, then segments rented from the shared pool, each double the last, so the exactly sized result
    // is the only allocation. It is held by value in the aggregator at the end of the push chain.
    internal struct SegmentedArrayBuilder<T>
    {
        public const int ScratchLength = 8;
        private const int MinimumSegmentLength = 16;

        [InlineArray(ScratchLength)]
        private struct Scratch
        {
            private T _element;
        }

        // 27 doublings from 16 pass Array.MaxLength
        private const int MaximumSegments = 27;

        private Scratch _scratch;
        private int _scratchCount;
        // null while the scratch fills
        private T[]? _current;
        private int _index;
        private int _countInFull;
        // the segments left behind, with the elements in each, which a collection copied whole may leave short of full;
        // rented when the first is left, so a frame holding the builder zeroes a reference rather than 27 of them
        private (T[] Segment, int Count)[]? _full;
        private int _fullCount;

        public readonly int Count => _countInFull + _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            var current = _current;
            var index = _index;
            if (current is not null)
            {
                if ((uint)index < (uint)current.Length)
                {
                    current[index] = item;
                    _index = index + 1;
                    return;
                }
            }
            else if ((uint)index < ScratchLength)
            {
                _scratch[index] = item;
                _index = index + 1;
                return;
            }

            AddSlow(item);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void AddSlow(T item)
        {
            Expand();
            _current![0] = item;
            _index = 1;
        }

        // an enumerator's elements, read in a loop of the builder's own: Add inlines into it even over a reference type,
        // where a push loop in a generic method calls the aggregator's ProcessNext through a dictionary lookup per element
        public void AddRange(IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                Add(enumerator.Current);
        }

        public void AddRange(ReadOnlySpan<T> span)
        {
            while (!span.IsEmpty)
            {
                Span<T> current = _current is null ? _scratch : _current;
                if (_index == current.Length)
                {
                    Expand();
                    continue;
                }

                var count = Math.Min(span.Length, current.Length - _index);
                span[..count].CopyTo(current[_index..]);
                _index += count;
                span = span[count..];
            }
        }

        // a batch's elements, a cold one copied by its collection straight into a segment with room for all of them
        public void AddRange(scoped ref Batch<T> batch)
        {
            if (!batch.IsCold)
            {
                AddRange(batch.GetSpan());
                return;
            }

            var count = batch.Count;
            if (count == 0)
                return;

            if (_current is null || _current.Length - _index < count)
                Expand(count);

            batch.CopyTo(_current!, _index);
            _index += count;
        }

        // the current buffer is left as it stands: the next is a rented segment of twice its length, or of minimum
        private void Expand(int minimum = 0)
        {
            var currentLength = _current is null ? ScratchLength : _current.Length;
            if (_current is null)
                _scratchCount = _index;
            else
            {
                _full ??= ArrayPool<(T[], int)>.Shared.Rent(MaximumSegments);
                _full[_fullCount++] = (_current, _index);
            }
            _countInFull += _index;

            var length = (int)Math.Min(Math.Max(Math.Max(MinimumSegmentLength, (long)currentLength * 2), minimum), Array.MaxLength - (long)_countInFull);
            if (length <= 0 || length < minimum)
                throw new OutOfMemoryException();

            _current = ArrayPool<T>.Shared.Rent(length);
            _index = 0;
        }

        public readonly void CopyTo(Span<T> destination)
        {
            ReadOnlySpan<T> scratch = _scratch;
            if (_current is null)
            {
                scratch[.._index].CopyTo(destination);
                return;
            }

            scratch[.._scratchCount].CopyTo(destination);
            var offset = _scratchCount;
            for (var i = 0; i < _fullCount; ++i)
            {
                var (segment, count) = _full![i];
                segment.AsSpan(0, count).CopyTo(destination[offset..]);
                offset += count;
            }
            _current.AsSpan(0, _index).CopyTo(destination[offset..]);
        }

        public readonly T[] ToArray()
        {
            var count = Count;
            if (count == 0)
                return Array.Empty<T>();

            // every slot is written by the copy
            var array = GC.AllocateUninitializedArray<T>(count);
            CopyTo(array);
            return array;
        }

        public void Dispose()
        {
            var clear = RuntimeHelpers.IsReferenceOrContainsReferences<T>();
            if (_full is not null)
            {
                for (var i = 0; i < _fullCount; ++i)
                    ArrayPool<T>.Shared.Return(_full[i].Segment, clearArray: clear);
                ArrayPool<(T[], int)>.Shared.Return(_full, clearArray: true);
                _full = null;
            }
            if (_current is not null)
                ArrayPool<T>.Shared.Return(_current, clearArray: clear);
            _fullCount = 0;
            _current = null;
            _index = 0;
            _scratchCount = 0;
            _countInFull = 0;
        }
    }
}
