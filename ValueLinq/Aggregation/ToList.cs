using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Cistern.ValueLinq.Aggregation
{
#if OLD_WAY
    struct ToList
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToList<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    static partial class Impl
    {
        internal static List<EnumeratorElement> ToList<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            try
            {
                return DoToList(ref enumerator);
            }
            finally
            {
                enumerator.Dispose();
            }

            static List<EnumeratorElement> DoToList(ref Enumerator enumerator)
            {
                var list =
                    enumerator.InitialSize switch
                    {
                        (_, var size) => new List<EnumeratorElement>(size),
                        _ => new List<EnumeratorElement>()
                    };

                while (enumerator.TryGetNext(out var current))
                    list.Add(current);

                return list;
            }
        }
    }

#endif

    struct ToListForward<T>
        : IForwardEnumerator<T>
    {
        private List<T> _list;

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_list;

        void IForwardEnumerator<T>.Init(int? size) => _list = size.HasValue ? new List<T>(size.Value) : new List<T>();

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _list.Add(input);
            return true;
        }
    }


    struct ToListViaArrayPoolForward<T>
        : IForwardEnumerator<T>
    {
        static readonly bool ClearArrayItems = !typeof(T).IsPrimitive;

        // logic requires that these constants be negative
        const int KNOWN_SIZE = int.MinValue;
        const int INIT = -1;

        ArrayPool<T> _pool;

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

        public ToListViaArrayPoolForward(ArrayPool<T> pool)
        {
            _0000_0010 = _0000_0020 = _0000_0040 = _0000_0080 = null;
            _0000_0100 = _0000_0200 = _0000_0400 = _0000_0800 = null;
            _0000_1000 = _0000_2000 = _0000_4000 = _0000_8000 = null;
            _0001_0000 = _0002_0000 = _0004_0000 = _0008_0000 = null;
            _0010_0000 = _0020_0000 = _0040_0000 = _0080_0000 = null;
            _0100_0000 = _0200_0000 = _0400_0000 = _0800_0000 = null;
            _1000_0000 = _2000_0000 = _4000_0000              = null;

            _pool = pool;

            _current = Array.Empty<T>();
            _currentIdx = 0;
            _arrayIdx = INIT;
        }

        private void Rent(ref T[] array, int size)
        {
            array = _pool.Rent(size);

            _current = array;
            _currentIdx = 0;

            _arrayIdx++;
        }

        private void RentNextArray()
        {
            switch(_arrayIdx)
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

            _pool.Return(_array, ClearArrayItems);
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

        TResult IForwardEnumerator<T>.GetResult<TResult>()
        {
            var list =
                _arrayIdx switch
                {
                    0 => CreateListFromSmallSingleArray(),
                    KNOWN_SIZE when _currentIdx < 10 => CreateListFromSmallSingleArray(),
                    KNOWN_SIZE => CreateListFromSingleArray(),
                    _ => CreateLargeList(),
                };

            ReturnArrays(); // should really be in a dispose

            return (TResult)(object)list;
        }

        private readonly List<T> CreateListFromSingleArray()
        {
            var listCreator = Interlocked.Exchange(ref _smallListCreator, null);
            if (listCreator == null)
                listCreator = new ToListSmallEnumerable();

            try
            {
                listCreator.Init(_current, _currentIdx);
                return new List<T>(listCreator); // relies on List<T> utilizing ICollection<T>.CopyTo
            }
            finally
            {
                listCreator.Init(Array.Empty<T>(), 0);
                _smallListCreator = listCreator;
            }
        }

        private readonly List<T> CreateListFromSmallSingleArray()
        {
            var list = new List<T>(_currentIdx);
            for (var i = 0; i < _currentIdx; ++i)
                list.Add(_current[i]);
            return list;
        }

        private List<T> CreateLargeList()
        {
            // try and use a shared listCreator, but just create one if there isn't a spare
            var listCreator = Interlocked.Exchange(ref _largeListCreator, null);
            if (listCreator == null)
                listCreator = new ToListViaArrayPoolForwardEnumerable();

            try
            {
                listCreator.Init(ref this);
                return new List<T>(listCreator); // relies on List<T> utilizing ICollection<T>.CopyTo
            }
            finally
            {
                var clear = default(ToListViaArrayPoolForward<T>);
                listCreator.Init(ref clear);
                _largeListCreator = listCreator; // don't really care when gets flushed out to memory, as we can always create a new one
            }
        }

        void IForwardEnumerator<T>.Init(int? size)
        {
            if (size.HasValue)
            {
                Rent(ref _0000_0010, size.Value);
                _arrayIdx = KNOWN_SIZE;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            for (;;)
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

        static ToListViaArrayPoolForwardEnumerable _largeListCreator;
        static ToListSmallEnumerable _smallListCreator;

        class ToListSmallEnumerable
            : ICollection<T>
        {
            T[] _data;
            int _count;

            public void Init(T[] data, int count) => (_data, _count) = (data, count);

            public int Count => _count;

            public bool IsReadOnly => true;

            public void CopyTo(T[] array, int arrayIndex)
            {
                var srcSpan = new Span<T>(_data, 0, _count);
                var dstSpan = new Span<T>(array, arrayIndex, array.Length-arrayIndex);
                srcSpan.CopyTo(dstSpan);
            }

            public void Dispose() { }

            public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

            public void Add(T item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(T item) => throw new NotImplementedException();
            public bool Remove(T item) => throw new NotImplementedException();
        }

        class ToListViaArrayPoolForwardEnumerable
            : ICollection<T>
        {
            // The polite thing here would be to ensure that the IEnumerator<T> interface works, rather than
            // relying on the List<> constructor using ICollection<T>.CopyTo
            ToListViaArrayPoolForward<T> _p;

            public void Init(ref ToListViaArrayPoolForward<T> p) => _p = p;

            public int Count =>
                _p._arrayIdx < 0 /* KNOWN_SIZE, INIT */
                    ? _p._currentIdx
                    : (0x_0000_0010 << _p._arrayIdx) - 0x_0000_0010 + _p._currentIdx;

            public bool IsReadOnly => true;

            public void CopyTo(T[] array, int arrayIndex)
            {
                var remaining = Count;

                var _ = true
                    && DoCopy(array, ref arrayIndex, _p._0000_0010, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0020, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0040, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0080, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._0000_0100, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0200, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0400, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_0800, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._0000_1000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_2000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_4000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0000_8000, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._0001_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0002_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0004_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0008_0000, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._0010_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0020_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0040_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0080_0000, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._0100_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0200_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0400_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._0800_0000, ref remaining)

                    && DoCopy(array, ref arrayIndex, _p._1000_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._2000_0000, ref remaining)
                    && DoCopy(array, ref arrayIndex, _p._4000_0000, ref remaining)
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

            public void Dispose() {}

            public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

            public void Add(T item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(T item) => throw new NotImplementedException();
            public bool Remove(T item) => throw new NotImplementedException();
        }
    }
}