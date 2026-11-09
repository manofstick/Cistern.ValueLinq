using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
            collector.CopyTo(result.AsSpan(startIdx));
            return result;
        }
    }

    [Aggregator]
    partial struct ToArrayForward<T>
        : IPushEnumerator<T>
    {
        private T[] _array;
        private int _idx;

        public ToArrayForward(int size)
        {
            _array = new T[size];
            _idx = 0;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            batch.CopyTo(_array, _idx);
            _idx += batch.Count;
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)_array;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _array[_idx++] = input;
            return true;
        }
    }



    static partial class Impl
    {
        // the source is disposed once it has ended, before the result is built, as a push disposes its source; disposed
        // says it has been, so the finally disposes it only on a path that threw before
        internal static EnumeratorElement[] ToArrayViaStack<EnumeratorElement, Enumerator, Allocator>(int maxStackItemCount, Allocator allocator, ref Enumerator enumerator)
                where Enumerator : IPullEnumerator<EnumeratorElement>
                where Allocator : IArrayAllocator<EnumeratorElement>
        {
            var disposed = false;
            try
            {
                return DoToArray(ref allocator, ref enumerator, maxStackItemCount, ref disposed);
            }
            finally
            {
                if (!disposed)
                    enumerator.Dispose();
            }

            static EnumeratorElement[] Create(int size)
            {
                if (size == 0)
                    return Array.Empty<EnumeratorElement>();
                return new EnumeratorElement[size];
            }

            static EnumeratorElement[] DoToArray(ref Allocator allocator, ref Enumerator enumerator, int maxStackItemCount, ref bool disposed)
            {
                Unsafe.SkipInit(out StackBuffer<EnumeratorElement> buffer);
                Span<EnumeratorElement> stack = buffer;
                var capacity = Math.Min(maxStackItemCount, StackBuffer<EnumeratorElement>.Capacity);

                var idx = 0;
                for (; idx < capacity; ++idx)
                {
                    if (!enumerator.MoveNext())
                    {
                        disposed = true;
                        enumerator.Dispose();

                        var array = Create(idx);
                        stack[..idx].CopyTo(array);
                        return array;
                    }
                    stack[idx] = enumerator.Current;
                }

                var result = PopulateRemainingArrayUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref allocator, ref enumerator, idx, ref disposed);
                stack[..idx].CopyTo(result);
                return result;
            }
        }

        // the source disposed once populating ends, normally or not, and the collector after it
        private static EnumeratorElement[] PopulateRemainingArrayUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref Allocator allocator, ref Enumerator enumerator, int idx, ref bool disposed)
            where Allocator : IArrayAllocator<EnumeratorElement>
            where Enumerator : IPullEnumerator<EnumeratorElement>
        {
            var creator = new ToArrayViaAllocatorForward<EnumeratorElement, Allocator>(allocator, idx, null);
            try
            {
                try
                {
                    creator.Populate(ref enumerator);
                }
                finally
                {
                    disposed = true;
                    enumerator.Dispose();
                }

                return creator.GetResult();
            }
            finally
            {
                creator.Dispose();
            }
        }

        internal static T[] ToArrayViaArrayPool<T, Enumerator>(ArrayPool<T> arrayPool, bool cleanBuffers, int? size, ref Enumerator enumerator)
                where Enumerator : IPullEnumerator<T>
        {
            var disposed = false;
            try
            {
                var allocator = new ArrayPoolAllocator<T>(arrayPool, cleanBuffers);
                return PopulateRemainingArrayUsingAllocator<ArrayPoolAllocator<T>, T, Enumerator>(ref allocator, ref enumerator, 0, ref disposed);
            }
            finally
            {
                if (!disposed)
                    enumerator.Dispose();
            }
        }
    }

    [Aggregator]
    partial struct ToArrayViaBuilderForward<T>
        : IPushEnumerator<T>
    {
        SegmentedArrayBuilder<T> _builder;

        public ToArrayViaBuilderForward() => _builder = new SegmentedArrayBuilder<T>();

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            _builder.AddRange(ref batch);
            return true;
        }

        public void Dispose() => _builder.Dispose();
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<T[], TPushResult>(_builder.ToArray());

        public bool ProcessNext(T input)
        {
            _builder.Add(input);
            return true;
        }
    }

    // NodeImpl.ToPooledArrayByRef's push: the result is a rented array rather than one of exactly the count
    [Aggregator]
    partial struct ToPooledArrayForward<T>
        : IPushEnumerator<T>
    {
        SegmentedArrayBuilder<T> _builder;
        readonly ArrayPool<T>? _pool;
        readonly bool _cleanBuffers;

        public ToPooledArrayForward(ArrayPool<T>? pool, bool cleanBuffers)
        {
            _builder = new SegmentedArrayBuilder<T>();
            _pool = pool;
            _cleanBuffers = cleanBuffers;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            _builder.AddRange(ref batch);
            return true;
        }

        public void Dispose() => _builder.Dispose();

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>()
        {
            var count = _builder.Count;
            if (count == 0)
                return Impl.ResultCast<PooledArray<T>, TPushResult>(PooledArray<T>.Owned(Array.Empty<T>()));

            var rented = PooledArray<T>.Rent(count, _pool, _cleanBuffers);
            _builder.CopyTo(rented.Array);
            return Impl.ResultCast<PooledArray<T>, TPushResult>(rented);
        }

        public bool ProcessNext(T input)
        {
            _builder.Add(input);
            return true;
        }
    }

    // a Concat's ToArray or ToList: the parts between its head and its tail are pushed into the builder, and once they
    // have run the result is finished here, the head and tail counted, the result sized, and the head, the builder and
    // the tail written into it, so the builder's 384 bytes are never copied out of the push
    [Aggregator]
    partial struct ConcatMaterialiseForward<T, TConcat>
        : IPushEnumerator<T>
        where TConcat : struct, CisternConcat.IParts<T>
    {
        private SegmentedArrayBuilder<T> _builder;
        private TConcat _concat;
        private readonly List<SystemIEnumerable.Node<T>>? _components;
        private readonly int _pushedFrom;
        private readonly int _pushedTo;
        private readonly int _leaves;
        private readonly bool _asList;

        public ConcatMaterialiseForward(TConcat concat, List<SystemIEnumerable.Node<T>>? components, int pushedFrom, int pushedTo, int leaves, bool asList)
        {
            _builder = new SegmentedArrayBuilder<T>();
            _concat = concat;
            _components = components;
            _pushedFrom = pushedFrom;
            _pushedTo = pushedTo;
            _leaves = leaves;
            _asList = asList;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            _builder.AddRange(ref batch);
            return true;
        }

        public void Dispose() => _builder.Dispose();

        TPushResult IPushEnumerator<T>.GetResult<TPushResult>()
        {
            var count = _concat.Count(_components, 0, _pushedFrom) + _builder.Count + _concat.Count(_components, _pushedTo, _leaves);
            if (count > Array.MaxLength)
                throw new OutOfMemoryException();

            if (_asList)
            {
                var list = ToListImpl.CreateWithCount<T>((int)count);
                Fill(new Optimizations.CopyTo<T>(list));
                return Impl.ResultCast<List<T>, TPushResult>(list);
            }

            if (count == 0)
                return Impl.ResultCast<T[], TPushResult>(Array.Empty<T>());

            // every slot is written by the copies
            var array = GC.AllocateUninitializedArray<T>((int)count);
            Fill(new Optimizations.CopyTo<T>(array));
            return Impl.ResultCast<T[], TPushResult>(array);
        }

        private void Fill(Optimizations.CopyTo<T> destination)
        {
            destination = _concat.Write(_components, 0, _pushedFrom, destination);
            _builder.CopyTo(destination.Span);
            _concat.Write(_components, _pushedTo, _leaves, destination.Advance(_builder.Count));
        }

        public bool ProcessNext(T input)
        {
            _builder.Add(input);
            return true;
        }
    }

    [Aggregator]
    partial struct ToArrayViaAllocatorForward<T, Allocator>
        : IPushEnumerator<T>
        where Allocator : IArrayAllocator<T>
    {
        ItemCollector<T, Allocator> collector;
        int _startIdx;
        Allocator _allocator;
        int? _size;

        public ToArrayViaAllocatorForward(Allocator allocator, int startIdx, int? size)
        {
            _startIdx = startIdx;
            _allocator = allocator;
            _size = size;
            collector = new ItemCollector<T, Allocator>(allocator, null);
        }

        // a known size is rented when the chain is initialised, which this, the last of it, is last in, so a chain whose
        // push fails before it starts holds nothing
        public static bool AllowInitialise => true;

        public void Initialise()
        {
            if (_size is not int size)
                return;

            _size = null;
            collector = new ItemCollector<T, Allocator>(_allocator, size);
        }

        public void Dispose() => collector.Dispose(); 
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)GetResult();

        public T[] GetResult() =>
            collector.TryGetKnownSized(out var memory)
                ? ToArrayImpl.CreateListFromSectionOfArray(memory, _startIdx)
                : ToArrayImpl.CreateListFromArrayCollection(ref collector, _startIdx);

        public bool ProcessNext(T input) => collector.Add(input);

        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IPullEnumerator<T> => collector.Populate(ref enumerator);
    }
}