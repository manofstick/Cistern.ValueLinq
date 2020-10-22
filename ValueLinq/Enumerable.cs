using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        public static ValueEnumerable<T, EnumerableNode<T>> OfEnumerable<T>(this IEnumerable<T> enumerable) => new ValueEnumerable<T, EnumerableNode<T>>(new EnumerableNode<T>(enumerable));
        public static ValueEnumerable<T, ArrayNode<T>> OfArray<T>(this T[] array) => new ValueEnumerable<T, ArrayNode<T>>(new ArrayNode<T>(array));
        public static ValueEnumerable<T, ListNode<T>> OfList<T>(this List<T> list) => new ValueEnumerable<T, ListNode<T>>(new ListNode<T>(list));

        public static ValueEnumerable<U, MapNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> f) where TPrior : INode => new ValueEnumerable<U, MapNode<T, U, TPrior>>(new MapNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, MapNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, MapInNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f) where TPrior : INode => new ValueEnumerable<U, MapInNode<T, U, TPrior>>(new MapInNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, MapInNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, MapInOutNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InOutFunc<T, U> f) where TPrior : INode => new ValueEnumerable<U, MapInOutNode<T, U, TPrior>>(new MapInOutNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, MapInOutNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InOutFunc<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, MapiNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> f) where TPrior : INode => new ValueEnumerable<U, MapiNode<T, U, TPrior>>(new MapiNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, MapiNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, int, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<T, FilterNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> f) where TPrior : INode => new ValueEnumerable<T, FilterNode<T, TPrior>>(new FilterNode<T, TPrior>(in prior.Node, f));
        public static ValueEnumerable<T, FilterNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, Func<T, bool> f) => inner.OfEnumerable().Where(f);

        public static ValueEnumerable<T, FilterInNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f) where TPrior : INode => new ValueEnumerable<T, FilterInNode<T, TPrior>>(new FilterInNode<T, TPrior>(in prior.Node, f));
        public static ValueEnumerable<T, FilterInNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) => inner.OfEnumerable().Where(f);

        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            inner.Node.CheckForOptimization<T, Optimizations.ToList_XXX, List<T>>(new Optimizations.ToList_XXX(), out var list) switch
            {
                false => Nodes<List<T>>.Aggregation<Inner, ToList>(in inner.Node),
                true => list,
            };

        public static List<T> ToList<T>(this IEnumerable<T> inner) => new List<T>(inner);

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode
        {
            var aggregate = new Aggregate<T, TAccumulate>(seed, func);
            var nodes = new Nodes<Aggregate<T, TAccumulate>, NodesEnd>(in aggregate, new NodesEnd());
            return source.Node.CreateObjectDescent<TAccumulate, Aggregate<T, TAccumulate>, NodesEnd>(ref nodes);
        }

        public static int Sum(this IEnumerable<int> inner) => inner.OfEnumerable().Sum();
        public static int Sum<Inner>(in this ValueEnumerable<int, Inner> inner) where Inner : INode => Nodes<int>.Aggregation<Inner, SumInt>(in inner.Node);
        public static double Sum(this IEnumerable<double> inner) => inner.OfEnumerable().Sum();
        public static double Sum<Inner>(in this ValueEnumerable<double, Inner> inner) where Inner : INode => Nodes<double>.Aggregation<Inner, SumDouble>(in inner.Node);

        public static int Count<T>(this IEnumerable<T> inner) => inner.OfEnumerable().Count();
        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode => Nodes<int>.Aggregation<Inner, Count>(in inner.Node);

        public static ValueEnumerable<T, EmptyNode<T>> Empty<T>() => new ValueEnumerable<T, EmptyNode<T>>(new EmptyNode<T>());

    }
}
