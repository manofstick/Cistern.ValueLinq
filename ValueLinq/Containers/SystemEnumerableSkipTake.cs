namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A slice of a source that cannot be sliced, SystemIEnumerable.Node's answer to the SkipTake request for one: the first _skip
// elements of the enumerator dropped, then at most _take of the rest pushed. Held in a NodeContainer as the enumerable
// with a start and count, so a further slice composes into it, and a Skip or Take above has nothing to do in the push.
public static partial class SystemEnumerableSkipTake
{
    [Answerer]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationToArray<T>
    {
        internal readonly IEnumerable<T> _enumerable;
        internal readonly long _skip;
        internal readonly long _take;

        public Node(IEnumerable<T> enumerable, long skip, long take)
        {
            _enumerable = enumerable;
            _skip = skip;
            _take = take;
        }

        public void GetCountInformation(out CountInformation info)
        {
            SystemIEnumerable.GetCountInformation(_enumerable, out info);

            info.MaximumLength =
                info.MaximumLength is long length
                    ? Math.Min(Math.Max(0, length - _skip), _take)
                    : _take == long.MaxValue ? null : _take;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Optimizations.SkipTake.Compose(_skip, _take, request.Skip, request.Take, out var skip, out var take);
            Unsafe.SkipInit(out response);
            Answer(_enumerable, skip, take, ref response);

            return true;
        }

        // answered before the CopyTo dry run and the push, which would each reach this node again only to decline
        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = SystemEnumerableSkipTake.ToArray(_enumerable, _skip, _take);
            return true;
        }

        internal TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct =>
            ExecutePush<T, TPushResult, TPushEnumerator>(_enumerable, _skip, _take, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    // nothing is left past a length the source knows, so the source is not created; a take of long.MaxValue is all of
    // it
    internal static void Answer<T>(IEnumerable<T> enumerable, long skip, long take, ref NodeContainer<T> container)
    {
        SystemIEnumerable.GetCountInformation(enumerable, out var info);
        if (take == 0 || skip >= info.MaximumLength)
        {
            container.SetEmpty();
            return;
        }

        container.SetNode(new Node<T>(enumerable, skip, take));
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(IEnumerable<TSource> enumerable, long skip, long take, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        IEnumerator<TSource> enumerator;
        try
        {
            enumerator = enumerable.GetEnumerator();
        }
        catch
        {
            fenum.Dispose();
            throw;
        }

        try
        {
            Loop(enumerator, skip, take, ref fenum);

            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static T[] ToArray<T>(IEnumerable<T> enumerable, long skip, long take)
    {
        using var e = enumerable.GetEnumerator();

        for (var i = 0L; i < skip; ++i)
        {
            if (!e.MoveNext())
                return Array.Empty<T>();
        }

        var builder = new SegmentedArrayBuilder<T>();
        try
        {
            while (take > 0 && e.MoveNext())
            {
                builder.Add(e.Current);
                --take;
            }

            return builder.ToArray();
        }
        finally
        {
            builder.Dispose();
        }
    }

    private static void Loop<TSource, TPushEnumerator>(IEnumerator<TSource> e, long skip, long take, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        using (e)
        {
            for (var i = 0L; i < skip; ++i)
            {
                if (!e.MoveNext())
                    return;
            }

            while (take > 0 && e.MoveNext())
            {
                if (!fenum.ProcessNext(e.Current))
                    return;

                --take;
            }
        }
    }
}
