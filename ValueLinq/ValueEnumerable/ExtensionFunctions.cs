using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public struct ValueEnumerable<T, TNode>
        : IValueEnumerable<T>
        , INode<T>
        where TNode : INode<T>
    {
        internal TNode Node;

        public ValueEnumerable(in TNode node) => Node = node;

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in Node);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        void INode.GetCountInformation(out CountInformation info) => Node.GetCountInformation(out info);

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator) => Node.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref tail, ref enumerator);
        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Node.CreateViaPushDescend<CreationType, Head, Tail>(ref nodes);

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result) => Node.TryPullOptimization<TRequest, TResult>(in request, out result);
        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum) => Node.CreateViaPull<TResult, FEnumerator>(fenum);
    }


    public static partial class Enumerable
    {
        public static T Aggregate<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, func);

        public static void ForEach<T, Inner>(in this ValueEnumerable<T, Inner> source, Action<T> func)
            where Inner : INode<T>
            => NodeImpl.ForEach(in source.Node, func);

        public static T ForEach<T, U, Inner>(in this ValueEnumerable<U, Inner> source, T seed, RefAction<T, U> func)
            where Inner : INode<U>
            => NodeImpl.ForEach(in source.Node, seed, func);

        public static T ForEach<T, U, Inner, RefAction>(in this ValueEnumerable<U, Inner> source, T seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<T, U>
            => NodeImpl.ForEach<T, U, Inner, RefAction>(in source.Node, seed, func);

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, seed, func);

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, seed, func, resultSelector);

        public static bool All<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.All(in source.Node, predicate);

        public static bool All<T, Inner, Predicate>(in this ValueEnumerable<T, Inner> source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
            => NodeImpl.All<T, Inner, Predicate>(in source.Node, predicate);

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source)
            where Inner : INode<T>
            => NodeImpl.Any<T, Inner>(in source.Node);

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Any(in source.Node, predicate);

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> selector)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, f));

        public static ValueEnumerable<U, SelectIdxNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> selector)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<T, WhereIdxNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, f));

        public static ValueEnumerable<T, SkipNode<T, TPrior>> Skip<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new (NodeImpl.Skip<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, SkipWhileNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new(NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, TakeNode<T, TPrior>> Take<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new (NodeImpl.Take<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, TakeWhileNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.TakeWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, TakeWhileIdxNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new(NodeImpl.TakeWhile(in prior.Node, predicate));

        public static T[] ToArray<T, Inner>(in this ValueEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToArray(in inner.Node, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        public static T[] ToArrayUsePool<T, Inner>(in this ValueEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            => NodeImpl.ToArrayUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static T[] ToArrayUseStack<T, Inner>(in this ValueEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToArrayUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToList(in inner.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        public static List<T> ToListUsePool<T, Inner>(in this ValueEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            => NodeImpl.ToListUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static List<T> ToListUseStack<T, Inner>(in this ValueEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToListUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.Last<T, Inner>(in inner.Node);

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Last(in inner.Node, predicate);

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.LastOrDefault<T, Inner>(in inner.Node);

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.LastOrDefault(in inner.Node, predicate);

        public static T First<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.First<T, Inner>(in inner.Node);

        public static T First<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.First(in inner.Node, predicate);

        public static T FirstOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.FirstOrDefault<T, Inner>(in inner.Node);

        public static T FirstOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.FirstOrDefault(in inner.Node, predicate);

        public static T Single<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> 
            => NodeImpl.Single<T, Inner>(in inner.Node);

        public static T Single<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Single(in inner.Node, predicate);

        public static T SingleOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.SingleOrDefault<T, Inner>(in inner.Node);

        public static T SingleOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.SingleOrDefault(in inner.Node, predicate);

        public static T ElementAt<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
            => NodeImpl.ElementAt<T, Inner>(in inner.Node, index);

        public static T ElementAtOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
            => NodeImpl.ElementAtOrDefault<T, Inner>(in inner.Node, index);

        public static decimal Average<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => NodeImpl.AverageDecimal(in inner.Node);
        public static double  Average<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>  => NodeImpl.AverageDouble(in inner.Node);
        public static float   Average<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>   => NodeImpl.AverageFloat(in inner.Node);
        public static double  Average<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>     => NodeImpl.AverageInt(in inner.Node);
        public static double  Average<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>    => NodeImpl.AverageLong(in inner.Node);

        public static decimal Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.AverageDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double> selector)  where Inner : INode<T> => NodeImpl.AverageDouble( NodeImpl.Select(in inner.Node, selector));
        public static float   Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float> selector)   where Inner : INode<T> => NodeImpl.AverageFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int> selector)     where Inner : INode<T> => NodeImpl.AverageInt(    NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long> selector)    where Inner : INode<T> => NodeImpl.AverageLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal? Average<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => NodeImpl.AverageNullableDecimal(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.AverageNullableDouble(in inner.Node);
        public static float?   Average<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.AverageNullableFloat(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.AverageNullableInt(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.AverageNullableLong(in inner.Node);

        public static decimal? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.AverageNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.AverageNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.AverageNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.AverageNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.AverageNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Min<Inner>(   in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.MinDecimal(   in inner.Node);
        public static double  Min<Inner>(   in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MinDouble(    in inner.Node);
        public static float   Min<Inner>(   in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MinFloat(     in inner.Node);
        public static int     Min<Inner>(   in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MinInt(       in inner.Node);
        public static long    Min<Inner>(   in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MinLong(      in inner.Node);
        public static T       Min<T, Inner>(in this ValueEnumerable<T,       Inner> inner) where Inner : INode<T>        => NodeImpl.Min<T, Inner>(in inner.Node);

        public static decimal Min<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.MinDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Min<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.MinDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Min<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.MinFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Min<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.MinInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Min<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.MinLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Min<T, U, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, U>       selector) where Inner : INode<T> => NodeImpl.Min<U, SelectNode<T,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Min<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  => NodeImpl.MinNullableDecimal(in inner.Node);
        public static double?  Min<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   => NodeImpl.MinNullableDouble( in inner.Node);
        public static float?   Min<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    => NodeImpl.MinNullableFloat(  in inner.Node);
        public static int?     Min<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      => NodeImpl.MinNullableInt(    in inner.Node);
        public static long?    Min<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     => NodeImpl.MinNullableLong(   in inner.Node);

        public static decimal? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.MinNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.MinNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.MinNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.MinNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.MinNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Max<Inner>(   in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.MaxDecimal(   in inner.Node);
        public static double  Max<Inner>(   in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MaxDouble(    in inner.Node);
        public static float   Max<Inner>(   in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MaxFloat(     in inner.Node);
        public static int     Max<Inner>(   in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MaxInt(       in inner.Node);
        public static long    Max<Inner>(   in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MaxLong(      in inner.Node);
        public static T       Max<T, Inner>(in this ValueEnumerable<T,       Inner> inner) where Inner : INode<T>        => NodeImpl.Max<T, Inner>(in inner.Node);

        public static decimal Max<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.MaxDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Max<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.MaxDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Max<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.MaxFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Max<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.MaxInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Max<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.MaxLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Max<T, U, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, U>       selector) where Inner : INode<T> => NodeImpl.Max<U, SelectNode<T,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Max<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => NodeImpl.MaxNullableDecimal(in inner.Node);
        public static double?  Max<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.MaxNullableDouble( in inner.Node);
        public static float?   Max<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.MaxNullableFloat(  in inner.Node);
        public static int?     Max<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.MaxNullableInt(    in inner.Node);
        public static long?    Max<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.MaxNullableLong(   in inner.Node);

        public static decimal? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.MaxNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.MaxNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.MaxNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.MaxNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.MaxNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Sum<Inner>(   in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.SumDecimal(   in inner.Node);
        public static double  Sum<Inner>(   in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.SumDouble(    in inner.Node);
        public static float   Sum<Inner>(   in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.SumFloat(     in inner.Node);
        public static int     Sum<Inner>(   in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.SumInt(       in inner.Node);
        public static long    Sum<Inner>(   in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.SumLong(      in inner.Node);

        public static decimal Sum<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.SumDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Sum<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.SumDouble( NodeImpl.Select(in inner.Node, selector));
        public static float   Sum<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.SumFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int     Sum<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.SumInt(    NodeImpl.Select(in inner.Node, selector));
        public static long    Sum<T, Inner>(   in this ValueEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.SumLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal? Sum<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  =>  NodeImpl.SumNullableDecimal(in inner.Node);
        public static double?  Sum<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   =>  NodeImpl.SumNullableDouble( in inner.Node);
        public static float?   Sum<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    =>  NodeImpl.SumNullableFloat(  in inner.Node);
        public static int?     Sum<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      =>  NodeImpl.SumNullableInt(    in inner.Node);
        public static long?    Sum<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     =>  NodeImpl.SumNullableLong(   in inner.Node);

        public static decimal? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.SumNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.SumNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.SumNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.SumNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.SumNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<T>
            => NodeImpl.Count<T, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static bool Count<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Count<T, Inner>(in source.Node, predicate);

        public static bool Contains<T, Inner>(in this ValueEnumerable<T, Inner> inner, T value)
            where Inner : INode<T>
            => NodeImpl.Contains(in inner.Node, value);

        public static bool Contains<T, Inner>(in this ValueEnumerable<T, Inner> inner, T value, IEqualityComparer<T> comparer)
            where Inner : INode<T>
            => NodeImpl.Contains(in inner.Node, value, comparer);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            => new (NodeImpl.SelectMany(in prior.Node, selector));

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TCollection, NodeU>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
            => new (NodeImpl.SelectMany(in prior.Node, collectionSelector, resultSelector));

        public static ValueEnumerable<T, ConcatNode<T, First, Second>> Concat<T, First, Second>(in this ValueEnumerable<T, First> first, in ValueEnumerable<T, Second> second)
            where First : INode<T>
            where Second : INode<T>
            => new (NodeImpl.Concat<T, First, Second>(in first.Node, in second.Node));

        public static ValueEnumerable<TSource, ReverseNode<TSource, Inner>> Reverse<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new (NodeImpl.Reverse<TSource, Inner>(in source.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo));

        public static ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>> Select<T, U, TPrior, IFunc>(in this ValueEnumerable<T, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode<T>
            where IFunc : IFuncBase<T, U>
            => new (NodeImpl.Select<T, U, TPrior, IFunc>(in prior.Node, selector, u));

        public static ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>> Where<T, TPrior, Predicate>(in this ValueEnumerable<T, TPrior> prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFuncBase<T, bool>
            => new (NodeImpl.Where<T, TPrior, Predicate>(in prior.Node, predicate));
    }

    public struct ValueOrderedEnumerable<T, TNode>
        : IValueEnumerable<T>
        , INode<T>
        where TNode : INode<T>
    {
        internal TNode Node;

        public ValueOrderedEnumerable(in TNode node) => Node = node;

        public ValueEnumerator<T> GetEnumerator() => Nodes<T>.CreateValueEnumerator(in Node);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Nodes<T>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        void INode.GetCountInformation(out CountInformation info) => Node.GetCountInformation(out info);

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator) => Node.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref tail, ref enumerator);
        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => Node.CreateViaPushDescend<CreationType, Head, Tail>(ref nodes);

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result) => Node.TryPullOptimization<TRequest, TResult>(in request, out result);
        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum) => Node.CreateViaPull<TResult, FEnumerator>(fenum);
    }


    public static partial class Enumerable
    {
        public static T Aggregate<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, func);

        public static void ForEach<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source, Action<T> func)
            where Inner : INode<T>
            => NodeImpl.ForEach(in source.Node, func);

        public static T ForEach<T, U, Inner>(in this ValueOrderedEnumerable<U, Inner> source, T seed, RefAction<T, U> func)
            where Inner : INode<U>
            => NodeImpl.ForEach(in source.Node, seed, func);

        public static T ForEach<T, U, Inner, RefAction>(in this ValueOrderedEnumerable<U, Inner> source, T seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<T, U>
            => NodeImpl.ForEach<T, U, Inner, RefAction>(in source.Node, seed, func);

        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueOrderedEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, seed, func);

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueOrderedEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
            => NodeImpl.Aggregate(in source.Node, seed, func, resultSelector);

        public static bool All<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.All(in source.Node, predicate);

        public static bool All<T, Inner, Predicate>(in this ValueOrderedEnumerable<T, Inner> source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
            => NodeImpl.All<T, Inner, Predicate>(in source.Node, predicate);

        public static bool Any<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source)
            where Inner : INode<T>
            => NodeImpl.Any<T, Inner>(in source.Node);

        public static bool Any<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Any(in source.Node, predicate);

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, U> selector)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, f));

        public static ValueEnumerable<U, SelectIdxNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, int, U> selector)
            where TPrior : INode<T>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<T, WhereIdxNode<T, TPrior>> Where<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new (NodeImpl.Where(in prior.Node, f));

        public static ValueEnumerable<T, SkipNode<T, TPrior>> Skip<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new (NodeImpl.Skip<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, SkipWhileNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new(NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, TakeNode<T, TPrior>> Take<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new (NodeImpl.Take<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, TakeWhileNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
            => new (NodeImpl.TakeWhile(in prior.Node, predicate));

        public static ValueEnumerable<T, TakeWhileIdxNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueOrderedEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
            => new(NodeImpl.TakeWhile(in prior.Node, predicate));

        public static T[] ToArray<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToArray(in inner.Node, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        public static T[] ToArrayUsePool<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            => NodeImpl.ToArrayUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static T[] ToArrayUseStack<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToArrayUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static List<T> ToList<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToList(in inner.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        public static List<T> ToListUsePool<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            => NodeImpl.ToListUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static List<T> ToListUseStack<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToListUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static T Last<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.Last<T, Inner>(in inner.Node);

        public static T Last<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Last(in inner.Node, predicate);

        public static T LastOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.LastOrDefault<T, Inner>(in inner.Node);

        public static T LastOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.LastOrDefault(in inner.Node, predicate);

        public static T First<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.First<T, Inner>(in inner.Node);

        public static T First<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.First(in inner.Node, predicate);

        public static T FirstOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.FirstOrDefault<T, Inner>(in inner.Node);

        public static T FirstOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.FirstOrDefault(in inner.Node, predicate);

        public static T Single<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T> 
            => NodeImpl.Single<T, Inner>(in inner.Node);

        public static T Single<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Single(in inner.Node, predicate);

        public static T SingleOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner)
            where Inner : INode<T>
            => NodeImpl.SingleOrDefault<T, Inner>(in inner.Node);

        public static T SingleOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.SingleOrDefault(in inner.Node, predicate);

        public static T ElementAt<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
            => NodeImpl.ElementAt<T, Inner>(in inner.Node, index);

        public static T ElementAtOrDefault<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
            => NodeImpl.ElementAtOrDefault<T, Inner>(in inner.Node, index);

        public static decimal Average<Inner>(in this ValueOrderedEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => NodeImpl.AverageDecimal(in inner.Node);
        public static double  Average<Inner>(in this ValueOrderedEnumerable<double,  Inner> inner) where Inner : INode<double>  => NodeImpl.AverageDouble(in inner.Node);
        public static float   Average<Inner>(in this ValueOrderedEnumerable<float,   Inner> inner) where Inner : INode<float>   => NodeImpl.AverageFloat(in inner.Node);
        public static double  Average<Inner>(in this ValueOrderedEnumerable<int,     Inner> inner) where Inner : INode<int>     => NodeImpl.AverageInt(in inner.Node);
        public static double  Average<Inner>(in this ValueOrderedEnumerable<long,    Inner> inner) where Inner : INode<long>    => NodeImpl.AverageLong(in inner.Node);

        public static decimal Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.AverageDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double> selector)  where Inner : INode<T> => NodeImpl.AverageDouble( NodeImpl.Select(in inner.Node, selector));
        public static float   Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float> selector)   where Inner : INode<T> => NodeImpl.AverageFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int> selector)     where Inner : INode<T> => NodeImpl.AverageInt(    NodeImpl.Select(in inner.Node, selector));
        public static double  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long> selector)    where Inner : INode<T> => NodeImpl.AverageLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal? Average<Inner>(in this ValueOrderedEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => NodeImpl.AverageNullableDecimal(in inner.Node);
        public static double?  Average<Inner>(in this ValueOrderedEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.AverageNullableDouble(in inner.Node);
        public static float?   Average<Inner>(in this ValueOrderedEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.AverageNullableFloat(in inner.Node);
        public static double?  Average<Inner>(in this ValueOrderedEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.AverageNullableInt(in inner.Node);
        public static double?  Average<Inner>(in this ValueOrderedEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.AverageNullableLong(in inner.Node);

        public static decimal? Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.AverageNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.AverageNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.AverageNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.AverageNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.AverageNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Min<Inner>(   in this ValueOrderedEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.MinDecimal(   in inner.Node);
        public static double  Min<Inner>(   in this ValueOrderedEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MinDouble(    in inner.Node);
        public static float   Min<Inner>(   in this ValueOrderedEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MinFloat(     in inner.Node);
        public static int     Min<Inner>(   in this ValueOrderedEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MinInt(       in inner.Node);
        public static long    Min<Inner>(   in this ValueOrderedEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MinLong(      in inner.Node);
        public static T       Min<T, Inner>(in this ValueOrderedEnumerable<T,       Inner> inner) where Inner : INode<T>        => NodeImpl.Min<T, Inner>(in inner.Node);

        public static decimal Min<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.MinDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Min<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.MinDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Min<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.MinFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Min<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.MinInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Min<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.MinLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Min<T, U, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, U>       selector) where Inner : INode<T> => NodeImpl.Min<U, SelectNode<T,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Min<Inner>(in this ValueOrderedEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  => NodeImpl.MinNullableDecimal(in inner.Node);
        public static double?  Min<Inner>(in this ValueOrderedEnumerable<double?,  Inner> inner) where Inner : INode<double?>   => NodeImpl.MinNullableDouble( in inner.Node);
        public static float?   Min<Inner>(in this ValueOrderedEnumerable<float?,   Inner> inner) where Inner : INode<float?>    => NodeImpl.MinNullableFloat(  in inner.Node);
        public static int?     Min<Inner>(in this ValueOrderedEnumerable<int?,     Inner> inner) where Inner : INode<int?>      => NodeImpl.MinNullableInt(    in inner.Node);
        public static long?    Min<Inner>(in this ValueOrderedEnumerable<long?,    Inner> inner) where Inner : INode<long?>     => NodeImpl.MinNullableLong(   in inner.Node);

        public static decimal? Min<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.MinNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Min<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.MinNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Min<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.MinNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Min<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.MinNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Min<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.MinNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Max<Inner>(   in this ValueOrderedEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.MaxDecimal(   in inner.Node);
        public static double  Max<Inner>(   in this ValueOrderedEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MaxDouble(    in inner.Node);
        public static float   Max<Inner>(   in this ValueOrderedEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MaxFloat(     in inner.Node);
        public static int     Max<Inner>(   in this ValueOrderedEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MaxInt(       in inner.Node);
        public static long    Max<Inner>(   in this ValueOrderedEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MaxLong(      in inner.Node);
        public static T       Max<T, Inner>(in this ValueOrderedEnumerable<T,       Inner> inner) where Inner : INode<T>        => NodeImpl.Max<T, Inner>(in inner.Node);

        public static decimal Max<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.MaxDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Max<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.MaxDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Max<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.MaxFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Max<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.MaxInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Max<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.MaxLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Max<T, U, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, U>       selector) where Inner : INode<T> => NodeImpl.Max<U, SelectNode<T,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Max<Inner>(in this ValueOrderedEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => NodeImpl.MaxNullableDecimal(in inner.Node);
        public static double?  Max<Inner>(in this ValueOrderedEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.MaxNullableDouble( in inner.Node);
        public static float?   Max<Inner>(in this ValueOrderedEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.MaxNullableFloat(  in inner.Node);
        public static int?     Max<Inner>(in this ValueOrderedEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.MaxNullableInt(    in inner.Node);
        public static long?    Max<Inner>(in this ValueOrderedEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.MaxNullableLong(   in inner.Node);

        public static decimal? Max<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.MaxNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Max<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.MaxNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Max<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.MaxNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Max<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.MaxNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Max<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.MaxNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Sum<Inner>(   in this ValueOrderedEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => NodeImpl.SumDecimal(   in inner.Node);
        public static double  Sum<Inner>(   in this ValueOrderedEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.SumDouble(    in inner.Node);
        public static float   Sum<Inner>(   in this ValueOrderedEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.SumFloat(     in inner.Node);
        public static int     Sum<Inner>(   in this ValueOrderedEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.SumInt(       in inner.Node);
        public static long    Sum<Inner>(   in this ValueOrderedEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.SumLong(      in inner.Node);

        public static decimal Sum<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> => NodeImpl.SumDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Sum<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double>  selector) where Inner : INode<T> => NodeImpl.SumDouble( NodeImpl.Select(in inner.Node, selector));
        public static float   Sum<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float>   selector) where Inner : INode<T> => NodeImpl.SumFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int     Sum<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int>     selector) where Inner : INode<T> => NodeImpl.SumInt(    NodeImpl.Select(in inner.Node, selector));
        public static long    Sum<T, Inner>(   in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long>    selector) where Inner : INode<T> => NodeImpl.SumLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal? Sum<Inner>(in this ValueOrderedEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  =>  NodeImpl.SumNullableDecimal(in inner.Node);
        public static double?  Sum<Inner>(in this ValueOrderedEnumerable<double?,  Inner> inner) where Inner : INode<double?>   =>  NodeImpl.SumNullableDouble( in inner.Node);
        public static float?   Sum<Inner>(in this ValueOrderedEnumerable<float?,   Inner> inner) where Inner : INode<float?>    =>  NodeImpl.SumNullableFloat(  in inner.Node);
        public static int?     Sum<Inner>(in this ValueOrderedEnumerable<int?,     Inner> inner) where Inner : INode<int?>      =>  NodeImpl.SumNullableInt(    in inner.Node);
        public static long?    Sum<Inner>(in this ValueOrderedEnumerable<long?,    Inner> inner) where Inner : INode<long?>     =>  NodeImpl.SumNullableLong(   in inner.Node);

        public static decimal? Sum<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> => NodeImpl.SumNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Sum<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, double?>  selector) where Inner : INode<T> => NodeImpl.SumNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Sum<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, float?>   selector) where Inner : INode<T> => NodeImpl.SumNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Sum<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, int?>     selector) where Inner : INode<T> => NodeImpl.SumNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Sum<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, Func<T, long?>    selector) where Inner : INode<T> => NodeImpl.SumNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static int Count<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<T>
            => NodeImpl.Count<T, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static bool Count<T, Inner>(in this ValueOrderedEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
            => NodeImpl.Count<T, Inner>(in source.Node, predicate);

        public static bool Contains<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, T value)
            where Inner : INode<T>
            => NodeImpl.Contains(in inner.Node, value);

        public static bool Contains<T, Inner>(in this ValueOrderedEnumerable<T, Inner> inner, T value, IEqualityComparer<T> comparer)
            where Inner : INode<T>
            => NodeImpl.Contains(in inner.Node, value, comparer);

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueOrderedEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            => new (NodeImpl.SelectMany(in prior.Node, selector));

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU>(in this ValueOrderedEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TCollection, NodeU>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
            => new (NodeImpl.SelectMany(in prior.Node, collectionSelector, resultSelector));

        public static ValueEnumerable<T, ConcatNode<T, First, Second>> Concat<T, First, Second>(in this ValueOrderedEnumerable<T, First> first, in ValueEnumerable<T, Second> second)
            where First : INode<T>
            where Second : INode<T>
            => new (NodeImpl.Concat<T, First, Second>(in first.Node, in second.Node));

        public static ValueEnumerable<TSource, ReverseNode<TSource, Inner>> Reverse<TSource, Inner>(in this ValueOrderedEnumerable<TSource, Inner> source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new (NodeImpl.Reverse<TSource, Inner>(in source.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo));

        public static ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>> Select<T, U, TPrior, IFunc>(in this ValueOrderedEnumerable<T, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode<T>
            where IFunc : IFuncBase<T, U>
            => new (NodeImpl.Select<T, U, TPrior, IFunc>(in prior.Node, selector, u));

        public static ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>> Where<T, TPrior, Predicate>(in this ValueOrderedEnumerable<T, TPrior> prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFuncBase<T, bool>
            => new (NodeImpl.Where<T, TPrior, Predicate>(in prior.Node, predicate));
    }

}
