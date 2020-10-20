using Cistern.ValueLinq.Aggregation;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public delegate void InOutFunc<T, U>(in T t, out U u);
    public delegate U InFunc<T, U>(in T t);

    public static class Enumerable
    {
        public static EnumerableNode<T> OfEnumerable<T>(this IEnumerable<T> enumerable) => new EnumerableNode<T>(enumerable);
        public static ArrayNode<T> OfArray<T>(this T[] array) => new ArrayNode<T>(array);
        public static ListNode<T> OfList<T>(this List<T> list) => new ListNode<T>(list);

        public static MapNode<T, U, Inner> Select<T, U, Inner>(this Inner inner, Func<T, U> f) where Inner : IValueEnumerable<T> => new MapNode<T, U, Inner>(in inner, f);
        public static MapNode<T, U, EnumerableNode<T>> Select<T, U>(this IEnumerable<T> inner, Func<T, U> f) => inner.OfEnumerable().Select(f);
        public static MapNode<T, U, ListNode<T>> Select<T, U>(this List<T> inner, Func<T, U> f) => inner.OfList().Select(f);
        public static MapNode<T, U, ArrayNode<T>> Select<T, U>(this T[] inner, Func<T, U> f) => inner.OfArray().Select(f);
        public static MapInNode<T, U, Inner> Select<T, U, Inner>(this Inner inner, InFunc<T, U> f) where Inner : IValueEnumerable<T> => new MapInNode<T, U, Inner>(in inner, f);
        public static MapInNode<T, U, EnumerableNode<T>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) => inner.OfEnumerable().Select(f);
        public static MapInNode<T, U, ListNode<T>> Select<T, U>(this List<T> inner, InFunc<T, U> f) => inner.OfList().Select(f);
        public static MapInNode<T, U, ArrayNode<T>> Select<T, U>(this T[] inner, InFunc<T, U> f) => inner.OfArray().Select(f);
        public static MapInOutNode<T, U, Inner> Select<T, U, Inner>(this Inner inner, InOutFunc<T, U> f) where Inner : IValueEnumerable<T> => new MapInOutNode<T, U, Inner>(in inner, f);
        public static MapInOutNode<T, U, EnumerableNode<T>> Select<T, U>(this IEnumerable<T> inner, InOutFunc<T, U> f) => inner.OfEnumerable().Select(f);
        public static MapInOutNode<T, U, ListNode<T>> Select<T, U>(this List<T> inner, InOutFunc<T, U> f) => inner.OfList().Select(f);
        public static MapInOutNode<T, U, ArrayNode<T>> Select<T, U>(this T[] inner, InOutFunc<T, U> f) => inner.OfArray().Select(f);

        public static FilterNode<T, Inner> Where<T, Inner>(this Inner inner, Func<T, bool> f) where  Inner : IValueEnumerable<T> => new FilterNode<T, Inner>(in inner, f);
        public static FilterNode<T, EnumerableNode<T>> Where<T>(this IEnumerable<T> inner, Func<T, bool> f) => inner.OfEnumerable().Where(f);
        public static FilterNode<T, ListNode<T>> Where<T>(this List<T> inner, Func<T, bool> f) => inner.OfList().Where(f);
        public static FilterNode<T, ArrayNode<T>> Where<T>(this T[] inner, Func<T, bool> f) => inner.OfArray().Where(f);
        public static FilterInNode<T, Inner> Where<T, Inner>(this Inner inner, InFunc<T, bool> f) where Inner : IValueEnumerable<T> => new FilterInNode<T, Inner>(in inner, f);
        public static FilterInNode<T, EnumerableNode<T>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) => inner.OfEnumerable().Where(f);
        public static FilterInNode<T, ListNode<T>> Where<T>(this List<T> inner, InFunc<T, bool> f) => inner.OfList().Where(f);
        public static FilterInNode<T, ArrayNode<T>> Where<T>(this T[] inner, InFunc<T, bool> f) => inner.OfArray().Where(f);

        public static List<T> ToList<T>(this IValueEnumerable<T> inner) => Nodes<List<T>>.Aggregation<IValueEnumerable<T>, ToList>(ref inner);
        public static List<T> ToList<T>(this IEnumerable<T> inner)
        {
            var enumerableNode = inner.OfEnumerable();
            return Nodes<List<T>>.Aggregation<EnumerableNode<T>, ToList>(ref enumerableNode);
        }
        public static int Count<Inner>(this Inner inner) where Inner : INode => Nodes<int>.Aggregation<Inner, Count>(ref inner);
        public static int Count<T>(this IEnumerable<T> inner) => inner.OfEnumerable().Count();

        public static T Sum<T>(this IValueEnumerable<T> inner)
        {
            if (typeof(T) == typeof(int))    return Nodes<T>.Aggregation<IValueEnumerable<T>, SumInt>(ref inner);
            if (typeof(T) == typeof(double)) return Nodes<T>.Aggregation<IValueEnumerable<T>, SumDouble>(ref inner);

            throw new InvalidOperationException();
        }

        public static int Sum_Int<Inner>(this Inner inner) where Inner : IValueEnumerable<int> => Nodes<int>.Aggregation<Inner, SumInt>(ref inner);
        public static double Sum_Double<Inner>(this Inner inner) where Inner : IValueEnumerable<double> => Nodes<double>.Aggregation<Inner, SumDouble>(ref inner);

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(this Inner source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : IValueEnumerable<T>
        {
            var aggregate = new Aggregate<T, TAccumulate>(seed, func);
            var nodes = new Nodes<Aggregate<T, TAccumulate>, NodesEnd>(in aggregate, new NodesEnd());
            return source.CreateObjectDescent<TAccumulate, Aggregate<T, TAccumulate>, NodesEnd>(ref nodes);
        }
    }
}
