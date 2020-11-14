using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Utils
{
    interface IArrayAllocator<T>
    {
        T[] Alloc(int minimumSize);
        void Free(T[] array);
    }

    struct ArrayPoolAllocator<T>
        : IArrayAllocator<T>
    {
        ArrayPool<T> _pool;
        bool _cleanBuffers;

        public ArrayPoolAllocator(ArrayPool<T> pool, bool cleanBuffers) => (_pool, _cleanBuffers) = (pool, cleanBuffers);

        public T[] Alloc(int minimumSize) => _pool.Rent(minimumSize);
        public void Free(T[] array) => _pool.Return(array, _cleanBuffers);
    }

    struct GarbageCollectedAllocator<T>
        : IArrayAllocator<T>
    {
        public T[] Alloc(int size) => new T[size];
        public void Free(T[] array) { }
    }

    struct ItemCollector<T, Allocator>
        where Allocator : IArrayAllocator<T>
    {
        // logic requires that these constants be negative
        const int KNOWN_SIZE = int.MinValue;
        const int INIT = -1;

        Allocator _allocator;

        T[] _0000_0010, _0000_0020, _0000_0040, _0000_0080;
        T[] _0000_0100, _0000_0200, _0000_0400, _0000_0800;
        T[] _0000_1000, _0000_2000, _0000_4000, _0000_8000;
        T[] _0001_0000, _0002_0000, _0004_0000, _0008_0000;
        T[] _0010_0000, _0020_0000, _0040_0000, _0080_0000;
        T[] _0100_0000, _0200_0000, _0400_0000, _0800_0000;
        T[] _1000_0000, _2000_0000, _4000_0000;

        T[] _current;
        int _currentIdx;
        int _arrayIdx;

        public int Count =>
            _arrayIdx < 0 /* KNOWN_SIZE, INIT */
                ? _currentIdx
                : (0x_0000_0010 << _arrayIdx) - 0x_0000_0010 + _currentIdx;

        public ItemCollector(Allocator allocator, int? size)
        {
            _0000_0010 = _0000_0020 = _0000_0040 = _0000_0080 = null;
            _0000_0100 = _0000_0200 = _0000_0400 = _0000_0800 = null;
            _0000_1000 = _0000_2000 = _0000_4000 = _0000_8000 = null;
            _0001_0000 = _0002_0000 = _0004_0000 = _0008_0000 = null;
            _0010_0000 = _0020_0000 = _0040_0000 = _0080_0000 = null;
            _0100_0000 = _0200_0000 = _0400_0000 = _0800_0000 = null;
            _1000_0000 = _2000_0000 = _4000_0000 = null;

            _allocator = allocator;

            _current = Array.Empty<T>();
            _currentIdx = 0;
            _arrayIdx = INIT;

            if (size.HasValue)
            {
                Rent(ref _0000_0010, size.Value);
                _arrayIdx = KNOWN_SIZE;
            }
        }

        public bool TryGetKnownSized(out Memory<T> memory)
        {
            if (_arrayIdx == 0 || _arrayIdx == KNOWN_SIZE)
            {
                memory = new Memory<T>(_current, 0, _currentIdx);
                return true;
            }
            memory = default;
            return false;
        }

        private void Rent(ref T[] array, int size)
        {
            array = _allocator.Alloc(size);

            _current = array;
            _currentIdx = 0;

            _arrayIdx++;
        }

        private void RentNextArray()
        {
            switch (_arrayIdx)
            {
                case INIT:
                    Rent(ref _0000_0010, 0x_0000_0010); return;
                case 00: Rent(ref _0000_0020, 0x_0000_0020); return;
                case 01: Rent(ref _0000_0040, 0x_0000_0040); return;
                case 02: Rent(ref _0000_0080, 0x_0000_0080); return;

                case 03: Rent(ref _0000_0100, 0x_0000_0100); return;
                case 04: Rent(ref _0000_0200, 0x_0000_0200); return;
                case 05: Rent(ref _0000_0400, 0x_0000_0400); return;
                case 06: Rent(ref _0000_0800, 0x_0000_0800); return;

                case 07: Rent(ref _0000_1000, 0x_0000_1000); return;
                case 08: Rent(ref _0000_2000, 0x_0000_2000); return;
                case 09: Rent(ref _0000_4000, 0x_0000_4000); return;
                case 10: Rent(ref _0000_8000, 0x_0000_8000); return;

                case 11: Rent(ref _0001_0000, 0x_0001_0000); return;
                case 12: Rent(ref _0002_0000, 0x_0002_0000); return;
                case 13: Rent(ref _0004_0000, 0x_0004_0000); return;
                case 14: Rent(ref _0008_0000, 0x_0008_0000); return;

                case 15: Rent(ref _0010_0000, 0x_0010_0000); return;
                case 16: Rent(ref _0020_0000, 0x_0020_0000); return;
                case 17: Rent(ref _0040_0000, 0x_0040_0000); return;
                case 18: Rent(ref _0080_0000, 0x_0080_0000); return;

                case 19: Rent(ref _0100_0000, 0x_0100_0000); return;
                case 20: Rent(ref _0200_0000, 0x_0200_0000); return;
                case 21: Rent(ref _0400_0000, 0x_0400_0000); return;
                case 22: Rent(ref _0800_0000, 0x_0800_0000); return;

                case 23: Rent(ref _1000_0000, 0x_1000_0000); return;
                case 24: Rent(ref _2000_0000, 0x_2000_0000); return;
                case 25: Rent(ref _4000_0000, 0x_4000_0000); return;
            }
        }

        private bool Return(ref T[] _array)
        {
            if (_array == null)
                return false;

            _allocator.Free(_array);
            _array = null;

            return true;
        }

        private void ReturnArrays()
        {
            var _ = true
                && Return(ref _0000_0010) && Return(ref _0000_0020) && Return(ref _0000_0040) && Return(ref _0000_0080)
                && Return(ref _0000_0100) && Return(ref _0000_0200) && Return(ref _0000_0400) && Return(ref _0000_0800)
                && Return(ref _0000_1000) && Return(ref _0000_2000) && Return(ref _0000_4000) && Return(ref _0000_8000)
                && Return(ref _0001_0000) && Return(ref _0002_0000) && Return(ref _0004_0000) && Return(ref _0008_0000)
                && Return(ref _0010_0000) && Return(ref _0020_0000) && Return(ref _0040_0000) && Return(ref _0080_0000)
                && Return(ref _0100_0000) && Return(ref _0200_0000) && Return(ref _0400_0000) && Return(ref _0800_0000)
                && Return(ref _1000_0000) && Return(ref _2000_0000) && Return(ref _4000_0000);
        }

        public void Dispose() => ReturnArrays();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Add(T input)
        {
            for (; ; )
            {
                if (_currentIdx < _current.Length)
                {
                    _current[_currentIdx] = input;
                    _currentIdx++;
                    return true;
                }

                RentNextArray();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var remaining = Count;

            var _ = true
                && DoCopy(array, ref arrayIndex, _0000_0010, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0020, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0040, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0080, ref remaining)

                && DoCopy(array, ref arrayIndex, _0000_0100, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0200, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0400, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_0800, ref remaining)

                && DoCopy(array, ref arrayIndex, _0000_1000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_2000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_4000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0000_8000, ref remaining)

                && DoCopy(array, ref arrayIndex, _0001_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0002_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0004_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0008_0000, ref remaining)

                && DoCopy(array, ref arrayIndex, _0010_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0020_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0040_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0080_0000, ref remaining)

                && DoCopy(array, ref arrayIndex, _0100_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0200_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0400_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _0800_0000, ref remaining)

                && DoCopy(array, ref arrayIndex, _1000_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _2000_0000, ref remaining)
                && DoCopy(array, ref arrayIndex, _4000_0000, ref remaining)
            ;

            static bool DoCopy(T[] dst, ref int arrayIndex, T[] src, ref int remaining)
            {
                var toCopy = Math.Min(src.Length, remaining);

                var srcSpan = new Span<T>(src, 0, toCopy);
                var dstSpan = new Span<T>(dst, arrayIndex, dst.Length - arrayIndex);
                srcSpan.CopyTo(dstSpan);

                remaining -= toCopy;
                arrayIndex += toCopy;

                return remaining > 0;
            }
        }


        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<T>
        {
            for (; ; )
            {
                RentNextArray();

                var current = _current;
                for (var i = 0; i < current.Length; ++i)
                {
                    if (!enumerator.TryGetNext(out current[i]))
                    {
                        _currentIdx = i;
                        return;
                    }
                }

                if (_arrayIdx == KNOWN_SIZE)
                {
                    _currentIdx = current.Length;
                    return;
                }
            }
        }
    }

}
