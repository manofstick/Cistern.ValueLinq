using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Maths;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public struct ValueEnumerable<TSource, TNode>
        : IValueEnumerable<TSource>
        , INode<TSource>
        where TNode : INode<TSource>
    {
        internal TNode Node;

        public ValueEnumerable(in TNode node)
            => Node = node;

        public ValueEnumerator<TSource> GetEnumerator()
            => Nodes<TSource>.CreateValueEnumerator(in Node);
        IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            => Nodes<TSource>.CreateEnumerator(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => ((IEnumerable<TSource>)this).GetEnumerator();

        void INode.GetCountInformation(out CountInformation info)
            => Node.GetCountInformation(out info);

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => Node.TryPullOptimization<TRequest, TResult, Nodes>(in request, ref nodes, out creation);
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
            => Node.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref tail, ref enumerator);
        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Node.CreateViaPullDescend<CreationType, Head, Tail>(ref nodes);

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => Node.TryPushOptimization<TRequest, TResult>(in request, out result);
        TResult INode<TSource>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
            => Node.CreateViaPush<TResult, FEnumerator>(fenum);
    }


    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, Func<TSource, TSource, TSource> func)
            where Inner : INode<TSource>
            => NodeImpl.Aggregate(in source.Node, func);

        public static void ForEach<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, Action<TSource> func)
            where Inner : INode<TSource>
            => NodeImpl.ForEach(in source.Node, func);

        public static TSource ForEach<TSource, U, Inner>(in this ValueEnumerable<U, Inner> source, TSource seed, RefAction<TSource, U> func)
            where Inner : INode<U>
            => NodeImpl.ForEach(in source.Node, seed, func);

        public static TSource ForEach<TSource, U, Inner, RefAction>(in this ValueEnumerable<U, Inner> source, TSource seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<TSource, U>
            => NodeImpl.ForEach<TSource, U, Inner, RefAction>(in source.Node, seed, func);

        public static TAccumulate Aggregate<TSource, TAccumulate, Inner>(in this ValueEnumerable<TSource, Inner> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            where Inner : INode<TSource>
            => NodeImpl.Aggregate(in source.Node, seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult, Inner>(in this ValueEnumerable<TSource, Inner> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<TSource>
            => NodeImpl.Aggregate(in source.Node, seed, func, resultSelector);

        public static bool All<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.All(in source.Node, predicate);

        public static bool All<TSource, Inner, Predicate>(in this ValueEnumerable<TSource, Inner> source, Predicate predicate)
            where Inner : INode<TSource>
            where Predicate : IFunc<TSource, bool>
            => NodeImpl.All<TSource, Inner, Predicate>(in source.Node, predicate);

        public static bool Any<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source)
            where Inner : INode<TSource>
            => NodeImpl.Any<TSource, Inner>(in source.Node);

        public static bool Any<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.Any(in source.Node, predicate);

        public static ValueEnumerable<U, SelectNode<TSource, U, TPrior>> Select<TSource, U, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, U> selector)
            where TPrior : INode<TSource>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<U, Select_InNode<TSource, U, TPrior>> Select<TSource, U, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, InFunc<TSource, U> f)
            where TPrior : INode<TSource>
            => new (NodeImpl.Select(in prior.Node, f));

        public static ValueEnumerable<U, SelectIdxNode<TSource, U, TPrior>> Select<TSource, U, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, int, U> selector)
            where TPrior : INode<TSource>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<TSource, WhereNode<TSource, TPrior>> Where<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<TSource, WhereIdxNode<TSource, TPrior>> Where<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Where_InNode<TSource, TPrior>> Where<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, InFunc<TSource, bool> f)
            where TPrior : INode<TSource>
            => new (NodeImpl.Where(in prior.Node, f));

        public static ValueEnumerable<TSource, SkipNode<TSource, TPrior>> Skip<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, int count)
            where TPrior : INode<TSource>
            => new (NodeImpl.Skip<TSource, TPrior>(in prior.Node, count));

        public static ValueEnumerable<TSource, SkipWhileNode<TSource, TPrior>> SkipWhile<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            => new(NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, SkipWhileIdxNode<TSource, TPrior>> SkipWhile<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            => new (NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, TakeNode<TSource, TPrior>> Take<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, int count)
            where TPrior : INode<TSource>
            => new (NodeImpl.Take<TSource, TPrior>(in prior.Node, count));

        public static ValueEnumerable<TSource, TakeWhileNode<TSource, TPrior>> TakeWhile<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            => new (NodeImpl.TakeWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, TakeWhileIdxNode<TSource, TPrior>> TakeWhile<TSource, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            => new(NodeImpl.TakeWhile(in prior.Node, predicate));

        public static TSource[] ToArray<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => NodeImpl.ToArray(in inner.Node, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        public static TSource[] ToArrayUsePool<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, ArrayPool<TSource> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<TSource>
            => NodeImpl.ToArrayUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static TSource[] ToArrayUseStack<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int maxStackItemCount = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => NodeImpl.ToArrayUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static List<TSource> ToList<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => NodeImpl.ToList(in inner.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        public static List<TSource> ToListUsePool<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, ArrayPool<TSource> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<TSource>
            => NodeImpl.ToListUsePool(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static List<TSource> ToListUseStack<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int maxStackItemCount = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => NodeImpl.ToListUseStack(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static HashSet<TSource> ToHashSet<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, IEqualityComparer<TSource> equalityComparer = null)
            where Inner : INode<TSource>
            => NodeImpl.ToHashSet(in inner.Node, equalityComparer);

        public static Dictionary<TKey, TSource> ToHashSet<TSource, TKey, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> equalityComparer = null)
            where Inner : INode<TSource>
            => NodeImpl.ToDictionary(in inner.Node, keySelector, equalityComparer);

        public static Dictionary<TKey, TValue> ToHashSet<TSource, TKey, TValue, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> equalityComparer = null)
            where Inner : INode<TSource>
            => NodeImpl.ToDictionary(in inner.Node, keySelector, valueSelector, equalityComparer);

        public static TSource Last<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource>
            => NodeImpl.Last<TSource, Inner>(in inner.Node);

        public static TSource Last<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.Last(in inner.Node, predicate);

        public static TSource LastOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource>
            => NodeImpl.LastOrDefault<TSource, Inner>(in inner.Node);

        public static TSource LastOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.LastOrDefault(in inner.Node, predicate);

        public static TSource First<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource>
            => NodeImpl.First<TSource, Inner>(in inner.Node);

        public static TSource First<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.First(in inner.Node, predicate);

        public static TSource FirstOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource>
            => NodeImpl.FirstOrDefault<TSource, Inner>(in inner.Node);

        public static TSource FirstOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.FirstOrDefault(in inner.Node, predicate);

        public static TSource Single<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource> 
            => NodeImpl.Single<TSource, Inner>(in inner.Node);

        public static TSource Single<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.Single(in inner.Node, predicate);

        public static TSource SingleOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner)
            where Inner : INode<TSource>
            => NodeImpl.SingleOrDefault<TSource, Inner>(in inner.Node);

        public static TSource SingleOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.SingleOrDefault(in inner.Node, predicate);

        public static TSource ElementAt<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int index)
            where Inner : INode<TSource>
            => NodeImpl.ElementAt<TSource, Inner>(in inner.Node, index);

        public static TSource ElementAtOrDefault<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, int index)
            where Inner : INode<TSource>
            => NodeImpl.ElementAtOrDefault<TSource, Inner>(in inner.Node, index);

        public static decimal Average<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<Decimal> => NodeImpl.AverageDecimal(in inner.Node);
        public static double  Average<Inner>(in this ValueEnumerable<double,  Inner> inner, SIMDOptions simdOptions) where Inner : INode<double>  => NodeImpl.AverageDouble(in inner.Node, simdOptions);
        public static float   Average<Inner>(in this ValueEnumerable<float,   Inner> inner, SIMDOptions simdOptions) where Inner : INode<float>   => NodeImpl.AverageFloat(in inner.Node, simdOptions);
        public static double  Average<Inner>(in this ValueEnumerable<int,     Inner> inner, SIMDOptions simdOptions) where Inner : INode<int>     => NodeImpl.AverageInt(in inner.Node, simdOptions);
        public static double  Average<Inner>(in this ValueEnumerable<long,    Inner> inner, SIMDOptions simdOptions) where Inner : INode<long>    => NodeImpl.AverageLong(in inner.Node, simdOptions);

        public static decimal Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> => NodeImpl.AverageDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double> selector)  where Inner : INode<TSource> => NodeImpl.AverageDouble( NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static float   Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float> selector)   where Inner : INode<TSource> => NodeImpl.AverageFloat(  NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static double  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int> selector)     where Inner : INode<TSource> => NodeImpl.AverageInt(    NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static double  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long> selector)    where Inner : INode<TSource> => NodeImpl.AverageLong(   NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);

        public static decimal? Average<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<Decimal?> => NodeImpl.AverageNullableDecimal(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.AverageNullableDouble(in inner.Node);
        public static float?   Average<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.AverageNullableFloat(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.AverageNullableInt(in inner.Node);
        public static double?  Average<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.AverageNullableLong(in inner.Node);

        public static decimal? Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> => NodeImpl.AverageNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> => NodeImpl.AverageNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> => NodeImpl.AverageNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> => NodeImpl.AverageNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> => NodeImpl.AverageNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Min<Inner>(   in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<Decimal>  => NodeImpl.MinDecimal(   in inner.Node);
        public static double  Min<Inner>(   in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MinDouble(    in inner.Node);
        public static float   Min<Inner>(   in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MinFloat(     in inner.Node);
        public static int     Min<Inner>(   in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MinInt(       in inner.Node);
        public static long    Min<Inner>(   in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MinLong(      in inner.Node);
        public static TSource       Min<TSource, Inner>(in this ValueEnumerable<TSource,       Inner> inner) where Inner : INode<TSource>        => NodeImpl.Min<TSource, Inner>(in inner.Node);

        public static decimal Min<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> => NodeImpl.MinDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Min<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double>  selector) where Inner : INode<TSource> => NodeImpl.MinDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Min<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float>   selector) where Inner : INode<TSource> => NodeImpl.MinFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Min<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int>     selector) where Inner : INode<TSource> => NodeImpl.MinInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Min<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long>    selector) where Inner : INode<TSource> => NodeImpl.MinLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Min<TSource, U, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, U>       selector) where Inner : INode<TSource> => NodeImpl.Min<U, SelectNode<TSource,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Min<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<Decimal?>  => NodeImpl.MinNullableDecimal(in inner.Node);
        public static double?  Min<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   => NodeImpl.MinNullableDouble( in inner.Node);
        public static float?   Min<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    => NodeImpl.MinNullableFloat(  in inner.Node);
        public static int?     Min<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      => NodeImpl.MinNullableInt(    in inner.Node);
        public static long?    Min<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     => NodeImpl.MinNullableLong(   in inner.Node);

        public static decimal? Min<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> => NodeImpl.MinNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Min<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> => NodeImpl.MinNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Min<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> => NodeImpl.MinNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Min<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> => NodeImpl.MinNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Min<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> => NodeImpl.MinNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Max<Inner>(   in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<Decimal> => NodeImpl.MaxDecimal(   in inner.Node);
        public static double  Max<Inner>(   in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => NodeImpl.MaxDouble(    in inner.Node);
        public static float   Max<Inner>(   in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => NodeImpl.MaxFloat(     in inner.Node);
        public static int     Max<Inner>(   in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => NodeImpl.MaxInt(       in inner.Node);
        public static long    Max<Inner>(   in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => NodeImpl.MaxLong(      in inner.Node);
        public static TSource       Max<TSource, Inner>(in this ValueEnumerable<TSource,       Inner> inner) where Inner : INode<TSource>        => NodeImpl.Max<TSource, Inner>(in inner.Node);

        public static decimal Max<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> => NodeImpl.MaxDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Max<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double>  selector) where Inner : INode<TSource> => NodeImpl.MaxDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Max<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float>   selector) where Inner : INode<TSource> => NodeImpl.MaxFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Max<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int>     selector) where Inner : INode<TSource> => NodeImpl.MaxInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Max<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long>    selector) where Inner : INode<TSource> => NodeImpl.MaxLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U       Max<TSource, U, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, U>       selector) where Inner : INode<TSource> => NodeImpl.Max<U, SelectNode<TSource,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Max<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<Decimal?> => NodeImpl.MaxNullableDecimal(in inner.Node);
        public static double?  Max<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => NodeImpl.MaxNullableDouble( in inner.Node);
        public static float?   Max<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => NodeImpl.MaxNullableFloat(  in inner.Node);
        public static int?     Max<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => NodeImpl.MaxNullableInt(    in inner.Node);
        public static long?    Max<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => NodeImpl.MaxNullableLong(   in inner.Node);

        public static decimal? Max<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> => NodeImpl.MaxNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Max<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> => NodeImpl.MaxNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Max<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> => NodeImpl.MaxNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Max<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> => NodeImpl.MaxNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Max<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> => NodeImpl.MaxNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Sum<Inner>(in this ValueEnumerable<decimal, Inner> inner)                                                   where Inner : INode<Decimal> => NodeImpl.SumDecimal(   in inner.Node);
        public static double  Sum<Inner>(in this ValueEnumerable<double,  Inner> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<double>  => NodeImpl.SumDouble(    in inner.Node, simdOptions);
        public static float   Sum<Inner>(in this ValueEnumerable<float,   Inner> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<float>   => NodeImpl.SumFloat(     in inner.Node, simdOptions);
        public static int     Sum<Inner>(in this ValueEnumerable<int,     Inner> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<int>     => NodeImpl.SumInt(       in inner.Node, simdOptions);
        public static long    Sum<Inner>(in this ValueEnumerable<long,    Inner> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<long>    => NodeImpl.SumLong(      in inner.Node, simdOptions);

        public static decimal Sum<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> => NodeImpl.SumDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Sum<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double>  selector) where Inner : INode<TSource> => NodeImpl.SumDouble( NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static float   Sum<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float>   selector) where Inner : INode<TSource> => NodeImpl.SumFloat(  NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static int     Sum<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int>     selector) where Inner : INode<TSource> => NodeImpl.SumInt(    NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static long    Sum<TSource, Inner>(   in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long>    selector) where Inner : INode<TSource> => NodeImpl.SumLong(   NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);

        public static decimal? Sum<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<Decimal?>  =>  NodeImpl.SumNullableDecimal(in inner.Node);
        public static double?  Sum<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   =>  NodeImpl.SumNullableDouble( in inner.Node);
        public static float?   Sum<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    =>  NodeImpl.SumNullableFloat(  in inner.Node);
        public static int?     Sum<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      =>  NodeImpl.SumNullableInt(    in inner.Node);
        public static long?    Sum<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     =>  NodeImpl.SumNullableLong(   in inner.Node);

        public static decimal? Sum<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> => NodeImpl.SumNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Sum<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> => NodeImpl.SumNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Sum<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> => NodeImpl.SumNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Sum<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> => NodeImpl.SumNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Sum<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> => NodeImpl.SumNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static int Count<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<TSource>
            => NodeImpl.Count<TSource, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static bool Count<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            => NodeImpl.Count<TSource, Inner>(in source.Node, predicate);

        public static bool Contains<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, TSource value)
            where Inner : INode<TSource>
            => NodeImpl.Contains(in inner.Node, value);

        public static bool Contains<TSource, Inner>(in this ValueEnumerable<TSource, Inner> inner, TSource value, IEqualityComparer<TSource> comparer)
            where Inner : INode<TSource>
            => NodeImpl.Contains(in inner.Node, value, comparer);

        public static ValueEnumerable<TResult, SelectManyNode<TResult, ValueEnumerable<TResult, NodeU>, SelectNode<TSource, ValueEnumerable<TResult, NodeU>, NodeT>>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            => new (NodeImpl.SelectMany<TSource, TResult, NodeT, ValueEnumerable<TResult, NodeU>>(in prior.Node, selector));

        public static ValueEnumerable<TResult, SelectManyNode<TResult, ValueEnumerable<TResult, NodeU>, SelectIdxNode<TSource, ValueEnumerable<TResult, NodeU>, NodeT>>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, int, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            => new(NodeImpl.SelectMany<TSource, TResult, NodeT, ValueEnumerable<TResult, NodeU>>(in prior.Node, selector));

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
        //    where NodeT : INode<TSource>
        //    where NodeU : INode<TResult>
        //    => new (NodeImpl.SelectMany(in prior.Node, selector));

        //public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TCollection, NodeU>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        //    where NodeT : INode<TSource>
        //    where NodeU : INode<TCollection>
        //    => new(NodeImpl.SelectMany(in prior.Node, collectionSelector, resultSelector));

        public static ValueEnumerable<TSource, ConcatNode<TSource, First, Second>> Concat<TSource, First, Second>(in this ValueEnumerable<TSource, First> first, in ValueEnumerable<TSource, Second> second)
            where First : INode<TSource>
            where Second : INode<TSource>
            => new (NodeImpl.Concat<TSource, First, Second>(in first.Node, in second.Node));

        public static ValueEnumerable<TSource, ReverseNode<TSource, Inner>> Reverse<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new (NodeImpl.Reverse<TSource, Inner>(in source.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo));

        public static ValueEnumerable<U, ValueSelectNode<TSource, U, TPrior, IFunc>> Select<TSource, U, TPrior, IFunc>(in this ValueEnumerable<TSource, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode<TSource>
            where IFunc : IFuncBase<TSource, U>
            => new (NodeImpl.Select<TSource, U, TPrior, IFunc>(in prior.Node, selector, u));

        public static ValueEnumerable<TSource, ValueWhereNode<TSource, TPrior, Predicate>> Where<TSource, TPrior, Predicate>(in this ValueEnumerable<TSource, TPrior> prior, Predicate predicate)
            where TPrior : INode<TSource>
            where Predicate : IFuncBase<TSource, bool>
            => new (NodeImpl.Where<TSource, TPrior, Predicate>(in prior.Node, predicate));

        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, TPrior>> OrderBy<TSource, TKey, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            where TPrior : INode<TSource>
            => new (NodeImpl.OrderBy<TSource, TKey, TPrior>(in prior.Node, keySelector, comparer, false));

        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, TPrior>> OrderByDescending<TSource, TKey, TPrior>(in this ValueEnumerable<TSource, TPrior> prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            where TPrior : INode<TSource>
            => new (NodeImpl.OrderBy<TSource, TKey, TPrior>(in prior.Node, keySelector, comparer, true));

        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>> ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in this ValueEnumerable<TSource, OrderByNode<TSource, TPriorKeySelector, TPrior>> prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            where TPrior : INode<TSource>
            where TPriorKeySelector : IKeySelectors<TSource>
            => new (NodeImpl.ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in prior.Node, keySelector, comparer, false));

        public static ValueEnumerable<TSource, OrderByNode<TSource, KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>> ThenByDescending<TSource, TKey, TPriorKeySelector, TPrior>(in this ValueEnumerable<TSource, OrderByNode<TSource, TPriorKeySelector, TPrior>> prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer = null)
            where TPrior : INode<TSource>
            where TPriorKeySelector : IKeySelectors<TSource>
            => new (NodeImpl.ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in prior.Node, keySelector, comparer, true));

        public static ValueEnumerable<(TFirst First, TSecond Second), ZipNode<TFirst, TSecond, FirstNode, SecondNode>> Zip<TFirst, TSecond, FirstNode, SecondNode>(in this ValueEnumerable<TFirst, FirstNode> first, in ValueEnumerable<TSecond, SecondNode> second)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            => new (NodeImpl.Zip<TFirst, TSecond, FirstNode, SecondNode>(in first.Node, in second.Node));

        public static (U, V) Fork<T, U, V, InnerNode>(in this ValueEnumerable<T, InnerNode> node, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v)
            where InnerNode : INode<T>
            => NodeImpl.Fork<T, U, V, InnerNode>(in node.Node, t2u, t2v);

        public static (U, V, W) Fork<T, U, V, W, InnerNode>(in this ValueEnumerable<T, InnerNode> node, Func<ValueEnumerable<T, Aggregation.Fork<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>>, W> t2w)
            where InnerNode : INode<T>
            => NodeImpl.Fork<T, U, V, W, InnerNode>(in node.Node, t2u, t2v, t2w);

        public static ValueEnumerable<TSource, ExceptNode<TSource, InnerNode>> Except<TSource, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer = null)
            where InnerNode : INode<TSource>
            => new (NodeImpl.Except<TSource, InnerNode>(in node.Node, second, comparer));
        public static ValueEnumerable<TSource, ExceptNode<TSource, InnerNode>> Distinct<TSource, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, IEqualityComparer<TSource> comparer = null)
            where InnerNode : INode<TSource>
            => ne w(NodeImpl.Except<TSource, InnerNode>(in node.Node, Array.Empty<TSource>(), comparer));

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, GroupByNode<TSource, TKey, InnerNode>> GroupBy<TSource, TKey, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => new (NodeImpl.GroupBy(node.Node, keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, GroupByNode<TSource, TKey, TElement, InnerNode>> GroupBy<TSource, TKey, TElement, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => new(NodeImpl.GroupBy(node.Node, keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, GroupByResultNode<TSource, TKey, TResult, InnerNode>> GroupBy<TSource, TKey, TResult, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => new(NodeImpl.GroupBy(node.Node, keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, GroupByResultNode<TSource, TKey, TElement, TResult, InnerNode>> GroupBy<TSource, TKey, TElement, TResult, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => new(NodeImpl.GroupBy(node.Node, keySelector, elementSelector, resultSelector, comparer));

        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => NodeImpl.ToLookup(node.Node, keySelector, comparer);
        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement, InnerNode>(in this ValueEnumerable<TSource, InnerNode> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer = null)
            where InnerNode : INode<TSource>
            => NodeImpl.ToLookup(node.Node, keySelector, elementSelector, comparer);

    }
}
