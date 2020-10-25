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
        public static T Aggregate<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var aggregate = new Reduce<T>(func);
            return Nodes<T>.Aggregation(in source.Node, in aggregate);
        }

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var aggregate = new Fold<T, TAccumulate>(seed, func);
            return Nodes<TAccumulate>.Aggregation(in source.Node, in aggregate);
        }

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var aggregate = new Fold<T, TAccumulate>(seed, func);
            return resultSelector(Nodes<TAccumulate>.Aggregation(in source.Node, in aggregate));
        }

        public static bool All<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var aggregate = new All<T>(predicate);
            return Nodes<bool>.Aggregation(in source.Node, in aggregate);
        }

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source)
            where Inner : INode
        {
            var aggregate = new Any<T>(null);
            return Nodes<bool>.Aggregation(in source.Node, in aggregate);
        }

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var aggregate = new Any<T>(predicate);
            return Nodes<bool>.Aggregation(in source.Node, in aggregate);
        }

        // -- 

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> selector) where TPrior : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectNode<T, U, TPrior>>(new SelectNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f) where TPrior : INode => new ValueEnumerable<U, Select_InNode<T, U, TPrior>>(new Select_InNode<T, U, TPrior>(in prior.Node, f));
        public static ValueEnumerable<U, SelectiNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> selector) where TPrior : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectiNode<T, U, TPrior>>(new SelectiNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate) where TPrior : INode
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereNode<T, TPrior>>(new WhereNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f) where TPrior : INode => new ValueEnumerable<T, Where_InNode<T, TPrior>>(new Where_InNode<T, TPrior>(in prior.Node, f));
        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            inner.Node.CheckForOptimization<T, Optimizations.ToList_XXX, List<T>>(new Optimizations.ToList_XXX(), out var list) switch
            {
                false => Nodes<List<T>>.Aggregation<Inner, ToList>(in inner.Node),
                true => list,
            };

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode =>
            inner.Node.CheckForOptimization<T, Optimizations.Last, T>(new Optimizations.Last(), out var last) switch
            {
                false => Nodes<T>.Aggregation<Inner, Last<T>>(in inner.Node),
                true => last,
            };


        public static int Sum<Inner>(in this ValueEnumerable<int, Inner> inner) where Inner : INode => Nodes<int>.Aggregation<Inner, SumInt>(in inner.Node);
        public static double Sum<Inner>(in this ValueEnumerable<double, Inner> inner) where Inner : INode => Nodes<double>.Aggregation<Inner, SumDouble>(in inner.Node);

        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner) where Inner : INode => Nodes<int>.Aggregation<Inner, Count>(in inner.Node);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode
            where NodeU : INode
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>>(new SelectManyNode<TSource, TResult, NodeT, NodeU>(in prior.Node, selector));
        }
    }
}
