using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.Utils;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;

using AverageDecimal = Cistern.ValueLinq.Aggregation.Average<decimal, decimal, decimal>;
using AverageDouble  = Cistern.ValueLinq.Aggregation.Average<double, double, double>;
using AverageFloat   = Cistern.ValueLinq.Aggregation.Average<float, double, float>;
using AverageInt     = Cistern.ValueLinq.Aggregation.Average<int, long, double>;
using AverageLong    = Cistern.ValueLinq.Aggregation.Average<long, long, double>;

using AverageDecimalNullable = Cistern.ValueLinq.Aggregation.AverageNullable<decimal, decimal, decimal>;
using AverageDoubleNullable  = Cistern.ValueLinq.Aggregation.AverageNullable<double, double, double>;
using AverageFloatNullable   = Cistern.ValueLinq.Aggregation.AverageNullable<float, double, float>;
using AverageIntNullable     = Cistern.ValueLinq.Aggregation.AverageNullable<int, long, double>;
using AverageLongNullable    = Cistern.ValueLinq.Aggregation.AverageNullable<long, long, double>;

using MinDecimal = Cistern.ValueLinq.Aggregation.MinNumber<decimal>;
using MinDouble  = Cistern.ValueLinq.Aggregation.MinNumber<double>;
using MinFloat   = Cistern.ValueLinq.Aggregation.MinNumber<float>;
using MinInt     = Cistern.ValueLinq.Aggregation.MinNumber<int>;
using MinLong    = Cistern.ValueLinq.Aggregation.MinNumber<long>;

using MinDecimalNullable = Cistern.ValueLinq.Aggregation.MinNumberNullable<decimal>;
using MinDoubleNullable  = Cistern.ValueLinq.Aggregation.MinNumberNullable<double>;
using MinFloatNullable   = Cistern.ValueLinq.Aggregation.MinNumberNullable<float>;
using MinIntNullable     = Cistern.ValueLinq.Aggregation.MinNumberNullable<int>;
using MinLongNullable    = Cistern.ValueLinq.Aggregation.MinNumberNullable<long>;

using MaxDecimal = Cistern.ValueLinq.Aggregation.MaxNumber<decimal>;
using MaxDouble  = Cistern.ValueLinq.Aggregation.MaxNumber<double>;
using MaxFloat   = Cistern.ValueLinq.Aggregation.MaxNumber<float>;
using MaxInt     = Cistern.ValueLinq.Aggregation.MaxNumber<int>;
using MaxLong    = Cistern.ValueLinq.Aggregation.MaxNumber<long>;

using MaxDecimalNullable = Cistern.ValueLinq.Aggregation.MaxNumberNullable<decimal>;
using MaxDoubleNullable  = Cistern.ValueLinq.Aggregation.MaxNumberNullable<double>;
using MaxFloatNullable   = Cistern.ValueLinq.Aggregation.MaxNumberNullable<float>;
using MaxIntNullable     = Cistern.ValueLinq.Aggregation.MaxNumberNullable<int>;
using MaxLongNullable    = Cistern.ValueLinq.Aggregation.MaxNumberNullable<long>;

using SumDecimal = Cistern.ValueLinq.Aggregation.Sum<decimal, decimal>;
using SumDouble  = Cistern.ValueLinq.Aggregation.Sum<double, double>;
using SumFloat   = Cistern.ValueLinq.Aggregation.Sum<float, double>;
using SumInt     = Cistern.ValueLinq.Aggregation.Sum<int, int>;
using SumLong    = Cistern.ValueLinq.Aggregation.Sum<long, long>;

using SumDecimalNullable = Cistern.ValueLinq.Aggregation.SumNullable<decimal, decimal>;
using SumDoubleNullable  = Cistern.ValueLinq.Aggregation.SumNullable<double, double>;
using SumFloatNullable   = Cistern.ValueLinq.Aggregation.SumNullable<float, double>;
using SumIntNullable     = Cistern.ValueLinq.Aggregation.SumNullable<int, int>;
using SumLongNullable    = Cistern.ValueLinq.Aggregation.SumNullable<long, long>;
using Cistern.ValueLinq.Maths;

namespace Cistern.ValueLinq
{
    internal static partial class NodeImpl
    {
        internal static int Count<T, Inner>(in Inner inner, bool ignorePotentialSideEffects) where Inner : INode<T>
            => TryCountWithoutPush<T, Inner>(in inner, ignorePotentialSideEffects, out var count)
                ? count
                : inner.CreateViaPush<int, Aggregation.Count<T>, Construct<Aggregation.Count<T>, Aggregation.Count<T>.Args>>(default(ChainOptions), new Construct<Aggregation.Count<T>, Aggregation.Count<T>.Args>(default));

        // the count from the node's known length, or from the node answering the Count request; false when the count
        // has to be pushed through it
        internal static bool TryCountWithoutPush<T, Inner>(in Inner inner, bool ignorePotentialSideEffects, out int count) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            if (!countInfo.IsStale && (ignorePotentialSideEffects || !countInfo.PotentialSideEffects) && countInfo.ActualLengthIsMaximumLength && countInfo.MaximumLength is long length && length <= int.MaxValue)
            {
                count = (int)length;
                return true;
            }

            return inner.TryPushOptimization<Optimizations.Count, int>(new Optimizations.Count { IgnorePotentialSideEffects = ignorePotentialSideEffects }, out count);
        }

        internal static long LongCount<T, Inner>(in Inner inner, bool ignorePotentialSideEffects) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);

            if (!countInfo.IsStale
                && (ignorePotentialSideEffects || !countInfo.PotentialSideEffects)
                && countInfo.MaximumLength.HasValue)
            {
                if (countInfo.ActualLengthIsMaximumLength)
                    return countInfo.MaximumLength.Value;

                // the int-valued Count request is asked only for a length bounded to an int, as a Concat answers it in checked ints
                if (countInfo.MaximumLength.Value <= int.MaxValue
                    && inner.TryPushOptimization<Optimizations.Count, int>(new Optimizations.Count { IgnorePotentialSideEffects = ignorePotentialSideEffects }, out var count))
                {
                    return count;
                }
            }

            // POTENTIAL OPTIMIZATION: new request LongCount: the Count request with a long result, answered by the
            // nodes that answer Count (a Concat or SelectMany adding its parts, a collection its Count), so a chain of
            // unbounded length is counted without a push
            return inner.CreateViaPush<long, Aggregation.LongCount<T>, Construct<Aggregation.LongCount<T>, Aggregation.LongCount<T>.Args>>(default(ChainOptions), new Construct<Aggregation.LongCount<T>, Aggregation.LongCount<T>.Args>(default));
        }

        internal static long LongCount<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<long, LongCountIf<T>, Construct<LongCountIf<T>, LongCountIf<T>.Args>>(default(ChainOptions), new Construct<LongCountIf<T>, LongCountIf<T>.Args>(new LongCountIf<T>.Args(predicate)));
        }

        // the Count request is not asked here, as a node may answer it by enumerating
        internal static bool TryGetNonEnumeratedCount<T, Inner>(in Inner inner, out int count) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);

            if (!countInfo.IsStale
             && (!countInfo.PotentialSideEffects || countInfo.LengthIsImmutable)
             && countInfo.ActualLengthIsMaximumLength
             && countInfo.MaximumLength is long length
             && length <= int.MaxValue)
            {
                count = (int)length;
                return true;
            }

            // a collection's count read when asked, so a Concat of List<T>s, whose cached information is stale, is counted
            return inner.TryPushOptimization<Optimizations.Count, int>(new Optimizations.Count { DryRun = true }, out count);
        }

        private static T[] ToArrayWithoutOptimizationCheck<T, Inner>(ref Inner inner, int maybeMaxCountForStackBasedPath, ArrayPool<T>? arrayPool, bool cleanBuffers)
                where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualSize is int size)
            {
                if (size == 0)
                    return Array.Empty<T>();

                return inner.CreateViaPush<T[], ToArrayForward<T>, Construct<ToArrayForward<T>, ToArrayForward<T>.Args>>(default(ChainOptions), new Construct<ToArrayForward<T>, ToArrayForward<T>.Args>(new ToArrayForward<T>.Args(size)));
            }

            if (arrayPool is null)
                return inner.CreateViaPush<T[], ToArrayViaBuilderForward<T>, Construct<ToArrayViaBuilderForward<T>, ToArrayViaBuilderForward<T>.Args>>(default(ChainOptions), new Construct<ToArrayViaBuilderForward<T>, ToArrayViaBuilderForward<T>.Args>(default));

            return inner.CreateViaPush<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, Construct<ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args>>(default(ChainOptions), new Construct<ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize)));
        }

        internal static T[] ToArrayByRef<T, Inner>(ref Inner inner, int? maybeMaxCountForStackBasedPath, ArrayPool<T>? arrayPool, bool cleanBuffers)
                where Inner : INode<T>
        {
            if (Optimizations.ToArray.Try<T, Inner>(ref inner, out var array))
                return array;

            if (Optimizations.CopyTo.TryGetCount<T, Inner>(ref inner, out var count))
            {
                if (count == 0)
                    return Array.Empty<T>();

                // every slot is written by the copy
                array = GC.AllocateUninitializedArray<T>(count);
                Optimizations.CopyTo.Copy<T, Inner>(ref inner, new Optimizations.CopyTo<T>(array));
                return array;
            }

            if (inner.TryPushOptimization<Optimizations.MaterialiseArray, T[]>(default(Optimizations.MaterialiseArray), out array))
                return array;

            return ToArrayWithoutOptimizationCheck(ref inner, maybeMaxCountForStackBasedPath ?? 64, arrayPool, cleanBuffers);
        }

        // ToArrayByRef for a buffer that does not outlive the call: a counted source is copied into a rented array rather
        // than one of its own length, and one pushed is rented into from the builder. An answer making an array of its
        // own (a ToArray, a MaterialiseArray) is taken as it is, as the copy into a rented array would cost more than
        // the allocation it saves. A source that throws leaves its rented array to the GC.
        internal static PooledArray<T> ToPooledArrayByRef<T, Inner>(ref Inner inner, ArrayPool<T>? pool, bool cleanBuffers)
                where Inner : INode<T>
        {
            if (Optimizations.CopyTo.TryGetCount<T, Inner>(ref inner, out var count))
            {
                if (count == 0)
                    return PooledArray<T>.Owned(Array.Empty<T>());

                var rented = PooledArray<T>.Rent(count, pool, cleanBuffers);
                return rented.WithCount(Optimizations.CopyTo.Copy<T, Inner>(ref inner, new Optimizations.CopyTo<T>(rented.Array)));
            }

            if (Optimizations.ToArray.Try<T, Inner>(ref inner, out var array)
             || inner.TryPushOptimization<Optimizations.MaterialiseArray, T[]>(default(Optimizations.MaterialiseArray), out array))
                return PooledArray<T>.Owned(array);

            return inner.CreateViaPush<PooledArray<T>, ToPooledArrayForward<T>, Construct<ToPooledArrayForward<T>, ToPooledArrayForward<T>.Args>>(default(ChainOptions), new Construct<ToPooledArrayForward<T>, ToPooledArrayForward<T>.Args>(new ToPooledArrayForward<T>.Args(pool, cleanBuffers)));
        }

        // the node is taken by value, as by First and Last: a copy made of an `in` node is read back with one wide load
        // over the caller's narrower stores
        internal static T[] ToArray<T, Inner>(Inner inner, int? maybeMaxCountForStackBasedPath, ArrayPool<T>? arrayPool, bool cleanBuffers)
                where Inner : INode<T>
            => ToArrayByRef(ref inner, maybeMaxCountForStackBasedPath ?? 64, arrayPool, cleanBuffers);

        // the chain pulled into the destination until either ends; the number of elements written
        private static int PullInto<T, Inner, InnerEnumerator>(in Inner inner, Span<T> destination)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            var enumerator = InnerEnumerator.Create(in inner);
            try
            {
                var written = 0;
                while (written < destination.Length && enumerator.MoveNext())
                    destination[written++] = enumerator.Current;
                return written;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        internal static void GetCountInformation<Node>(this Node node, out CountInformation ci) where Node : INode => node.GetCountInformation(out ci);

        internal static T Aggregate<T, Inner>(in Inner source, Func<T, T, T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ReduceForward<T>, Construct<ReduceForward<T>, ReduceForward<T>.Args>>(default(ChainOptions), new Construct<ReduceForward<T>, ReduceForward<T>.Args>(new ReduceForward<T>.Args(func)));
        }

        internal static void ForEach<T, Inner>(in Inner source, Action<T> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            source.CreateViaPush<T, ForEachForward<T>, Construct<ForEachForward<T>, ForEachForward<T>.Args>>(default(ChainOptions), new Construct<ForEachForward<T>, ForEachForward<T>.Args>(new ForEachForward<T>.Args(func)));
        }

        internal static T ForEach<T, U, Inner>(in Inner source, T seed, RefAction<T, U> func)
            where Inner : INode<U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ForEachForwardRef<T, U>, Construct<ForEachForwardRef<T, U>, ForEachForwardRef<T, U>.Args>>(default(ChainOptions), new Construct<ForEachForwardRef<T, U>, ForEachForwardRef<T, U>.Args>(new ForEachForwardRef<T, U>.Args(seed, func)));
        }

        internal static T ForEach<T, U, Inner, RefAction>(in Inner source, T seed, RefAction func)
            where Inner : INode<U>
            where RefAction : IRefAction<T, U>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<T, ValueForeachForwardRef<T, U, RefAction>, Construct<ValueForeachForwardRef<T, U, RefAction>, ValueForeachForwardRef<T, U, RefAction>.Args>>(default(ChainOptions), new Construct<ValueForeachForwardRef<T, U, RefAction>, ValueForeachForwardRef<T, U, RefAction>.Args>(new ValueForeachForwardRef<T, U, RefAction>.Args(seed, func)));
        }


        internal static TAccumulate Aggregate<T, TAccumulate, Inner>(in Inner source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return source.CreateViaPush<TAccumulate, FoldForward<T, TAccumulate>, Construct<FoldForward<T, TAccumulate>, FoldForward<T, TAccumulate>.Args>>(default(ChainOptions), new Construct<FoldForward<T, TAccumulate>, FoldForward<T, TAccumulate>.Args>(new FoldForward<T, TAccumulate>.Args(func, seed)));
        }

        internal static TResult Aggregate<T, TAccumulate, TResult, Inner>(in Inner source, TAccumulate seed, Func<TAccumulate, T, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<T>
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return resultSelector(source.CreateViaPush<TAccumulate, FoldForward<T, TAccumulate>, Construct<FoldForward<T, TAccumulate>, FoldForward<T, TAccumulate>.Args>>(default(ChainOptions), new Construct<FoldForward<T, TAccumulate>, FoldForward<T, TAccumulate>.Args>(new FoldForward<T, TAccumulate>.Args(func, seed))));
        }

        internal static bool All<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, All<T>, Construct<All<T>, All<T>.Args>>(default(ChainOptions), new Construct<All<T>, All<T>.Args>(new All<T>.Args(predicate)));
        }

        internal static bool All<T, Inner, Predicate>(in Inner source, Predicate predicate)
            where Inner : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, All<T, Predicate>, Construct<All<T, Predicate>, All<T, Predicate>.Args>>(default(ChainOptions), new Construct<All<T, Predicate>, All<T, Predicate>.Args>(new All<T, Predicate>.Args(predicate)));
        }

        internal static bool Any<T, Inner>(Inner source)
            where Inner : INode<T>
        {
            if (source.TryPushOptimization<Optimizations.Any, bool>(new Optimizations.Any(), out var any))
                return any;

            return source.CreateViaPush<bool, Anything<T>, Construct<Anything<T>, Anything<T>.Args>>(default(ChainOptions), new Construct<Anything<T>, Anything<T>.Args>(default));
        }

        internal static bool Any<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<bool, Any<T>, Construct<Any<T>, Any<T>.Args>>(default(ChainOptions), new Construct<Any<T>, Any<T>.Args>(new Any<T>.Args(predicate)));
        }

        // -- 

        internal static Nodes.Select.Node<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, Func<T, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new Nodes.Select.Node<T, U, TPrior>(in prior, selector);
        }
        internal static Nodes.Select_In.Node<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, InFunc<T, U> f)
            where TPrior : INode<T>
            => new Nodes.Select_In.Node<T, U, TPrior>(in prior, f);

        internal static Nodes.SelectIdx.Node<T, U, TPrior> Select<T, U, TPrior>(in TPrior prior, Func<T, int, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new Nodes.SelectIdx.Node<T, U, TPrior>(in prior, selector);
        }
        internal static Nodes.Map.Node<T, U, TPrior> Map<T, U, TPrior>(in TPrior prior, Func<T, U> selector)
            where TPrior : INode<T>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new Nodes.Map.Node<T, U, TPrior>(in prior, selector);
        }
        internal static Nodes.Filter.Node<T, TPrior> Filter<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.Filter.Node<T, TPrior>(in prior, predicate);
        }
        internal static Nodes.Where.Node<T, TPrior> Where<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.Where.Node<T, TPrior>(in prior, predicate);
        }
        internal static Nodes.WhereIdx.Node<T, TPrior> Where<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.WhereIdx.Node<T, TPrior>(in prior, predicate);
        }
        internal static Nodes.Where_In.Node<T, TPrior> Where<T, TPrior>(in TPrior prior, InFunc<T, bool> f)
            where TPrior : INode<T>
            => new Nodes.Where_In.Node<T, TPrior>(in prior, f);

        internal static Nodes.Skip.Node<T, TPrior> Skip<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
            => new Nodes.Skip.Node<T, TPrior>(in prior, count);

        // SkipLast(count) is Take(..^count), as it is in System.Linq
        internal static Nodes.TakeRange.Node<T, TPrior> SkipLast<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
            => new Nodes.TakeRange.Node<T, TPrior>(in prior, new Range(Index.Start, Index.FromEnd(Math.Max(0, count))));

        internal static Nodes.SkipWhile.Node<T, TPrior> SkipWhile<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.SkipWhile.Node<T, TPrior>(in prior, predicate);
        }
        internal static Nodes.SkipWhileIdx.Node<T, TPrior> SkipWhile<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.SkipWhileIdx.Node<T, TPrior>(in prior, predicate);
        }

        internal static Nodes.Take.Node<T, TPrior> Take<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
                => new Nodes.Take.Node<T, TPrior>(in prior, count);

        internal static Nodes.TakeRange.Node<T, TPrior> Take<T, TPrior>(in TPrior prior, Range range)
            where TPrior : INode<T>
                => new Nodes.TakeRange.Node<T, TPrior>(in prior, range);

        // TakeLast(count) is Take(^count..), as it is in System.Linq
        internal static Nodes.TakeRange.Node<T, TPrior> TakeLast<T, TPrior>(in TPrior prior, int count)
            where TPrior : INode<T>
                => new Nodes.TakeRange.Node<T, TPrior>(in prior, new Range(Index.FromEnd(Math.Max(0, count)), Index.End));

        internal static Nodes.TakeWhile.Node<T, TPrior> TakeWhile<T, TPrior>(in TPrior prior, Func<T, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.TakeWhile.Node<T, TPrior>(in prior, predicate);
        }
        internal static Nodes.TakeWhileIdx.Node<T, TPrior> TakeWhile<T, TPrior>(in TPrior prior, Func<T, int, bool> predicate)
            where TPrior : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.TakeWhileIdx.Node<T, TPrior>(in prior, predicate);
        }

        //internal static T[] ToArray<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
        //    where Inner : INode<T>
        //    => NodeImpl.ToArray(in inner, maybeMaxCountForStackBasedPath, in arrayPoolInfo);

        internal static T[] ToArrayUsePool<T, Inner, InnerEnumerator>(in Inner inner, ArrayPool<T>? maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            if (viaPull)
            {
                var enumerator = InnerEnumerator.Create(in inner);
                return Impl.ToArrayViaArrayPool<T, InnerEnumerator>(arrayPool, cleanBuffers, info.ActualSize, ref enumerator);
            }

            return inner.CreateViaPush<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, Construct<ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args>>(default(ChainOptions), new Construct<ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>, ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>.Args(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize)));
        }

        internal static T[] ToArrayUseStack<T, Inner, InnerEnumerator>(in Inner inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            var enumerator = InnerEnumerator.Create(in inner);
            return arrayPoolInfo.HasValue
                ? Impl.ToArrayViaStack<T, InnerEnumerator, ArrayPoolAllocator<T>>(maxStackItemCount, new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), ref enumerator)
                : Impl.ToArrayViaStack<T, InnerEnumerator, GarbageCollectedAllocator<T>>(maxStackItemCount, default, ref enumerator);
        }


        internal static List<T> ToList<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
        {
            var copyable = inner;
            if (Optimizations.CopyTo.TryGetCount<T, Inner>(ref copyable, out var count))
            {
                var list = new List<T>(count);
                System.Runtime.InteropServices.CollectionsMarshal.SetCount(list, count);
                Optimizations.CopyTo.Copy<T, Inner>(ref copyable, new Optimizations.CopyTo<T>(list));
                return list;
            }

            if (copyable.TryPushOptimization<Optimizations.MaterialiseList, List<T>>(default(Optimizations.MaterialiseList), out var materialised))
                return materialised;

            return ToListByPush<T, Inner>(in inner, in arrayPoolInfo);
        }

        // as ToList, with the chain's enumerator: a chain whose count is known but that cannot copy itself out (a Select
        // over a Range) is pulled straight into the list's storage, rather than added to it element by element, unless it
        // holds an erased source, which is pulled through its boxed enumerator and pushed faster
        internal static List<T> ToList<T, Inner, InnerEnumerator>(in Inner inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            var copyable = inner;
            if (Optimizations.CopyTo.TryGetCount<T, Inner>(ref copyable, out var count))
            {
                var list = new List<T>(count);
                System.Runtime.InteropServices.CollectionsMarshal.SetCount(list, count);
                Optimizations.CopyTo.Copy<T, Inner>(ref copyable, new Optimizations.CopyTo<T>(list));
                return list;
            }

            if (copyable.TryPushOptimization<Optimizations.MaterialiseList, List<T>>(default(Optimizations.MaterialiseList), out var materialised))
                return materialised;

            if (!InnerEnumerator.RebuildChanges
                && TryGetNonEnumeratedCount<T, Inner>(in copyable, out count))
            {
                var list = new List<T>(count);
                System.Runtime.InteropServices.CollectionsMarshal.SetCount(list, count);
                var pulled = PullInto<T, Inner, InnerEnumerator>(in copyable, System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list));
                if (pulled < count)
                    System.Runtime.InteropServices.CollectionsMarshal.SetCount(list, pulled);
                return list;
            }

            return ToListByPush<T, Inner>(in inner, in arrayPoolInfo);
        }

        // out of line, so the sized copy before it does not carry these three pushes' frames
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static List<T> ToListByPush<T, Inner>(in Inner inner, in (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (!info.IsStale
                && (info.ActualLengthIsMaximumLength || info.MaximumLength <= 4))
            {
                if (info.MaximumLength == 0)
                    return new List<T>();

                return inner.CreateViaPush<List<T>, ToListForward<T>, Construct<ToListForward<T>, ToListForward<T>.Args>>(default(ChainOptions), new Construct<ToListForward<T>, ToListForward<T>.Args>(new ToListForward<T>.Args(info.ActualSize)));
            }

            if (!arrayPoolInfo.HasValue)
                return inner.CreateViaPush<List<T>, ToListViaBuilderForward<T>, Construct<ToListViaBuilderForward<T>, ToListViaBuilderForward<T>.Args>>(default(ChainOptions), new Construct<ToListViaBuilderForward<T>, ToListViaBuilderForward<T>.Args>(default));

            return inner.CreateViaPush<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, Construct<ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args>>(default(ChainOptions), new Construct<ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args(new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), 0, null)));
        }

        internal static List<T> ToListUsePool<T, Inner, InnerEnumerator>(in Inner inner, ArrayPool<T>? maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            inner.GetCountInformation(out var info);

            var cleanBuffers = maybeCleanBuffers ?? !CachedTypeInfo<T>.IsPrimitive;
            var arrayPool = maybeArrayPool ?? ArrayPool<T>.Shared;

            if (viaPull)
            {
                var enumerator = InnerEnumerator.Create(in inner);
                return Impl.ToListViaArrayPool<T, InnerEnumerator>(arrayPool, cleanBuffers, info.ActualSize, ref enumerator);
            }

            return inner.CreateViaPush<List<T>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, Construct<ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args>>(default(ChainOptions), new Construct<ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>, ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args>(new ToListViaArrayPoolForward<T, ArrayPoolAllocator<T>>.Args(new ArrayPoolAllocator<T>(arrayPool, cleanBuffers), 0, info.ActualSize)));
        }

        internal static List<T> ToListUseStack<T, Inner, InnerEnumerator>(in Inner inner, int maxStackItemCount = 64, (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<T>
            where InnerEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Inner, InnerEnumerator>
        {
            var enumerator = InnerEnumerator.Create(in inner);
            return arrayPoolInfo.HasValue
                ? Impl.ToListViaStack<T, InnerEnumerator, ArrayPoolAllocator<T>>(maxStackItemCount, new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), ref enumerator)
                : Impl.ToListViaStack<T, InnerEnumerator, GarbageCollectedAllocator<T>>(maxStackItemCount, default, ref enumerator);
        }

        internal static HashSet<T> ToHashSet<T, Inner>(in Inner inner, IEqualityComparer<T>? comparer = null)
            where Inner : INode<T>
            => inner.CreateViaPush<HashSet<T>, ToHashSet<T>, Construct<ToHashSet<T>, ToHashSet<T>.Args>>(default(ChainOptions), new Construct<ToHashSet<T>, ToHashSet<T>.Args>(new ToHashSet<T>.Args(comparer)));

        internal static Dictionary<TKey, T> ToDictionary<T, TKey, Inner>(in Inner inner, Func<T, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where Inner : INode<T>
            => inner.CreateViaPush<Dictionary<TKey, T>, ToDictionary<T, TKey>, Construct<ToDictionary<T, TKey>, ToDictionary<T, TKey>.Args>>(default(ChainOptions), new Construct<ToDictionary<T, TKey>, ToDictionary<T, TKey>.Args>(new ToDictionary<T, TKey>.Args(keySelector, comparer, DictionaryCapacity<T, Inner>(in inner))));

        internal static Dictionary<TKey, TValue> ToDictionary<T, TKey, TValue, Inner>(in Inner inner, Func<T, TKey> keySelector, Func<T, TValue> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where Inner : INode<T>
            => inner.CreateViaPush<Dictionary<TKey, TValue>, ToDictionary<T, TKey, TValue>, Construct<ToDictionary<T, TKey, TValue>, ToDictionary<T, TKey, TValue>.Args>>(default(ChainOptions), new Construct<ToDictionary<T, TKey, TValue>, ToDictionary<T, TKey, TValue>.Args>(new ToDictionary<T, TKey, TValue>.Args(keySelector, elementSelector, comparer, DictionaryCapacity<T, Inner>(in inner))));

        internal static Dictionary<TKey, TValue> ToDictionaryFromKeyValuePairs<TKey, TValue, Inner>(in Inner inner, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
            where Inner : INode<KeyValuePair<TKey, TValue>>
            => inner.CreateViaPush<Dictionary<TKey, TValue>, ToDictionaryFromKeyValuePairs<TKey, TValue>, Construct<ToDictionaryFromKeyValuePairs<TKey, TValue>, ToDictionaryFromKeyValuePairs<TKey, TValue>.Args>>(default(ChainOptions), new Construct<ToDictionaryFromKeyValuePairs<TKey, TValue>, ToDictionaryFromKeyValuePairs<TKey, TValue>.Args>(new ToDictionaryFromKeyValuePairs<TKey, TValue>.Args(comparer, DictionaryCapacity<KeyValuePair<TKey, TValue>, Inner>(in inner))));

        internal static Dictionary<TKey, TValue> ToDictionaryFromTuples<TKey, TValue, Inner>(in Inner inner, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
            where Inner : INode<(TKey Key, TValue Value)>
            => inner.CreateViaPush<Dictionary<TKey, TValue>, ToDictionaryFromTuples<TKey, TValue>, Construct<ToDictionaryFromTuples<TKey, TValue>, ToDictionaryFromTuples<TKey, TValue>.Args>>(default(ChainOptions), new Construct<ToDictionaryFromTuples<TKey, TValue>, ToDictionaryFromTuples<TKey, TValue>.Args>(new ToDictionaryFromTuples<TKey, TValue>.Args(comparer, DictionaryCapacity<(TKey Key, TValue Value), Inner>(in inner))));

        // a count known without running anything sizes the dictionary, as System.Linq sizes one from an ICollection<T>
        private static int DictionaryCapacity<T, Inner>(in Inner inner)
            where Inner : INode<T>
            => TryGetNonEnumeratedCount<T, Inner>(in inner, out var count) ? count : 0;

        // Last, First and ElementAt keep the push they fall back to in a method of its own, so what is left (the request)
        // is small enough for the JIT to inline into the caller, which has just written the node. They take the node by
        // value: a call on an `in` node of a type parameter is made on a copy, read with one wide load over the caller's
        // narrower stores, which defeats store forwarding wherever the node is not promoted. Last's request is a cached
        // value, since constructing `^1` inside it is enough for the JIT to judge the inline unprofitable
        private static readonly Optimizations.ElementAt LastRequest = new(^1);

        internal static T Last<T, Inner>(Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in LastRequest, out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                // answered as empty: running the query again would only reach the same throw, at the cost of the work
                // the answer already did (an OrderBy's sort, a Reverse's copy)
                (true, (false, _)) => throw new InvalidOperationException(),
                _ => LastByPush<T, Inner>(in inner)
            };

        private static T LastByPush<T, Inner>(in Inner inner)
            where Inner : INode<T>
            => inner.CreateViaPush<T, Last<T>, Construct<Last<T>, Last<T>.Args>>(default(ChainOptions), new Construct<Last<T>, Last<T>.Args>(default));

        internal static T Last<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            // when the source can be walked backwards, the last match is the first match of the reversal, so the
            // scan stops at the first hit instead of running the whole sequence (matches System.Linq's short circuit)
            var tmp = inner;
            if (Optimizations.Reverse.Try<T, Inner>(ref tmp, out var reversed))
                return reversed.CreateViaPush<T, FirstPredicate<T>, Construct<FirstPredicate<T>, FirstPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstPredicate<T>, FirstPredicate<T>.Args>(new FirstPredicate<T>.Args(predicate)));

            if (inner.TryPushOptimization<Optimizations.LastMatch<T>, (bool, T)>(new Optimizations.LastMatch<T>(predicate), out var match))
            {
                if (!match.Item1)
                    ThrowHelper.ThrowNoMatchException();
                return match.Item2;
            }

            return inner.CreateViaPush<T, LastPredicate<T>, Construct<LastPredicate<T>, LastPredicate<T>.Args>>(default(ChainOptions), new Construct<LastPredicate<T>, LastPredicate<T>.Args>(new LastPredicate<T>.Args(predicate)));
        }

        internal static T? LastOrDefault<T, Inner>(Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in LastRequest, out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => default,
                _ => LastOrDefaultByPush<T, Inner>(in inner)
            };

        private static T? LastOrDefaultByPush<T, Inner>(in Inner inner)
            where Inner : INode<T>
            => inner.CreateViaPush<T?, LastOrDefault<T>, Construct<LastOrDefault<T>, LastOrDefault<T>.Args>>(default(ChainOptions), new Construct<LastOrDefault<T>, LastOrDefault<T>.Args>(default));

        internal static T? LastOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var tmp = inner;
            if (Optimizations.Reverse.Try<T, Inner>(ref tmp, out var reversed))
                return reversed.CreateViaPush<T?, FirstOrDefaultPredicate<T>, Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>(new FirstOrDefaultPredicate<T>.Args(predicate)));

            if (inner.TryPushOptimization<Optimizations.LastMatch<T>, (bool, T)>(new Optimizations.LastMatch<T>(predicate), out var match))
                return match.Item1 ? match.Item2 : default;

            return inner.CreateViaPush<T?, LastOrDefaultPredicate<T>, Construct<LastOrDefaultPredicate<T>, LastOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<LastOrDefaultPredicate<T>, LastOrDefaultPredicate<T>.Args>(new LastOrDefaultPredicate<T>.Args(predicate)));
        }

        internal static T LastOrDefault<T, Inner>(Inner inner, T defaultValue)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in LastRequest, out var maybeLast), maybeLast) switch
            {
                (true, (true, var last)) => last,
                (true, (false, _)) => defaultValue,
                _ => LastOrDefaultByPush<T, Inner>(in inner, defaultValue)
            };

        private static T LastOrDefaultByPush<T, Inner>(in Inner inner, T defaultValue)
            where Inner : INode<T>
            => inner.CreateViaPush<T, LastOrDefault<T>, Construct<LastOrDefault<T>, LastOrDefault<T>.Args>>(default(ChainOptions), new Construct<LastOrDefault<T>, LastOrDefault<T>.Args>(new LastOrDefault<T>.Args(defaultValue)));

        internal static T LastOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate, T defaultValue)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            var reversible = inner;
            if (Optimizations.Reverse.Try<T, Inner>(ref reversible, out var reversed))
                return reversed.CreateViaPush<T, FirstOrDefaultPredicate<T>, Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>(new FirstOrDefaultPredicate<T>.Args(predicate, defaultValue)));

            if (inner.TryPushOptimization<Optimizations.LastMatch<T>, (bool, T)>(new Optimizations.LastMatch<T>(predicate), out var match))
                return match.Item1 ? match.Item2 : defaultValue;

            return inner.CreateViaPush<T, LastOrDefaultPredicate<T>, Construct<LastOrDefaultPredicate<T>, LastOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<LastOrDefaultPredicate<T>, LastOrDefaultPredicate<T>.Args>(new LastOrDefaultPredicate<T>.Args(predicate, defaultValue)));
        }

        internal static T First<T, Inner>(Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(0), out var maybeFirst), maybeFirst) switch
            {
                (true, (true, var first)) => first,
                (true, (false, _)) => throw new InvalidOperationException(),
                _ => FirstByPush<T, Inner>(in inner)
            };

        private static T FirstByPush<T, Inner>(in Inner inner)
            where Inner : INode<T>
            => inner.CreateViaPush<T, First<T>, Construct<First<T>, First<T>.Args>>(default(ChainOptions), new Construct<First<T>, First<T>.Args>(default));

        internal static T First<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, FirstPredicate<T>, Construct<FirstPredicate<T>, FirstPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstPredicate<T>, FirstPredicate<T>.Args>(new FirstPredicate<T>.Args(predicate)));
        }

        internal static T? FirstOrDefault<T, Inner>(Inner inner)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(0), out var maybeFirst), maybeFirst) switch
            {
                (true, (true, var first)) => first,
                (true, (false, _)) => default,
                _ => FirstOrDefaultByPush<T, Inner>(in inner)
            };

        private static T? FirstOrDefaultByPush<T, Inner>(in Inner inner)
            where Inner : INode<T>
            => inner.CreateViaPush<T?, FirstOrDefault<T>, Construct<FirstOrDefault<T>, FirstOrDefault<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefault<T>, FirstOrDefault<T>.Args>(default));

        internal static T? FirstOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T?, FirstOrDefaultPredicate<T>, Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>(new FirstOrDefaultPredicate<T>.Args(predicate)));
        }

        internal static T FirstOrDefault<T, Inner>(Inner inner, T defaultValue)
            where Inner : INode<T> =>
            (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(0), out var maybeFirst), maybeFirst) switch
            {
                (true, (true, var first)) => first,
                (true, (false, _)) => defaultValue,
                _ => FirstOrDefaultByPush<T, Inner>(in inner, defaultValue)
            };

        private static T FirstOrDefaultByPush<T, Inner>(in Inner inner, T defaultValue)
            where Inner : INode<T>
            => inner.CreateViaPush<T, FirstOrDefault<T>, Construct<FirstOrDefault<T>, FirstOrDefault<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefault<T>, FirstOrDefault<T>.Args>(new FirstOrDefault<T>.Args(defaultValue)));

        internal static T FirstOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate, T defaultValue)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            // POTENTIAL OPTIMIZATION: new request FirstMatch: the first element matching a predicate, as (found,
            // element), answered by an OrderBy with one scan of its source for the least matching element (the
            // earliest among equal keys, keeping the sort's stability) instead of sorting it all; the same request
            // would serve FirstOrDefault(predicate) and First(predicate)
            return inner.CreateViaPush<T, FirstOrDefaultPredicate<T>, Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<FirstOrDefaultPredicate<T>, FirstOrDefaultPredicate<T>.Args>(new FirstOrDefaultPredicate<T>.Args(predicate, defaultValue)));
        }

        internal static T Single<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T, Single<T>, Construct<Single<T>, Single<T>.Args>>(default(ChainOptions), new Construct<Single<T>, Single<T>.Args>(default));

        internal static T Single<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, SinglePredicate<T>, Construct<SinglePredicate<T>, SinglePredicate<T>.Args>>(default(ChainOptions), new Construct<SinglePredicate<T>, SinglePredicate<T>.Args>(new SinglePredicate<T>.Args(predicate)));
        }

        internal static T? SingleOrDefault<T, Inner>(in Inner inner)
            where Inner : INode<T> => inner.CreateViaPush<T?, SingleOrDefault<T>, Construct<SingleOrDefault<T>, SingleOrDefault<T>.Args>>(default(ChainOptions), new Construct<SingleOrDefault<T>, SingleOrDefault<T>.Args>(default));

        internal static T? SingleOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T?, SingleOrDefaultPredicate<T>, Construct<SingleOrDefaultPredicate<T>, SingleOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<SingleOrDefaultPredicate<T>, SingleOrDefaultPredicate<T>.Args>(new SingleOrDefaultPredicate<T>.Args(predicate)));
        }

        // POTENTIAL OPTIMIZATION: new request NonEnumeratedSingle: whether the chain holds no element, one, or more,
        // answered without running anything; a collection answers from its Count (System.Linq's IList<T> path, which
        // throws for a count above one without enumerating), and OrderBy, Reverse and Shuffle, whose order does not
        // change the answer, forward it to their source where nothing they run could throw (an OrderBy's key selector
        // runs over every element today, so its exceptions are part of the answer). Count information alone does not
        // do it: an OrderBy reports its source's length with no side effects, so a throwing key selector would give
        // way to "more than one element"
        internal static T SingleOrDefault<T, Inner>(in Inner inner, T defaultValue)
            where Inner : INode<T> =>
            inner.CreateViaPush<T, SingleOrDefault<T>, Construct<SingleOrDefault<T>, SingleOrDefault<T>.Args>>(default(ChainOptions), new Construct<SingleOrDefault<T>, SingleOrDefault<T>.Args>(new SingleOrDefault<T>.Args(defaultValue)));

        internal static T SingleOrDefault<T, Inner>(in Inner inner, Func<T, bool> predicate, T defaultValue)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return inner.CreateViaPush<T, SingleOrDefaultPredicate<T>, Construct<SingleOrDefaultPredicate<T>, SingleOrDefaultPredicate<T>.Args>>(default(ChainOptions), new Construct<SingleOrDefaultPredicate<T>, SingleOrDefaultPredicate<T>.Args>(new SingleOrDefaultPredicate<T>.Args(predicate, defaultValue)));
        }

        // a negative index is out of range whatever the sequence holds, so nothing is run to find that out
        internal static T ElementAt<T, Inner>(Inner inner, int index)
            where Inner : INode<T>
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(index), out var maybe))
                return maybe.Item1 ? maybe.Item2 : throw new ArgumentOutOfRangeException(nameof(index));

            return ElementAtByPush<T, Inner>(in inner, index);
        }

        private static T ElementAtByPush<T, Inner>(in Inner inner, int index)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            return inner.CreateViaPush<T, ElementAt<T>, Construct<ElementAt<T>, ElementAt<T>.Args>>(default(ChainOptions), new Construct<ElementAt<T>, ElementAt<T>.Args>(new ElementAt<T>.Args(index, countInfo.ActualSize)));
        }

        internal static T? ElementAtOrDefault<T, Inner>(Inner inner, int index)
            where Inner : INode<T>
        {
            if (index < 0)
                return default;

            if (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(index), out var maybe))
                return maybe.Item1 ? maybe.Item2 : default;

            return ElementAtOrDefaultByPush<T, Inner>(in inner, index);
        }

        private static T? ElementAtOrDefaultByPush<T, Inner>(in Inner inner, int index)
            where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            return inner.CreateViaPush<T?, ElementAtOrDefault<T>, Construct<ElementAtOrDefault<T>, ElementAtOrDefault<T>.Args>>(default(ChainOptions), new Construct<ElementAtOrDefault<T>, ElementAtOrDefault<T>.Args>(new ElementAtOrDefault<T>.Args(index, countInfo.ActualSize)));
        }

        internal static T ElementAt<T, Inner>(Inner inner, Index index)
            where Inner : INode<T>
        {
            if (!index.IsFromEnd)
                return ElementAt<T, Inner>(inner, index.Value);

            if (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(index), out var maybe))
                return maybe.Item1 ? maybe.Item2 : throw new ArgumentOutOfRangeException(nameof(index));

            return ElementAtFromEndUnanswered<T, Inner>(in inner, index.Value);
        }

        // a from-end index no node can answer is found by buffering the last index.Value elements, the first of which
        // is the one asked for
        private static T ElementAtFromEndUnanswered<T, Inner>(in Inner inner, int fromEnd)
            where Inner : INode<T>
        {
            if (fromEnd == 0)
                throw new ArgumentOutOfRangeException("index");

            // a count read without enumerating turns the index into one from the start, as System.Linq does
            if (TryGetNonEnumeratedCount<T, Inner>(in inner, out var count))
                return ElementAt<T, Inner>(inner, count - fromEnd);

            return ElementAt<T, Nodes.TakeRange.Node<T, Inner>>(new Nodes.TakeRange.Node<T, Inner>(in inner, new Range(^fromEnd, new Index(fromEnd - 1, fromEnd: true))), 0);
        }

        internal static T? ElementAtOrDefault<T, Inner>(Inner inner, Index index)
            where Inner : INode<T>
        {
            if (!index.IsFromEnd)
                return ElementAtOrDefault<T, Inner>(inner, index.Value);

            if (inner.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(index), out var maybe))
                return maybe.Item1 ? maybe.Item2 : default;

            return ElementAtOrDefaultFromEndUnanswered<T, Inner>(in inner, index.Value);
        }

        private static T? ElementAtOrDefaultFromEndUnanswered<T, Inner>(in Inner inner, int fromEnd)
            where Inner : INode<T>
        {
            if (fromEnd == 0)
                return default;

            if (TryGetNonEnumeratedCount<T, Inner>(in inner, out var count))
                return ElementAtOrDefault<T, Inner>(inner, count - fromEnd);

            return ElementAtOrDefault<T, Nodes.TakeRange.Node<T, Inner>>(new Nodes.TakeRange.Node<T, Inner>(in inner, new Range(^fromEnd, new Index(fromEnd - 1, fromEnd: true))), 0);
        }

        internal static decimal AverageDecimal<Inner>(in Inner inner) where Inner : INode<decimal> => inner.CreateViaPush<decimal, AverageDecimal, Construct<AverageDecimal, AverageDecimal.Args>>(default(ChainOptions), new Construct<AverageDecimal, AverageDecimal.Args>(new AverageDecimal.Args(SIMDOptions.OnlyIfSame)));
        internal static double  AverageDouble<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<double>  => inner.CreateViaPush<double,  AverageDouble, Construct<AverageDouble, AverageDouble.Args>>(default(ChainOptions), new Construct<AverageDouble, AverageDouble.Args>(new AverageDouble.Args(simdOptions)));
        internal static float   AverageFloat<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<float>   => inner.CreateViaPush<float,   AverageFloat, Construct<AverageFloat, AverageFloat.Args>>(default(ChainOptions), new Construct<AverageFloat, AverageFloat.Args>(new AverageFloat.Args(simdOptions)));
        internal static double  AverageInt<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<int>     => inner.CreateViaPush<double,  AverageInt, Construct<AverageInt, AverageInt.Args>>(default(ChainOptions), new Construct<AverageInt, AverageInt.Args>(new AverageInt.Args(simdOptions)));
        internal static double  AverageLong<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<long>    => inner.CreateViaPush<double,  AverageLong, Construct<AverageLong, AverageLong.Args>>(default(ChainOptions), new Construct<AverageLong, AverageLong.Args>(new AverageLong.Args(simdOptions)));

        internal static decimal Average<T, Inner>(in Inner inner, Func<T, decimal> selector) where Inner : INode<T> =>
            inner.CreateViaPush<decimal, Nodes.Select.Push<T, decimal, AverageDecimal>, Nodes.Select.Filler<T, decimal, AverageDecimal, Construct<AverageDecimal, AverageDecimal.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, decimal, AverageDecimal, Construct<AverageDecimal, AverageDecimal.Args>>(selector, new Construct<AverageDecimal, AverageDecimal.Args>(default)));
        internal static double Average<T, Inner>(in Inner inner, Func<T, double> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, Nodes.Select.Push<T, double, AverageDouble>, Nodes.Select.Filler<T, double, AverageDouble, Construct<AverageDouble, AverageDouble.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, double, AverageDouble, Construct<AverageDouble, AverageDouble.Args>>(selector, new Construct<AverageDouble, AverageDouble.Args>(default)));
        internal static float Average<T, Inner>(in Inner inner, Func<T, float> selector) where Inner : INode<T> =>
            inner.CreateViaPush<float, Nodes.Select.Push<T, float, AverageFloat>, Nodes.Select.Filler<T, float, AverageFloat, Construct<AverageFloat, AverageFloat.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, float, AverageFloat, Construct<AverageFloat, AverageFloat.Args>>(selector, new Construct<AverageFloat, AverageFloat.Args>(default)));
        internal static double Average<T, Inner>(in Inner inner, Func<T, int> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, Nodes.Select.Push<T, int, AverageInt>, Nodes.Select.Filler<T, int, AverageInt, Construct<AverageInt, AverageInt.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, int, AverageInt, Construct<AverageInt, AverageInt.Args>>(selector, new Construct<AverageInt, AverageInt.Args>(default)));
        internal static double Average<T, Inner>(in Inner inner, Func<T, long> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double, Nodes.Select.Push<T, long, AverageLong>, Nodes.Select.Filler<T, long, AverageLong, Construct<AverageLong, AverageLong.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, long, AverageLong, Construct<AverageLong, AverageLong.Args>>(selector, new Construct<AverageLong, AverageLong.Args>(default)));

        internal static decimal? AverageNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal?, AverageDecimalNullable, Construct<AverageDecimalNullable, AverageDecimalNullable.Args>>(default(ChainOptions), new Construct<AverageDecimalNullable, AverageDecimalNullable.Args>(new AverageDecimalNullable.Args(true)));
        internal static double?  AverageNullableDouble <Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double?,  AverageDoubleNullable, Construct<AverageDoubleNullable, AverageDoubleNullable.Args>>(default(ChainOptions), new Construct<AverageDoubleNullable, AverageDoubleNullable.Args>(new AverageDoubleNullable.Args(true)));
        internal static float?   AverageNullableFloat  <Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float?,   AverageFloatNullable, Construct<AverageFloatNullable, AverageFloatNullable.Args>>(default(ChainOptions), new Construct<AverageFloatNullable, AverageFloatNullable.Args>(new AverageFloatNullable.Args(true)));
        internal static double?  AverageNullableInt    <Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<double?,     AverageIntNullable, Construct<AverageIntNullable, AverageIntNullable.Args>>(default(ChainOptions), new Construct<AverageIntNullable, AverageIntNullable.Args>(new AverageIntNullable.Args(true)));
        internal static double?  AverageNullableLong   <Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<double?,    AverageLongNullable, Construct<AverageLongNullable, AverageLongNullable.Args>>(default(ChainOptions), new Construct<AverageLongNullable, AverageLongNullable.Args>(new AverageLongNullable.Args(true)));

        internal static decimal? Average<T, Inner>(in Inner inner, Func<T, decimal?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<decimal?, Nodes.Select.Push<T, decimal?, AverageDecimalNullable>, Nodes.Select.Filler<T, decimal?, AverageDecimalNullable, Construct<AverageDecimalNullable, AverageDecimalNullable.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, decimal?, AverageDecimalNullable, Construct<AverageDecimalNullable, AverageDecimalNullable.Args>>(selector, new Construct<AverageDecimalNullable, AverageDecimalNullable.Args>(default)));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, double?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, Nodes.Select.Push<T, double?, AverageDoubleNullable>, Nodes.Select.Filler<T, double?, AverageDoubleNullable, Construct<AverageDoubleNullable, AverageDoubleNullable.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, double?, AverageDoubleNullable, Construct<AverageDoubleNullable, AverageDoubleNullable.Args>>(selector, new Construct<AverageDoubleNullable, AverageDoubleNullable.Args>(default)));
        internal static float? Average<T, Inner>(in Inner inner, Func<T, float?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<float?, Nodes.Select.Push<T, float?, AverageFloatNullable>, Nodes.Select.Filler<T, float?, AverageFloatNullable, Construct<AverageFloatNullable, AverageFloatNullable.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, float?, AverageFloatNullable, Construct<AverageFloatNullable, AverageFloatNullable.Args>>(selector, new Construct<AverageFloatNullable, AverageFloatNullable.Args>(default)));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, int?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, Nodes.Select.Push<T, int?, AverageIntNullable>, Nodes.Select.Filler<T, int?, AverageIntNullable, Construct<AverageIntNullable, AverageIntNullable.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, int?, AverageIntNullable, Construct<AverageIntNullable, AverageIntNullable.Args>>(selector, new Construct<AverageIntNullable, AverageIntNullable.Args>(default)));
        internal static double? Average<T, Inner>(in Inner inner, Func<T, long?> selector) where Inner : INode<T> =>
            inner.CreateViaPush<double?, Nodes.Select.Push<T, long?, AverageLongNullable>, Nodes.Select.Filler<T, long?, AverageLongNullable, Construct<AverageLongNullable, AverageLongNullable.Args>>>(default(ChainOptions), new Nodes.Select.Filler<T, long?, AverageLongNullable, Construct<AverageLongNullable, AverageLongNullable.Args>>(selector, new Construct<AverageLongNullable, AverageLongNullable.Args>(default)));

        internal static decimal MinDecimal<Inner>(in Inner inner) where Inner : INode<decimal>  => inner.CreateViaPush<decimal, MinDecimal, Construct<MinDecimal, MinDecimal.Args>>(default(ChainOptions), new Construct<MinDecimal, MinDecimal.Args>(new MinDecimal.Args(true)));
        internal static double  MinDouble<Inner>(in Inner inner) where Inner : INode<double>   => inner.CreateViaPush<double,  MinDouble, Construct<MinDouble, MinDouble.Args>>(default(ChainOptions), new Construct<MinDouble, MinDouble.Args>(new MinDouble.Args(true)));
        internal static float   MinFloat<Inner>(in Inner inner) where Inner : INode<float>    => inner.CreateViaPush<float,   MinFloat, Construct<MinFloat, MinFloat.Args>>(default(ChainOptions), new Construct<MinFloat, MinFloat.Args>(new MinFloat.Args(true)));
        internal static int     MinInt<Inner>(in Inner inner) where Inner : INode<int>      => inner.CreateViaPush<int,     MinInt, Construct<MinInt, MinInt.Args>>(default(ChainOptions), new Construct<MinInt, MinInt.Args>(new MinInt.Args(true)));
        internal static long    MinLong<Inner>(in Inner inner) where Inner : INode<long>     => inner.CreateViaPush<long,    MinLong, Construct<MinLong, MinLong.Args>>(default(ChainOptions), new Construct<MinLong, MinLong.Args>(new MinLong.Args(true)));
        internal static T?      Min<T, Inner>(in Inner inner) where Inner : INode<T>        => inner.CreateViaPush<T?,      Min<T>, Construct<Min<T>, Min<T>.Args>>(default(ChainOptions), new Construct<Min<T>, Min<T>.Args>(new Min<T>.Args(true)));

        // POTENTIAL OPTIMIZATION: new request Extremum: the least (or greatest) element under a given comparer, as found
        // and element, answered without a scan where the chain knows it: Repeat its element, Range its first (or last)
        // under Comparer<int>.Default, an OrderBy on the identity key under the same comparer its first (or last)
        // element by one selection; forwarded by Reverse and Shuffle, whose order does not change the answer. It would
        // serve Max(comparer) from the other end and Min()/Max() over a generic T with the default comparer
        internal static T? Min<T, Inner>(in Inner inner, IComparer<T>? comparer) where Inner : INode<T> =>
            inner.CreateViaPush<T?, MinComparer<T>, Construct<MinComparer<T>, MinComparer<T>.Args>>(default(ChainOptions), new Construct<MinComparer<T>, MinComparer<T>.Args>(new MinComparer<T>.Args(comparer)));

        // POTENTIAL OPTIMIZATION: the Extremum request above under a key selector: an OrderBy whose first key is this selector and comparer would answer with one selection skipping null keys, in place of its sort
        internal static T? MinBy<T, TKey, Inner>(in Inner inner, Func<T, TKey> keySelector, IComparer<TKey>? comparer) where Inner : INode<T> =>
            inner.CreateViaPush<T?, MinBy<T, TKey>, Construct<MinBy<T, TKey>, MinBy<T, TKey>.Args>>(default(ChainOptions), new Construct<MinBy<T, TKey>, MinBy<T, TKey>.Args>(new MinBy<T, TKey>.Args(keySelector, comparer)));

        internal static decimal? MinNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?>  => inner.CreateViaPush<decimal?, MinDecimalNullable, Construct<MinDecimalNullable, MinDecimalNullable.Args>>(default(ChainOptions), new Construct<MinDecimalNullable, MinDecimalNullable.Args>(new MinDecimalNullable.Args(true)));
        internal static double?  MinNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>   => inner.CreateViaPush<double?,  MinDoubleNullable, Construct<MinDoubleNullable, MinDoubleNullable.Args>>(default(ChainOptions), new Construct<MinDoubleNullable, MinDoubleNullable.Args>(new MinDoubleNullable.Args(true)));
        internal static float?   MinNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>    => inner.CreateViaPush<float?,   MinFloatNullable, Construct<MinFloatNullable, MinFloatNullable.Args>>(default(ChainOptions), new Construct<MinFloatNullable, MinFloatNullable.Args>(new MinFloatNullable.Args(true)));
        internal static int?     MinNullableInt<Inner>(in Inner inner) where Inner : INode<int?>      => inner.CreateViaPush<int?,     MinIntNullable, Construct<MinIntNullable, MinIntNullable.Args>>(default(ChainOptions), new Construct<MinIntNullable, MinIntNullable.Args>(new MinIntNullable.Args(true)));
        internal static long?    MinNullableLong<Inner>(in Inner inner) where Inner : INode<long?>     => inner.CreateViaPush<long?,    MinLongNullable, Construct<MinLongNullable, MinLongNullable.Args>>(default(ChainOptions), new Construct<MinLongNullable, MinLongNullable.Args>(new MinLongNullable.Args(true)));

        internal static decimal MaxDecimal<Inner>(in Inner inner) where Inner : INode<decimal>  => inner.CreateViaPush<decimal, MaxDecimal, Construct<MaxDecimal, MaxDecimal.Args>>(default(ChainOptions), new Construct<MaxDecimal, MaxDecimal.Args>(new MaxDecimal.Args(true)));
        internal static double  MaxDouble<Inner>(in Inner inner) where Inner : INode<double>   => inner.CreateViaPush<double,  MaxDouble, Construct<MaxDouble, MaxDouble.Args>>(default(ChainOptions), new Construct<MaxDouble, MaxDouble.Args>(new MaxDouble.Args(true)));
        internal static float   MaxFloat<Inner>(in Inner inner) where Inner : INode<float>    => inner.CreateViaPush<float,   MaxFloat, Construct<MaxFloat, MaxFloat.Args>>(default(ChainOptions), new Construct<MaxFloat, MaxFloat.Args>(new MaxFloat.Args(true)));
        internal static int     MaxInt<Inner>(in Inner inner) where Inner : INode<int>      => inner.CreateViaPush<int,     MaxInt, Construct<MaxInt, MaxInt.Args>>(default(ChainOptions), new Construct<MaxInt, MaxInt.Args>(new MaxInt.Args(true)));
        internal static long    MaxLong<Inner>(in Inner inner) where Inner : INode<long>     => inner.CreateViaPush<long,    MaxLong, Construct<MaxLong, MaxLong.Args>>(default(ChainOptions), new Construct<MaxLong, MaxLong.Args>(new MaxLong.Args(true)));
        internal static T?      Max<T, Inner>(in Inner inner) where Inner : INode<T>        => inner.CreateViaPush<T?,      Max<T>, Construct<Max<T>, Max<T>.Args>>(default(ChainOptions), new Construct<Max<T>, Max<T>.Args>(new Max<T>.Args(true)));

        // POTENTIAL OPTIMIZATION: the Extremum request above, answered from the other end
        internal static T? Max<T, Inner>(in Inner inner, IComparer<T>? comparer) where Inner : INode<T> =>
            inner.CreateViaPush<T?, MaxComparer<T>, Construct<MaxComparer<T>, MaxComparer<T>.Args>>(default(ChainOptions), new Construct<MaxComparer<T>, MaxComparer<T>.Args>(new MaxComparer<T>.Args(comparer)));

        // POTENTIAL OPTIMIZATION: the Extremum request above under a key selector, from the other end: an OrderBy whose first key is this selector and comparer would answer with one selection skipping null keys, in place of its sort
        internal static T? MaxBy<T, TKey, Inner>(in Inner inner, Func<T, TKey> keySelector, IComparer<TKey>? comparer) where Inner : INode<T> =>
            inner.CreateViaPush<T?, MaxBy<T, TKey>, Construct<MaxBy<T, TKey>, MaxBy<T, TKey>.Args>>(default(ChainOptions), new Construct<MaxBy<T, TKey>, MaxBy<T, TKey>.Args>(new MaxBy<T, TKey>.Args(keySelector, comparer)));

        internal static decimal? MaxNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal?, MaxDecimalNullable, Construct<MaxDecimalNullable, MaxDecimalNullable.Args>>(default(ChainOptions), new Construct<MaxDecimalNullable, MaxDecimalNullable.Args>(new MaxDecimalNullable.Args(true)));
        internal static double?  MaxNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double?,  MaxDoubleNullable, Construct<MaxDoubleNullable, MaxDoubleNullable.Args>>(default(ChainOptions), new Construct<MaxDoubleNullable, MaxDoubleNullable.Args>(new MaxDoubleNullable.Args(true)));
        internal static float?   MaxNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float?,   MaxFloatNullable, Construct<MaxFloatNullable, MaxFloatNullable.Args>>(default(ChainOptions), new Construct<MaxFloatNullable, MaxFloatNullable.Args>(new MaxFloatNullable.Args(true)));
        internal static int?     MaxNullableInt<Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<int?,     MaxIntNullable, Construct<MaxIntNullable, MaxIntNullable.Args>>(default(ChainOptions), new Construct<MaxIntNullable, MaxIntNullable.Args>(new MaxIntNullable.Args(true)));
        internal static long?    MaxNullableLong<Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<long?,    MaxLongNullable, Construct<MaxLongNullable, MaxLongNullable.Args>>(default(ChainOptions), new Construct<MaxLongNullable, MaxLongNullable.Args>(new MaxLongNullable.Args(true)));

        internal static decimal SumDecimal<Inner>(in Inner inner) where Inner : INode<decimal> => inner.CreateViaPush<decimal, SumDecimal, Construct<SumDecimal, SumDecimal.Args>>(default(ChainOptions), new Construct<SumDecimal, SumDecimal.Args>(new SumDecimal.Args(SIMDOptions.OnlyIfSame)));
        internal static double  SumDouble<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<double>  => inner.CreateViaPush<double,  SumDouble, Construct<SumDouble, SumDouble.Args>>(default(ChainOptions), new Construct<SumDouble, SumDouble.Args>(new SumDouble.Args(simdOptions)));
        internal static float   SumFloat<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<float>   => inner.CreateViaPush<float,   SumFloat, Construct<SumFloat, SumFloat.Args>>(default(ChainOptions), new Construct<SumFloat, SumFloat.Args>(new SumFloat.Args(simdOptions)));
        internal static int     SumInt<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<int>     => inner.CreateViaPush<int,     SumInt, Construct<SumInt, SumInt.Args>>(default(ChainOptions), new Construct<SumInt, SumInt.Args>(new SumInt.Args(simdOptions)));
        internal static long    SumLong<Inner>(in Inner inner, SIMDOptions simdOptions) where Inner : INode<long>    => inner.CreateViaPush<long,    SumLong, Construct<SumLong, SumLong.Args>>(default(ChainOptions), new Construct<SumLong, SumLong.Args>(new SumLong.Args(simdOptions)));

        internal static decimal? SumNullableDecimal<Inner>(in Inner inner) where Inner : INode<decimal?> => inner.CreateViaPush<decimal, SumDecimalNullable, Construct<SumDecimalNullable, SumDecimalNullable.Args>>(default(ChainOptions), new Construct<SumDecimalNullable, SumDecimalNullable.Args>(new SumDecimalNullable.Args(true)));
        internal static double?  SumNullableDouble<Inner>(in Inner inner) where Inner : INode<double?>  => inner.CreateViaPush<double,  SumDoubleNullable, Construct<SumDoubleNullable, SumDoubleNullable.Args>>(default(ChainOptions), new Construct<SumDoubleNullable, SumDoubleNullable.Args>(new SumDoubleNullable.Args(true)));
        internal static float?   SumNullableFloat<Inner>(in Inner inner) where Inner : INode<float?>   => inner.CreateViaPush<float,   SumFloatNullable, Construct<SumFloatNullable, SumFloatNullable.Args>>(default(ChainOptions), new Construct<SumFloatNullable, SumFloatNullable.Args>(new SumFloatNullable.Args(true)));
        internal static int?     SumNullableInt<Inner>(in Inner inner) where Inner : INode<int?>     => inner.CreateViaPush<int,     SumIntNullable, Construct<SumIntNullable, SumIntNullable.Args>>(default(ChainOptions), new Construct<SumIntNullable, SumIntNullable.Args>(new SumIntNullable.Args(true)));
        internal static long?    SumNullableLong<Inner>(in Inner inner) where Inner : INode<long?>    => inner.CreateViaPush<long,    SumLongNullable, Construct<SumLongNullable, SumLongNullable.Args>>(default(ChainOptions), new Construct<SumLongNullable, SumLongNullable.Args>(new SumLongNullable.Args(true)));

        internal static int Count<T, Inner>(in Inner source, Func<T, bool> predicate)
            where Inner : INode<T>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return source.CreateViaPush<int, CountIf<T>, Construct<CountIf<T>, CountIf<T>.Args>>(default(ChainOptions), new Construct<CountIf<T>, CountIf<T>.Args>(new CountIf<T>.Args(predicate)));
        }

        internal static bool Contains<T, Inner>(Inner inner, T value) where Inner : INode<T>
        {
            if (inner.TryPushOptimization<Optimizations.Contains<T>, bool>(new Optimizations.Contains<T>(value), out var found))
                return found;

            return inner.CreateViaPush<bool, Contains<T>, Construct<Contains<T>, Contains<T>.Args>>(default(ChainOptions), new Construct<Contains<T>, Contains<T>.Args>(new Contains<T>.Args(value)));
        }
        internal static bool Contains<T, Inner>(in Inner inner, T value, IEqualityComparer<T>? comparer) where Inner : INode<T> =>
            inner.CreateViaPush<bool, ContainsByComparer<T>, Construct<ContainsByComparer<T>, ContainsByComparer<T>.Args>>(default(ChainOptions), new Construct<ContainsByComparer<T>, ContainsByComparer<T>.Args>(new ContainsByComparer<T>.Args(comparer, value)));

        // POTENTIAL OPTIMIZATION: two lengths known without enumerating (CountInformation) that differ answer false, as System.Linq answers two ICollection<T>s of different Count; CountInformation cannot be used as it is, since OrderBy and Cast report their source's length with no PotentialSideEffects while running a key selector or a cast that System.Linq would run, so it needs a flag for a length reached with no user code
        internal static bool SequenceEqual<T, First, Second, SecondEnumerator>(in First first, in Second second, IEqualityComparer<T>? comparer)
            where First : INode<T>
            where Second : INode<T>
            where SecondEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<Second, SecondEnumerator>
        {
            var firstSpannable = first;
            var secondSpannable = second;

            if (Optimizations.AsSpanSource.TryGet<T, First>(ref firstSpannable, out var firstSource)
                && Optimizations.AsSpanSource.TryGet<T, Second>(ref secondSpannable, out var secondSource))
            {
                var firstSpan = firstSource.GetSpan();
                var secondSpan = secondSource.GetSpan();
                var spansEqual = MemoryExtensions.SequenceEqual(firstSpan, secondSpan, comparer);

                return spansEqual;
            }

            var secondEnumerator = PullEnumerator.Create<T, Second, SecondEnumerator>(in second);

            try
            {
                var equal = first.CreateViaPush<bool, SequenceEqual<T>, Construct<SequenceEqual<T>, SequenceEqual<T>.Args>>(default(ChainOptions), new Construct<SequenceEqual<T>, SequenceEqual<T>.Args>(new SequenceEqual<T>.Args(secondEnumerator, comparer)));

                return equal;
            }
            finally
            {
                secondEnumerator.Dispose();
            }
        }


        internal static Nodes.SelectMany.Node<TResult,NodeU, Nodes.Select.Node<TSource, NodeU, NodeT>> SelectMany<TSource, TResult, NodeT, NodeU>(in NodeT prior, Func<TSource, NodeU> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new (new(prior, selector));
        }
        internal static Nodes.SelectMany.Node<TResult,NodeU, Nodes.SelectIdx.Node<TSource, NodeU, NodeT>> SelectMany<TSource, TResult, NodeT, NodeU>(in NodeT prior, Func<TSource, int, NodeU> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(prior, selector));
        }
        // an inner sequence met as IEnumerable<T>; the functor keeps the query free of a per-call closure
        internal static Nodes.SelectMany.Node<TResult,SystemIEnumerable.Node<TResult>, Nodes.ValueSelect.Node<TSource, SystemIEnumerable.Node<TResult>, NodeT, EnumerableNodeSelector<TSource, TResult>>> SelectManyEnumerable<TSource, TResult, NodeT>(in NodeT prior, Func<TSource, IEnumerable<TResult>> selector)
            where NodeT : INode<TSource>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(in prior, new EnumerableNodeSelector<TSource, TResult>(selector)));
        }

        internal static Nodes.SelectMany.Node<TResult,SystemIEnumerable.Node<TResult>, Nodes.SelectIdx.Node<TSource, SystemIEnumerable.Node<TResult>, NodeT>> SelectManyEnumerable<TSource, TResult, NodeT>(in NodeT prior, Func<TSource, int, IEnumerable<TResult>> selector)
            where NodeT : INode<TSource>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(in prior, (source, idx) => new SystemIEnumerable.Node<TResult>(selector(source, idx))));
        }

        internal static Nodes.SelectMany.Node<TResult,SystemArray.Node<TResult>, Nodes.ValueSelect.Node<TSource, SystemArray.Node<TResult>, NodeT, ArrayNodeSelector<TSource, TResult>>> SelectManyArray<TSource, TResult, NodeT>(in NodeT prior, Func<TSource, TResult[]> selector)
            where NodeT : INode<TSource>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new(new(in prior, new ArrayNodeSelector<TSource, TResult>(selector)));
        }

        internal static Nodes.SelectManyResult.Node<TSource, TCollection, TResult,SystemIEnumerable.Node<TCollection>, Nodes.Select.Node<TSource, (TSource, SystemIEnumerable.Node<TCollection>), NodeT>> SelectManyEnumerable<TSource, TCollection, TResult, NodeT>(in NodeT prior, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new(new(in prior, source => (source, new SystemIEnumerable.Node<TCollection>(collectionSelector(source)))), resultSelector);
        }

        internal static Nodes.SelectManyResult.Node<TSource, TCollection, TResult,SystemIEnumerable.Node<TCollection>, Nodes.SelectIdx.Node<TSource, (TSource, SystemIEnumerable.Node<TCollection>), NodeT>> SelectManyEnumerable<TSource, TCollection, TResult, NodeT>(in NodeT prior, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new(new(in prior, (source, idx) => (source, new SystemIEnumerable.Node<TCollection>(collectionSelector(source, idx)))), resultSelector);
        }

        // an inner sequence that is a Cistern chain, whose node type is known, so its push is fused into SelectMany's
        internal static Nodes.SelectManyResult.Node<TSource, TCollection, TResult,ValueEnumerable<TCollection, NodeU, NodeUEnumerator>, Nodes.Select.Node<TSource, (TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>), NodeT>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU, NodeUEnumerator>(in NodeT prior, Func<TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
            where NodeUEnumerator : struct, IPullEnumerator<TCollection>, INodeEnumerator<NodeU, NodeUEnumerator>
        {
            if (collectionSelector == null)
                throw new ArgumentNullException(nameof(collectionSelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            return new(new(in prior, source => (source, collectionSelector(source))), resultSelector);
        }

        internal static CisternConcat.Node<T, First, Second> Concat<T, First, Second>(in First first, in Second second)
            where First : INode<T>
            where Second : INode<T>
            => new CisternConcat.Node<T, First, Second>(in first, in second);

        internal static Nodes.AppendPrepend.Node<T, Inner> Append<T, Inner>(in Inner source, T element)
            where Inner : INode<T>
            => new Nodes.AppendPrepend.Node<T, Inner>(in source, default(Nodes.AppendPrepend.Elements<T>).Append(element));

        internal static Nodes.AppendPrepend.Node<T, Inner> Prepend<T, Inner>(in Inner source, T element)
            where Inner : INode<T>
            => new Nodes.AppendPrepend.Node<T, Inner>(in source, default(Nodes.AppendPrepend.Elements<T>).Prepend(element));

        internal static Nodes.Reverse.Node<TSource, Inner> Reverse<TSource, Inner>(in Inner source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            => new Nodes.Reverse.Node<TSource, Inner>(in source, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        internal static Nodes.Shuffle.Node<TSource, Inner> Shuffle<TSource, Inner>(in Inner source)
            where Inner : INode<TSource>
            => new Nodes.Shuffle.Node<TSource, Inner>(in source);

        internal static Nodes.ValueSelect.Node<T, U, TPrior, IFunc> Select<T, U, TPrior, IFunc>(in TPrior prior, IFunc selector, U? u = default)
            where TPrior : INode<T>
            where IFunc : IFunc<T, U>
        {
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new Nodes.ValueSelect.Node<T, U, TPrior, IFunc>(in prior, selector);
        }
        internal static Nodes.ValueWhere.Node<T, TPrior, Predicate> Where<T, TPrior, Predicate>(in TPrior prior, Predicate predicate)
            where TPrior : INode<T>
            where Predicate : IFunc<T, bool>
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return new Nodes.ValueWhere.Node<T, TPrior, Predicate>(in prior, predicate);
        }

        public static Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior> OrderBy<TSource, TKey, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
            where TPrior : INode<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return new Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>(in prior, new Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>(new Nodes.OrderBy.KeySelectorsRoot<TSource>(), keySelector, comparer ?? Comparer<TKey>.Default, descending));
        }

        // POTENTIAL OPTIMIZATION: an IKeySelectors of its own for the identity key, the keys copied from the elements with no delegate call per element, for the types not sorted directly (OrderBy.Node.SortInPlace)
        internal static Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TSource, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior> Order<TSource, TPrior>(in TPrior prior, IComparer<TSource>? comparer, bool descending)
            where TPrior : INode<TSource>
            => OrderBy(in prior, Nodes.OrderBy.Identity<TSource>.KeySelector, comparer, descending);

        public static Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorSelector>, TPrior> ThenBy<TSource, TKey, TPriorSelector, TPrior>(in Nodes.OrderBy.Node<TSource, TPriorSelector, TPrior> prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer, bool descending)
            where TPrior : INode<TSource>
            where TPriorSelector: Nodes.OrderBy.IKeySelectors<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            return new Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorSelector>, TPrior>(in prior._nodeT, new Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorSelector>(prior._keySelectors, keySelector, comparer ?? Comparer<TKey>.Default, descending));
        }

        internal static Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>> Zip<TFirst, TSecond, FirstNode, SecondNode, SecondNodeEnumerator>(in FirstNode first, in SecondNode second)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
            => new Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>(in first, in second, default);

        internal static Nodes.Zip.Node<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.FuncSelector<TFirst, TSecond, TResult>> Zip<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator>(in FirstNode first, in SecondNode second, Func<TFirst, TSecond, TResult> resultSelector)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
        {
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var selector = new Nodes.Zip.FuncSelector<TFirst, TSecond, TResult>(resultSelector);

            var zip = new Nodes.Zip.Node<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.FuncSelector<TFirst, TSecond, TResult>>(in first, in second, selector);

            return zip;
        }

        internal static Nodes.Zip.Node<(TFirst, TSecond), TThird, (TFirst, TSecond, TThird), Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>, ThirdNode, ThirdNodeEnumerator, Nodes.Zip.TripleSelector<TFirst, TSecond, TThird>> Zip<TFirst, TSecond, TThird, FirstNode, SecondNode, ThirdNode, SecondNodeEnumerator, ThirdNodeEnumerator>(in FirstNode first, in SecondNode second, in ThirdNode third)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where ThirdNode : INode<TThird>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
            where ThirdNodeEnumerator : struct, IPullEnumerator<TThird>, INodeEnumerator<ThirdNode, ThirdNodeEnumerator>
        {
            // POTENTIAL OPTIMIZATION: three sources answering AsSpanSource zipped in one loop, as FastEnumerate zips two; the outer node pulls the third through its heap enumerator
            var firstTwo = Zip<TFirst, TSecond, FirstNode, SecondNode, SecondNodeEnumerator>(in first, in second);

            var zip = new Nodes.Zip.Node<(TFirst, TSecond), TThird, (TFirst, TSecond, TThird), Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>, ThirdNode, ThirdNodeEnumerator, Nodes.Zip.TripleSelector<TFirst, TSecond, TThird>>(in firstTwo, in third, default);

            return zip;
        }

        internal static (U, V) Fork<T, U, V, Inner>(in Inner inner, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, U> getFirstValue, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, V> getSecondValue)
            where Inner : INode<T>
        {
            var fork = new Fork<T, U, V, Inner>(inner, getSecondValue);

            var u = getFirstValue(new (fork));
            var v = fork.SecondValue;

            return (u, v);
        }
        internal static (U, V, W) Fork<T, U, V, W, Inner>(in Inner inner, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, U> getFirstValue, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, V> getSecondValue, Func<ValueEnumerable<T, Fork<T>, ForkEnumerator<T>>, W> getThirdValue)
            where Inner : INode<T>
        {
            var forkInner = new Fork<T, (U, V), W, Inner>(inner, getThirdValue);
            var forkOuter = new Fork<T, U, V, Fork<T, (U, V), W, Inner>>(forkInner, getSecondValue);

            var u = getFirstValue(new (forkOuter));
            var v = forkOuter.SecondValue;
            var w = forkInner.SecondValue;

            return (u, v, w);
        }

        internal static Nodes.Except.Node<TSource, TSource, TPrior, Nodes.Except.IdentityKey<TSource>> Except<TSource, TPrior>(in TPrior prior, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
            where TPrior : INode<TSource>
            // ExceptNode's constructor validates second
            => new Nodes.Except.Node<TSource, TSource, TPrior, Nodes.Except.IdentityKey<TSource>>(prior, second, comparer, default);

        internal static Nodes.Except.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>> DistinctBy<TSource, TKey, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var keys = new FuncToIFunc<TSource, TKey>(keySelector);
            var second = Array.Empty<TKey>();

            var node = new Nodes.Except.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>>(prior, second, comparer, keys);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>> ExceptBy<TSource, TKey, TPrior>(in TPrior prior, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var keys = new FuncToIFunc<TSource, TKey>(keySelector);

            var node = new Nodes.Except.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>>(prior, second, comparer, keys);

            return node;
        }

        internal static Nodes.Intersect.Node<TSource, TSource, TPrior, Nodes.Except.IdentityKey<TSource>> Intersect<TSource, TPrior>(in TPrior prior, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
            where TPrior : INode<TSource>
        {
            var node = new Nodes.Intersect.Node<TSource, TSource, TPrior, Nodes.Except.IdentityKey<TSource>>(in prior, second, comparer, default);

            return node;
        }

        internal static Nodes.Intersect.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>> IntersectBy<TSource, TKey, TPrior>(in TPrior prior, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var keys = new FuncToIFunc<TSource, TKey>(keySelector);

            var node = new Nodes.Intersect.Node<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>>(in prior, second, comparer, keys);

            return node;
        }

        internal static Nodes.Join.Node<TOuter, TInner, TKey, TResult, TPrior, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>> Join<TOuter, TInner, TKey, TResult, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var innerNode = new SystemIEnumerable.Node<TInner>(inner);
            var selector = new Nodes.Zip.FuncSelector<TOuter, TInner, TResult>(resultSelector);

            var node = new Nodes.Join.Node<TOuter, TInner, TKey, TResult, TPrior, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>(in prior, innerNode, outerKeySelector, innerKeySelector, comparer, selector);

            return node;
        }

        internal static Nodes.LeftJoin.Node<TOuter, TInner, TKey, TResult, TPrior, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>> LeftJoin<TOuter, TInner, TKey, TResult, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var innerNode = new SystemIEnumerable.Node<TInner>(inner);
            var lookupSource = new Nodes.LeftJoin.LookupSource<TInner, TKey, SystemIEnumerable.Node<TInner>>(in innerNode, innerKeySelector, comparer);
            var selector = new Nodes.Zip.FuncSelector<TOuter, TInner, TResult>(resultSelector);

            var node = new Nodes.LeftJoin.Node<TOuter, TInner, TKey, TResult, TPrior, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>(in prior, in lookupSource, outerKeySelector, selector);

            return node;
        }

        internal static Nodes.LeftJoin.Node<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, TPrior, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>> RightJoin<TOuter, TInner, TKey, TResult, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var innerNode = new SystemIEnumerable.Node<TInner>(inner);
            var lookupSource = new Nodes.LeftJoin.LookupSource<TOuter, TKey, TPrior>(in prior, outerKeySelector, comparer);
            var selector = new Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>(resultSelector);

            var node = new Nodes.LeftJoin.Node<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, TPrior, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>>(in innerNode, in lookupSource, innerKeySelector, selector);

            return node;
        }

        internal static Nodes.FullJoin.Node<TOuter, TInner, TKey, TResult, TPrior, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>> FullJoin<TOuter, TInner, TKey, TResult, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var selector = new Nodes.Zip.FuncSelector<TOuter, TInner, TResult>(resultSelector);

            return FullJoin<TOuter, TInner, TKey, TResult, TPrior, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>(in prior, inner, outerKeySelector, innerKeySelector, in selector, comparer);
        }

        internal static Nodes.FullJoin.Node<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), TPrior, Nodes.FullJoin.PairSelector<TOuter, TInner>> FullJoin<TOuter, TInner, TKey, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));

            return FullJoin<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), TPrior, Nodes.FullJoin.PairSelector<TOuter, TInner>>(in prior, inner, outerKeySelector, innerKeySelector, default(Nodes.FullJoin.PairSelector<TOuter, TInner>), comparer);
        }

        private static Nodes.FullJoin.Node<TOuter, TInner, TKey, TResult, TPrior, TSelector> FullJoin<TOuter, TInner, TKey, TResult, TPrior, TSelector>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, in TSelector selector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
            where TSelector : struct, Nodes.Zip.ISelector<TOuter, TInner, TResult>
        {
            // an outer sequence of no elements and no user code, as System.Linq's empty array, joins inner to nothing
            prior.GetCountInformation(out var outerInfo);
            var emptySide =
                outerInfo.ActualSize == 0 && outerInfo.LengthIsImmutable && !outerInfo.PotentialSideEffects ? Nodes.FullJoin.OuterEmpty
                : inner is TInner[] { Length: 0 } ? Nodes.FullJoin.InnerEmpty
                : Nodes.FullJoin.NeitherEmpty;

            var innerNode = new SystemIEnumerable.Node<TInner>(inner);

            var node = new Nodes.FullJoin.Node<TOuter, TInner, TKey, TResult, TPrior, TSelector>(in prior, innerNode, outerKeySelector, innerKeySelector, comparer, selector, emptySide);

            return node;
        }

        internal static Nodes.GroupJoin.Node<TOuter, TInner, TKey, TResult, TPrior> GroupJoin<TOuter, TInner, TKey, TResult, TPrior>(in TPrior prior, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TOuter>
        {
            if (inner == null)
                throw new ArgumentNullException(nameof(inner));
            if (outerKeySelector == null)
                throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null)
                throw new ArgumentNullException(nameof(innerKeySelector));
            if (resultSelector == null)
                throw new ArgumentNullException(nameof(resultSelector));

            var innerNode = new SystemIEnumerable.Node<TInner>(inner);
            var lookupSource = new Nodes.LeftJoin.LookupSource<TInner, TKey, SystemIEnumerable.Node<TInner>>(in innerNode, innerKeySelector, comparer);

            var node = new Nodes.GroupJoin.Node<TOuter, TInner, TKey, TResult, TPrior>(in prior, in lookupSource, outerKeySelector, resultSelector);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, TPrior, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>> Union<TSource, TPrior>(in TPrior prior, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
            where TPrior : INode<TSource>
        {
            var node =
                UnionByKey<TSource, TSource, TPrior, Nodes.Except.IdentityKey<TSource>>(in prior, second, comparer, default);

            return node;
        }

        // System.Linq's Union of a Union under an equal comparer unions the parts of both under the inner one's comparer,
        // so each element is hashed once and a chain of Unions is flattened as a chain of Concats is; out of line, as the
        // IEnumerable<T> Union holds it as one arm of its switch
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal static Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>> UnionOfEnumerables<TSource>(in ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
        {
            ref readonly var union = ref first.Node;

            if (union._second is TSource[] { Length: 0 }
                && AreEqualityComparersEqual(comparer, union._comparer))
            {
                IEnumerable<TSource> parts =
                    new ValueEnumerable<TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>>(in union._nodeT);

                var partsNode =
                    Enumerable.ToNode(parts);

                var flattened =
                    Union<TSource, SystemIEnumerable.Node<TSource>>(partsNode, second, union._comparer);

                return flattened;
            }

            IEnumerable<TSource> unioned = first;

            var unionedNode =
                Enumerable.ToNode(unioned);

            var node =
                Union<TSource, SystemIEnumerable.Node<TSource>>(unionedNode, second, comparer);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, TPrior, SystemIEnumerable.Node<TSource>>, FuncToIFunc<TSource, TKey>> UnionBy<TSource, TKey, TPrior>(in TPrior prior, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var keys =
                new FuncToIFunc<TSource, TKey>(keySelector);

            var node =
                UnionByKey<TSource, TKey, TPrior, FuncToIFunc<TSource, TKey>>(in prior, second, comparer, keys);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, TPrior, SystemIEnumerable.Node<TSource>>, TKeySelector> UnionByKey<TSource, TKey, TPrior, TKeySelector>(in TPrior prior, IEnumerable<TSource> second, IEqualityComparer<TKey>? comparer, TKeySelector keySelector)
            where TPrior : INode<TSource>
            where TKeySelector : struct, IFunc<TSource, TKey>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var secondNode =
                Enumerable.ToNode(second);

            var node =
                UnionByKey<TSource, TKey, TPrior, SystemIEnumerable.Node<TSource>, TKeySelector>(in prior, in secondNode, comparer, keySelector);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, TPrior, TSecond>, Nodes.Except.IdentityKey<TSource>> Union<TSource, TPrior, TSecond>(in TPrior prior, in TSecond second, IEqualityComparer<TSource>? comparer)
            where TPrior : INode<TSource>
            where TSecond : INode<TSource>
        {
            var node =
                UnionByKey<TSource, TSource, TPrior, TSecond, Nodes.Except.IdentityKey<TSource>>(in prior, in second, comparer, default);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, TPrior, TSecond>, FuncToIFunc<TSource, TKey>> UnionBy<TSource, TKey, TPrior, TSecond>(in TPrior prior, in TSecond second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
            where TSecond : INode<TSource>
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var keys =
                new FuncToIFunc<TSource, TKey>(keySelector);

            var node =
                UnionByKey<TSource, TKey, TPrior, TSecond, FuncToIFunc<TSource, TKey>>(in prior, in second, comparer, keys);

            return node;
        }

        internal static Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, TPrior, TSecond>, TKeySelector> UnionByKey<TSource, TKey, TPrior, TSecond, TKeySelector>(in TPrior prior, in TSecond second, IEqualityComparer<TKey>? comparer, TKeySelector keySelector)
            where TPrior : INode<TSource>
            where TSecond : INode<TSource>
            where TKeySelector : struct, IFunc<TSource, TKey>
        {
            var parts =
                new CisternConcat.Node<TSource, TPrior, TSecond>(in prior, in second);

            var excluded = Array.Empty<TKey>();

            var node =
                new Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, TPrior, TSecond>, TKeySelector>(parts, excluded, comparer, keySelector);

            return node;
        }

        // a Union of a Union whose parts are typed: as UnionOfEnumerables, the parts are handed on as one sequence, so
        // both outcomes have the one type
        internal static Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>> UnionOfUnion<TSource, First, Second, FirstEnumerator, SecondEnumerator>(in ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, First, Second>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, First, Second>, CisternConcat.Pull<TSource, First, Second, FirstEnumerator, SecondEnumerator>, Nodes.Except.IdentityKey<TSource>>> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
            where First : INode<TSource>
            where Second : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            where SecondEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Second, SecondEnumerator>
        {
            ref readonly var union = ref first.Node;

            if (union._second is TSource[] { Length: 0 }
                && AreEqualityComparersEqual(comparer, union._comparer))
            {
                IEnumerable<TSource> parts =
                    new ValueEnumerable<TSource, CisternConcat.Node<TSource, First, Second>, CisternConcat.Pull<TSource, First, Second, FirstEnumerator, SecondEnumerator>>(in union._nodeT);

                var partsNode =
                    Enumerable.ToNode(parts);

                var flattened =
                    Union<TSource, SystemIEnumerable.Node<TSource>>(partsNode, second, union._comparer);

                return flattened;
            }

            IEnumerable<TSource> unioned = first;

            var unionedNode =
                Enumerable.ToNode(unioned);

            var node =
                Union<TSource, SystemIEnumerable.Node<TSource>>(unionedNode, second, comparer);

            return node;
        }

        private static bool AreEqualityComparersEqual<T>(IEqualityComparer<T>? left, IEqualityComparer<T>? right)
        {
            var defaultComparer = EqualityComparer<T>.Default;

            var equal =
                (left, right) switch
                {
                    (null, null) => true,
                    (null, {} other) => other == defaultComparer || other.Equals(defaultComparer),
                    ({} other, null) => other == defaultComparer || other.Equals(defaultComparer),
                    ({} one, {} another) => one == another || one.Equals(another),
                };

            return equal;
        }

        internal static Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, TPrior> AggregateBy<TSource, TKey, TAccumulate, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer)
            where TPrior : INode<TSource>
            where TKey : notnull
        {
            var node = new Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, TPrior>(in prior, keySelector, seed, func, keyComparer);

            return node;
        }

        internal static Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, TPrior> AggregateBy<TSource, TKey, TAccumulate, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer)
            where TPrior : INode<TSource>
            where TKey : notnull
        {
            var node = new Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, TPrior>(in prior, keySelector, seedSelector, func, keyComparer);

            return node;
        }

        internal static Nodes.DefaultIfEmpty.Node<TSource, TPrior> DefaultIfEmpty<TSource, TPrior>(in TPrior prior, TSource defaultValue)
            where TPrior : INode<TSource>
            => new Nodes.DefaultIfEmpty.Node<TSource, TPrior>(in prior, defaultValue);

        internal static Nodes.Indexed.Node<TSource, TPrior> Indexed<TSource, TPrior>(in TPrior prior)
            where TPrior : INode<TSource>
            => new Nodes.Indexed.Node<TSource, TPrior>(in prior, 0);

        internal static Nodes.Chunk.Node<TSource, TPrior> Chunk<TSource, TPrior>(in TPrior prior, int size)
            where TPrior : INode<TSource>
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

            var node = new Nodes.Chunk.Node<TSource, TPrior>(in prior, size);

            return node;
        }

        internal static Nodes.CountBy.Node<TSource, TKey, TPrior> CountBy<TSource, TKey, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer)
            where TPrior : INode<TSource>
            where TKey : notnull
        {
            var node = new Nodes.CountBy.Node<TSource, TKey, TPrior>(in prior, keySelector, keyComparer);

            return node;
        }

        public static Nodes.GroupBy.Node<TSource, TKey, TPrior> GroupBy<TSource, TKey, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return new (prior, keySelector, comparer);
        }
        public static Nodes.GroupBy.Node<TSource, TKey, TElement, TPrior> GroupBy<TSource, TKey, TElement, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return new(prior, keySelector, elementSelector, comparer);
        }
        public static Nodes.GroupByResult.Node<TSource, TKey, TResult, TPrior> GroupBy<TSource, TKey, TResult, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return new(prior, keySelector, resultSelector, comparer);
        }
        public static Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, TPrior> GroupBy<TSource, TKey, TElement, TResult, TPrior>(in TPrior prior, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return new(prior, keySelector, elementSelector, resultSelector, comparer);
        }

        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey, TPrior>(in TPrior source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return source.CreateViaPush<Nodes.GroupBy.Lookup<TKey, TSource>, ToLookup<TSource, TKey>, Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey>, ToLookup<TSource, TKey>.Args>(new ToLookup<TSource, TKey>.Args(comparer, keySelector)));
        }
        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement, TPrior>(in TPrior source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
            where TPrior : INode<TSource>
        {
            return source.CreateViaPush<Nodes.GroupBy.Lookup<TKey, TElement>, ToLookup<TSource, TKey, TElement>, Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>>(default(ChainOptions), new Construct<ToLookup<TSource, TKey, TElement>, ToLookup<TSource, TKey, TElement>.Args>(new ToLookup<TSource, TKey, TElement>.Args(comparer, keySelector, elementSelector)));
        }
    }
}
