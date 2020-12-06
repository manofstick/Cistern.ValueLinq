using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        internal static EnumerableNode<T> ToNode<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new (source);
        }

        internal static ArrayNode<T> ToNode<T>(T[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(source);
        }

        internal static MemoryNode<T> ToNode<T>(ReadOnlyMemory<T> source)
            => new(source);

        internal static ListNode<T> ToNode<T>(List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new(source);
        }


        public static ValueEnumerable<T, EnumerableNode<T>> OfEnumerable<T>(this IEnumerable<T> source) => new (ToNode(source));
        public static ValueEnumerable<T, ArrayNode<T>> OfArray<T>(this T[] source) => new(ToNode(source));
        public static ValueEnumerable<T, MemoryNode<T>> OfMemory<T>(this ReadOnlyMemory<T> source) => new(ToNode(source));
        public static ValueEnumerable<T, ListNode<T>> OfList<T>(this List<T> source) => new(ToNode(source));


        public static ValueEnumerable<T, SpanNode<TObject, T>> FromSpan<TObject, T>(TObject obj, GetSpan<TObject,T> getSpan) =>
            new ValueEnumerable<T, SpanNode<TObject, T>>(new SpanNode<TObject, T>(obj, getSpan));

        public static ValueEnumerable<T, ListSegmentNode<T>> OfListByIndex<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, ListSegmentNode<T>>(new ListSegmentNode<T>(source, 0, source.Count));
        }

        public static ValueEnumerable<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>> OfListByEnumerator<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>>(new GenericEnumeratorNode<T, List<T>, List<T>.Enumerator>(source, source => source.GetEnumerator(), source.Count));
        }

        public static ValueEnumerable<T, EmptyNode<T>> Empty<T>() => new ValueEnumerable<T, EmptyNode<T>>(new EmptyNode<T>());

        public static ValueEnumerable<int, RangeNode> Range(int start, int count) => new ValueEnumerable<int, RangeNode>(new RangeNode(start, count));

        public static ValueEnumerable<T, RepeatNode<T>> Repeat<T>(T element, int count) => new ValueEnumerable<T, RepeatNode<T>>(new RepeatNode<T>(element, count));

        public static ValueEnumerable<T, ReturnNode<T>> Return<T>(T element) => new ValueEnumerable<T, ReturnNode<T>>(new ReturnNode<T>(element));

        public static ValueEnumerable<T, GenericEnumeratorNode<T, Enumerable, Enumerator>> OfEnumeratorConstraint<T, Enumerable, Enumerator>(this Enumerable source, Func<Enumerable, Enumerator> getEnumerator, int? count = null)
            where Enumerable : IEnumerable<T>
            where Enumerator : IEnumerator<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, GenericEnumeratorNode<T, Enumerable, Enumerator>>(new GenericEnumeratorNode<T, Enumerable, Enumerator>(source, getEnumerator, count));
        }

        public static ValueEnumerable<T, IReadOnlyListNode<T, List>> OfReadOnlyListConstraint<T, List>(this List source)
            where List : IReadOnlyList<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, IReadOnlyListNode<T, List>>(new IReadOnlyListNode<T, List>(source));
        }

    }
}
