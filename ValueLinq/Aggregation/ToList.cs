using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Cistern.ValueLinq.Aggregation
{
    namespace ToListImpl
    {
        /// <summary>
        /// Base class for List[T] populator. Relies on **undocumented** functionality in the constructor for List[T] where it
        /// queries for ICollection[T] and only uses the Count and CopyTo functions. I believe this is a reasonable assumption
        /// but would be nice if it was documented, so it could more conclusively be relied upon.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        abstract class ListPopulator<T>
            : ICollection<T>
        {
            public bool IsReadOnly => true;

            public abstract int Count { get; }
            public abstract void CopyTo(T[] array, int arrayIndex);

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new InvalidOperationException();
            IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException();
            void ICollection<T>.Add(T item) => throw new InvalidOperationException();
            void ICollection<T>.Clear() => throw new InvalidOperationException();
            bool ICollection<T>.Contains(T item) => throw new InvalidOperationException();
            bool ICollection<T>.Remove(T item) => throw new InvalidOperationException();
        }

        class CreatedEmptyIndexableList<T>
            : ListPopulator<T>
        {
            private static CreatedEmptyIndexableList<T> _maybeInstance; // cache and reuse or discard

            public static List<T> Create(int size)
            {
                var listCreator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listCreator == null)
                    listCreator = new CreatedEmptyIndexableList<T>();

                listCreator._count = size;

                var list = new List<T>(listCreator);

                listCreator._count = int.MinValue;

                _maybeInstance = listCreator; // might splat another, might not get flushed in time, but we don't care

                return list;
            }

            int _count;
            private CreatedEmptyIndexableList() { }

            public override int Count => _count;
            public override void CopyTo(T[] array, int arrayIndex) { /* we don't copy anything, so list will be empty */ }
        }

        class CreateListFromSectionOfArray<T>
            : ListPopulator<T>
        {
            private static CreateListFromSectionOfArray<T> _maybeInstance; // cache and reuse or discard

            private static List<T> CreateSmall(T[] data, int count)
            {
                var list = new List<T>(count);
                for (var i = 0; i < count; ++i)
                    list.Add(data[i]);
                return list;
            }

            public static List<T> Create(T[] data, int count)
            {
                if (count <= 10)
                    return CreateSmall(data, count);

                var listCreator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listCreator == null)
                    listCreator = new CreateListFromSectionOfArray<T>();

                listCreator._data = data;
                listCreator._count = count;

                var list = new List<T>(listCreator);

                listCreator._count = int.MinValue;
                listCreator._data = null;

                _maybeInstance = listCreator; // might splat another, might not get flushed in time, but we don't care

                return list;
            }

            T[] _data;
            int _count;

            public override int Count => _count;

            public override void CopyTo(T[] array, int arrayIndex)
            {
                var srcSpan = new Span<T>(_data, 0, _count);
                var dstSpan = new Span<T>(array, arrayIndex, array.Length - arrayIndex);

                srcSpan.CopyTo(dstSpan);
            }
        }
    }

    struct ToListViaStack
        : INode
    {
        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToListViaStack<EnumeratorElement, Enumerator>(ref enumerator);

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static List<EnumeratorElement> ToListViaStack<EnumeratorElement, Enumerator>(ref Enumerator enumerator)
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

            static List<EnumeratorElement> Create(int size) =>
                ToListImpl.CreatedEmptyIndexableList<EnumeratorElement>.Create(size);

            static List<EnumeratorElement> StackBasedPopulate(ref Enumerator enumerator, int idx)
            {
                List<EnumeratorElement> result;

                if (!enumerator.TryGetNext(out var t1))
                {
                    result = Create(idx);
                    goto _0;
                }
                if (!enumerator.TryGetNext(out var t2))
                {
                    result = Create(idx + 1);
                    goto _1;
                }
                if (!enumerator.TryGetNext(out var t3))
                {
                    result = Create(idx + 2);
                    goto _2;
                }
                if (!enumerator.TryGetNext(out var t4))
                {
                    result = Create(idx + 3);
                    goto _3;
                }

                result = StackBasedPopulate(ref enumerator, idx+4);

                result[idx + 3] = t4;
            _3: result[idx + 2] = t3;
            _2: result[idx + 1] = t2;
            _1: result[idx + 0] = t1;

            _0: return result;
            }

            static List<EnumeratorElement> DoToList(ref Enumerator enumerator) => StackBasedPopulate(ref enumerator, 0);
        }

        internal static List<EnumeratorElement> ToListViaArrayPool<EnumeratorElement, Enumerator>(ArrayPool<EnumeratorElement> arrayPool, bool cleanBuffers, int? size, ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            var creator = new ToListViaArrayPoolForward<EnumeratorElement>(arrayPool, cleanBuffers, size);
            try
            {
                creator.Populate(ref enumerator);
                return creator.GetResult();
            }
            finally
            {
                creator.Dispose();
                enumerator.Dispose();
            }
        }

    }

    struct ToListForward<T>
        : IForwardEnumerator<T>
    {
        private List<T> _list;

        public ToListForward(int? size) => _list = size.HasValue ? new List<T>(size.Value) : new List<T>();

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_list;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _list.Add(input);
            return true;
        }
    }

    struct ToListViaArrayPool<T>
        : INode
    {
        ArrayPool<T> _arrayPool;
        bool _cleanBuffers;
        int? _size;

        public ToListViaArrayPool(ArrayPool<T> arrayPool, bool cleanBuffers, int? size) => (_arrayPool, _cleanBuffers, _size) = (arrayPool, cleanBuffers, size);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToListViaArrayPool<EnumeratorElement, Enumerator>((ArrayPool<EnumeratorElement>)(object)_arrayPool, _cleanBuffers, _size, ref enumerator);

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    struct ToListViaArrayPoolForward<T>
        : IForwardEnumerator<T>
    {
        // logic requires that these constants be negative
        const int KNOWN_SIZE = int.MinValue;
        const int INIT = -1;

        ArrayPool<T> _pool;
        bool _cleanBuffers;

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

        public ToListViaArrayPoolForward(ArrayPool<T> pool, bool cleanBuffers, int? size)
        {
            _0000_0010 = _0000_0020 = _0000_0040 = _0000_0080 = null;
            _0000_0100 = _0000_0200 = _0000_0400 = _0000_0800 = null;
            _0000_1000 = _0000_2000 = _0000_4000 = _0000_8000 = null;
            _0001_0000 = _0002_0000 = _0004_0000 = _0008_0000 = null;
            _0010_0000 = _0020_0000 = _0040_0000 = _0080_0000 = null;
            _0100_0000 = _0200_0000 = _0400_0000 = _0800_0000 = null;
            _1000_0000 = _2000_0000 = _4000_0000              = null;

            _pool = pool;
            _cleanBuffers = cleanBuffers;

            _current = Array.Empty<T>();
            _currentIdx = 0;
            _arrayIdx = INIT;

            if (size.HasValue)
            {
                Rent(ref _0000_0010, size.Value);
                _arrayIdx = KNOWN_SIZE;
            }
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

            _pool.Return(_array, _cleanBuffers);
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

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => ReturnArrays(); 
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public List<T> GetResult()
        {
            var list = 
                (_arrayIdx == 0 || _arrayIdx == KNOWN_SIZE)
                    ? ToListImpl.CreateListFromSectionOfArray<T>.Create(_current, _currentIdx)
                    : CreateListFromArrayCollection.Create(ref this);

            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ProcessNext(T input)
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

        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<T>
        {
            for (;;)
            {
                RentNextArray();

                var current = _current;
                for (var i=0; i < current.Length; ++i)
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

        class CreateListFromArrayCollection
            : ToListImpl.ListPopulator<T>
        {
            static CreateListFromArrayCollection _maybeInstance;

            public static List<T> Create(ref ToListViaArrayPoolForward<T> p)
            {
                var listCreator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listCreator == null)
                    listCreator = new CreateListFromArrayCollection();

                listCreator._p = p;

                var list = new List<T>(listCreator);

                listCreator._p = default; // release references

                _maybeInstance = listCreator; // might splat another, might not get flushed in time, but we don't care

                return list;
            }

            private CreateListFromArrayCollection() {}

            // The polite thing here would be to ensure that the IEnumerator<T> interface works, rather than
            // relying on the List<> constructor using ICollection<T>.CopyTo
            ToListViaArrayPoolForward<T> _p;

            public override int Count =>
                _p._arrayIdx < 0 /* KNOWN_SIZE, INIT */
                    ? _p._currentIdx
                    : (0x_0000_0010 << _p._arrayIdx) - 0x_0000_0010 + _p._currentIdx;

            public override void CopyTo(T[] array, int arrayIndex)
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
        }
    }
}