using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Utils
{
    // A buffer that does not outlive the call that built it: Count elements from the start of an array rented from the
    // pool, or of one allocated (a ToArray answer the source made, a buffer too small to rent), which is left to the GC.
    // It is never handed to user code as an array, so nothing can hold it once it is returned.
    internal readonly struct PooledArray<T>
    {
        internal readonly T[] Array;
        internal readonly int Count;
        private readonly ArrayPool<T>? _pool;
        private readonly bool _clear;

        internal PooledArray(T[] array, int count, ArrayPool<T>? pool, bool clear)
        {
            Array = array;
            Count = count;
            _pool = pool;
            _clear = clear;
        }

        internal static PooledArray<T> Owned(T[] array) => new(array, array.Length, null, false);

        // Renting, returning and a pulled buffer's lease cost about 50 ns together, which a buffer under this many bytes
        // does not save in allocation: measured on .NET 11 with foreach over OrderBy of 16 to 512 ints, where renting
        // was 50-60 ns slower to 128 elements and level from 256
        private const int MinimumRentedBytes = 1024;

        internal static PooledArray<T> Rent(int count, ArrayPool<T>? pool, bool cleanBuffers)
        {
            if ((long)count * Unsafe.SizeOf<T>() < MinimumRentedBytes)
                return Owned(GC.AllocateUninitializedArray<T>(count));

            pool ??= ArrayPool<T>.Shared;
            return new(pool.Rent(count), count, pool, cleanBuffers || RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }

        internal PooledArray<T> WithCount(int count) => new(Array, count, _pool, _clear);

        internal bool IsRented => _pool is not null;

        internal Span<T> Span => Array.AsSpan(0, Count);

        internal void Return() => _pool?.Return(Array, _clear);
    }
}
