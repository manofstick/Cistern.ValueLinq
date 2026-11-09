using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationCopyTo<T>
    {
        bool CopyTo(in CopyTo<T> request, out int response);
    }

    /// <summary>
    /// Copies the sequence into the destination at its offset, answering the number of elements copied. Without a
    /// destination it is the dry run: it answers the exact number of elements, when every one of them can be copied out
    /// with nothing enumerated, and runs nothing, so a caller can size its destination before anything is copied. The
    /// copy itself may run user code once per element in sequence order, as the push would (Select's selector).
    /// A request with a destination is asked only of a node whose dry run answered, and is then never declined. A node
    /// made of parts answers the dry run only when all of its parts do, and copies each into its slice of the destination
    /// with Advance. The destination is a newly allocated array or list whose count has been set (so a reference slot
    /// holds null until written), as a span cannot be carried in a
    /// request: the requests are unpacked through object.
    /// </summary>
    readonly struct CopyTo<T>
    {
        private readonly T[]? _array;
        private readonly List<T>? _list;
        private readonly int _offset;

        private CopyTo(T[]? array, List<T>? list, int offset)
        {
            _array = array;
            _list = list;
            _offset = offset;
        }

        public CopyTo(T[] array) : this(array, null, 0) { }

        // the list's count is set to the copied length before the copy, so its storage is written directly
        public CopyTo(List<T> list) : this(null, list, 0) { }

        public bool IsDryRun => _array is null && _list is null;

        public Span<T> Span =>
            _array is not null
                ? _array.AsSpan(_offset)
                : CollectionsMarshal.AsSpan(_list).Slice(_offset);

        public CopyTo<T> Advance(int count) => new(_array, _list, _offset + count);

        // for a source whose own CopyTo takes an array and an index (ICollection<T>)
        public bool TryGetArray([MaybeNullWhen(false)] out T[] array, out int offset)
        {
            (array, offset) = (_array, _offset);
            return array is not null;
        }

        // a collection copied in once, by its own CopyTo: into the array, or into the list's storage by AddRange, the list
        // cut back to the offset for it and set back to its count after; the number of elements copied
        public int CopyFrom(ICollection<T> collection)
        {
            var copied = collection.Count;
            if (copied == 0)
                return 0;

            if (_array is not null)
            {
                collection.CopyTo(_array, _offset);
                return copied;
            }

            var count = _list!.Count;
            CollectionsMarshal.SetCount(_list, _offset);
            _list.AddRange(collection);
            var added = _list.Count - _offset;
            CollectionsMarshal.SetCount(_list, count);
            return added;
        }
    }

    /// <summary>
    /// The dry run and the copy. A container that can expose its elements as a span need not answer either: both fall
    /// back to AsSpanSource.
    /// </summary>
    static class CopyTo
    {
        public static bool TryGetCount<T, Node>(ref Node node, out int count)
            where Node : INode<T>
        {
            if (node.TryPushOptimization<CopyTo<T>, int>(default(CopyTo<T>), out count))
                return true;

            if (AsSpanSource.TryGet<T, Node>(ref node, out var source))
            {
                count = source.GetSpan().Length;
                return true;
            }

            count = 0;
            return false;
        }

        // the number of elements copied
        public static int Copy<T, Node>(ref Node node, in CopyTo<T> destination)
            where Node : INode<T>
        {
            if (node.TryPushOptimization<CopyTo<T>, int>(destination, out var copied))
                return copied;

            if (!AsSpanSource.TryGet<T, Node>(ref node, out var source))
                throw new InvalidOperationException("Should not be called unless CopyTo.TryGetCount has answered for the same node");

            var span = source.GetSpan();
            span.CopyTo(destination.Span);

            return span.Length;
        }
    }
}
