using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Aggregation
{
    static class ToListImpl
    {
        // A list already at the given count, its elements left for the caller to fill through CollectionsMarshal.AsSpan.
        internal static List<T> CreateWithCount<T>(int count)
        {
            var list = new List<T>(count);
            CollectionsMarshal.SetCount(list, count);
            return list;
        }

        internal static List<T> CreateFromSectionOfArray<T>(Memory<T> memory, int startIdx)
        {
            var list = CreateWithCount<T>(startIdx + memory.Length);
            memory.Span.CopyTo(CollectionsMarshal.AsSpan(list)[startIdx..]);
            return list;
        }

        internal static List<T> CreateFromCollector<T, Allocator>(ref ItemCollector<T, Allocator> collector, int startIdx)
            where Allocator : IArrayAllocator<T>
        {
            var list = CreateWithCount<T>(startIdx + collector.Count);
            collector.CopyTo(CollectionsMarshal.AsSpan(list)[startIdx..]);
            return list;
        }
    }

    static partial class Impl
    {
        // the source is disposed once it has ended, before the result is built, as a push disposes its source; disposed
        // says it has been, so the finally disposes it only on a path that threw before
        internal static List<EnumeratorElement> ToListViaStack<EnumeratorElement, Enumerator, Allocator>(int maxStackItemCount, Allocator allocator, ref Enumerator enumerator)
                where Enumerator : IPullEnumerator<EnumeratorElement>
                where Allocator : IArrayAllocator<EnumeratorElement>
        {
            var disposed = false;
            try
            {
                return DoToList(ref allocator, ref enumerator, maxStackItemCount, ref disposed);
            }
            finally
            {
                if (!disposed)
                    enumerator.Dispose();
            }

            static List<EnumeratorElement> Create(int size) =>
                ToListImpl.CreateWithCount<EnumeratorElement>(size);

            static List<EnumeratorElement> DoToList(ref Allocator allocator, ref Enumerator enumerator, int maxStackItemCount, ref bool disposed)
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

                        var list = Create(idx);
                        stack[..idx].CopyTo(CollectionsMarshal.AsSpan(list));
                        return list;
                    }
                    stack[idx] = enumerator.Current;
                }

                var result = PopulateRemainingUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref allocator, ref enumerator, idx, ref disposed);
                stack[..idx].CopyTo(CollectionsMarshal.AsSpan(result));
                return result;
            }
        }

        // the source disposed once populating ends, normally or not, and the collector after it
        private static List<EnumeratorElement> PopulateRemainingUsingAllocator<Allocator, EnumeratorElement, Enumerator>(ref Allocator allocator, ref Enumerator enumerator, int idx, ref bool disposed)
            where Allocator : IArrayAllocator<EnumeratorElement>
            where Enumerator : IPullEnumerator<EnumeratorElement>
        {
            var creator = new ToListViaArrayPoolForward<EnumeratorElement, Allocator>(allocator, idx, null);
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

        internal static List<T> ToListViaArrayPool<T, Enumerator>(ArrayPool<T> arrayPool, bool cleanBuffers, int? size, ref Enumerator enumerator)
                where Enumerator : IPullEnumerator<T>
        {
            var disposed = false;
            try
            {
                var allocator = new ArrayPoolAllocator<T>(arrayPool, cleanBuffers);
                return PopulateRemainingUsingAllocator<ArrayPoolAllocator<T>, T, Enumerator>(ref allocator, ref enumerator, 0, ref disposed);
            }
            finally
            {
                if (!disposed)
                    enumerator.Dispose();
            }
        }
    }

    [Aggregator]
    readonly partial struct ToListForward<T>
        : IPushEnumerator<T>
    {
        private readonly List<T> _list;

        public ToListForward(int? size) => _list = size.HasValue ? new List<T>(size.Value) : new List<T>();

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            batch.AddTo(_list);
            return true;
        }
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => (TPushResult)(object)_list;

        bool IPushEnumerator<T>.ProcessNext(T input)
        {
            _list.Add(input);
            return true;
        }
    }

    [Aggregator]
    partial struct ToListViaBuilderForward<T>
        : IPushEnumerator<T>
    {
        SegmentedArrayBuilder<T> _builder;

        public ToListViaBuilderForward() => _builder = new SegmentedArrayBuilder<T>();

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.ColdCopy;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            _builder.AddRange(ref batch);
            return true;
        }

        public void Dispose() => _builder.Dispose();
        TPushResult IPushEnumerator<T>.GetResult<TPushResult>() => Impl.ResultCast<List<T>, TPushResult>(GetResult());

        private List<T> GetResult()
        {
            var list = ToListImpl.CreateWithCount<T>(_builder.Count);
            _builder.CopyTo(CollectionsMarshal.AsSpan(list));
            return list;
        }

        public bool ProcessNext(T input)
        {
            _builder.Add(input);
            return true;
        }
    }

    [Aggregator]
    partial struct ToListViaArrayPoolForward<T, Allocator>
        : IPushEnumerator<T>
        where Allocator : IArrayAllocator<T>
    {
        ItemCollector<T, Allocator> collector;
        int _startIdx;
        Allocator _allocator;
        int? _size;

        public ToListViaArrayPoolForward(Allocator allocator, int startIdx, int? size)
        {
            _startIdx = startIdx;
            _allocator = allocator;
            _size = size;
            collector = new ItemCollector<T, Allocator>(allocator, null);
        }

        // a known size is rented when the chain is initialised, as ToArrayViaAllocatorForward's
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

        public List<T> GetResult() =>
            collector.TryGetKnownSized(out var memory)
                ? ToListImpl.CreateFromSectionOfArray(memory, _startIdx)
                : ToListImpl.CreateFromCollector(ref collector, _startIdx);

        public bool ProcessNext(T input) => collector.Add(input);

        internal void Populate<Enumerator>(ref Enumerator enumerator)
            where Enumerator : IPullEnumerator<T> => collector.Populate(ref enumerator);
    }
}