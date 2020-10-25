using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static ValueEnumerable<T, EnumerableNode<T>> OfEnumerable<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, EnumerableNode<T>>(new EnumerableNode<T>(source));
        }

        public static ValueEnumerable<T, ArrayNode<T>> OfArray<T>(this T[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, ArrayNode<T>>(new ArrayNode<T>(source));
        }

        public static ValueEnumerable<T, ListByIndexNode<T>> OfListByIndex<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, ListByIndexNode<T>>(new ListByIndexNode<T>(source));
        }

        public static ValueEnumerable<T, ListByIndexNode<T>> OfListByEnumerator<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, ListByIndexNode<T>>(new ListByIndexNode<T>(source));
        }

        public static ValueEnumerable<T, EmptyNode<T>> Empty<T>() => new ValueEnumerable<T, EmptyNode<T>>(new EmptyNode<T>());

        public static ValueEnumerable<int, RangeNode> Range(int start, int count) => new ValueEnumerable<int, RangeNode>(new RangeNode(start, count));

        public static ValueEnumerable<T, RepeatNode<T>> Repeat<T>(T element, int count) => new ValueEnumerable<T, RepeatNode<T>>(new RepeatNode<T>(element, count));

    }
}
