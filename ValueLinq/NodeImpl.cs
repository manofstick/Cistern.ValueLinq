using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;
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


namespace Cistern.ValueLinq
{
    internal static partial class NodeImpl
    {
        internal static int Count<T, Inner>(in Inner inner, bool ignorePotentialSideEffects) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            if (!countInfo.IsStale && (ignorePotentialSideEffects || !countInfo.PotentialSideEffects) && countInfo.ActualLengthIsMaximumLength && countInfo.MaximumLength.HasValue && countInfo.MaximumLength.Value <= int.MaxValue)
                return (int)countInfo.MaximumLength.Value;

            return inner.TryPushOptimization<Optimizations.Count, int>(new Optimizations.Count { IgnorePotentialSideEffects = ignorePotentialSideEffects }, out var count) switch
            {
                false => inner.CreateViaPush<int, Aggregation.Count<T>>(new Aggregation.Count<T>()),
                true => count
            };
        }

        private static T[] ToArrayWithoutOptimizationCheck<T, Inner>(in Inner inner, int maybeMaxCountForStackBasedPath, in (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo)
                where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualLengthIsMaximumLength)
            {
                if (info.MaximumLength == 0)
                    return Array.Empty<T>();

                return inner.CreateViaPush<T[], ToArrayForward<T>>(new ToArrayForward<T>(info.ActualSize.Value));
            }

            if (!arrayPoolInfo.HasValue)
            {
#if PREFER_PULL_IMPLEMENTATION
                if (info.MaximumLength <= maybeMaxCountForStackBasedPath.GetValueOrDefault())
                    return Nodes<T[]>.Aggregation<Inner, ToArrayViaStackAndGarbage<T>>(in inner, new ToArrayViaStackAndGarbage<T>(maybeMaxCountForStackBasedPath.Value));

                return inner.CreateObjectViaFastEnumerator<T[], ToArrayViaAllocatorForward<T, GarbageCollectedAllocator<T>>>(new ToArrayViaAllocatorForward<T, GarbageCollectedAllocator<T>>(default, 0, null));
#else
                return Nodes<T[]>.Aggregation<Inner, ToArrayViaStackAndGarbage<T>>(in inner, new ToArrayViaStackAndGarbage<T>(maybeMaxCountForStackBasedPath));
#endif
            }

            return inner.CreateViaPush<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), 0, info.ActualSize));
        }

        internal static T[] ToArray<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath, in (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo)
                where Inner : INode<T>
        {
            if (inner.TryPushOptimization<Optimizations.ToArray, T[]>(default, out var array))
                return array;

            return ToArrayWithoutOptimizationCheck(in inner, maybeMaxCountForStackBasedPath ?? 64, in arrayPoolInfo);
        }



        internal static void GetCountInformation<Node>(this Node node, out CountInformation ci) where Node : INode => node.GetCountInformation(out ci);

        internal static T Aggregate<T, Inner>(in Inner source, Func<T, T, T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ReduceForward<T>>(new ReduceForward<T>(func));
        }

        internal static void ForEach<T, Inner>(in Inner source, Action<T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            source.CreateViaPush<T, ForEachForward<T>>(new ForEachForward<T>(func));
        }

        internal static T ForEach<T, U, Inner>(in Inner source, T seed, RefAction<T, U> func)
            where Inner : INode<U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ForEachForwardRef<T, U>>(new ForEachForwardRef<T, U>(seed, func));
        }

        internal static T ForEach<T, U, Inner, RefAction>(in Inner source, T seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<T, U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ValueForeachForwardRef<T, U, RefAction>>(new ValueForeachForwardRef<T, U, RefAction>(seed, func));
        }


        internal static TAccumulate Aggregate<T, TAccumulate, Inner>(in Inner source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed));
        }

        internal static TResult Aggregate<T, TAccumulate, TResult, Inner>(in Inner source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return resultSelector(source.CreateViaPush<TAccumulate, FoldForward<T, TAccumulate>>(new FoldForward<T, TAccumulate>(func, seed)));
        }

        internal static bool All<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, All<T, FuncToIFunc<T, bool>>>(new All<T, FuncToIFunc<T, bool>>(new FuncToIFunc<T, bool>(predicate)));
        }

        internal static bool All<T, Inner, Predicate>(in Inner source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, All<T, Predicate>>(new All<T, Predicate>(predicate));
        }

        internal static bool Any<T, Inner>(in Inner source)
            where Inner : INode<T>
            => source.CreateViaPush<bool, Anything<T>>(new Anything<T>());

        internal static bool Any<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, Any<T>>(new Any<T>(predicate));
        }

        // -- 

        internal static SelectNode<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, Func<T, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new SelectNode<T, U, TPrior>(in prior, selector);
        }
        internal static Select_InNode<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new Select_InNode<T, U, TPrior>(in prior, f);

        internal static SelectIdxNode<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, Func<T, int, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new SelectIdxNode<T, U, TPrior>(in prior, selector);
        }
        internal static WhereNode<T, TPrior> Where<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new WhereNode<T, TPrior>(in prior, predicate);
        }
        internal static WhereIdxNode<T, TPrior> Where<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new WhereIdxNode<T, TPrior>(in prior, predicate);
        }
        internal static Where_InNode<T, TPrior> Where<T, TPrior>(in TPrior prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new Where_InNode<T, TPrior>(in prior, f);

        internal static SkipNode<T, TPrior> Skip<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
            => new SkipNode<T, TPrior>(in prior, count);

        internal static SkipWhileNode<T, TPrior> SkipWhile<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new SkipWhileNode<T, TPrior>(in prior, predicate);
        }
        internal static SkipWhileIdxNode<T, TPrior> SkipWhile<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new SkipWhileIdxNode<T, TPrior>(in prior, predicate);
        }

        internal static TakeNode<T, TPrior> Take<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
                => new TakeNode<T, TPrior>(in prior, count);

        internal static TakeWhileNode<T, TPrior> TakeWhile<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new TakeWhileNode<T, TPrior>(in prior, predicate);
        }
        internal static TakeWhileIdxNode<T, TPrior> TakeWhile<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new TakeWhileIdxNode<T, TPrior>(in prior, predicate);
        }

        //internal static T[] ToArray<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
        //    where Inner : INode<T>
        //    => NodeImpl.ToArray(in inner, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        internal static T[] ToArrayUsePool<T, Inner>(in Inner inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            return viaPull
                ? Nodes<T[]>.Aggregation<Inner, ToArrayViaArrayPool<T>>(in inner, new ToArrayViaArrayPool<T>(arrayPool, cleanBuffers, info.ActualSize))
                : inner.CreateViaPush<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize));
        }

        internal static T[] ToArrayUseStack<T, Inner>(in Inner inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => arrayPoolInfo.HasValue
                ? Nodes<T[]>.Aggregation<Inner, ToArrayViaStackMemoryPool<T>>(in inner, new ToArrayViaStackMemoryPool<T>(maxStackItemCount, arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers))
                : Nodes<T[]>.Aggregation<Inner, ToArrayViaStackAndGarbage<T>>(in inner, new ToArrayViaStackAndGarbage<T>(maxStackItemCount));


        internal static List<T> ToList<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualLengthIsMaximumLength || info.MaximumLength <= 4)
            {
                if (info.MaximumLength == 0)
                    return new List<T>();

                return inner.CreateViaPush<List<T>, ToListForward<T>>(new ToListForward<T>(info.ActualSize));
            }

            if (!arrayPoolInfo.HasValue)
            {
                if (info.MaximumLength <= maybeMaxCountForStackBasedPath.GetValueOrDefault())
                    return Nodes<List<T>>.Aggregation<Inner, ToListViaStackAndGarbage<T>>(in inner, new ToListViaStackAndGarbage<T>(maybeMaxCountForStackBasedPath.Value));

                return inner.CreateViaPush<List<T>, ToListForward<T>>(new ToListForward<T>(null));
            }

            return inner.CreateViaPush<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), 0, null));
        }

        internal static List<T> ToListUsePool<T, Inner>(in Inner inner, ArrayPool<T> maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            return viaPull
                ? Nodes<List<T>>.Aggregation<Inner, ToListViaArrayPool<T>>(in inner, new ToListViaArrayPool<T>(arrayPool, cleanBuffers, info.ActualSize))
                : inner.CreateViaPush<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize));
        }

        internal static List<T> ToListUseStack<T, Inner>(in Inner inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            => arrayPoolInfo.HasValue
                ? Nodes<List<T>>.Aggregation<Inner, ToListViaStackMemoryPool<T>>(in inner, new ToListViaStackMemoryPool<T>(maxStackItemCount, arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers))
                : Nodes<List<T>>.Aggregation<Inner, ToListViaStackAndGarbage<T>>(in inner, new ToListViaStackAndGarbage<T>(maxStackItemCount));

        internal static T Last<T, Inner>(in Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                _ => inner.CreateViaPush<T, Last<T>>(new Last<T>())
            };

        internal static T Last<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, LastPredicate<T>>(new LastPredicate<T>(predicate));
        }

        internal static T LastOrDefault<T, Inner>(in Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.TryLast, (bool, T)>(new Optimizations.TryLast(), out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => default,
                _ => inner.CreateViaPush<T, LastOrDefault<T>>(new LastOrDefault<T>())
            };

        internal static T LastOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, LastOrDefaultPredicate<T>>(new LastOrDefaultPredicate<T>(predicate));
        }

        internal static T First<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T, First<T>>(new First<T>());

        internal static T First<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, FirstPredicate<T>>(new FirstPredicate<T>(predicate));
        }

        internal static T FirstOrDefault<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T, FirstOrDefault<T>>(new FirstOrDefault<T>());

        internal static T FirstOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, FirstOrDefaultPredicate<T>>(new FirstOrDefaultPredicate<T>(predicate));
        }

        internal static T Single<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T, Single<T>>(new Single<T>());

        internal static T Single<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, SinglePredicate<T>>(new SinglePredicate<T>(predicate));
        }

        internal static T SingleOrDefault<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T, SingleOrDefault<T>>(new SingleOrDefault<T>());

        internal static T SingleOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, SingleOrDefaultPredicate<T>>(new SingleOrDefaultPredicate<T>(predicate));
        }


        internal static T ElementAt<T, Inner>(in Inner inner, int index)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            return inner.CreateViaPush<T, ElementAt<T>>(new ElementAt<T>(index, countInfo.ActualSize));
        }

        internal static T ElementAtOrDefault<T, Inner>(in Inner inner, int index)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            return inner.CreateViaPush<T, ElementAtOrDefault<T>>(new ElementAtOrDefault<T>(index, countInfo.ActualSize));
        }

        internal static decimal AverageDecimal<Inner>(in Inner inner) where Inner : INode<decimal> => inner.CreateViaPush<decimal, AverageDecimal>(new AverageDecimal(true));
        internal static double  AverageDouble<Inner>(in Inner inner) where Inner : INode<double>  => inner.CreateViaPush<double,  AverageDouble >(new AverageDouble(true));
        internal static float   AverageFloat<Inner>(in Inner inner) where Inner : INode<float>   => inner.CreateViaPush<float,   AverageFloat  >(new AverageFloat(true));
        internal static double  AverageInt<Inner>(in Inner inner) where Inner : INode<int>     => inner.CreateViaPush<double,  AverageInt    >(new AverageInt(true));
        internal static double  AverageLong<Inner>(in Inner inner) where Inner : INode<long>    => inner.CreateViaPush<double,  AverageLong   >(new AverageLong(true));

        internal static decimal Average<T, Inner>(in Inner inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.CreateViaPush<decimal, SelectFoward<T, decimal, AverageDecimal>>(new SelectFoward<T, decimal, AverageDecimal>(new AverageDecimal(default), selector));
        internal static double Average<T, Inner>(in Inner inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, SelectFoward<T, double, AverageDouble>>(new SelectFoward<T, double, AverageDouble>(new AverageDouble(default), selector));
        internal static float Average<T, Inner>(in Inner inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.CreateViaPush<float, SelectFoward<T, float, AverageFloat>>(new SelectFoward<T, float, AverageFloat>(new AverageFloat(default), selector));
        internal static double Average<T, Inner>(in Inner inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, SelectFoward<T, int, AverageInt>>(new SelectFoward<T, int, AverageInt>(new AverageInt(default), selector));
        internal static double Average<T, Inner>(in Inner inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, SelectFoward<T, long, AverageLong>>(new SelectFoward<T, long, AverageLong>(new AverageLong(default), selector));

        internal static decimal? AverageNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal?, AverageDecimalNullable>(new AverageDecimalNullable(true));
        internal static double?  AverageNullableDouble <Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double?,  AverageDoubleNullable >(new AverageDoubleNullable(true));
        internal static float?   AverageNullableFloat  <Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float?,   AverageFloatNullable  >(new AverageFloatNullable(true));
        internal static double?  AverageNullableInt    <Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<double?,     AverageIntNullable    >(new AverageIntNullable(true));
        internal static double?  AverageNullableLong   <Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<double?,    AverageLongNullable   >(new AverageLongNullable(true));

        internal static decimal? Average<T, Inner>(in Inner inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<decimal?, SelectFoward<T, decimal?, AverageDecimalNullable>>(new SelectFoward<T, decimal?, AverageDecimalNullable>(new AverageDecimalNullable(default), selector));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, SelectFoward<T, double?, AverageDoubleNullable>>(new SelectFoward<T, double?, AverageDoubleNullable>(new AverageDoubleNullable(default), selector));
        internal static float? Average<T, Inner>(in Inner inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<float?, SelectFoward<T, float?, AverageFloatNullable>>(new SelectFoward<T, float?, AverageFloatNullable>(new AverageFloatNullable(default), selector));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, SelectFoward<T, int?, AverageIntNullable>>(new SelectFoward<T, int?, AverageIntNullable>(new AverageIntNullable(default), selector));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, SelectFoward<T, long?, AverageLongNullable>>(new SelectFoward<T, long?, AverageLongNullable>(new AverageLongNullable(default), selector));

        internal static decimal MinDecimal<Inner>(in Inner inner) where Inner : INode<decimal>  => inner.CreateViaPush<decimal, MinDecimal>(new MinDecimal(true));
        internal static double  MinDouble<Inner>(in Inner inner) where Inner : INode<double>   => inner.CreateViaPush<double,  MinDouble >(new MinDouble(true));
        internal static float   MinFloat<Inner>(in Inner inner) where Inner : INode<float>    => inner.CreateViaPush<float,   MinFloat  >(new MinFloat(true));
        internal static int     MinInt<Inner>(in Inner inner) where Inner : INode<int>      => inner.CreateViaPush<int,     MinInt    >(new MinInt(true));
        internal static long    MinLong<Inner>(in Inner inner) where Inner : INode<long>     => inner.CreateViaPush<long,    MinLong   >(new MinLong(true));
        internal static T       Min<T, Inner>(in Inner inner) where Inner : INode<T>        => inner.CreateViaPush<T,       Min<T>    >(new Min<T>(true));

        internal static decimal? MinNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?>  => inner.CreateViaPush<decimal?, MinDecimalNullable>(new MinDecimalNullable(true));
        internal static double?  MinNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>   => inner.CreateViaPush<double?,  MinDoubleNullable >(new MinDoubleNullable(true));
        internal static float?   MinNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>    => inner.CreateViaPush<float?,   MinFloatNullable  >(new MinFloatNullable(true));
        internal static int?     MinNullableInt<Inner>(in Inner inner) where Inner : INode<int?>      => inner.CreateViaPush<int?,     MinIntNullable    >(new MinIntNullable(true));
        internal static long?    MinNullableLong<Inner>(in Inner inner) where Inner : INode<long?>     => inner.CreateViaPush<long?,    MinLongNullable   >(new MinLongNullable(true));

        internal static decimal MaxDecimal<Inner>(in Inner inner) where Inner : INode<decimal>  => inner.CreateViaPush<decimal, MaxDecimal>(new MaxDecimal(true));
        internal static double  MaxDouble<Inner>(in Inner inner) where Inner : INode<double>   => inner.CreateViaPush<double,  MaxDouble >(new MaxDouble(true));
        internal static float   MaxFloat<Inner>(in Inner inner) where Inner : INode<float>    => inner.CreateViaPush<float,   MaxFloat  >(new MaxFloat(true));
        internal static int     MaxInt<Inner>(in Inner inner) where Inner : INode<int>      => inner.CreateViaPush<int,     MaxInt    >(new MaxInt(true));
        internal static long    MaxLong<Inner>(in Inner inner) where Inner : INode<long>     => inner.CreateViaPush<long,    MaxLong   >(new MaxLong(true));
        internal static T       Max<T, Inner>(in Inner inner) where Inner : INode<T>        => inner.CreateViaPush<T,       Max<T>    >(new Max<T>(true));

        internal static decimal? MaxNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal?, MaxDecimalNullable>(new MaxDecimalNullable(true));
        internal static double?  MaxNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double?,  MaxDoubleNullable >(new MaxDoubleNullable(true));
        internal static float?   MaxNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float?,   MaxFloatNullable  >(new MaxFloatNullable(true));
        internal static int?     MaxNullableInt<Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<int?,     MaxIntNullable    >(new MaxIntNullable(true));
        internal static long?    MaxNullableLong<Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<long?,    MaxLongNullable   >(new MaxLongNullable(true));

        internal static decimal SumDecimal<Inner>(in Inner inner) where Inner : INode<decimal> => inner.CreateViaPush<decimal, SumDecimal>(new SumDecimal(true));
        internal static double  SumDouble<Inner>(in Inner inner) where Inner : INode<double>  => inner.CreateViaPush<double,  SumDouble >(new SumDouble(true));
        internal static float   SumFloat<Inner>(in Inner inner) where Inner : INode<float>   => inner.CreateViaPush<float,   SumFloat  >(new SumFloat(true));
        internal static int     SumInt<Inner>(in Inner inner) where Inner : INode<int>     => inner.CreateViaPush<int,     SumInt    >(new SumInt(true));
        internal static long    SumLong<Inner>(in Inner inner) where Inner : INode<long>    => inner.CreateViaPush<long,    SumLong   >(new SumLong(true));

        internal static decimal? SumNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal, SumDecimalNullable>(new SumDecimalNullable(true));
        internal static double?  SumNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double,  SumDoubleNullable >(new SumDoubleNullable(true));
        internal static float?   SumNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float,   SumFloatNullable  >(new SumFloatNullable(true));
        internal static int?     SumNullableInt<Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<int,     SumIntNullable    >(new SumIntNullable(true));
        internal static long?    SumNullableLong<Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<long,    SumLongNullable   >(new SumLongNullable(true));

        internal static bool Count<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, CountIf<T>>(new CountIf<T>(predicate));
        }

        internal static bool Contains<T, Inner>(in Inner inner, T value) where Inner : INode<T> =>
            inner.CreateViaPush<bool, Contains<T>>(new Contains<T>(value));
        internal static bool Contains<T, Inner>(in Inner inner, T value, IEqualityComparer<T> comparer) where Inner : INode<T> =>
            inner.CreateViaPush<bool, ContainsByComparer<T>>(new ContainsByComparer<T>(comparer, value));


        internal static SelectManyNode<TSource, TResult, NodeT, NodeU> SelectMany<TSource, TResult, NodeT, NodeU>(in NodeT prior, Func<TSource, ValueEnumerable<TResult, NodeU>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new SelectManyNode<TSource, TResult, NodeT, NodeU>(in prior, selector);
        }

        internal static SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU> SelectMany<TSource, TCollection, TResult, NodeT, NodeU>(in NodeT prior, Func<TSource, ValueEnumerable<TCollection, NodeU>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new SelectManyNode<TSource, TCollection, TResult, NodeT, NodeU>(in prior, collectionSelector, resultSelector);
        }

        internal static ConcatNode<T, First, Second> Concat<T, First, Second>(in First first, in Second second)
            where First : INode<T>
            where Second : INode<T>
            => new ConcatNode<T, First, Second>(in first, in second);

        internal static ReverseNode<TSource, Inner> Reverse<TSource, Inner>(in Inner source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new ReverseNode<TSource, Inner>(in source, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        // -- Value based operators

        internal static ValueSelectNode<T, U, TPrior, IFunc> Select<T, U, TPrior, IFunc>(in TPrior prior, IFunc selector, U u = default)
            where TPrior : INode<T>
            where IFunc : IFuncBase<T, U>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ValueSelectNode<T, U, TPrior, IFunc>(in prior, selector);
        }
        internal static ValueWhereNode<T, TPrior, Predicate> Where<T, TPrior, Predicate>(in TPrior prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFuncBase<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new ValueWhereNode<T, TPrior, Predicate>(in prior, predicate);
        }

        public static OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, TPrior> OrderBy<TSource, TKey, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            where TPrior : INode<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return new OrderByNode<TSource, KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>, TPrior>(in prior, new KeySelectors<TSource, TKey, KeySelectorsRoot<TSource>>(new KeySelectorsRoot<TSource>(), keySelector, comparer ?? Comparer<TKey>.Default, descending));
        }

        public static OrderByNode<TSource, KeySelectors<TSource, TKey, TPriorSelector>, TPrior> ThenBy<TSource, TKey, TPriorSelector, TPrior>(in OrderByNode<TSource, TPriorSelector, TPrior> prior, Func<TSource, TKey> keySelector, IComparer<TKey> comparer, bool descending)
            where TPrior : INode<TSource>
            where TPriorSelector: IKeySelectors<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return new OrderByNode<TSource, KeySelectors<TSource, TKey, TPriorSelector>, TPrior>(in prior._nodeT, new KeySelectors<TSource, TKey, TPriorSelector>(prior._keySelectors, keySelector, comparer ?? Comparer<TKey>.Default, descending));
        }

    }
}
