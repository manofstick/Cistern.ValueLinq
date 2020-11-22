using Cistern.ValueLinq.Aggregation;
using Cistern.ValueLinq.Utils;
using System;
using System.Buffers;

namespace Cistern.ValueLinq
{
    public static partial class NodeImpl
    {
        internal static int Count<T, Inner>(in Inner inner, bool ignorePotentialSideEffects) where Inner : INode<T>
        {
            inner.GetCountInformation(out var countInfo);
            if (!countInfo.IsStale && (ignorePotentialSideEffects || !countInfo.PotentialSideEffects) && countInfo.ActualLengthIsMaximumLength && countInfo.MaximumLength.HasValue && countInfo.MaximumLength.Value <= int.MaxValue)
                return (int)countInfo.MaximumLength.Value;

            return inner.CheckForOptimization<Optimizations.Count, int>(new Optimizations.Count { IgnorePotentialSideEffects = ignorePotentialSideEffects }, out var count) switch
            {
                false => inner.CreateObjectViaFastEnumerator<int, Aggregation.Count<T>>(new Aggregation.Count<T>()),
                true => count
            };
        }

        public static T[] ToArray<T, Inner>(in Inner inner, int? maybeMaxCountForStackBasedPath, in (ArrayPool<T> arrayPool, bool cleanBuffers)? arrayPoolInfo)
                where Inner : INode<T>
        {
            inner.GetCountInformation(out var info);

            if (info.ActualLengthIsMaximumLength)
            {
                if (info.MaximumLength == 0)
                    return Array.Empty<T>();

                return inner.CreateObjectViaFastEnumerator<T[], ToArrayForward<T>>(new ToArrayForward<T>(info.ActualSize.Value));
            }

            if (!arrayPoolInfo.HasValue)
            {
                if (info.MaximumLength <= maybeMaxCountForStackBasedPath.GetValueOrDefault())
                    return Nodes<T[]>.Aggregation<Inner, ToArrayViaStackAndGarbage<T>>(in inner, new ToArrayViaStackAndGarbage<T>(maybeMaxCountForStackBasedPath.Value));

                return inner.CreateObjectViaFastEnumerator<T[], ToArrayViaAllocatorForward<T, GarbageCollectedAllocator<T>>>(new ToArrayViaAllocatorForward<T, GarbageCollectedAllocator<T>>(default, 0, null));
            }

            return inner.CreateObjectViaFastEnumerator<T[], ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>>(new ToArrayViaAllocatorForward<T, ArrayPoolAllocator<T>>(new ArrayPoolAllocator<T>(arrayPoolInfo.Value.arrayPool, arrayPoolInfo.Value.cleanBuffers), 0, info.ActualSize));
        }
    }
}
