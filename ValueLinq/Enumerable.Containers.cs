using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections;
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

        public static ValueEnumerable<T, ListByEnumeratorNode<T>> OfListByEnumerator<T>(this List<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, ListByEnumeratorNode<T>>(new ListByEnumeratorNode<T>(source));
        }

        public static ValueEnumerable<T, EmptyNode<T>> Empty<T>() => new ValueEnumerable<T, EmptyNode<T>>(new EmptyNode<T>());

        public static ValueEnumerable<int, RangeNode> Range(int start, int count) => new ValueEnumerable<int, RangeNode>(new RangeNode(start, count));

        public static ValueEnumerable<T, RepeatNode<T>> Repeat<T>(T element, int count) => new ValueEnumerable<T, RepeatNode<T>>(new RepeatNode<T>(element, count));

        public static ValueEnumerable<T, ReturnNode<T>> Return<T>(T element) => new ValueEnumerable<T, ReturnNode<T>>(new ReturnNode<T>(element));

        public static ValueEnumerable<T, GenericEnumeratorNode<T, Enumerable, Enumerator>> OfEnumerableGeneric<T, Enumerable, Enumerator>(this Enumerable source, Func<Enumerable, Enumerator> getEnumerator, int? count = null)
            where Enumerable : IEnumerable<T>
            where Enumerator : IEnumerator<T>
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new ValueEnumerable<T, GenericEnumeratorNode<T, Enumerable, Enumerator>>(new GenericEnumeratorNode<T, Enumerable, Enumerator>(source, getEnumerator, count));
        }

    }
}
