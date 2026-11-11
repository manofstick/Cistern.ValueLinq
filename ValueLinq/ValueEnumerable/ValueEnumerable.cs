using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using Cistern.ValueLinq.Maths;
using Cistern.ValueLinq.Nodes;
using Cistern.ValueLinq.ValueEnumerable;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public readonly struct ValueEnumerable<TSource, TNode, TEnumerator>
        : IEnumerable<TSource>
        , IPullEnumerable<TSource>
        , INode<TSource>
        where TNode : INode<TSource>
        where TEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TNode, TEnumerator>
    {
        internal readonly TNode Node;

        // an empty collection expression is built through this constructor, and would enumerate a default node
        [Obsolete("A ValueEnumerable cannot be created empty; target IEnumerable<T> or state the element type", error: true)]
        public ValueEnumerable() => Node = default!;

        public ValueEnumerable(in TNode node)
            => Node = node;

        public ValueEnumerator<TSource, TEnumerator> GetEnumerator()
            => TEnumerator.PreferRebuilt
                ? new(PullEnumerator.Create<TSource, TNode, TEnumerator>(in Node))
                : new(TEnumerator.Create(in Node));
        IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            => PullEnumerator.Create<TSource, TNode, TEnumerator>(in Node);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => ((IEnumerable<TSource>)this).GetEnumerator();
        PullEnumerator<TSource> IPullEnumerable<TSource>.CreatePullEnumerator()
            => PullEnumerator.Create<TSource, TNode, TEnumerator>(in Node);
        void IPullEnumerable<TSource>.Push(in ChainOptions options, ErasedChain.PushSink<TSource> sink)
            => Node.CreateViaPush<bool, ErasedChain.PushForward<TSource>, Construct<ErasedChain.PushForward<TSource>, ErasedChain.PushForward<TSource>.Args>>(in options, new Construct<ErasedChain.PushForward<TSource>, ErasedChain.PushForward<TSource>.Args>(new ErasedChain.PushForward<TSource>.Args(sink)));

        void INode.GetCountInformation(out CountInformation info)
            => Node.GetCountInformation(out info);

        bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            => Node.TryPushOptimization<TRequest, TRequestResult>(in request, out result);
        TPushResult INode<TSource>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => Node.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in options, in filler);
    }

    // a ValueEnumerable used as a node (an inner sequence a SelectMany selector returns) is pulled through its own
    // chain's enumerator
    [Puller]
    public partial struct ValueEnumerableEnumerator<TSource, TNode, TEnumerator>
        where TNode : INode<TSource>
        where TEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TNode, TEnumerator>
    {
        private TEnumerator _enumerator;

        private ValueEnumerableEnumerator(in TEnumerator enumerator) => _enumerator = enumerator;

        private static ValueEnumerableEnumerator<TSource, TNode, TEnumerator> Create(in ValueEnumerable<TSource, TNode, TEnumerator> node)
            => new(TEnumerator.Create(in node.Node));

        static bool IPullEnumerator<TSource>.RebuildChanges => TEnumerator.RebuildChanges;

        static TBuilt IPullEnumerator<TSource>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            ref readonly var enumerable = ref ChainBuilder.As<TOwnNode, ValueEnumerable<TSource, TNode, TEnumerator>>(in node);
            var above = new ValueEnumerableChainBuilder<TSource, TBuilt, TBuilder>(in builder);
            return TEnumerator.Rebuild<TNode, TBuilt, ValueEnumerableChainBuilder<TSource, TBuilt, TBuilder>>(in enumerable.Node, in above);
        }

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.MoveNext();

        public TSource Current => _enumerator.Current;

        bool IPullEnumerator<TSource>.TryPushRest(ErasedChain.PushSink<TSource> sink)
        {
            PullEnumerator.PushRest<TSource, TEnumerator>(ref _enumerator, sink);
            return true;
        }
    }

    readonly ref struct ValueEnumerableChainBuilder<TSource, TBuilt, TBuilder>
        : IChainBuilder<TSource, TBuilt>
        where TBuilder : IChainBuilder<TSource, TBuilt>, allows ref struct
    {
        private readonly ref readonly ErasedRef _builder;

        public ValueEnumerableChainBuilder(in TBuilder builder) => _builder = ref Erase.Ref(in builder);

        TBuilt IChainBuilder<TSource, TBuilt>.Build<TNode, TEnumerator>(in TNode node)
            => Erase.Deref<TBuilder>(in _builder).Build<ValueEnumerable<TSource, TNode, TEnumerator>, ValueEnumerableEnumerator<TSource, TNode, TEnumerator>>(new ValueEnumerable<TSource, TNode, TEnumerator>(in node));
    }

    public static partial class Enumerable
    {
        public static TSource Aggregate<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Func<TSource, TSource, TSource> func)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Aggregate(in source.Node, func);

        public static void ForEach<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Action<TSource> func)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ForEach(in source.Node, func);

        public static TSource ForEach<TSource, U, Inner, InnerEnumerator>(in this ValueEnumerable<U, Inner, InnerEnumerator> source, TSource seed, RefAction<TSource, U> func)
            where Inner : INode<U>
            where InnerEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ForEach(in source.Node, seed, func);

        public static TSource ForEach<TSource, U, Inner, RefAction, InnerEnumerator>(in this ValueEnumerable<U, Inner, InnerEnumerator> source, TSource seed, RefAction func)
            where Inner : INode<U>
            where InnerEnumerator : struct, IPullEnumerator<U>, INodeEnumerator<Inner, InnerEnumerator>
            where RefAction : IRefAction<TSource, U>
            => NodeImpl.ForEach<TSource, U, Inner, RefAction>(in source.Node, seed, func);

        public static TAccumulate Aggregate<TSource, TAccumulate, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Aggregate(in source.Node, seed, func);

        public static TResult Aggregate<TSource, TAccumulate, TResult, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, Func<TAccumulate, TResult> resultSelector)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Aggregate(in source.Node, seed, func, resultSelector);

        public static bool All<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.All(in source.Node, predicate);

        public static bool All<TSource, Inner, Predicate, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Predicate predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            where Predicate : IFunc<TSource, bool>
            => NodeImpl.All<TSource, Inner, Predicate>(in source.Node, predicate);

        public static bool Any<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Any<TSource, Inner>(source.Node);

        public static bool Any<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Any(in source.Node, predicate);

        /// <summary>
        /// Fuses every Cistern chain reached as an <see cref="IEnumerable{T}"/> below this point, including sequences
        /// passed to an operator (a Concat part, Zip's second) and those a selector returns, with the chain pushed into
        /// it, so the two run as one push. Each distinct pairing of chains is then compiled separately; unfused, each
        /// chain is compiled once and the boundary costs a virtual call per element. Pulling is not affected: a chain
        /// reached as <see cref="IEnumerable{T}"/> is always pulled through its own enumerator.
        /// </summary>
        public static ValueEnumerable<TSource, Nodes.Options.Node<TSource, Inner>, Nodes.Options.Pull<TSource, Inner, InnerEnumerator>> Fuse<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (new Nodes.Options.Node<TSource, Inner>(in source.Node, new ChainOptionsOverride { Fuse = true }));

        /// <summary>
        /// Hands the chain out as one heap <see cref="IEnumerable{T}"/>, behaving as <paramref name="options"/> asks.
        /// </summary>
        public static IEnumerable<TSource> ToEnumerable<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, EnumerableOptions options = EnumerableOptions.None)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => ChainEnumerable.Create<TSource, Inner, InnerEnumerator>(in source.Node, options);

        public static ValueEnumerable<U, Nodes.Select.Node<TSource, U, TPrior>, Nodes.Select.Pull<TSource, U, TPrior, TPriorEnumerator>> Select<TSource, U, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, U> selector)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Select(in prior.Node, selector));

        public static ValueEnumerable<U, Nodes.Select_In.Node<TSource, U, TPrior>, Nodes.Select_In.Pull<TSource, U, TPrior, TPriorEnumerator>> Select<TSource, U, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, InFunc<TSource, U> f)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Select(in prior.Node, f));

        public static ValueEnumerable<U, Nodes.SelectIdx.Node<TSource, U, TPrior>, Nodes.SelectIdx.Pull<TSource, U, TPrior, TPriorEnumerator>> Select<TSource, U, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, int, U> selector)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Select(in prior.Node, selector));

        /// <summary>
        /// Select with a pure selector: one with no side effects whose result depends only on its argument, so it is
        /// run only for the elements a result is made of (never to count, test for any element, or skip).
        /// </summary>
        public static ValueEnumerable<U, Nodes.Map.Node<TSource, U, TPrior>, Nodes.Map.Pull<TSource, U, TPrior, TPriorEnumerator>> Map<TSource, U, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, U> selector)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Map(in prior.Node, selector));

        /// <summary>
        /// Where with a pure predicate: one with no side effects whose result depends only on its argument, so it may
        /// be run in any order (from the end, for a from-end index or a reversal).
        /// </summary>
        public static ValueEnumerable<TSource, Nodes.Filter.Node<TSource, TPrior>, Nodes.Filter.Pull<TSource, TPrior, TPriorEnumerator>> Filter<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Filter(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.Where.Node<TSource, TPrior>, Nodes.Where.Pull<TSource, TPrior, TPriorEnumerator>> Where<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.WhereIdx.Node<TSource, TPrior>, Nodes.WhereIdx.Pull<TSource, TPrior, TPriorEnumerator>> Where<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Where(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.Where_In.Node<TSource, TPrior>, Nodes.Where_In.Pull<TSource, TPrior, TPriorEnumerator>> Where<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, InFunc<TSource, bool> f)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Where(in prior.Node, f));

        public static ValueEnumerable<TSource, Nodes.Skip.Node<TSource, TPrior>, Nodes.Skip.Pull<TSource, TPrior, TPriorEnumerator>> Skip<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Skip<TSource, TPrior>(in prior.Node, count));

        // a skip over a Skip adds into it rather than stacking on it
        public static ValueEnumerable<TSource, Nodes.Skip.Node<TSource, TPrior>, Nodes.Skip.Pull<TSource, TPrior, TPriorEnumerator>> Skip<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, Nodes.Skip.Node<TSource, TPrior>, Nodes.Skip.Pull<TSource, TPrior, TPriorEnumerator>> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (prior.Node.Then(count));

        public static ValueEnumerable<TSource, Nodes.TakeRange.Node<TSource, TPrior>, Nodes.TakeRange.Pull<TSource, TPrior, TPriorEnumerator>> SkipLast<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.SkipLast<TSource, TPrior>(in prior.Node, count));

        public static ValueEnumerable<TSource, Nodes.SkipWhile.Node<TSource, TPrior>, Nodes.SkipWhile.Pull<TSource, TPrior, TPriorEnumerator>> SkipWhile<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new(NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.SkipWhileIdx.Node<TSource, TPrior>, Nodes.SkipWhileIdx.Pull<TSource, TPrior, TPriorEnumerator>> SkipWhile<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.SkipWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.Take.Node<TSource, TPrior>, Nodes.Take.Pull<TSource, TPrior, TPriorEnumerator>> Take<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Take<TSource, TPrior>(in prior.Node, count));

        // a take over a Take narrows it rather than stacking on it
        public static ValueEnumerable<TSource, Nodes.Take.Node<TSource, TPrior>, Nodes.Take.Pull<TSource, TPrior, TPriorEnumerator>> Take<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, Nodes.Take.Node<TSource, TPrior>, Nodes.Take.Pull<TSource, TPrior, TPriorEnumerator>> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (prior.Node.Then(count));

        public static ValueEnumerable<TSource, Nodes.TakeRange.Node<TSource, TPrior>, Nodes.TakeRange.Pull<TSource, TPrior, TPriorEnumerator>> Take<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Range range)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Take<TSource, TPrior>(in prior.Node, range));

        public static ValueEnumerable<TSource, Nodes.TakeRange.Node<TSource, TPrior>, Nodes.TakeRange.Pull<TSource, TPrior, TPriorEnumerator>> TakeLast<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, int count)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.TakeLast<TSource, TPrior>(in prior.Node, count));

        public static ValueEnumerable<TSource, Nodes.TakeWhile.Node<TSource, TPrior>, Nodes.TakeWhile.Pull<TSource, TPrior, TPriorEnumerator>> TakeWhile<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.TakeWhile(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.TakeWhileIdx.Node<TSource, TPrior>, Nodes.TakeWhileIdx.Pull<TSource, TPrior, TPriorEnumerator>> TakeWhile<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, int, bool> predicate)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new(NodeImpl.TakeWhile(in prior.Node, predicate));

        public static TSource[] ToArray<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToArray(inner.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo?.arrayPool, arrayPoolInfo?.cleanBuffers ?? false);

        public static TSource[] ToArrayUsePool<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, ArrayPool<TSource>? maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToArrayUsePool<TSource, Inner, InnerEnumerator>(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static TSource[] ToArrayUseStack<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int maxStackItemCount = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToArrayUseStack<TSource, Inner, InnerEnumerator>(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static List<TSource> ToList<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToList<TSource, Inner, InnerEnumerator>(in inner.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo);

        public static List<TSource> ToListUsePool<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, ArrayPool<TSource>? maybeArrayPool = null, bool? maybeCleanBuffers = null, bool viaPull = false)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToListUsePool<TSource, Inner, InnerEnumerator>(in inner.Node, maybeArrayPool, maybeCleanBuffers, viaPull);

        public static List<TSource> ToListUseStack<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int maxStackItemCount = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToListUseStack<TSource, Inner, InnerEnumerator>(in inner.Node, maxStackItemCount, arrayPoolInfo);

        public static HashSet<TSource> ToHashSet<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, IEqualityComparer<TSource>? equalityComparer = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToHashSet(in inner.Node, equalityComparer);

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionary(in inner.Node, keySelector, comparer);

        public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionary(in inner.Node, keySelector, elementSelector, comparer);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue, Inner, InnerEnumerator>(in this ValueEnumerable<KeyValuePair<TKey, TValue>, Inner, InnerEnumerator> inner)
            where TKey : notnull
            where Inner : INode<KeyValuePair<TKey, TValue>>
            where InnerEnumerator : struct, IPullEnumerator<KeyValuePair<TKey, TValue>>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionaryFromKeyValuePairs<TKey, TValue, Inner>(in inner.Node, null);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue, Inner, InnerEnumerator>(in this ValueEnumerable<KeyValuePair<TKey, TValue>, Inner, InnerEnumerator> inner, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
            where Inner : INode<KeyValuePair<TKey, TValue>>
            where InnerEnumerator : struct, IPullEnumerator<KeyValuePair<TKey, TValue>>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionaryFromKeyValuePairs<TKey, TValue, Inner>(in inner.Node, comparer);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue, Inner, InnerEnumerator>(in this ValueEnumerable<(TKey Key, TValue Value), Inner, InnerEnumerator> inner)
            where TKey : notnull
            where Inner : INode<(TKey Key, TValue Value)>
            where InnerEnumerator : struct, IPullEnumerator<(TKey Key, TValue Value)>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionaryFromTuples<TKey, TValue, Inner>(in inner.Node, null);

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue, Inner, InnerEnumerator>(in this ValueEnumerable<(TKey Key, TValue Value), Inner, InnerEnumerator> inner, IEqualityComparer<TKey>? comparer)
            where TKey : notnull
            where Inner : INode<(TKey Key, TValue Value)>
            where InnerEnumerator : struct, IPullEnumerator<(TKey Key, TValue Value)>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ToDictionaryFromTuples<TKey, TValue, Inner>(in inner.Node, comparer);

        public static TSource Last<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Last<TSource, Inner>(inner.Node);

        public static TSource Last<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Last(in inner.Node, predicate);

        public static TSource? LastOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LastOrDefault<TSource, Inner>(inner.Node);

        public static TSource? LastOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LastOrDefault(in inner.Node, predicate);

        public static TSource LastOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LastOrDefault<TSource, Inner>(inner.Node, defaultValue);

        public static TSource LastOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LastOrDefault(in inner.Node, predicate, defaultValue);

        public static TSource First<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.First<TSource, Inner>(inner.Node);

        public static TSource First<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.First(in inner.Node, predicate);

        public static TSource? FirstOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.FirstOrDefault<TSource, Inner>(inner.Node);

        public static TSource? FirstOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.FirstOrDefault(in inner.Node, predicate);

        public static TSource FirstOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.FirstOrDefault<TSource, Inner>(inner.Node, defaultValue);

        public static TSource FirstOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.FirstOrDefault(in inner.Node, predicate, defaultValue);

        public static TSource Single<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Single<TSource, Inner>(in inner.Node);

        public static TSource Single<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Single(in inner.Node, predicate);

        public static TSource? SingleOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.SingleOrDefault<TSource, Inner>(in inner.Node);

        public static TSource? SingleOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.SingleOrDefault(in inner.Node, predicate);

        public static TSource SingleOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.SingleOrDefault<TSource, Inner>(in inner.Node, defaultValue);

        public static TSource SingleOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, bool> predicate, TSource defaultValue)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.SingleOrDefault(in inner.Node, predicate, defaultValue);

        public static TSource ElementAt<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int index)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ElementAt<TSource, Inner>(inner.Node, index);

        public static TSource? ElementAtOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, int index)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ElementAtOrDefault<TSource, Inner>(inner.Node, index);

        public static TSource ElementAt<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Index index)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ElementAt<TSource, Inner>(inner.Node, index);

        public static TSource? ElementAtOrDefault<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Index index)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.ElementAtOrDefault<TSource, Inner>(inner.Node, index);

        public static decimal Average<Inner, InnerEnumerator>(in this ValueEnumerable<decimal, Inner, InnerEnumerator> inner) where Inner : INode<Decimal> where InnerEnumerator : struct, IPullEnumerator<decimal>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageDecimal(in inner.Node);
        public static double  Average<Inner, InnerEnumerator>(in this ValueEnumerable<double,  Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<double> where InnerEnumerator : struct, IPullEnumerator<double>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.AverageDouble(in inner.Node, simdOptions);
        public static float   Average<Inner, InnerEnumerator>(in this ValueEnumerable<float,   Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<float> where InnerEnumerator : struct, IPullEnumerator<float>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.AverageFloat(in inner.Node, simdOptions);
        public static double  Average<Inner, InnerEnumerator>(in this ValueEnumerable<int,     Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<int> where InnerEnumerator : struct, IPullEnumerator<int>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.AverageInt(in inner.Node, simdOptions);
        public static double  Average<Inner, InnerEnumerator>(in this ValueEnumerable<long,    Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<long> where InnerEnumerator : struct, IPullEnumerator<long>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.AverageLong(in inner.Node, simdOptions);

        public static decimal Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double> selector)  where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageDouble( NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static float   Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float> selector)   where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageFloat(  NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static double  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int> selector)     where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageInt(    NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static double  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long> selector)    where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageLong(   NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);

        public static decimal? Average<Inner, InnerEnumerator>(in this ValueEnumerable<decimal?, Inner, InnerEnumerator> inner) where Inner : INode<Decimal?> where InnerEnumerator : struct, IPullEnumerator<decimal?>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableDecimal(in inner.Node);
        public static double?  Average<Inner, InnerEnumerator>(in this ValueEnumerable<double?,  Inner, InnerEnumerator> inner) where Inner : INode<double?> where InnerEnumerator : struct, IPullEnumerator<double?>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.AverageNullableDouble(in inner.Node);
        public static float?   Average<Inner, InnerEnumerator>(in this ValueEnumerable<float?,   Inner, InnerEnumerator> inner) where Inner : INode<float?> where InnerEnumerator : struct, IPullEnumerator<float?>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.AverageNullableFloat(in inner.Node);
        public static double?  Average<Inner, InnerEnumerator>(in this ValueEnumerable<int?,     Inner, InnerEnumerator> inner) where Inner : INode<int?> where InnerEnumerator : struct, IPullEnumerator<int?>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.AverageNullableInt(in inner.Node);
        public static double?  Average<Inner, InnerEnumerator>(in this ValueEnumerable<long?,    Inner, InnerEnumerator> inner) where Inner : INode<long?> where InnerEnumerator : struct, IPullEnumerator<long?>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.AverageNullableLong(in inner.Node);

        public static decimal? Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static double?  Average<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.AverageNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Min<Inner, InnerEnumerator>(   in this ValueEnumerable<decimal, Inner, InnerEnumerator> inner) where Inner : INode<Decimal> where InnerEnumerator : struct, IPullEnumerator<decimal>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.MinDecimal(   in inner.Node);
        public static double  Min<Inner, InnerEnumerator>(   in this ValueEnumerable<double,  Inner, InnerEnumerator> inner) where Inner : INode<double> where InnerEnumerator : struct, IPullEnumerator<double>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.MinDouble(    in inner.Node);
        public static float   Min<Inner, InnerEnumerator>(   in this ValueEnumerable<float,   Inner, InnerEnumerator> inner) where Inner : INode<float> where InnerEnumerator : struct, IPullEnumerator<float>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.MinFloat(     in inner.Node);
        public static int     Min<Inner, InnerEnumerator>(   in this ValueEnumerable<int,     Inner, InnerEnumerator> inner) where Inner : INode<int> where InnerEnumerator : struct, IPullEnumerator<int>, INodeEnumerator<Inner, InnerEnumerator>      => NodeImpl.MinInt(       in inner.Node);
        public static long    Min<Inner, InnerEnumerator>(   in this ValueEnumerable<long,    Inner, InnerEnumerator> inner) where Inner : INode<long> where InnerEnumerator : struct, IPullEnumerator<long>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.MinLong(      in inner.Node);
        public static TSource?      Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource,       Inner, InnerEnumerator> inner) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>        => NodeImpl.Min<TSource, Inner>(in inner.Node);
        public static TSource?      Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource,       Inner, InnerEnumerator> inner, IComparer<TSource>? comparer) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.Min<TSource, Inner>(in inner.Node, comparer);

        public static TSource? MinBy<TSource, TKey, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinBy<TSource, TKey, Inner>(in inner.Node, keySelector, null);
        public static TSource? MinBy<TSource, TKey, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinBy<TSource, TKey, Inner>(in inner.Node, keySelector, comparer);

        public static decimal Min<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Min<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Min<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Min<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Min<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U?      Min<TSource, U, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, U>       selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.Min<U, Nodes.Select.Node<TSource,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Min<Inner, InnerEnumerator>(in this ValueEnumerable<decimal?, Inner, InnerEnumerator> inner) where Inner : INode<Decimal?> where InnerEnumerator : struct, IPullEnumerator<decimal?>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.MinNullableDecimal(in inner.Node);
        public static double?  Min<Inner, InnerEnumerator>(in this ValueEnumerable<double?,  Inner, InnerEnumerator> inner) where Inner : INode<double?> where InnerEnumerator : struct, IPullEnumerator<double?>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.MinNullableDouble( in inner.Node);
        public static float?   Min<Inner, InnerEnumerator>(in this ValueEnumerable<float?,   Inner, InnerEnumerator> inner) where Inner : INode<float?> where InnerEnumerator : struct, IPullEnumerator<float?>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.MinNullableFloat(  in inner.Node);
        public static int?     Min<Inner, InnerEnumerator>(in this ValueEnumerable<int?,     Inner, InnerEnumerator> inner) where Inner : INode<int?> where InnerEnumerator : struct, IPullEnumerator<int?>, INodeEnumerator<Inner, InnerEnumerator>      => NodeImpl.MinNullableInt(    in inner.Node);
        public static long?    Min<Inner, InnerEnumerator>(in this ValueEnumerable<long?,    Inner, InnerEnumerator> inner) where Inner : INode<long?> where InnerEnumerator : struct, IPullEnumerator<long?>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.MinNullableLong(   in inner.Node);

        public static decimal? Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Min<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MinNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Max<Inner, InnerEnumerator>(   in this ValueEnumerable<decimal, Inner, InnerEnumerator> inner) where Inner : INode<Decimal> where InnerEnumerator : struct, IPullEnumerator<decimal>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxDecimal(   in inner.Node);
        public static double  Max<Inner, InnerEnumerator>(   in this ValueEnumerable<double,  Inner, InnerEnumerator> inner) where Inner : INode<double> where InnerEnumerator : struct, IPullEnumerator<double>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.MaxDouble(    in inner.Node);
        public static float   Max<Inner, InnerEnumerator>(   in this ValueEnumerable<float,   Inner, InnerEnumerator> inner) where Inner : INode<float> where InnerEnumerator : struct, IPullEnumerator<float>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.MaxFloat(     in inner.Node);
        public static int     Max<Inner, InnerEnumerator>(   in this ValueEnumerable<int,     Inner, InnerEnumerator> inner) where Inner : INode<int> where InnerEnumerator : struct, IPullEnumerator<int>, INodeEnumerator<Inner, InnerEnumerator>      => NodeImpl.MaxInt(       in inner.Node);
        public static long    Max<Inner, InnerEnumerator>(   in this ValueEnumerable<long,    Inner, InnerEnumerator> inner) where Inner : INode<long> where InnerEnumerator : struct, IPullEnumerator<long>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.MaxLong(      in inner.Node);
        public static TSource?      Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource,       Inner, InnerEnumerator> inner) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>        => NodeImpl.Max<TSource, Inner>(in inner.Node);
        public static TSource?      Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource,       Inner, InnerEnumerator> inner, IComparer<TSource>? comparer) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.Max<TSource, Inner>(in inner.Node, comparer);

        public static TSource? MaxBy<TSource, TKey, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxBy<TSource, TKey, Inner>(in inner.Node, keySelector, null);
        public static TSource? MaxBy<TSource, TKey, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxBy<TSource, TKey, Inner>(in inner.Node, keySelector, comparer);

        public static decimal Max<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxDecimal(                   NodeImpl.Select(in inner.Node, selector));
        public static double  Max<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxDouble(                    NodeImpl.Select(in inner.Node, selector));
        public static float   Max<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxFloat(                     NodeImpl.Select(in inner.Node, selector));
        public static int     Max<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxInt(                       NodeImpl.Select(in inner.Node, selector));
        public static long    Max<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxLong(                      NodeImpl.Select(in inner.Node, selector));
        public static U?      Max<TSource, U, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, U>       selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.Max<U, Nodes.Select.Node<TSource,U,Inner>>(NodeImpl.Select(in inner.Node, selector));

        public static decimal? Max<Inner, InnerEnumerator>(in this ValueEnumerable<decimal?, Inner, InnerEnumerator> inner) where Inner : INode<Decimal?> where InnerEnumerator : struct, IPullEnumerator<decimal?>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableDecimal(in inner.Node);
        public static double?  Max<Inner, InnerEnumerator>(in this ValueEnumerable<double?,  Inner, InnerEnumerator> inner) where Inner : INode<double?> where InnerEnumerator : struct, IPullEnumerator<double?>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.MaxNullableDouble( in inner.Node);
        public static float?   Max<Inner, InnerEnumerator>(in this ValueEnumerable<float?,   Inner, InnerEnumerator> inner) where Inner : INode<float?> where InnerEnumerator : struct, IPullEnumerator<float?>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.MaxNullableFloat(  in inner.Node);
        public static int?     Max<Inner, InnerEnumerator>(in this ValueEnumerable<int?,     Inner, InnerEnumerator> inner) where Inner : INode<int?> where InnerEnumerator : struct, IPullEnumerator<int?>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.MaxNullableInt(    in inner.Node);
        public static long?    Max<Inner, InnerEnumerator>(in this ValueEnumerable<long?,    Inner, InnerEnumerator> inner) where Inner : INode<long?> where InnerEnumerator : struct, IPullEnumerator<long?>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.MaxNullableLong(   in inner.Node);

        public static decimal? Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Max<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.MaxNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static decimal Sum<Inner, InnerEnumerator>(in this ValueEnumerable<decimal, Inner, InnerEnumerator> inner)                                                  where Inner : INode<Decimal> where InnerEnumerator : struct, IPullEnumerator<decimal>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumDecimal(   in inner.Node);
        public static double  Sum<Inner, InnerEnumerator>(in this ValueEnumerable<double,  Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<double> where InnerEnumerator : struct, IPullEnumerator<double>, INodeEnumerator<Inner, InnerEnumerator>  => NodeImpl.SumDouble(    in inner.Node, simdOptions);
        public static float   Sum<Inner, InnerEnumerator>(in this ValueEnumerable<float,   Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<float> where InnerEnumerator : struct, IPullEnumerator<float>, INodeEnumerator<Inner, InnerEnumerator>   => NodeImpl.SumFloat(     in inner.Node, simdOptions);
        public static int     Sum<Inner, InnerEnumerator>(in this ValueEnumerable<int,     Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<int> where InnerEnumerator : struct, IPullEnumerator<int>, INodeEnumerator<Inner, InnerEnumerator>     => NodeImpl.SumInt(       in inner.Node, simdOptions);
        public static long    Sum<Inner, InnerEnumerator>(in this ValueEnumerable<long,    Inner, InnerEnumerator> inner, SIMDOptions simdOptions = SIMDOptions.OnlyIfSame) where Inner : INode<long> where InnerEnumerator : struct, IPullEnumerator<long>, INodeEnumerator<Inner, InnerEnumerator>    => NodeImpl.SumLong(      in inner.Node, simdOptions);

        public static decimal Sum<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double  Sum<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumDouble( NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static float   Sum<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumFloat(  NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static int     Sum<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumInt(    NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);
        public static long    Sum<TSource, Inner, InnerEnumerator>(   in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumLong(   NodeImpl.Select(in inner.Node, selector), SIMDOptions.OnlyIfSame);

        public static decimal? Sum<Inner, InnerEnumerator>(in this ValueEnumerable<decimal?, Inner, InnerEnumerator> inner) where Inner : INode<Decimal?> where InnerEnumerator : struct, IPullEnumerator<decimal?>, INodeEnumerator<Inner, InnerEnumerator>  =>  NodeImpl.SumNullableDecimal(in inner.Node);
        public static double?  Sum<Inner, InnerEnumerator>(in this ValueEnumerable<double?,  Inner, InnerEnumerator> inner) where Inner : INode<double?> where InnerEnumerator : struct, IPullEnumerator<double?>, INodeEnumerator<Inner, InnerEnumerator>   =>  NodeImpl.SumNullableDouble( in inner.Node);
        public static float?   Sum<Inner, InnerEnumerator>(in this ValueEnumerable<float?,   Inner, InnerEnumerator> inner) where Inner : INode<float?> where InnerEnumerator : struct, IPullEnumerator<float?>, INodeEnumerator<Inner, InnerEnumerator>    =>  NodeImpl.SumNullableFloat(  in inner.Node);
        public static int?     Sum<Inner, InnerEnumerator>(in this ValueEnumerable<int?,     Inner, InnerEnumerator> inner) where Inner : INode<int?> where InnerEnumerator : struct, IPullEnumerator<int?>, INodeEnumerator<Inner, InnerEnumerator>      =>  NodeImpl.SumNullableInt(    in inner.Node);
        public static long?    Sum<Inner, InnerEnumerator>(in this ValueEnumerable<long?,    Inner, InnerEnumerator> inner) where Inner : INode<long?> where InnerEnumerator : struct, IPullEnumerator<long?>, INodeEnumerator<Inner, InnerEnumerator>     =>  NodeImpl.SumNullableLong(   in inner.Node);

        public static decimal? Sum<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, decimal?> selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumNullableDecimal(NodeImpl.Select(in inner.Node, selector));
        public static double?  Sum<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, double?>  selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumNullableDouble( NodeImpl.Select(in inner.Node, selector));
        public static float?   Sum<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, float?>   selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumNullableFloat(  NodeImpl.Select(in inner.Node, selector));
        public static int?     Sum<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, int?>     selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumNullableInt(    NodeImpl.Select(in inner.Node, selector));
        public static long?    Sum<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, Func<TSource, long?>    selector) where Inner : INode<TSource> where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator> => NodeImpl.SumNullableLong(   NodeImpl.Select(in inner.Node, selector));

        public static int Count<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Count<TSource, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static int Count<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Count<TSource, Inner>(in source.Node, predicate);

        public static long LongCount<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, bool ignorePotentialSideEffects = false)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LongCount<TSource, Inner>(in inner.Node, ignorePotentialSideEffects);

        public static long LongCount<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, Func<TSource, bool> predicate)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.LongCount<TSource, Inner>(in source.Node, predicate);

        public static bool TryGetNonEnumeratedCount<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, out int count)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.TryGetNonEnumeratedCount<TSource, Inner>(in source.Node, out count);

        public static bool Contains<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, TSource value)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Contains(inner.Node, value);

        public static bool Contains<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> inner, TSource value, IEqualityComparer<TSource> comparer)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => NodeImpl.Contains(in inner.Node, value, comparer);

        public static bool SequenceEqual<TSource, First, FirstEnumerator, Second, SecondEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, in ValueEnumerable<TSource, Second, SecondEnumerator> second)
            where First : INode<TSource>
            where Second : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            where SecondEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Second, SecondEnumerator>
            => NodeImpl.SequenceEqual<TSource, First, Second, SecondEnumerator>(in first.Node, in second.Node, null);

        public static bool SequenceEqual<TSource, First, FirstEnumerator, Second, SecondEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, in ValueEnumerable<TSource, Second, SecondEnumerator> second, IEqualityComparer<TSource>? comparer)
            where First : INode<TSource>
            where Second : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            where SecondEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Second, SecondEnumerator>
            => NodeImpl.SequenceEqual<TSource, First, Second, SecondEnumerator>(in first.Node, in second.Node, comparer);

        public static bool SequenceEqual<TSource, First, FirstEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, IEnumerable<TSource> second)
            where First : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            => first.SequenceEqual(second, null);

        public static bool SequenceEqual<TSource, First, FirstEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer)
            where First : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            var secondNode = new SystemIEnumerable.Node<TSource>(second);
            var equal = NodeImpl.SequenceEqual<TSource, First, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>(in first.Node, in secondNode, comparer);

            return equal;
        }

        public static ValueEnumerable<TResult, Nodes.SelectMany.Node<TResult,ValueEnumerable<TResult, NodeU, NodeUEnumerator>, Nodes.Select.Node<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT>>, Nodes.SelectMany.Pull<TResult,ValueEnumerable<TResult, NodeU, NodeUEnumerator>, Nodes.Select.Node<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT>, Nodes.Select.Pull<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT, NodeTEnumerator>, ValueEnumerableEnumerator<TResult, NodeU, NodeUEnumerator>>> SelectMany<TSource, TResult, NodeT, NodeU, NodeTEnumerator, NodeUEnumerator>(in this ValueEnumerable<TSource, NodeT, NodeTEnumerator> prior, Func<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            where NodeTEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<NodeT, NodeTEnumerator>
            where NodeUEnumerator : struct, IPullEnumerator<TResult>, INodeEnumerator<NodeU, NodeUEnumerator>
            => new (NodeImpl.SelectMany<TSource, TResult, NodeT, ValueEnumerable<TResult, NodeU, NodeUEnumerator>>(in prior.Node, selector));

        public static ValueEnumerable<TResult, Nodes.SelectMany.Node<TResult,ValueEnumerable<TResult, NodeU, NodeUEnumerator>, Nodes.SelectIdx.Node<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT>>, Nodes.SelectMany.Pull<TResult,ValueEnumerable<TResult, NodeU, NodeUEnumerator>, Nodes.SelectIdx.Node<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT>, Nodes.SelectIdx.Pull<TSource, ValueEnumerable<TResult, NodeU, NodeUEnumerator>, NodeT, NodeTEnumerator>, ValueEnumerableEnumerator<TResult, NodeU, NodeUEnumerator>>> SelectMany<TSource, TResult, NodeT, NodeU, NodeTEnumerator, NodeUEnumerator>(in this ValueEnumerable<TSource, NodeT, NodeTEnumerator> prior, Func<TSource, int, ValueEnumerable<TResult, NodeU, NodeUEnumerator>> selector)
            where NodeT : INode<TSource>
            where NodeU : INode<TResult>
            where NodeTEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<NodeT, NodeTEnumerator>
            where NodeUEnumerator : struct, IPullEnumerator<TResult>, INodeEnumerator<NodeU, NodeUEnumerator>
            => new(NodeImpl.SelectMany<TSource, TResult, NodeT, ValueEnumerable<TResult, NodeU, NodeUEnumerator>>(in prior.Node, selector));

        public static ValueEnumerable<TResult, Nodes.SelectManyResult.Node<TSource, TCollection, TResult,ValueEnumerable<TCollection, NodeU, NodeUEnumerator>, Nodes.Select.Node<TSource, (TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>), NodeT>>, Nodes.SelectManyResult.Pull<TSource, TCollection, TResult,ValueEnumerable<TCollection, NodeU, NodeUEnumerator>, Nodes.Select.Node<TSource, (TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>), NodeT>, Nodes.Select.Pull<TSource, (TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>), NodeT, NodeTEnumerator>, ValueEnumerableEnumerator<TCollection, NodeU, NodeUEnumerator>>> SelectMany<TSource, TCollection, TResult, NodeT, NodeU, NodeTEnumerator, NodeUEnumerator>(in this ValueEnumerable<TSource, NodeT, NodeTEnumerator> prior, Func<TSource, ValueEnumerable<TCollection, NodeU, NodeUEnumerator>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
            where NodeT : INode<TSource>
            where NodeU : INode<TCollection>
            where NodeTEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<NodeT, NodeTEnumerator>
            where NodeUEnumerator : struct, IPullEnumerator<TCollection>, INodeEnumerator<NodeU, NodeUEnumerator>
            => new(NodeImpl.SelectMany<TSource, TCollection, TResult, NodeT, NodeU, NodeUEnumerator>(in prior.Node, collectionSelector, resultSelector));

        public static ValueEnumerable<TSource, CisternConcat.Node<TSource, First, Second>, CisternConcat.Pull<TSource, First, Second, FirstEnumerator, SecondEnumerator>> Concat<TSource, First, Second, FirstEnumerator, SecondEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, in ValueEnumerable<TSource, Second, SecondEnumerator> second)
            where First : INode<TSource>
            where Second : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            where SecondEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Second, SecondEnumerator>
            => new (NodeImpl.Concat<TSource, First, Second>(in first.Node, in second.Node));

        public static ValueEnumerable<TSource, CisternConcat.Node<TSource, First, SystemArray.Node<TSource>>, CisternConcat.Pull<TSource, First, SystemArray.Node<TSource>, FirstEnumerator, SystemArray.Pull<TSource>>> Concat<TSource, First, FirstEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, TSource[] second)
            where First : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return new (NodeImpl.Concat<TSource, First, SystemArray.Node<TSource>>(in first.Node, ToNode(second)));
        }

        public static ValueEnumerable<TSource, CisternConcat.Node<TSource, First, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, First, SystemIEnumerable.Node<TSource>, FirstEnumerator, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>> Concat<TSource, First, FirstEnumerator>(in this ValueEnumerable<TSource, First, FirstEnumerator> first, IEnumerable<TSource> second)
            where First : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
        {
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            return new (NodeImpl.Concat<TSource, First, SystemIEnumerable.Node<TSource>>(in first.Node, ToNode(second)));
        }

        public static ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> Append<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, TSource element)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (NodeImpl.Append<TSource, Inner>(in source.Node, element));

        public static ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> Prepend<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, TSource element)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (NodeImpl.Prepend<TSource, Inner>(in source.Node, element));

        // more specific than the above, so an Append or Prepend onto one extends that node and the type does not grow
        public static ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> Append<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> source, TSource element)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (source.Node.Append(element));

        public static ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> Prepend<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Nodes.AppendPrepend.Node<TSource, Inner>, Nodes.AppendPrepend.Pull<TSource, Inner, InnerEnumerator>> source, TSource element)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (source.Node.Prepend(element));

        public static ValueEnumerable<TSource, Nodes.Reverse.Node<TSource, Inner>, Nodes.Reverse.Pull<TSource, Inner>> Reverse<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source, int? maybeMaxCountForStackBasedPath = 64, (ArrayPool<TSource> arrayPool, bool cleanBuffers)? arrayPoolInfo = null)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (NodeImpl.Reverse<TSource, Inner>(in source.Node, maybeMaxCountForStackBasedPath, arrayPoolInfo));

        public static ValueEnumerable<TSource, Nodes.Shuffle.Node<TSource, Inner>, Nodes.Shuffle.Pull<TSource, Inner>> Shuffle<TSource, Inner, InnerEnumerator>(in this ValueEnumerable<TSource, Inner, InnerEnumerator> source)
            where Inner : INode<TSource>
            where InnerEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Inner, InnerEnumerator>
            => new (NodeImpl.Shuffle<TSource, Inner>(in source.Node));

        public static ValueEnumerable<U, Nodes.ValueSelect.Node<TSource, U, TPrior, IFunc>, Nodes.ValueSelect.Pull<TSource, U, TPrior, TPriorEnumerator, IFunc>> Select<TSource, U, TPrior, IFunc, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, IFunc selector, U? u = default)
            where TPrior : INode<TSource>
            where IFunc : IFunc<TSource, U>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Select<TSource, U, TPrior, IFunc>(in prior.Node, selector, u));

        public static ValueEnumerable<TSource, Nodes.ValueWhere.Node<TSource, TPrior, Predicate>, Nodes.ValueWhere.Pull<TSource, TPrior, TPriorEnumerator, Predicate>> Where<TSource, TPrior, Predicate, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Predicate predicate)
            where TPrior : INode<TSource>
            where Predicate : IFunc<TSource, bool>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Where<TSource, TPrior, Predicate>(in prior.Node, predicate));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>> OrderBy<TSource, TKey, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.OrderBy<TSource, TKey, TPrior>(in prior.Node, keySelector, comparer, false));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>> OrderByDescending<TSource, TKey, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.OrderBy<TSource, TKey, TPrior>(in prior.Node, keySelector, comparer, true));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TSource, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TSource, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>> Order<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, IComparer<TSource>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Order<TSource, TPrior>(in prior.Node, comparer, false));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TSource, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TSource, Nodes.OrderBy.KeySelectorsRoot<TSource>>, TPrior>> OrderDescending<TSource, TPrior, TPriorEnumerator>(in this ValueEnumerable<TSource, TPrior, TPriorEnumerator> prior, IComparer<TSource>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<TPrior, TPriorEnumerator>
            => new (NodeImpl.Order<TSource, TPrior>(in prior.Node, comparer, true));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>> ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in this ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, TPriorKeySelector, TPrior>, Nodes.OrderBy.Pull<TSource, TPriorKeySelector, TPrior>> prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorKeySelector : Nodes.OrderBy.IKeySelectors<TSource>
            => new (NodeImpl.ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in prior.Node, keySelector, comparer, false));

        public static ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>, Nodes.OrderBy.Pull<TSource, Nodes.OrderBy.KeySelectors<TSource, TKey, TPriorKeySelector>, TPrior>> ThenByDescending<TSource, TKey, TPriorKeySelector, TPrior>(in this ValueEnumerable<TSource, Nodes.OrderBy.Node<TSource, TPriorKeySelector, TPrior>, Nodes.OrderBy.Pull<TSource, TPriorKeySelector, TPrior>> prior, Func<TSource, TKey> keySelector, IComparer<TKey>? comparer = null)
            where TPrior : INode<TSource>
            where TPriorKeySelector : Nodes.OrderBy.IKeySelectors<TSource>
            => new (NodeImpl.ThenBy<TSource, TKey, TPriorKeySelector, TPrior>(in prior.Node, keySelector, comparer, true));

        public static ValueEnumerable<(TFirst First, TSecond Second), Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>,Nodes.Zip.Pull<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>, FirstNodeEnumerator>> Zip<TFirst, TSecond, FirstNode, SecondNode, FirstNodeEnumerator, SecondNodeEnumerator>(in this ValueEnumerable<TFirst, FirstNode, FirstNodeEnumerator> first, in ValueEnumerable<TSecond, SecondNode, SecondNodeEnumerator> second)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where FirstNodeEnumerator : struct, IPullEnumerator<TFirst>, INodeEnumerator<FirstNode, FirstNodeEnumerator>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
            => new (NodeImpl.Zip<TFirst, TSecond, FirstNode, SecondNode, SecondNodeEnumerator>(in first.Node, in second.Node));

        public static ValueEnumerable<TResult, Nodes.Zip.Node<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.FuncSelector<TFirst, TSecond, TResult>>, Nodes.Zip.Pull<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.FuncSelector<TFirst, TSecond, TResult>, FirstNodeEnumerator>> Zip<TFirst, TSecond, TResult, FirstNode, SecondNode, FirstNodeEnumerator, SecondNodeEnumerator>(in this ValueEnumerable<TFirst, FirstNode, FirstNodeEnumerator> first, in ValueEnumerable<TSecond, SecondNode, SecondNodeEnumerator> second, Func<TFirst, TSecond, TResult> resultSelector)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where FirstNodeEnumerator : struct, IPullEnumerator<TFirst>, INodeEnumerator<FirstNode, FirstNodeEnumerator>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
            => new (NodeImpl.Zip<TFirst, TSecond, TResult, FirstNode, SecondNode, SecondNodeEnumerator>(in first.Node, in second.Node, resultSelector));

        public static ValueEnumerable<(TFirst First, TSecond Second, TThird Third), Nodes.Zip.Node<(TFirst, TSecond), TThird, (TFirst, TSecond, TThird), Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>, ThirdNode, ThirdNodeEnumerator, Nodes.Zip.TripleSelector<TFirst, TSecond, TThird>>, Nodes.Zip.Pull<(TFirst, TSecond), TThird, (TFirst, TSecond, TThird), Nodes.Zip.Node<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>>, ThirdNode, ThirdNodeEnumerator, Nodes.Zip.TripleSelector<TFirst, TSecond, TThird>, Nodes.Zip.Pull<TFirst, TSecond, (TFirst, TSecond), FirstNode, SecondNode, SecondNodeEnumerator, Nodes.Zip.PairSelector<TFirst, TSecond>, FirstNodeEnumerator>>> Zip<TFirst, TSecond, TThird, FirstNode, SecondNode, ThirdNode, FirstNodeEnumerator, SecondNodeEnumerator, ThirdNodeEnumerator>(in this ValueEnumerable<TFirst, FirstNode, FirstNodeEnumerator> first, in ValueEnumerable<TSecond, SecondNode, SecondNodeEnumerator> second, in ValueEnumerable<TThird, ThirdNode, ThirdNodeEnumerator> third)
            where FirstNode : INode<TFirst>
            where SecondNode : INode<TSecond>
            where ThirdNode : INode<TThird>
            where FirstNodeEnumerator : struct, IPullEnumerator<TFirst>, INodeEnumerator<FirstNode, FirstNodeEnumerator>
            where SecondNodeEnumerator : struct, IPullEnumerator<TSecond>, INodeEnumerator<SecondNode, SecondNodeEnumerator>
            where ThirdNodeEnumerator : struct, IPullEnumerator<TThird>, INodeEnumerator<ThirdNode, ThirdNodeEnumerator>
            => new (NodeImpl.Zip<TFirst, TSecond, TThird, FirstNode, SecondNode, ThirdNode, SecondNodeEnumerator, ThirdNodeEnumerator>(in first.Node, in second.Node, in third.Node));

        public static (U, V) Fork<T, U, V, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<T, InnerNode, InnerNodeEnumerator> node, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v)
            where InnerNode : INode<T>
            where InnerNodeEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => NodeImpl.Fork<T, U, V, InnerNode>(in node.Node, t2u, t2v);

        public static (U, V, W) Fork<T, U, V, W, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<T, InnerNode, InnerNodeEnumerator> node, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, U> t2u, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, V> t2v, Func<ValueEnumerable<T, Aggregation.Fork<T>, Aggregation.ForkEnumerator<T>>, W> t2w)
            where InnerNode : INode<T>
            where InnerNodeEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => NodeImpl.Fork<T, U, V, W, InnerNode>(in node.Node, t2u, t2v, t2w);

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, InnerNode, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, InnerNode, InnerNodeEnumerator, Nodes.Except.IdentityKey<TSource>>> Except<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Except<TSource, InnerNode>(in node.Node, second, comparer));
        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, InnerNode, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, InnerNode, InnerNodeEnumerator, Nodes.Except.IdentityKey<TSource>>> Distinct<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, IEqualityComparer<TSource>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Except<TSource, InnerNode>(in node.Node, Array.Empty<TSource>(), comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TKey, InnerNode, FuncToIFunc<TSource, TKey>>, Nodes.Except.Pull<TSource, TKey, InnerNode, InnerNodeEnumerator, FuncToIFunc<TSource, TKey>>> DistinctBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.DistinctBy<TSource, TKey, InnerNode>(in node.Node, keySelector, comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TKey, InnerNode, FuncToIFunc<TSource, TKey>>, Nodes.Except.Pull<TSource, TKey, InnerNode, InnerNodeEnumerator, FuncToIFunc<TSource, TKey>>> ExceptBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.ExceptBy<TSource, TKey, InnerNode>(in node.Node, second, keySelector, comparer));

        public static ValueEnumerable<TSource, Nodes.Intersect.Node<TSource, TSource, InnerNode, Nodes.Except.IdentityKey<TSource>>, Nodes.Intersect.Pull<TSource, TSource, InnerNode, InnerNodeEnumerator, Nodes.Except.IdentityKey<TSource>>> Intersect<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Intersect<TSource, InnerNode>(in node.Node, second, comparer));

        public static ValueEnumerable<TSource, Nodes.Intersect.Node<TSource, TKey, InnerNode, FuncToIFunc<TSource, TKey>>, Nodes.Intersect.Pull<TSource, TKey, InnerNode, InnerNodeEnumerator, FuncToIFunc<TSource, TKey>>> IntersectBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, IEnumerable<TKey> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.IntersectBy<TSource, TKey, InnerNode>(in node.Node, second, keySelector, comparer));

        public static ValueEnumerable<TResult, Nodes.Join.Node<TOuter, TInner, TKey, TResult, OuterNode, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.Join.Pull<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> Join<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.Join<TOuter, TInner, TKey, TResult, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

        public static ValueEnumerable<(TOuter Outer, TInner Inner), Nodes.Join.Node<TOuter, TInner, TKey, (TOuter Outer, TInner Inner), OuterNode, Nodes.Zip.PairSelector<TOuter, TInner>>, Nodes.Join.Pull<TOuter, TInner, TKey, (TOuter Outer, TInner Inner), OuterNode, OuterNodeEnumerator, Nodes.Zip.PairSelector<TOuter, TInner>>> Join<TOuter, TInner, TKey, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.Join<TOuter, TInner, TKey, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, comparer));

        public static ValueEnumerable<TResult, Nodes.LeftJoin.Node<TOuter, TInner, TKey, TResult, OuterNode, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.LeftJoin.Pull<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator, SystemIEnumerable.Node<TInner>, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> LeftJoin<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.LeftJoin<TOuter, TInner, TKey, TResult, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

        public static ValueEnumerable<(TOuter Outer, TInner? Inner), Nodes.LeftJoin.Node<TOuter, TInner, TKey, (TOuter Outer, TInner? Inner), OuterNode, SystemIEnumerable.Node<TInner>, Nodes.LeftJoin.PairSelector<TOuter, TInner>>, Nodes.LeftJoin.Pull<TOuter, TInner, TKey, (TOuter Outer, TInner? Inner), OuterNode, OuterNodeEnumerator, SystemIEnumerable.Node<TInner>, Nodes.LeftJoin.PairSelector<TOuter, TInner>>> LeftJoin<TOuter, TInner, TKey, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.LeftJoin<TOuter, TInner, TKey, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, comparer));

        public static ValueEnumerable<TResult, Nodes.LeftJoin.Node<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, OuterNode, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>>, Nodes.LeftJoin.Pull<TInner, TOuter, TKey, TResult, SystemIEnumerable.Node<TInner>, ErasedEnumerator<TInner, SystemIEnumerable.Node<TInner>>, OuterNode, Nodes.RightJoin.FuncSelector<TInner, TOuter, TResult>>> RightJoin<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.RightJoin<TOuter, TInner, TKey, TResult, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

        public static ValueEnumerable<(TOuter? Outer, TInner Inner), Nodes.LeftJoin.Node<TInner, TOuter, TKey, (TOuter? Outer, TInner Inner), SystemIEnumerable.Node<TInner>, OuterNode, Nodes.RightJoin.PairSelector<TInner, TOuter>>, Nodes.LeftJoin.Pull<TInner, TOuter, TKey, (TOuter? Outer, TInner Inner), SystemIEnumerable.Node<TInner>, ErasedEnumerator<TInner, SystemIEnumerable.Node<TInner>>, OuterNode, Nodes.RightJoin.PairSelector<TInner, TOuter>>> RightJoin<TOuter, TInner, TKey, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.RightJoin<TOuter, TInner, TKey, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, comparer));

        public static ValueEnumerable<TResult, Nodes.FullJoin.Node<TOuter, TInner, TKey, TResult, OuterNode, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>, Nodes.FullJoin.Pull<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator, Nodes.Zip.FuncSelector<TOuter, TInner, TResult>>> FullJoin<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter?, TInner?, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.FullJoin<TOuter, TInner, TKey, TResult, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

        public static ValueEnumerable<(TOuter? Outer, TInner? Inner), Nodes.FullJoin.Node<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), OuterNode, Nodes.FullJoin.PairSelector<TOuter, TInner>>, Nodes.FullJoin.Pull<TOuter, TInner, TKey, (TOuter? Outer, TInner? Inner), OuterNode, OuterNodeEnumerator, Nodes.FullJoin.PairSelector<TOuter, TInner>>> FullJoin<TOuter, TInner, TKey, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.FullJoin<TOuter, TInner, TKey, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupJoin.Node<TOuter, TInner, TKey, TResult, OuterNode>, Nodes.GroupJoin.Pull<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>> GroupJoin<TOuter, TInner, TKey, TResult, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, Func<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.GroupJoin<TOuter, TInner, TKey, TResult, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, resultSelector, comparer));

        public static ValueEnumerable<System.Linq.IGrouping<TOuter, TInner>, Nodes.GroupJoin.Node<TOuter, TInner, TKey, System.Linq.IGrouping<TOuter, TInner>, OuterNode>, Nodes.GroupJoin.Pull<TOuter, TInner, TKey, System.Linq.IGrouping<TOuter, TInner>, OuterNode, OuterNodeEnumerator>> GroupJoin<TOuter, TInner, TKey, OuterNode, OuterNodeEnumerator>(in this ValueEnumerable<TOuter, OuterNode, OuterNodeEnumerator> outer, IEnumerable<TInner> inner, Func<TOuter, TKey> outerKeySelector, Func<TInner, TKey> innerKeySelector, IEqualityComparer<TKey>? comparer = null)
            where OuterNode : INode<TOuter>
            where OuterNodeEnumerator : struct, IPullEnumerator<TOuter>, INodeEnumerator<OuterNode, OuterNodeEnumerator>
            => new (NodeImpl.GroupJoin<TOuter, TInner, TKey, OuterNode>(in outer.Node, inner, outerKeySelector, innerKeySelector, comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, InnerNode, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, InnerNode, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, InnerNode, SystemIEnumerable.Node<TSource>, InnerNodeEnumerator, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> Union<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Union<TSource, InnerNode>(in first.Node, second, comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> Union<TSource>(in this ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
            => new (NodeImpl.UnionOfEnumerables(in first, second, comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, SystemIEnumerable.Node<TSource>, SystemIEnumerable.Node<TSource>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, Nodes.Except.IdentityKey<TSource>>> Union<TSource, First, Second, FirstEnumerator, SecondEnumerator>(in this ValueEnumerable<TSource, Nodes.Except.Node<TSource, TSource, CisternConcat.Node<TSource, First, Second>, Nodes.Except.IdentityKey<TSource>>, Nodes.Except.Pull<TSource, TSource, CisternConcat.Node<TSource, First, Second>, CisternConcat.Pull<TSource, First, Second, FirstEnumerator, SecondEnumerator>, Nodes.Except.IdentityKey<TSource>>> first, IEnumerable<TSource> second, IEqualityComparer<TSource>? comparer = null)
            where First : INode<TSource>
            where Second : INode<TSource>
            where FirstEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<First, FirstEnumerator>
            where SecondEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<Second, SecondEnumerator>
            => new (NodeImpl.UnionOfUnion(in first, second, comparer));

        public static ValueEnumerable<TSource, Nodes.Except.Node<TSource, TKey, CisternConcat.Node<TSource, InnerNode, SystemIEnumerable.Node<TSource>>, FuncToIFunc<TSource, TKey>>, Nodes.Except.Pull<TSource, TKey, CisternConcat.Node<TSource, InnerNode, SystemIEnumerable.Node<TSource>>, CisternConcat.Pull<TSource, InnerNode, SystemIEnumerable.Node<TSource>, InnerNodeEnumerator, ErasedEnumerator<TSource, SystemIEnumerable.Node<TSource>>>, FuncToIFunc<TSource, TKey>>> UnionBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.UnionBy<TSource, TKey, InnerNode>(in first.Node, second, keySelector, comparer));

        public static ValueEnumerable<KeyValuePair<TKey, TAccumulate>, Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, InnerNode>, Nodes.AggregateBy.Pull<TSource, TKey, TAccumulate, InnerNode>> AggregateBy<TSource, TKey, TAccumulate, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            where TKey : notnull
            => new (NodeImpl.AggregateBy<TSource, TKey, TAccumulate, InnerNode>(in node.Node, keySelector, seed, func, keyComparer));

        public static ValueEnumerable<KeyValuePair<TKey, TAccumulate>, Nodes.AggregateBy.Node<TSource, TKey, TAccumulate, InnerNode>, Nodes.AggregateBy.Pull<TSource, TKey, TAccumulate, InnerNode>> AggregateBy<TSource, TKey, TAccumulate, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, Func<TKey, TAccumulate> seedSelector, Func<TAccumulate, TSource, TAccumulate> func, IEqualityComparer<TKey>? keyComparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            where TKey : notnull
            => new (NodeImpl.AggregateBy<TSource, TKey, TAccumulate, InnerNode>(in node.Node, keySelector, seedSelector, func, keyComparer));

        public static ValueEnumerable<TSource, Nodes.DefaultIfEmpty.Node<TSource, InnerNode>, Nodes.DefaultIfEmpty.Pull<TSource, InnerNode, InnerNodeEnumerator>> DefaultIfEmpty<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.DefaultIfEmpty<TSource, InnerNode>(in node.Node, default!));

        public static ValueEnumerable<TSource, Nodes.DefaultIfEmpty.Node<TSource, InnerNode>, Nodes.DefaultIfEmpty.Pull<TSource, InnerNode, InnerNodeEnumerator>> DefaultIfEmpty<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, TSource defaultValue)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.DefaultIfEmpty<TSource, InnerNode>(in node.Node, defaultValue));

        public static ValueEnumerable<(int Index, TSource Item), Nodes.Indexed.Node<TSource, InnerNode>, Nodes.Indexed.Pull<TSource, InnerNode, InnerNodeEnumerator>> Index<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Indexed<TSource, InnerNode>(in node.Node));

        public static ValueEnumerable<TSource[], Nodes.Chunk.Node<TSource, InnerNode>, Nodes.Chunk.Pull<TSource, InnerNode, InnerNodeEnumerator>> Chunk<TSource, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, int size)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.Chunk<TSource, InnerNode>(in node.Node, size));

        public static ValueEnumerable<KeyValuePair<TKey, int>, Nodes.CountBy.Node<TSource, TKey, InnerNode>, Nodes.CountBy.Pull<TSource, TKey, InnerNode>> CountBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? keyComparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            where TKey : notnull
            => new (NodeImpl.CountBy<TSource, TKey, InnerNode>(in node.Node, keySelector, keyComparer));

        public static ValueEnumerable<System.Linq.IGrouping<TKey, TSource>, Nodes.GroupBy.Node<TSource, TKey, InnerNode>, Nodes.GroupBy.Pull<TSource, TKey, InnerNode>> GroupBy<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new (NodeImpl.GroupBy(node.Node, keySelector, comparer));
        public static ValueEnumerable<System.Linq.IGrouping<TKey, TElement>, Nodes.GroupBy.Node<TSource, TKey, TElement, InnerNode>, Nodes.GroupBy.Pull<TSource, TKey, TElement, InnerNode>> GroupBy<TSource, TKey, TElement, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new(NodeImpl.GroupBy(node.Node, keySelector, elementSelector, comparer));

        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TResult, InnerNode>, Nodes.GroupByResult.Pull<TSource, TKey, TResult, InnerNode>> GroupBy<TSource, TKey, TResult, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, Func<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new(NodeImpl.GroupBy(node.Node, keySelector, resultSelector, comparer));
        public static ValueEnumerable<TResult, Nodes.GroupByResult.Node<TSource, TKey, TElement, TResult, InnerNode>, Nodes.GroupByResult.Pull<TSource, TKey, TElement, TResult, InnerNode>> GroupBy<TSource, TKey, TElement, TResult, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => new(NodeImpl.GroupBy(node.Node, keySelector, elementSelector, resultSelector, comparer));

        public static System.Linq.ILookup<TKey, TSource> ToLookup<TSource, TKey, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => NodeImpl.ToLookup(node.Node, keySelector, comparer);
        public static System.Linq.ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement, InnerNode, InnerNodeEnumerator>(in this ValueEnumerable<TSource, InnerNode, InnerNodeEnumerator> node, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer = null)
            where InnerNode : INode<TSource>
            where InnerNodeEnumerator : struct, IPullEnumerator<TSource>, INodeEnumerator<InnerNode, InnerNodeEnumerator>
            => NodeImpl.ToLookup(node.Node, keySelector, elementSelector, comparer);

    }
}
