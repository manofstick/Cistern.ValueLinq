namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// A source held as IEnumerable<T>, whose shape is found at run time: each use dispatches on what it turns out to be, an
// array, a List<T>, a Cistern chain, a collection with a struct enumerator, and pulls or pushes it as that.
public static partial class SystemIEnumerable
{
    public readonly struct Node<T>
        : IErasedNode<T>
    {
        internal readonly IEnumerable<T> _enumerable;

        public void GetCountInformation(out CountInformation info) =>
            SystemIEnumerable.GetCountInformation<T>(_enumerable, out info);

        public Node(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _enumerable = source;
        }

        PullEnumerator<T> IErasedNode<T>.CreatePullEnumerator() =>
            SystemIEnumerable.CreatePullEnumerator(_enumerable);

        readonly bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result) =>
            SystemIEnumerable.TryPushOptimization<T, TRequest, TRequestResult>(_enumerable, in request, out result);

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            ExecutePush<T, TPushResult, TPushEnumerator>(in options, _enumerable, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    // also the enumerator of a Node whose chain Rebuild rebuilt for a source with no container of its own, which
    // is then pulled through its IEnumerator<T> typed in the chain
    [Puller]
    internal partial struct Pull<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public Pull(IEnumerator<T> enumerator) => _enumerator = enumerator;

        private static Pull<T> Create(in Node<T> node)
            => new(node._enumerable.GetEnumerator());

        public void Dispose() { _enumerator.Dispose(); }

        public bool MoveNext() => _enumerator.MoveNext();

        public T Current => _enumerator.Current;

        // a wrapper can hand out a Cistern chain's own enumerator, which pushes what it can rather than being pulled
        bool IPullEnumerator<T>.TryPushRest(ErasedChain.PushSink<T> sink)
        {
            if (_enumerator is not PullEnumerator<T> pull)
                return false;

            pull.PushRest(sink);
            return true;
        }
    }

    // the generic collection interfaces as well as the non-generic one, which a collection can implement without
    // the other, as Count reads them; one sequence of checks, as this is asked of most queries
    internal static void GetCountInformation<T>(IEnumerable<T> enumerable, out CountInformation info)
    {
        if (enumerable is T[] array)
            info = new CountInformation(array.Length, true);
        else if (enumerable is ICollection<T> c)
            info = new CountInformation(c.Count, false);
        else if (enumerable is IReadOnlyCollection<T> r)
            info = new CountInformation(r.Count, false);
        else if (enumerable is System.Collections.ICollection nc)
            info = new CountInformation(nc.Count, false);
        else if (enumerable is INode n)
            n.GetCountInformation(out info);
        else
            info = new CountInformation(null, false);
    }

    internal static void GetCountInformation(System.Collections.IEnumerable _enumerable, out CountInformation info)
    {
        if (_enumerable is System.Collections.ICollection c)
        {
            var lengthIsImmutable = _enumerable is Array;
            info = new CountInformation(c.Count, lengthIsImmutable);
        }
        else if (_enumerable is INode n)
            n.GetCountInformation(out info);
        else
            info = new CountInformation(null, false);
    }

    internal static bool TryPushOptimization<T, TRequest, TResult>(IEnumerable<T> enumerable, in TRequest request, [MaybeNullWhen(false)] out TResult result)
        where TRequest : notnull
    {
        // a Contains is answered by a collection's own lookup and a SplitConcat only by a Cistern node, so neither goes
        // through the casts below that fail for a collection such as a HashSet<T>
        if (typeof(TRequest) == typeof(Optimizations.Contains<T>)
            && enumerable is ICollection<T>)
        {
            return new Vanilla<T>(enumerable).TryPushOptimization<TRequest, TResult>(in request, out result);
        }

        if (typeof(TRequest) == typeof(Optimizations.SplitConcat)
            && enumerable is not INode)
        {
            result = default;
            return false;
        }

        return enumerable switch
        {
            T[] array    => new SystemMemory.Node<T>(array).TryPushOptimization<TRequest, TResult>(in request, out result),
            List<T> list => new SystemList.Node<T>(list).TryPushOptimization<TRequest, TResult>(in request, out result),
            INode node   => node.TryPushOptimization<TRequest, TResult>(in request, out result),
            IList<T> list => new SystemIList.Node<T>(list, 0, list.Count).TryPushOptimization<TRequest, TResult>(in request, out result),
            IReadOnlyList<T> list => new SystemIReadOnlyList.Node<T>(list, 0, list.Count).TryPushOptimization<TRequest, TResult>(in request, out result),
            _            => new Vanilla<T>(enumerable).TryPushOptimization<TRequest, TResult>(in request, out result),
        };
    }

    // the requests a source that is none of the shapes above answers: it is only ever asked, never pushed or pulled
    [Answerer]
    private readonly partial struct Vanilla<T>
        : INode
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationContains<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationAny
    {
        private readonly IEnumerable<T> _enumerable;

        public Vanilla(IEnumerable<T> enumerable) => _enumerable = enumerable;

        public void GetCountInformation(out CountInformation info) =>
            SystemIEnumerable.GetCountInformation<T>(_enumerable, out info);

        // a source that cannot be sliced is skipped and taken at its enumerator instead, so the Skip and Take nodes
        // that asked leave the push
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            SystemEnumerableSkipTake.Answer(_enumerable, request.Skip, request.Take, ref response);
            return true;
        }

        // a dry run reads the count of a collection, generic or not, and declines a sequence that has to be enumerated
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                SystemIEnumerable.GetCountInformation(_enumerable, out var info);
                response = info.ActualSize.GetValueOrDefault();
                return info.ActualSize.HasValue;
            }

            response = SystemIEnumerable.Count(_enumerable);
            return true;
        }

        // a collection's own lookup (a HashSet's, under its comparer), as System.Linq's Contains asks an ICollection<T>
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            if (_enumerable is ICollection<T> collection)
            {
                response = collection.Contains(request.Value);
                return true;
            }

            response = false;
            return false;
        }

        // a collection is copied by its own CopyTo, straight into an array or a list's storage; anything else has to be
        // enumerated
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (_enumerable is not ICollection<T> collection)
            {
                response = 0;
                return false;
            }

            response = request.IsDryRun
                ? collection.Count
                : request.CopyFrom(collection);
            return true;
        }

        // a collection's count, as System.Linq's Any reads it; anything else has to be enumerated
        private bool Any(in Optimizations.Any request, out bool response)
        {
            if (_enumerable is not ICollection<T> collection)
            {
                response = false;
                return false;
            }

            response = collection.Count != 0;
            return true;
        }
    }

    internal static int Count<T>(IEnumerable<T> _enumerable)
    {
        return _enumerable switch
        {
            ICollection<T> c => c.Count,
            IReadOnlyCollection<T> c => c.Count,
            var other => Iterate(other)
        };

        static int Iterate(IEnumerable<T> ts)
        {
            var e = ts.GetEnumerator();

            // as in ExecutePush, a Cistern chain's enumerator handed out by a wrapper is counted by pushing
            if (e is PullEnumerator<T> pull)
                return ErasedChain.PushRest<T, int, Aggregation.Count<T>>(pull, new Aggregation.Count<T>());

            checked
            {
                int count = 0;
                using (e)
                {
                    while (e.MoveNext())
                        ++count;
                }
                return count;
            }
        }
    }

    // the shape is chosen here, at run time, and its enumerator boxed: the chain applied to an IEnumerable<T> has one
    // type whatever the source turns out to be
    internal static PullEnumerator<TSource> CreatePullEnumerator<TSource>(IEnumerable<TSource> enumerable)
    {
        return enumerable switch
        {
            TSource[] array when array.Length == 0  => PullEnumerator<TSource>.Empty,
            TSource[] array                         => new PullEnumerator<SystemArray.Pull<TSource>, TSource>(new SystemArray.Pull<TSource>(array, 0, array.Length)),
            List<TSource> list when list.Count == 0 => PullEnumerator<TSource>.Empty,
            List<TSource> list                      => new PullEnumerator<SystemList.Pull<TSource>, TSource>(new SystemList.Pull<TSource>(new SystemList.Node<TSource>(list))),
            // a Cistern chain hands over its own enumerator; a fused chain, a Lookup or a grouping is met
            // through its IEnumerable<T>
            ValueEnumerable.IPullEnumerable<TSource> p => p.CreatePullEnumerator(),
            Queue<TSource> q                        => new PullEnumerator<SystemEnumerableGeneric.Pull<TSource, Queue<TSource>.Enumerator>, TSource>(new(q.GetEnumerator())),
            Stack<TSource> s                        => new PullEnumerator<SystemEnumerableGeneric.Pull<TSource, Stack<TSource>.Enumerator>, TSource>(new(s.GetEnumerator())),
            HashSet<TSource> h                      => new PullEnumerator<SystemEnumerableGeneric.Pull<TSource, HashSet<TSource>.Enumerator>, TSource>(new(h.GetEnumerator())),
            LinkedList<TSource> l                   => new PullEnumerator<SystemEnumerableGeneric.Pull<TSource, LinkedList<TSource>.Enumerator>, TSource>(new(l.GetEnumerator())),
            _                                       => Vanilla(enumerable),
        };

        static PullEnumerator<TSource> Vanilla(IEnumerable<TSource> enumerable)
        {
            // a wrapper can hand out a Cistern chain's own enumerator, which is then pulled as that chain
            var enumerator = enumerable.GetEnumerator();
            if (enumerator is PullEnumerator<TSource> pull)
                return pull;

            return new PullEnumerator<Pull<TSource>, TSource>(new Pull<TSource>(enumerator));
        }
    }

    // a container is built over as itself, through its struct enumerator where it has one; a Cistern chain is built
    // over as it is, being already its own heap enumerator; anything else is pulled through its IEnumerator<T>. Each
    // arm is its own instantiation of the whole chain, compiled only when the arm is taken.
    internal static TBuilt Rebuild<T, TBuilt, TBuilder>(in Node<T> node, in TBuilder builder)
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        return node._enumerable switch
        {
            T[] array                                   => overArray(array, in builder),
            List<T> list                                => overList(list, in builder),
            IList<T> list                               => overIList(list, in builder),
            ValueEnumerable.IPullEnumerable<T>          => builder.Build<Node<T>, ErasedEnumerator<T, Node<T>>>(in node),
            Queue<T> queue                              => overCollection<Queue<T>, Queue<T>.Enumerator>(queue, static q => q.GetEnumerator(), in builder),
            Stack<T> stack                              => overCollection<Stack<T>, Stack<T>.Enumerator>(stack, static s => s.GetEnumerator(), in builder),
            HashSet<T> hashSet                          => overCollection<HashSet<T>, HashSet<T>.Enumerator>(hashSet, static h => h.GetEnumerator(), in builder),
            LinkedList<T> linkedList                    => overCollection<LinkedList<T>, LinkedList<T>.Enumerator>(linkedList, static l => l.GetEnumerator(), in builder),
            _                                           => overEnumerator(in node, in builder),
        };

        [MethodImpl(MethodImplOptions.NoInlining)]
        static TBuilt overArray(T[] array, in TBuilder builder)
            => builder.Build<SystemArray.Node<T>, SystemArray.Pull<T>>(new SystemArray.Node<T>(array));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static TBuilt overList(List<T> list, in TBuilder builder)
            => builder.Build<SystemList.Node<T>, SystemList.Pull<T>>(new SystemList.Node<T>(list));

        // read by its indexer, as it is pushed
        [MethodImpl(MethodImplOptions.NoInlining)]
        static TBuilt overIList(IList<T> list, in TBuilder builder)
            => builder.Build<SystemIList.Node<T>, SystemIList.Pull<T>>(new SystemIList.Node<T>(list, 0, list.Count));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static TBuilt overCollection<TCollection, TEnumerator>(TCollection collection, Func<TCollection, TEnumerator> getEnumerator, in TBuilder builder)
            where TCollection : IEnumerable<T>
            where TEnumerator : IEnumerator<T>
            => builder.Build<SystemEnumerableGeneric.Node<T, TCollection, TEnumerator>, SystemEnumerableGeneric.NodeEnumerator<T, TCollection, TEnumerator>>(new SystemEnumerableGeneric.Node<T, TCollection, TEnumerator>(collection, getEnumerator, null));

        [MethodImpl(MethodImplOptions.NoInlining)]
        static TBuilt overEnumerator(in Node<T> node, in TBuilder builder)
            => builder.Build<Node<T>, Pull<T>>(in node);
    }

    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(IEnumerable<TSource> _enumerable, in TPushEnumerator fenum)
         where TPushEnumerator : IPushEnumerator<TSource>
        => ExecutePush<TSource, TResult, TPushEnumerator>(default(ChainOptions), _enumerable, in fenum);

    // the chain handed in is disposed, as a push that ran would have; the parameter name matches System.Linq's exceptions
    internal static TResult NullSource<TSource, TResult, TPushEnumerator>(TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TSource>
    {
        fenum.Dispose();
        throw new ArgumentNullException("source");
    }

    // a source ExecutePush reads through its own IEnumerator<T> (Vanilla): none of the containers its switch tests for
    internal static bool IsOpaque<TSource>(IEnumerable<TSource>? enumerable) =>
        enumerable is not (null or TSource[] or List<TSource> or INode<TSource> or ICollection<TSource>);

    // out of line, so a container's loop is compiled inside this method, under its profile, rather than inlined with it
    // into a caller compiled without one (Distinct().ToArray() over an array measured 38 → 55 µs inlined)
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(in ChainOptions options, IEnumerable<TSource> _enumerable, in TPushEnumerator fenum)
         where TPushEnumerator : IPushEnumerator<TSource>
    {
        return _enumerable switch
        {
            null                => NullSource<TSource, TResult, TPushEnumerator>(fenum),
            TSource[] array     => SystemArray.ExecutePush<TSource, TResult, TPushEnumerator>(array, fenum),
            List<TSource> list  => SystemList.ExecutePush<TSource, TResult, TPushEnumerator>(list, fenum),
            // as for pull (CreateObjectDescent)
            ValueEnumerable.IPullEnumerable<TSource> p when !options.Fuse => ErasedChain.Push<TSource, TResult, TPushEnumerator>(in options, p, in fenum),
            INode<TSource> node => HandOn(in options, node, in fenum),
            ICollection<TSource> or IReadOnlyCollection<TSource> => Collection(_enumerable, fenum),
            _                   => Vanilla(_enumerable, fenum),
        };

        // out of line, so a caller inlining the push carries only the arms above
        [MethodImpl(MethodImplOptions.NoInlining)]
        static TResult Collection(IEnumerable<TSource> enumerable, TPushEnumerator fenum)
        {
            return enumerable switch
            {
                // a collection's elements copied out by its own CopyTo, as System.Linq copies an ICollection<T> it
                // materialises, where the chain takes them so; the collections below with an enumerator called directly
                // are direct, given cold only to a chain that copies them into its own storage
                ICollection<TSource> c when Takes(in fenum, ColdRequires(c)) => PushCollection(c, fenum),
                // any other list is read through its indexer, so no enumerator is allocated
                IList<TSource> list => SystemIList.ExecutePush<TSource, TResult, TPushEnumerator>(list, 0, list.Count, fenum),
                // collections whose struct enumerator IEnumerable<T>.GetEnumerator would box; over a reference type it
                // runs as shared code, measured at twice the boxed enumerator's time pushed (and faster pulled)
                Queue<TSource> q when typeof(TSource).IsValueType      => SystemEnumerableGeneric.ExecutePush<TSource, TResult, TPushEnumerator, Queue<TSource>.Enumerator>(q.GetEnumerator(), fenum),
                Stack<TSource> s when typeof(TSource).IsValueType      => SystemEnumerableGeneric.ExecutePush<TSource, TResult, TPushEnumerator, Stack<TSource>.Enumerator>(s.GetEnumerator(), fenum),
                HashSet<TSource> h when typeof(TSource).IsValueType    => SystemEnumerableGeneric.ExecutePush<TSource, TResult, TPushEnumerator, HashSet<TSource>.Enumerator>(h.GetEnumerator(), fenum),
                LinkedList<TSource> l when typeof(TSource).IsValueType => SystemEnumerableGeneric.ExecutePush<TSource, TResult, TPushEnumerator, LinkedList<TSource>.Enumerator>(l.GetEnumerator(), fenum),
                _ => Vanilla(enumerable, fenum),
            };
        }

        // a Cistern node's push owns the chain once it has started it; disposed here when that push fails first
        static TResult HandOn(in ChainOptions options, INode<TSource> node, in TPushEnumerator fenum)
        {
            var started = false;
            try
            {
                var downstream = new Downstream<TPushEnumerator>(in fenum);
                var guard = new Guard<TPushEnumerator, Downstream<TPushEnumerator>>(ref started, in downstream);
                return node.CreateViaPush<TResult, TPushEnumerator, Guard<TPushEnumerator, Downstream<TPushEnumerator>>>(in options, in guard);
            }
            finally
            {
                if (!started)
                {
                    var chain = fenum;
                    chain.Dispose();
                }
            }
        }

        // a Concat part or inner chain pushed into an erased sink asks the chain behind it
        static bool Takes(in TPushEnumerator fenum, BatchKinds requires) =>
            typeof(TPushEnumerator) == typeof(ErasedChain.PushForward<TSource>)
                ? ((ErasedChain.PushForward<TSource>)(object)fenum).Sink.Takes(requires)
                : TPushEnumerator.AllowBatchesWhen.Takes(requires);

        static BatchKinds ColdRequires(ICollection<TSource> collection) =>
            BatchKindsExtensions.ColdRequires(collection.Count, IsDirect(collection));

        // over a reference type the struct enumerator runs as shared code, measured slower than the boxed one, so the
        // collection is read as one without
        static bool IsDirect(ICollection<TSource> collection) =>
            typeof(TSource).IsValueType
            && collection is Queue<TSource> or Stack<TSource> or HashSet<TSource> or LinkedList<TSource>;

        static TResult PushCollection(ICollection<TSource> collection, TPushEnumerator fenum)
        {
            var batch = new Batch<TSource>(collection, collection.Count, ColdRequires(collection));
            try
            {
                // released before the result is taken
                try
                {
                    _ = fenum.ProcessBatch(ref batch);
                }
                finally
                {
                    batch.Release();
                }

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        static TResult Vanilla(IEnumerable<TSource> e, TPushEnumerator fenum)
        {
            // an aggregator whose Dispose is empty runs from a copy no handler reads, which measured faster for reasons
            // not settled; any other chain runs in place, disposed however the push ends
            var withoutDispose =
                typeof(TPushEnumerator).IsAssignableTo(typeof(IInternal__Aggregator__NotForManualUse))
                && !typeof(TPushEnumerator).IsAssignableTo(typeof(IInternal__ImplementsDispose__NotForManualUse));

            IEnumerator<TSource> enumerator;
            if (withoutDispose)
                enumerator = e.GetEnumerator();
            else
            {
                try
                {
                    enumerator = e.GetEnumerator();
                }
                catch
                {
                    fenum.Dispose();
                    throw;
                }
            }

            // a wrapper can hand out a Cistern chain's own enumerator, which pushes what it can rather than being
            // pulled, and then owns the downstream's result and disposal
            if (enumerator is PullEnumerator<TSource> pull)
                return ErasedChain.PushRest<TSource, TResult, TPushEnumerator>(pull, in fenum);

            if (withoutDispose)
            {
                var local = fenum;
                using (enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        if (!local.ProcessNext(enumerator.Current))
                            break;
                    }
                }

                return local.GetResult<TResult>();
            }
            else
            {
                try
                {
                    using (enumerator)
                    {
                        while (enumerator.MoveNext())
                        {
                            if (!fenum.ProcessNext(enumerator.Current))
                                break;
                        }
                    }

                    return fenum.GetResult<TResult>();
                }
                finally
                {
                    fenum.Dispose();
                }
            }
        }
    }
}
