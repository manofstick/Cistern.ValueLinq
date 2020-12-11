using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
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

        class AddRangeOnList<TObject, T>
            : ListPopulator<T>
        {
            private static AddRangeOnList<TObject, T> _maybeInstance; // cache and reuse or discard

            public static void Populate(List<T> list, TObject obj, Containers.GetSpan<TObject, T> getSpan)
            {
                var listPopulator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listPopulator == null)
                    listPopulator = new AddRangeOnList<TObject, T>();

                listPopulator.obj = obj;
                listPopulator.getSpan = getSpan;

                list.AddRange(listPopulator);

                listPopulator.obj = default;
                listPopulator.getSpan = null;

                _maybeInstance = listPopulator; // might splat another, might not get flushed in time, but we don't care
            }

            TObject obj;
            Containers.GetSpan<TObject, T> getSpan;

            private AddRangeOnList() { }

            public override int Count => getSpan(obj).Length;
            public override void CopyTo(T[] array, int arrayIndex)
            {
                var from = getSpan(obj);
                from.CopyTo(new Span<T>(array)[arrayIndex..]);
            }
        }


        class CreateListFromSectionOfArray<T>
            : ListPopulator<T>
        {
            private static CreateListFromSectionOfArray<T> _maybeInstance; // cache and reuse or discard

            private static List<T> CreateSmall(Memory<T> memory)
            {
                var data = memory.Span;
                var list = new List<T>(data.Length);
                for (var i = 0; i < data.Length; ++i)
                    list.Add(data[i]);
                return list;
            }

            public static List<T> Create(Memory<T> memory, int startIdx)
            {
                if (memory.Length <= 10)
                    return CreateSmall(memory);

                var listCreator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listCreator == null)
                    listCreator = new CreateListFromSectionOfArray<T>();

                listCreator.memory = memory;
                listCreator.startIdx = startIdx;

                var list = new List<T>(listCreator);

                listCreator.startIdx = int.MinValue;
                listCreator.memory = default;

                _maybeInstance = listCreator; // might splat another, might not get flushed in time, but we don't care

                return list;
            }

            Memory<T> memory;
            int startIdx;

            public override int Count => startIdx + memory.Length;

            public override void CopyTo(T[] array, int arrayIndex)
            {
                var srcSpan = memory.Span;
                var dstSpan = new Span<T>(array, startIdx+arrayIndex, array.Length - arrayIndex);

                srcSpan.CopyTo(dstSpan);
            }
        }
    }

    struct ToListViaStackMemoryPool<T>
        : INode
    {
        int _maxStackItemCount;
        ArrayPool<T> _arrayPool;
        bool _cleanBuffers;

        public ToListViaStackMemoryPool(int maxStackItemCount, ArrayPool<T> arrayPool, bool cleanBuffers) => (_maxStackItemCount, _arrayPool, _cleanBuffers) = (maxStackItemCount, arrayPool, cleanBuffers);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToListViaStack<EnumeratorElement, Enumerator, ArrayPoolAllocator<EnumeratorElement>>(_maxStackItemCount, new ArrayPoolAllocator<EnumeratorElement>((ArrayPool<EnumeratorElement>)(object)_arrayPool, _cleanBuffers), ref enumerator);

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    struct ToListViaStackAndGarbage<T>
        : INode
    {
        int _maxStackItemCount;

        public ToListViaStackAndGarbage(int maxStackItemCount) => (_maxStackItemCount) = (maxStackItemCount);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToListViaStack<EnumeratorElement, Enumerator, GarbageCollectedAllocator<EnumeratorElement>>(_maxStackItemCount, default, ref enumerator);

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static List<EnumeratorElement> ToListViaStack<EnumeratorElement, Enumerator, Allocator>(int maxStackItemCount, Allocator allocator, ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<EnumeratorElement>
                where Allocator : IArrayAllocator<EnumeratorElement>
        {
            try
            {
                return DoToList(ref allocator, ref enumerator, maxStackItemCount);
            }
            finally
            {
                enumerator.Dispose();
            }
        
            static List<EnumeratorElement> Create(int size) =>
                ToListImpl.CreatedEmptyIndexableList<EnumeratorElement>.Create(size);

            static List<EnumeratorElement> StackBasedPopulate(ref Allocator allocator, ref Enumerator enumerator, int remaining, int idx)
            {
                if (remaining <= 0)
                    return PopulateRemainingUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref allocator, ref enumerator, idx);

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

                result = StackBasedPopulate(ref allocator, ref enumerator, remaining-4, idx+4);

                result[idx + 3] = t4;
            _3: result[idx + 2] = t3;
            _2: result[idx + 1] = t2;
            _1: result[idx + 0] = t1;

            _0: return result;
            }

            static List<EnumeratorElement> DoToList(ref Allocator allocator, ref Enumerator enumerator, int remaining) => StackBasedPopulate(ref allocator, ref enumerator, remaining, 0);
        }

        private static List<EnumeratorElement> PopulateRemainingUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref Allocator allocator, ref Enumerator enumerator, int idx)
            where Allocator : IArrayAllocator<EnumeratorElement>
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            var creator = new ToListViaArrayPoolForward<EnumeratorElement, Allocator>(allocator, idx, null);
            try
            {
                creator.Populate(ref enumerator);
                return creator.GetResult();
            }
            finally
            {
                creator.Dispose();
            }
        }

        internal static List<T> ToListViaArrayPool<T, Enumerator>(ArrayPool<T> arrayPool, bool cleanBuffers, int? size, ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<T>
        {
            try
            {
                var allocator = new ArrayPoolAllocator<T>(arrayPool, cleanBuffers);
                return PopulateRemainingUsingAllocator<ArrayPoolAllocator<T>, T, Enumerator>(ref allocator, ref enumerator, 0);
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }

    struct ToListForward<T>
        : IForwardEnumerator<T>
    {
        private List<T> _list;

        public ToListForward(int? size) => _list = size.HasValue ? new List<T>(size.Value) : new List<T>();

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                ToListImpl.AddRangeOnList<TObject, T>.Populate(_list, obj, getSpan);
                return BatchProcessResult.SuccessAndContinue;
            }

            return BatchProcessResult.Unavailable;
        }
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

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToListViaArrayPool<EnumeratorElement, Enumerator>((ArrayPool<EnumeratorElement>)(object)_arrayPool, _cleanBuffers, _size, ref enumerator);

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    struct ToListViaArrayPoolForward<T, Allocator>
        : IForwardEnumerator<T>
        where Allocator : IArrayAllocator<T>
    {
        ItemCollector<T, Allocator> collector;
        int _startIdx;

        public ToListViaArrayPoolForward(Allocator allocator, int startIdx, int? size) => (_startIdx, collector) = (startIdx, new ItemCollector<T, Allocator>(allocator, size));

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => collector.Dispose(); 
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public List<T> GetResult() =>
            collector.TryGetKnownSized(out var memory)
                ? ToListImpl.CreateListFromSectionOfArray<T>.Create(memory, _startIdx)
                : CreateListFromArrayCollection.Create(ref collector, _startIdx);

        public bool ProcessNext(T input) => collector.Add(input);

        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<T> => collector.Populate(ref enumerator);

        class CreateListFromArrayCollection
            : ToListImpl.ListPopulator<T>
        {
            static CreateListFromArrayCollection _maybeInstance;

            public static List<T> Create(ref ItemCollector<T, Allocator> p, int startIdx)
            {
                var listCreator = Interlocked.Exchange(ref _maybeInstance, null);
                if (listCreator == null)
                    listCreator = new CreateListFromArrayCollection();

                listCreator._p = p;
                listCreator.startIdx = startIdx;

                var list = new List<T>(listCreator);

                listCreator.startIdx = int.MinValue;
                listCreator._p = default; // release references

                _maybeInstance = listCreator; // might splat another, might not get flushed in time, but we don't care

                return list;
            }

            private CreateListFromArrayCollection() {}

            // The polite thing here would be to ensure that the IEnumerator<T> interface works, rather than
            // relying on the List<> constructor using ICollection<T>.CopyTo
            ItemCollector<T, Allocator> _p;
            int startIdx;

            public override int Count => startIdx+_p.Count;

            public override void CopyTo(T[] array, int arrayIndex) => _p.CopyTo(array, startIdx+arrayIndex);
        }
    }
}