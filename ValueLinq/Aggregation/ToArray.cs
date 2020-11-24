using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;

namespace Cistern.ValueLinq.Aggregation
{
    static class ToArrayImpl
    {
        internal static T[] CreateListFromSectionOfArray<T>(Memory<T> memory, int startIdx)
        {
            if (startIdx + memory.Length == 0)
                return Array.Empty<T>();

            var result = new T[startIdx+memory.Length];
            memory.CopyTo(new Memory<T>(result, startIdx, memory.Length));
            return result;
        }

        internal static T[] CreateListFromArrayCollection<T, Allocator>(ref ItemCollector<T, Allocator> collector, int startIdx) where Allocator : IArrayAllocator<T>
        {
            if (startIdx + collector.Count == 0)
                return Array.Empty<T>();

            var result = new T[startIdx+collector.Count];
            collector.CopyTo(result, startIdx);
            return result;
        }
    }

    struct ToArrayForward<T>
        : IForwardEnumerator<T>
    {
        private T[] _array;
        private int _idx;

        public ToArrayForward(int size) => (_array, _idx) = (new T[size], 0);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request)
        {
            if (typeof(TRequest) == typeof(Containers.GetSpan<TObject, T>))
            {
                var getSpan = (Containers.GetSpan<TObject, T>)(object)request;
                var input = getSpan(obj);
                var output = new Span<T>(_array, _idx, input.Length);
                input.CopyTo(output);
                _idx += input.Length;
                return BatchProcessResult.SuccessAndContinue;
            }
            
            return BatchProcessResult.Unavailable;
        }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_array;

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            _array[_idx++] = input;
            return true;
        }
    }


    struct ToArrayViaStackMemoryPool<T>
        : INode
    {
        int _maxStackItemCount;
        ArrayPool<T> _arrayPool;
        bool _cleanBuffers;

        public ToArrayViaStackMemoryPool(int maxStackItemCount, ArrayPool<T> arrayPool, bool cleanBuffers) => (_maxStackItemCount, _arrayPool, _cleanBuffers) = (maxStackItemCount, arrayPool, cleanBuffers);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToArrayViaStack<EnumeratorElement, Enumerator, ArrayPoolAllocator<EnumeratorElement>>(_maxStackItemCount, new ArrayPoolAllocator<EnumeratorElement>((ArrayPool<EnumeratorElement>)(object)_arrayPool, _cleanBuffers), ref enumerator);

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    struct ToArrayViaStackAndGarbage<T>
        : INode
    {
        int _maxStackItemCount;

        public ToArrayViaStackAndGarbage(int maxStackItemCount) => (_maxStackItemCount) = (maxStackItemCount);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToArrayViaStack<EnumeratorElement, Enumerator, GarbageCollectedAllocator<EnumeratorElement>>(_maxStackItemCount, default, ref enumerator);

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    static partial class Impl
    {
        internal static EnumeratorElement[] ToArrayViaStack<EnumeratorElement, Enumerator, Allocator>(int maxStackItemCount, Allocator allocator, ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<EnumeratorElement>
                where Allocator : IArrayAllocator<EnumeratorElement>
        {
            try
            {
                return DoToArray(ref allocator, ref enumerator, maxStackItemCount);
            }
            finally
            {
                enumerator.Dispose();
            }

            static EnumeratorElement[] Create(int size)
            {
                if (size == 0)
                    return Array.Empty<EnumeratorElement>();
                return new EnumeratorElement[size];
            }

            static EnumeratorElement[] StackBasedPopulate(ref Allocator allocator, ref Enumerator enumerator, int remaining, int idx)
            {
                if (remaining <= 0)
                    return PopulateRemainingArrayUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref allocator, ref enumerator, idx);

                EnumeratorElement[] result;

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

            static EnumeratorElement[] DoToArray(ref Allocator allocator, ref Enumerator enumerator, int remaining) => StackBasedPopulate(ref allocator, ref enumerator, remaining, 0);
        }

        private static EnumeratorElement[] PopulateRemainingArrayUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref Allocator allocator, ref Enumerator enumerator, int idx)
            where Allocator : IArrayAllocator<EnumeratorElement>
            where Enumerator : IFastEnumerator<EnumeratorElement>
        {
            var creator = new ToArrayViaAllocatorForward<EnumeratorElement, Allocator>(allocator, idx, null);
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

        internal static T[] ToArrayViaArrayPool<T, Enumerator>(ArrayPool<T> arrayPool, bool cleanBuffers, int? size, ref Enumerator enumerator)
                where Enumerator : IFastEnumerator<T>
        {
            try
            {
                var allocator = new ArrayPoolAllocator<T>(arrayPool, cleanBuffers);
                return PopulateRemainingArrayUsingAllocator<ArrayPoolAllocator<T>, T, Enumerator>(ref allocator, ref enumerator, 0);
            }
            finally
            {
                enumerator.Dispose();
            }
        }
    }

    struct ToArrayViaArrayPool<T>
        : INode
    {
        ArrayPool<T> _arrayPool;
        bool _cleanBuffers;
        int? _size;

        public ToArrayViaArrayPool(ArrayPool<T> arrayPool, bool cleanBuffers, int? size) => (_arrayPool, _cleanBuffers, _size) = (arrayPool, cleanBuffers, size);

        public void GetCountInformation(out CountInformation info) => Impl.CountInfo(out info);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
            => (CreationType)(object)Impl.ToArrayViaArrayPool<EnumeratorElement, Enumerator>((ArrayPool<EnumeratorElement>)(object)_arrayPool, _cleanBuffers, _size, ref enumerator);

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);
    }

    struct ToArrayViaAllocatorForward<T, Allocator>
        : IForwardEnumerator<T>
        where Allocator : IArrayAllocator<T>
    {
        ItemCollector<T, Allocator> collector;
        int _startIdx;

        public ToArrayViaAllocatorForward(Allocator allocator, int startIdx, int? size) => (_startIdx, collector) = (startIdx, new ItemCollector<T, Allocator>(allocator, size));

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => collector.Dispose(); 
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)GetResult();

        public T[] GetResult() =>
            collector.TryGetKnownSized(out var memory)
                ? ToArrayImpl.CreateListFromSectionOfArray(memory, _startIdx)
                : ToArrayImpl.CreateListFromArrayCollection(ref collector, _startIdx);

        public bool ProcessNext(T input) => collector.Add(input);

        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IFastEnumerator<T> => collector.Populate(ref enumerator);
    }
}