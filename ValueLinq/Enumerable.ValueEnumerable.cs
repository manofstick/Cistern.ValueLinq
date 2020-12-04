using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Collections.Generic;

using AverageDecimal = Cistern.ValueLinq.Aggregation.Average<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDouble  = Cistern.ValueLinq.Aggregation.Average<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using AverageFloat   = Cistern.ValueLinq.Aggregation.Average<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using AverageInt     = Cistern.ValueLinq.Aggregation.Average<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using AverageLong    = Cistern.ValueLinq.Aggregation.Average<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using AverageDecimalNullable = Cistern.ValueLinq.Aggregation.AverageNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using AverageDoubleNullable  = Cistern.ValueLinq.Aggregation.AverageNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using AverageFloatNullable   = Cistern.ValueLinq.Aggregation.AverageNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using AverageIntNullable     = Cistern.ValueLinq.Aggregation.AverageNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using AverageLongNullable    = Cistern.ValueLinq.Aggregation.AverageNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MinDecimal = Cistern.ValueLinq.Aggregation.Min<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDouble  = Cistern.ValueLinq.Aggregation.Min<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MinFloat   = Cistern.ValueLinq.Aggregation.Min<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MinInt     = Cistern.ValueLinq.Aggregation.Min<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MinLong    = Cistern.ValueLinq.Aggregation.Min<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MinDecimalNullable = Cistern.ValueLinq.Aggregation.MinNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MinDoubleNullable  = Cistern.ValueLinq.Aggregation.MinNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MinFloatNullable   = Cistern.ValueLinq.Aggregation.MinNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MinIntNullable     = Cistern.ValueLinq.Aggregation.MinNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MinLongNullable    = Cistern.ValueLinq.Aggregation.MinNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MaxDecimal = Cistern.ValueLinq.Aggregation.Max<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDouble  = Cistern.ValueLinq.Aggregation.Max<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MaxFloat   = Cistern.ValueLinq.Aggregation.Max<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MaxInt     = Cistern.ValueLinq.Aggregation.Max<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MaxLong    = Cistern.ValueLinq.Aggregation.Max<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using MaxDecimalNullable = Cistern.ValueLinq.Aggregation.MaxNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using MaxDoubleNullable  = Cistern.ValueLinq.Aggregation.MaxNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using MaxFloatNullable   = Cistern.ValueLinq.Aggregation.MaxNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using MaxIntNullable     = Cistern.ValueLinq.Aggregation.MaxNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using MaxLongNullable    = Cistern.ValueLinq.Aggregation.MaxNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using SumDecimal = Cistern.ValueLinq.Aggregation.Sum<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDouble  = Cistern.ValueLinq.Aggregation.Sum<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using SumFloat   = Cistern.ValueLinq.Aggregation.Sum<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using SumInt     = Cistern.ValueLinq.Aggregation.Sum<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using SumLong    = Cistern.ValueLinq.Aggregation.Sum<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;

using SumDecimalNullable = Cistern.ValueLinq.Aggregation.SumNullable<decimal, decimal, decimal, Cistern.ValueLinq.Maths.OpsDecimal>;
using SumDoubleNullable  = Cistern.ValueLinq.Aggregation.SumNullable<double,  double,  double,  Cistern.ValueLinq.Maths.OpsDouble>;
using SumFloatNullable   = Cistern.ValueLinq.Aggregation.SumNullable<float,   double,  float,   Cistern.ValueLinq.Maths.OpsFloat>;
using SumIntNullable     = Cistern.ValueLinq.Aggregation.SumNullable<int,     int,     double,  Cistern.ValueLinq.Maths.OpsInt>;
using SumLongNullable    = Cistern.ValueLinq.Aggregation.SumNullable<long,    long,    double,  Cistern.ValueLinq.Maths.OpsLong>;
using System.Buffers;
using Cistern.ValueLinq.Utils;

namespace Cistern.ValueLinq
{
    public static partial class Enumerable
    {
        private static void GetCountInformation<Node>(this Node node, out CountInformation ci) where Node : INode => node.GetCountInformation(out ci);

        public static T Aggregate<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, T, T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, ReduceForward<T>>(new ReduceForward<T>(func));
        }

        public static void ForEach<T, Inner>(in this ValueEnumerable<T, Inner> source, Action<T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            source.Node.CreateObjectViaFastEnumerator<T, ForEachForward<T>>(new ForEachForward<T>(func));
        }

        public static T ForEach<T, U, Inner>(in this ValueEnumerable<U, Inner> source, T seed, RefAction<T, U> func)
            where Inner : INode<U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, ForEachForwardRef<T, U>>(new ForEachForwardRef<T, U>(seed, func));
        }

        public static T Foreach<T, U, Inner, RefAction>(in this ValueEnumerable<U, Inner> source, T seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<T, U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<T, ValueForeachForwardRef<T, U, RefAction>>(new ValueForeachForwardRef<T, U, RefAction>(seed, func));
        }


        public static TAccumulate Aggregate<T, TAccumulate, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.Node.CreateObjectViaFastEnumerator<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed));
        }

        public static TResult Aggregate<T, TAccumulate, TResult, Inner>(in this ValueEnumerable<T, Inner> source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return resultSelector(source.Node.CreateObjectViaFastEnumerator<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed)));
        }

        public static bool All<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, All<T, FuncToIFunc<T, bool>>>(new All<T, FuncToIFunc<T, bool>>(new FuncToIFunc<T, bool>(predicate)));
        }

        public static bool All<T, Inner, Predicate>(in this ValueEnumerable<T, Inner> source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, All<T, Predicate>>(new All<T, Predicate>(predicate));
        }

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source)
            where Inner : INode<T>
            => source.Node.CreateObjectViaFastEnumerator<bool, Anything<T>>(new Anything<T>());

        public static bool Any<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, Any<T>>(new Any<T>(predicate));
        }

        // -- 

        public static ValueEnumerable<U, SelectNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectNode<T, U, TPrior>>(new SelectNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<U, Select_InNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new ValueEnumerable<U, Select_InNode<T, U, TPrior>>(new Select_InNode<T, U, TPrior>(in prior.Node, f));

        public static ValueEnumerable<U, SelectIdxNode<T, U, TPrior>> Select<T, U, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, SelectIdxNode<T, U, TPrior>>(new SelectIdxNode<T, U, TPrior>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, WhereNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereNode<T, TPrior>>(new WhereNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, WhereIdxNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, WhereIdxNode<T, TPrior>>(new WhereIdxNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, Where_InNode<T, TPrior>> Where<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new ValueEnumerable<T, Where_InNode<T, TPrior>>(new Where_InNode<T, TPrior>(in prior.Node, f));

        public static ValueEnumerable<T, SkipNode<T, TPrior>> Skip<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
            => new ValueEnumerable<T, SkipNode<T, TPrior>>(new SkipNode<T, TPrior>(in prior.Node, count));

        public static ValueEnumerable<T, SkipWhileNode<T, TPrior>> SkipWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, SkipWhileNode<T, TPrior>>(new SkipWhileNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>> SkipWhileIdx<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, SkipWhileIdxNode<T, TPrior>>(new SkipWhileIdxNode<T, TPrior>(in prior.Node, predicate));
        }

        public static ValueEnumerable<T, TakeNode<T, TPrior>> Take<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, int count)
            where TPrior : INode<T>
                => new ValueEnumerable<T, TakeNode<T, TPrior>>(new TakeNode<T, TPrior>(in prior.Node, count));
        public static ValueEnumerable<T, TakeWhileNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, TakeWhileNode<T, TPrior>>(new TakeWhileNode<T, TPrior>(in prior.Node, predicate));
        }
        public static ValueEnumerable<T, TakeWhileIdxNode<T, TPrior>> TakeWhile<T, TPrior>(in this ValueEnumerable<T, TPrior> prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, TakeWhileIdxNode<T, TPrior>>(new TakeWhileIdxNode<T, TPrior>(in prior.Node, predicate));
        }

        public static T[] ToArray<T, Inner>(in this ValueEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => NodeImpl.ToArray(in inner.Node, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        public static T[] ToArrayUsePool<T, Inner>(in this ValueEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            return viaPull
                ? Nodes<T[]>.Aggregation<Inner, ToArrayViaArrayPool<T>>(in inner.Node, new ToArrayViaArrayPool<T>(arrayPool, cleanBuffers, info.ActualSize))
                : inner.Node.CreateObjectViaFastEnumerator<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize));
        }

        public static T[] ToArrayUseStack<T, Inner>(in this ValueEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => arrayPoolInfo.HasValue
                ? Nodes<T[]>.Aggregation<Inner, ToArrayViaStackMemoryPool<T>>(in inner.Node, new ToArrayViaStackMemoryPool<T>(maxStackItemCount, arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers))
                : Nodes<T[]>.Aggregation<Inner, ToArrayViaStackAndGarbage<T>>(in inner.Node, new ToArrayViaStackAndGarbage<T>(maxStackItemCount));


        public static List<T> ToList<T, Inner>(in this ValueEnumerable<T, Inner> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualLengthIsMaximumLength || info.MaximumLength <= 4)
            {
                if (info.MaximumLength == 0)
                    return new List<T>();

                return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListForward<T>>(new ToListForward<T>(info.ActualSize));
            }

            if (!arrayPoolInfo.HasValue)
            {
                if (info.MaximumLength <= maybeMaxCountForStackBasedPath.GetValueOrDefault())
                    return Nodes<List<T>>.Aggregation<Inner, ToListViaStackAndGarbage<T>>(in inner.Node, new ToListViaStackAndGarbage<T>(maybeMaxCountForStackBasedPath.Value));

                return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListForward<T>>(new ToListForward<T>(null));
            }

            return inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), 0, null));
        }

        public static List<T> ToListUsePool<T, Inner>(in this ValueEnumerable<T, Inner> inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            return viaPull
                ? Nodes<List<T>>.Aggregation<Inner, ToListViaArrayPool<T>>(in inner.Node, new ToListViaArrayPool<T>(arrayPool, cleanBuffers, info.ActualSize))
                : inner.Node.CreateObjectViaFastEnumerator<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize));
        }

        public static List<T> ToListUseStack<T, Inner>(in this ValueEnumerable<T, Inner> inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => arrayPoolInfo.HasValue
                ? Nodes<List<T>>.Aggregation<Inner, ToListViaStackMemoryPool<T>>(in inner.Node, new ToListViaStackMemoryPool<T>(maxStackItemCount, arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers))
                : Nodes<List<T>>.Aggregation<Inner, ToListViaStackAndGarbage<T>>(in inner.Node, new ToListViaStackAndGarbage<T>(maxStackItemCount));

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> =>
            (inner.Node.CheckForOptimization<Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, Last<T>>(new Last<T>())
            };

        public static T Last<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, LastPredicate<T>>(new LastPredicate<T>(predicate));
        }

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> =>
            (inner.Node.CheckForOptimization<Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => default,
                _ => inner.Node.CreateObjectViaFastEnumerator<T, LastOrDefault<T>>(new LastOrDefault<T>())
            };

        public static T LastOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, LastOrDefaultPredicate<T>>(new LastOrDefaultPredicate<T>(predicate));
        }

        public static T First<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> => inner.Node.CreateObjectViaFastEnumerator<T, First<T>>(new First<T>());

        public static T First<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, FirstPredicate<T>>(new FirstPredicate<T>(predicate));
        }

        public static T FirstOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> => inner.Node.CreateObjectViaFastEnumerator<T, FirstOrDefault<T>>(new FirstOrDefault<T>());

        public static T FirstOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, FirstOrDefaultPredicate<T>>(new FirstOrDefaultPredicate<T>(predicate));
        }

        public static T Single<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> => inner.Node.CreateObjectViaFastEnumerator<T, Single<T>>(new Single<T>());

        public static T Single<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, SinglePredicate<T>>(new SinglePredicate<T>(predicate));
        }

        public static T SingleOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner)
            where Inner : INode<T> => inner.Node.CreateObjectViaFastEnumerator<T, SingleOrDefault<T>>(new SingleOrDefault<T>());

        public static T SingleOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.Node.CreateObjectViaFastEnumerator<T, SingleOrDefaultPredicate<T>>(new SingleOrDefaultPredicate<T>(predicate));
        }


        public static T ElementAt<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
        {
            inner.Node.GetCountInformation(out var countInfo);
            return inner.Node.CreateObjectViaFastEnumerator<T, ElementAt<T>>(new ElementAt<T>(index, countInfo.ActualSize));
        }

        public static T ElementAtOrDefault<T, Inner>(in this ValueEnumerable<T, Inner> inner, int index)
            where Inner : INode<T>
        {
            inner.Node.GetCountInformation(out var countInfo);
            return inner.Node.CreateObjectViaFastEnumerator<T, ElementAtOrDefault<T>>(new ElementAtOrDefault<T>(index, countInfo.ActualSize));
        }

        public static decimal Average<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => inner.Node.CreateObjectViaFastEnumerator<decimal, AverageDecimal>(new AverageDecimal(true));
        public static double  Average<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>  => inner.Node.CreateObjectViaFastEnumerator<double,  AverageDouble >(new AverageDouble(true));
        public static float   Average<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>   => inner.Node.CreateObjectViaFastEnumerator<float,   AverageFloat  >(new AverageFloat(true));
        public static double  Average<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>     => inner.Node.CreateObjectViaFastEnumerator<double,  AverageInt    >(new AverageInt(true));
        public static double  Average<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>    => inner.Node.CreateObjectViaFastEnumerator<double,  AverageLong   >(new AverageLong(true));

        public static decimal Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal, SelectFoward<T, decimal, AverageDecimal>>(new SelectFoward<T, decimal, AverageDecimal>(new AverageDecimal(default), selector));
        public static double Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, double, AverageDouble>>(new SelectFoward<T, double, AverageDouble>(new AverageDouble(default), selector));
        public static float Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float, SelectFoward<T, float, AverageFloat>>(new SelectFoward<T, float, AverageFloat>(new AverageFloat(default), selector));
        public static double Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, int, AverageInt>>(new SelectFoward<T, int, AverageInt>(new AverageInt(default), selector));
        public static double Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, long, AverageLong>>(new SelectFoward<T, long, AverageLong>(new AverageLong(default), selector));

        public static decimal? Average<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double?,  AverageDoubleNullable >(new AverageDoubleNullable(true));
        public static float?   Average<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float?,   AverageFloatNullable  >(new AverageFloatNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<double?,     AverageIntNullable    >(new AverageIntNullable(true));
        public static double?  Average<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<double?,    AverageLongNullable   >(new AverageLongNullable(true));

        public static decimal? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal?, SelectFoward<T, decimal?, AverageDecimalNullable>>(new SelectFoward<T, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(default), selector));
        public static double? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, double?, AverageDoubleNullable>>(new SelectFoward<T, double?, AverageDoubleNullable>(new AverageDoubleNullable(default), selector));
        public static float? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float?, SelectFoward<T, float?, AverageFloatNullable>>(new SelectFoward<T, float?, AverageFloatNullable>(new AverageFloatNullable(default), selector));
        public static double? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, int?, AverageIntNullable>>(new SelectFoward<T, int?, AverageIntNullable>(new AverageIntNullable(default), selector));
        public static double? Average<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, long?, AverageLongNullable>>(new SelectFoward<T, long?, AverageLongNullable>(new AverageLongNullable(default), selector));

        public static decimal Min<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => inner.Node.CreateObjectViaFastEnumerator<decimal, MinDecimal>(new MinDecimal(true));
        public static double  Min<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => inner.Node.CreateObjectViaFastEnumerator<double,  MinDouble >(new MinDouble(true));
        public static float   Min<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => inner.Node.CreateObjectViaFastEnumerator<float,   MinFloat  >(new MinFloat(true));
        public static int     Min<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => inner.Node.CreateObjectViaFastEnumerator<int,     MinInt    >(new MinInt(true));
        public static long    Min<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => inner.Node.CreateObjectViaFastEnumerator<long,    MinLong   >(new MinLong(true));
        public static T       Min<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode<T>        => inner.Node.CreateObjectViaFastEnumerator<T,       Min<T>    >(new Min<T>(true));

        public static decimal Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal, SelectFoward<T, decimal, MinDecimal>>(new SelectFoward<T, decimal, MinDecimal>(new MinDecimal(default), selector));
        public static double Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, double, MinDouble>>(new SelectFoward<T, double, MinDouble>(new MinDouble(default), selector));
        public static float Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float, SelectFoward<T, float, MinFloat>>(new SelectFoward<T, float, MinFloat>(new MinFloat(default), selector));
        public static int Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int, SelectFoward<T, int, MinInt>>(new SelectFoward<T, int, MinInt>(new MinInt(default), selector));
        public static long Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long, SelectFoward<T, long, MinLong>>(new SelectFoward<T, long, MinLong>(new MinLong(default), selector));
        public static U Min<T, U, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, U> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<U, SelectFoward<T, U, Min<U>>>(new SelectFoward<T, U, Min<U>>(new Min<U>(default), selector));

        public static decimal? Min<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?>  => inner.Node.CreateObjectViaFastEnumerator<decimal?, MinDecimalNullable>(new MinDecimalNullable(true));
        public static double?  Min<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>   => inner.Node.CreateObjectViaFastEnumerator<double?,  MinDoubleNullable >(new MinDoubleNullable(true));
        public static float?   Min<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>    => inner.Node.CreateObjectViaFastEnumerator<float?,   MinFloatNullable  >(new MinFloatNullable(true));
        public static int?     Min<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>      => inner.Node.CreateObjectViaFastEnumerator<int?,     MinIntNullable    >(new MinIntNullable(true));
        public static long?    Min<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>     => inner.Node.CreateObjectViaFastEnumerator<long?,    MinLongNullable   >(new MinLongNullable(true));

        public static decimal? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal?, SelectFoward<T, decimal?, MinDecimalNullable>>(new SelectFoward<T, decimal?, MinDecimalNullable>(new MinDecimalNullable(default), selector));
        public static double? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, double?, MinDoubleNullable>>(new SelectFoward<T, double?, MinDoubleNullable>(new MinDoubleNullable(default), selector));
        public static float? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float?, SelectFoward<T, float?, MinFloatNullable>>(new SelectFoward<T, float?, MinFloatNullable>(new MinFloatNullable(default), selector));
        public static int? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int?, SelectFoward<T, int?, MinIntNullable>>(new SelectFoward<T, int?, MinIntNullable>(new MinIntNullable(default), selector));
        public static long? Min<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long?, SelectFoward<T, long?, MinLongNullable>>(new SelectFoward<T, long?, MinLongNullable>(new MinLongNullable(default), selector));

        public static decimal Max<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal>  => inner.Node.CreateObjectViaFastEnumerator<decimal, MaxDecimal>(new MaxDecimal(true));
        public static double  Max<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>   => inner.Node.CreateObjectViaFastEnumerator<double,  MaxDouble >(new MaxDouble(true));
        public static float   Max<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>    => inner.Node.CreateObjectViaFastEnumerator<float,   MaxFloat  >(new MaxFloat(true));
        public static int     Max<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>      => inner.Node.CreateObjectViaFastEnumerator<int,     MaxInt    >(new MaxInt(true));
        public static long    Max<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>     => inner.Node.CreateObjectViaFastEnumerator<long,    MaxLong   >(new MaxLong(true));
        public static T       Max<T, Inner>(in this ValueEnumerable<T,    Inner> inner) where Inner : INode<T>        => inner.Node.CreateObjectViaFastEnumerator<T,       Max<T>    >(new Max<T>(true));

        public static decimal Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal, SelectFoward<T, decimal, MaxDecimal>>(new SelectFoward<T, decimal, MaxDecimal>(new MaxDecimal(default), selector));
        public static double Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, double, MaxDouble>>(new SelectFoward<T, double, MaxDouble>(new MaxDouble(default), selector));
        public static float Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float, SelectFoward<T, float, MaxFloat>>(new SelectFoward<T, float, MaxFloat>(new MaxFloat(default), selector));
        public static int Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int, SelectFoward<T, int, MaxInt>>(new SelectFoward<T, int, MaxInt>(new MaxInt(default), selector));
        public static long Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long, SelectFoward<T, long, MaxLong>>(new SelectFoward<T, long, MaxLong>(new MaxLong(default), selector));
        public static U Max<T, U, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, U> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<U, SelectFoward<T, U, Max<U>>>(new SelectFoward<T, U, Max<U>>(new Max<U>(default), selector));

        public static decimal? Max<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true));
        public static double?  Max<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double?,  MaxDoubleNullable >(new MaxDoubleNullable(true));
        public static float?   Max<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float?,   MaxFloatNullable  >(new MaxFloatNullable(true));
        public static int?     Max<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<int?,     MaxIntNullable    >(new MaxIntNullable(true));
        public static long?    Max<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<long?,    MaxLongNullable   >(new MaxLongNullable(true));

        public static decimal? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal?, SelectFoward<T, decimal?, MaxDecimalNullable>>(new SelectFoward<T, decimal?, MaxDecimalNullable>(new MaxDecimalNullable(default), selector));
        public static double? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, double?, MaxDoubleNullable>>(new SelectFoward<T, double?, MaxDoubleNullable>(new MaxDoubleNullable(default), selector));
        public static float? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float?, SelectFoward<T, float?, MaxFloatNullable>>(new SelectFoward<T, float?, MaxFloatNullable>(new MaxFloatNullable(default), selector));
        public static int? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int?, SelectFoward<T, int?, MaxIntNullable>>(new SelectFoward<T, int?, MaxIntNullable>(new MaxIntNullable(default), selector));
        public static long? Max<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long?, SelectFoward<T, long?, MaxLongNullable>>(new SelectFoward<T, long?, MaxLongNullable>(new MaxLongNullable(default), selector));

        public static decimal Sum<Inner>(in this ValueEnumerable<decimal, Inner> inner) where Inner : INode<decimal> => inner.Node.CreateObjectViaFastEnumerator<decimal, SumDecimal>(new SumDecimal(true));
        public static double  Sum<Inner>(in this ValueEnumerable<double,  Inner> inner) where Inner : INode<double>  => inner.Node.CreateObjectViaFastEnumerator<double,  SumDouble >(new SumDouble(true));
        public static float   Sum<Inner>(in this ValueEnumerable<float,   Inner> inner) where Inner : INode<float>   => inner.Node.CreateObjectViaFastEnumerator<float,   SumFloat  >(new SumFloat(true));
        public static int     Sum<Inner>(in this ValueEnumerable<int,     Inner> inner) where Inner : INode<int>     => inner.Node.CreateObjectViaFastEnumerator<int,     SumInt    >(new SumInt(true));
        public static long    Sum<Inner>(in this ValueEnumerable<long,    Inner> inner) where Inner : INode<long>    => inner.Node.CreateObjectViaFastEnumerator<long,    SumLong   >(new SumLong(true));

        public static decimal Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal, SelectFoward<T, decimal, SumDecimal>>(new SelectFoward<T, decimal, SumDecimal>(new SumDecimal(default), selector));
        public static double Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double, SelectFoward<T, double, SumDouble>>(new SelectFoward<T, double, SumDouble>(new SumDouble(default), selector));
        public static float Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float, SelectFoward<T, float, SumFloat>>(new SelectFoward<T, float, SumFloat>(new SumFloat(default), selector));
        public static int Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int, SelectFoward<T, int, SumInt>>(new SelectFoward<T, int, SumInt>(new SumInt(default), selector));
        public static long Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long, SelectFoward<T, long, SumLong>>(new SelectFoward<T, long, SumLong>(new SumLong(default), selector));

        public static decimal? Sum<Inner>(in this ValueEnumerable<decimal?, Inner> inner) where Inner : INode<decimal?> => inner.Node.CreateObjectViaFastEnumerator<decimal, SumDecimalNullable>(new SumDecimalNullable(true));
        public static double?  Sum<Inner>(in this ValueEnumerable<double?,  Inner> inner) where Inner : INode<double?>  => inner.Node.CreateObjectViaFastEnumerator<double,  SumDoubleNullable >(new SumDoubleNullable(true));
        public static float?   Sum<Inner>(in this ValueEnumerable<float?,   Inner> inner) where Inner : INode<float?>   => inner.Node.CreateObjectViaFastEnumerator<float,   SumFloatNullable  >(new SumFloatNullable(true));
        public static int?     Sum<Inner>(in this ValueEnumerable<int?,     Inner> inner) where Inner : INode<int?>     => inner.Node.CreateObjectViaFastEnumerator<int,     SumIntNullable    >(new SumIntNullable(true));
        public static long?    Sum<Inner>(in this ValueEnumerable<long?,    Inner> inner) where Inner : INode<long?>    => inner.Node.CreateObjectViaFastEnumerator<long,    SumLongNullable   >(new SumLongNullable(true));

        public static decimal? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<decimal?, SelectFoward<T, decimal?, SumDecimalNullable>>(new SelectFoward<T, decimal?, SumDecimalNullable>(new SumDecimalNullable(default), selector));
        public static double? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<double?, SelectFoward<T, double?, SumDoubleNullable>>(new SelectFoward<T, double?, SumDoubleNullable>(new SumDoubleNullable(default), selector));
        public static float? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<float?, SelectFoward<T, float?, SumFloatNullable>>(new SelectFoward<T, float?, SumFloatNullable>(new SumFloatNullable(default), selector));
        public static int? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<int?, SelectFoward<T, int?, SumIntNullable>>(new SelectFoward<T, int?, SumIntNullable>(new SumIntNullable(default), selector));
        public static long? Sum<T, Inner>(in this ValueEnumerable<T, Inner> inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<long?, SelectFoward<T, long?, SumLongNullable>>(new SelectFoward<T, long?, SumLongNullable>(new SumLongNullable(default), selector));

        public static int Count<T, Inner>(in this ValueEnumerable<T, Inner> inner, bool ignorePotentialSideEffects = false) where Inner : INode<T> =>
            NodeImpl.Count<T, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static bool Count<T, Inner>(in this ValueEnumerable<T, Inner> source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.Node.CreateObjectViaFastEnumerator<bool, CountIf<T>>(new CountIf<T>(predicate));
        }

        public static bool Contains<T, Inner>(in this ValueEnumerable<T, Inner> inner, T value) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<bool, Contains<T>>(new Contains<T>(value));
        public static bool Contains<T, Inner>(in this ValueEnumerable<T, Inner> inner, T value, IEqualityComparer<T> comparer) where Inner : INode<T> =>
            inner.Node.CreateObjectViaFastEnumerator<bool, ContainsByComparer<T>>(new ContainsByComparer<T>(comparer, value));


        public static ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>> SelectMany<TSource, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<TResult, SelectManyNode<TSource, TResult, NodeT, NodeU>>(new SelectManyNode<TSource, TResult, NodeT, NodeU>(in prior.Node, selector));
        }

        public static ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU>(in this ValueEnumerable<TSource, NodeT> prior, Func<TSource, ValueEnumerable<TCollection, NodeU>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new ValueEnumerable<TResult, SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>>(new SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>(in prior.Node, collectionSelector, resultSelector));
        }

        public static ValueEnumerable<T, ConcatNode<T, First, Second>> Concat<T, First, Second>(in this ValueEnumerable<T, First> first, in ValueEnumerable<T, Second> second)
            where First : INode<T>
            where Second : INode<T>
            => new ValueEnumerable<T, ConcatNode<T, First, Second>>(new ConcatNode<T, First, Second>(first.Node, second.Node));

        public static ValueEnumerable<TSource, ReverseNode<TSource, Inner>> Reverse<TSource, Inner>(in this ValueEnumerable<TSource, Inner> source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new ValueEnumerable<TSource, ReverseNode<TSource, Inner>>(new ReverseNode<TSource, Inner>(in source.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo));

        // -- Value based operators

        public static ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>> Select<T, U, TPrior, IFunc>(in this ValueEnumerable<T, TPrior> prior, IFunc selector, U u = default)
            where TPrior : INode<T>
            where IFunc : IFuncBase<T, U>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueEnumerable<U, ValueSelectNode<T, U, TPrior, IFunc>>(new ValueSelectNode<T, U, TPrior, IFunc>(in prior.Node, selector));
        }
        public static ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>> Where<T, TPrior, Predicate>(in this ValueEnumerable<T, TPrior> prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFuncBase<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueEnumerable<T, ValueWhereNode<T, TPrior, Predicate>>(new ValueWhereNode<T, TPrior, Predicate>(in prior.Node, predicate));
        }
    }
}
