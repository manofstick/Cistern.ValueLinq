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

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> f) where TPrior : INode => new ValueEnumerable<U, SelectNode<T, U, TPrior>>(new SelectNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, SelectNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f) where TPrior : INode => new ValueEnumerable<U, Select_InNode<T, U, TPrior>>(new Select_InNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, Select_InNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, InFunc<T, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<U, SelectiNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> f) where TPrior : INode => new ValueEnumerable<U, SelectiNode<T, U, TPrior>>(new SelectiNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, SelectiNode<T, U, EnumerableNode<T>>> Select<T, U>(this IEnumerable<T> inner, Func<T, int, U> f) => inner.OfEnumerable().Select(f);

        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> f) where TPrior : INode => new ValueEnumerable<T, WhereNode<T, TPrior>>(new WhereNode<T, TPrior>(in prior.Node, f));
        public static ValueEnumerable<T, WhereNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, Func<T, bool> f) => inner.OfEnumerable().Where(f);

        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f) where TPrior : INode => new ValueEnumerable<T, Where_InNode<T, TPrior>>(new Where_InNode<T, TPrior>(in prior.Node, f));
        public static ValueEnumerable<T, Where_InNode<T, EnumerableNode<T>>> Where<T>(this IEnumerable<T> inner, InFunc<T, bool> f) => inner.OfEnumerable().Where(f);

        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            inner.Node.CheckForOptimization<T, Optimizations.ToList_XXX, List<T>>(new Optimizations.ToList_XXX(), out var list) switch
            {
                false => Nodes<List<T>>.Aggregation<Inner, ToList>(in inner.Node),
                true => list,
            };

#if TEMP_DISABLED
        public static List<T> ToList<T>(this IEnumerable<T> inner) => new List<T>(inner);
#endif
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

#if TEMP_DISABLED
        public static int Count<T>(this IEnumerable<T> inner) => inner.OfEnumerable().Count();
#endif
        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode => Nodes<int>.Aggregation<Inner, Count>(in inner.Node);
        public static ValueEnumerable<T, EmptyNode<T>> Empty<T>() => new ValueEnumerable<T, EmptyNode<T>>(new EmptyNode<T>());

        public static ValueEnumerable<int, RangeNode> Range(int start, int count) => new ValueEnumerable<int, RangeNode>(new RangeNode(start, count));
    }
}
