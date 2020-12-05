using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class ValueLinqArray
    {

        public static bool Any<TSource>(this TSource[] source) => source.Length > 0;

        // --

        public static TSource Last<TSource>(this TSource[] source) => source.OfArray().Last();
        public static TSource LastOrDefault<TSource>(this TSource[] source) => source.OfArray().LastOrDefault();

        public static List<T> ToList<T>(this T[] source) => source.OfArray().ToList();

        public static int Count<T>(this T[] source) => source.Length;

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, EnumerableNode<TResult>>> SelectMany<TSource, TResult>(this TSource[] source, Func<TSource, IEnumerable<TResult>> selector)
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfArray().SelectMany(src => selector(src).OfEnumerable());
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, ArrayNode<TSource>, NodeU>> SelectMany<TSource, TResult, NodeU>(this TSource[] source, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return source.OfArray().SelectMany(selector);
        }

        public static IEnumerable<T> Concat<T>(this T[] first, T[] second) => first.OfArray().Concat(second.OfArray());

        public static TSource ElementAt<TSource>(this TSource[] source, int index) => source.OfArray().ElementAt(index);
        public static TSource ElementAtOrDefault<TSource>(this TSource[] source, int index) => source.OfArray().ElementAtOrDefault(index);

    }
}
