using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value) =>
            source switch
            {
                ICollection<TSource> c => c.Contains(value),
                _ => EnumerableNode.FastEnumerateSwitch<TSource, bool, Contains<TSource>>(source, new Contains<TSource>(value))
            };


        public static bool Any<TSource>(this IEnumerable<TSource> source)
            => source switch
            {
                System.Collections.ICollection c => c.Count > 0,
                ICollection<TSource> c => c.Count > 0,
                IReadOnlyCollection<TSource> c => c.Count > 0,
                _ => source.OfEnumerable().Any()
            };



        public static TSource Last<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().Last();

        public static TSource LastOrDefault<TSource>(this IEnumerable<TSource> source) =>
            source.OfEnumerable().LastOrDefault();

        public static T[] ToArray<T>(this IEnumerable<T> source)
        {
            return source is ICollection<T> c
                ? ICollectionToArray(c)
                : source.OfEnumerable().ToArray();

            static T[] ICollectionToArray(ICollection<T> c)
            {
                var count = c.Count;
                if (count <= 0)
                    return Array.Empty<T>();

                var result = new T[count];
                c.CopyTo(result, 0);
                return result;
            }
        }

        public static List<T> ToList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is INode
                ? source.OfEnumerable().ToList()
                : new List<T>(source);
        }

        public static List<T> ToListUseStack<T>(this IEnumerable<T> source, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T>
                ? new List<T>(source)
                : source.OfEnumerable().ToListUseStack(maxStackItemCount, arrayPoolInfo);
        }

        public static List<T> ToListUsePool<T>(this IEnumerable<T> source, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ICollection<T>
                ? new List<T>(source)
                : source.OfEnumerable().ToListUsePool(maybeArrayPool, maybeCleanBuffers, viaPull);
        }


        public static int Count<T>(this IEnumerable<T> inner, bool ignorePotentialSideEffects = false) =>
            inner.OfEnumerable().Count(ignorePotentialSideEffects);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, EnumerableNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfEnumerable().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, EnumerableNode<TSource>, EnumerableNode<TCollection>>> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return source.OfEnumerable().SelectMany(src => collectionSelector(src).OfEnumerable(), resultSelector);
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, EnumerableNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this IEnumerable<TSource> source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfEnumerable().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return first.OfEnumerable().Concat(second.OfEnumerable());
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index) =>
            source.OfEnumerable().ElementAt(index);

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index) => source.OfEnumerable().ElementAtOrDefault(index);



        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, EnumerableNode<TSource>>> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            => source.OfEnumerable().OrderBy(keySelector, comparer);
        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, EnumerableNode<TSource>>> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            => source.OfEnumerable().OrderByDescending(keySelector, comparer);
    }
}
