namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Runtime.CompilerServices;

// a node whose elements exist once it has read its source into an array of its own; a window of it is built with no
// more work than the window needs (OrderBy orders only the window's elements)
internal interface IBuffered<T>
{
    ReadOnlyMemory<T> GetWindow(long skip, long take);

    // whether the window holds the value, where it is answered without building the buffer; the node's own Contains is
    // this over its whole sequence, so each node decides both in one place
    bool TryContains(long skip, long take, T value, out bool found);
}

// A window of a buffering node (OrderBy, Reverse, Shuffle), its answer to the SkipTake request. The buffer is built only
// when the window is run, so a request a Skip or Take forwards through the window builds nothing it does not use.
internal static partial class BufferedWindow
{
    [Answerer]
    internal readonly partial struct Node<T, TBuffered>
        : INode<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationContains<T>
        , Optimizations.IOptimizationAny
        where TBuffered : struct, INode<T>, IBuffered<T>
    {
        internal readonly TBuffered _buffered;
        internal readonly long _skip;
        internal readonly long _take;

        public Node(in TBuffered buffered, long skip, long take)
        {
            _buffered = buffered;
            _skip = skip;
            _take = take;
        }

        public void GetCountInformation(out CountInformation info)
        {
            _buffered.GetCountInformation(out info);
            if (info.MaximumLength is long length)
                info.MaximumLength = Math.Min(Math.Max(0, length - _skip), _take);
        }

        // the buffering node counts its source without building its buffer
        private bool Count(in Optimizations.Count request, out int response)
        {
            int count;
            if (!request.DryRun)
                count = NodeImpl.Count<T, TBuffered>(in _buffered, request.IgnorePotentialSideEffects);
            else if (!NodeImpl.TryGetNonEnumeratedCount<T, TBuffered>(in _buffered, out count))
            {
                response = default;
                return false;
            }

            response = (int)Math.Min(Math.Max(0, count - _skip), _take);
            return true;
        }

        // an index from the start is the buffering node's index past the skip; a from-end index needs the window's length
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            if (!index.IsFromEnd)
            {
                if (index.Value >= _take)
                {
                    response = (false, default!);
                    return true;
                }

                if (_skip <= int.MaxValue - index.Value)
                {
                    var buffered = _buffered;
                    return buffered.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(new Index((int)(_skip + index.Value))), out response);
                }
            }

            response = default;
            return false;
        }

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = Window().ToArray();
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Optimizations.SkipTake.Compose(_skip, _take, request.Skip, request.Take, out var skip, out var take);

            Unsafe.SkipInit(out response);
            if (take == 0)
                response.SetEmpty();
            else
                response.SetNode(new Node<T, TBuffered>(in _buffered, skip, take));

            return true;
        }

        public TPushResult CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            where TPushEnumerator : IPushEnumerator<T>
            where TPushFiller : IPushFiller<TPushEnumerator>, allows ref struct
            => SystemMemory.ExecutePush<T, TPushResult, TPushEnumerator>(Window(), PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        // whatever the buffering node does to the order, the window holds an element exactly when its take is positive
        // and the source has more than the skip; the buffering node answers both from its source, building nothing
        private bool Any(in Optimizations.Any request, out bool response)
        {
            response =
                _take > 0
                && (_skip == 0
                    ? NodeImpl.Any<T, TBuffered>(_buffered)
                    : NodeImpl.Count<T, TBuffered>(in _buffered, false) > _skip);
            return true;
        }

        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            var buffered = _buffered;
            return buffered.TryContains(_skip, _take, request.Value, out response);
        }

        internal ReadOnlyMemory<T> Window()
        {
            var buffered = _buffered;
            return buffered.GetWindow(_skip, _take);
        }
    }

    // the window is read on the first MoveNext, as OrderBy sorts there; a window known to be empty never reads it
    [Puller]
    internal partial struct Pull<T, TBuffered>
        : Optimizations.IPullOptimizationMergeSkipTake<T>
        where TBuffered : struct, INode<T>, IBuffered<T>
    {
        private enum State : byte
        {
            Unread,
            Read,
            Done,
        }

        private Node<T, TBuffered> _node;
        private SystemMemory.Pull<T> _enumerator;
        private State _state;

        private Pull(in Node<T, TBuffered> node, State state)
        {
            _node = node;
            _enumerator = default;
            _state = state;
        }

        private static Pull<T, TBuffered> Create(in Node<T, TBuffered> node)
            => new(in node, IsEmpty(in node) ? State.Done : State.Unread);

        // as Skip and Take leave a source unenumerated when they yield nothing of it
        private static bool IsEmpty(in Node<T, TBuffered> node)
        {
            if (node._take <= 0)
                return true;

            node.GetCountInformation(out var info);

            return !info.PotentialSideEffects && !info.IsStale && info.MaximumLength <= 0;
        }

        static bool IPullEnumerator<T>.RebuildChanges => false;

        static TBuilt IPullEnumerator<T>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
            => ChainBuilder.Identity<T, Node<T, TBuffered>, Pull<T, TBuffered>, TOwnNode, TBuilt, TBuilder>(in node, in builder);

        private static bool MergeSkipTake<TOwnNode, TBuilt, TBuilder>(in Optimizations.MergeSkipTake request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
            where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
        {
            ref readonly var window = ref ChainBuilder.As<TOwnNode, Node<T, TBuffered>>(in node);
            Optimizations.SkipTake.Compose(window._skip, window._take, request.Skip, request.Take, out var composedSkip, out var composedTake);

            built = builder.Build<Node<T, TBuffered>, Pull<T, TBuffered>>(new Node<T, TBuffered>(in window._buffered, composedSkip, composedTake));
            return true;
        }

        public bool MoveNext()
        {
            if (_state == State.Unread)
            {
                _enumerator = new SystemMemory.Pull<T>(_node.Window());
                _state = State.Read;
            }

            return
                _state == State.Read
                && _enumerator.MoveNext();
        }

        public T Current => _enumerator.Current;

        public void Dispose() => (_node, _enumerator, _state) = (default, default, State.Done);
    }

    // the window of a buffering node, built in place of the Skip or Take merged into it
    internal static TBuilt Window<T, TBuffered, TOwnNode, TBuilt, TBuilder>(in TOwnNode node, long skip, long take, in TBuilder builder)
        where TBuffered : struct, INode<T>, IBuffered<T>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
    {
        ref readonly var buffered = ref ChainBuilder.As<TOwnNode, TBuffered>(in node);
        Optimizations.SkipTake.Compose(0, long.MaxValue, skip, take, out var composedSkip, out var composedTake);

        return builder.Build<Node<T, TBuffered>, Pull<T, TBuffered>>(new Node<T, TBuffered>(in buffered, composedSkip, composedTake));
    }

    // A buffering node neither adds nor removes elements, so its source answers Count and Any in its place and nothing
    // is buffered: a dry run reads the source's count without enumerating it, a real count asks the source.
    internal static bool Count<T, TSource>(in TSource source, in Optimizations.Count request, out int response)
        where TSource : INode<T>
    {
        if (request.DryRun)
            return NodeImpl.TryGetNonEnumeratedCount<T, TSource>(in source, out response);

        response = NodeImpl.Count<T, TSource>(in source, request.IgnorePotentialSideEffects);
        return true;
    }

    internal static bool Any<T, TSource>(in TSource source, out bool response)
        where TSource : INode<T>
    {
        response = NodeImpl.Any<T, TSource>(source);
        return true;
    }

    // the window covers the whole sequence: nothing skipped, and the take unbounded or at least the source's length
    internal static bool CoversAll<T, TSource>(in TSource source, long skip, long take)
        where TSource : INode<T>
        => skip == 0
        && (take == long.MaxValue
         || (NodeImpl.TryGetNonEnumeratedCount<T, TSource>(in source, out var count) && take >= count));

    // the window's part of a buffer built whole
    internal static ReadOnlyMemory<T> Slice<T>(T[] buffer, long skip, long take)
    {
        var start = (int)Math.Min(skip, buffer.Length);
        var count = (int)Math.Min(take, buffer.Length - start);

        return new ReadOnlyMemory<T>(buffer, start, count);
    }
}
