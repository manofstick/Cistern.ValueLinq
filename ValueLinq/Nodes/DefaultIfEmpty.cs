namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public static partial class DefaultIfEmpty
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationContains<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationAsSpanSource<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal T _defaultValue;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            if (info.MaximumLength is long maximumLength)
                info.MaximumLength = Math.Max(1, maximumLength);
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            int sourceCount;
            if (!request.DryRun)
                sourceCount = NodeImpl.Count<T, NodeT>(in _nodeT, request.IgnorePotentialSideEffects);
            else if (!NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out sourceCount))
            {
                response = default;

                return false;
            }

            response = Math.Max(1, sourceCount);

            return true;
        }

        // any value but the default is held here exactly where the source holds it, and the default where the source holds
        // it or is empty, which only a known count says without running it; the source is asked in its place
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            if (EqualityComparer<T>.Default.Equals(request.Value, _defaultValue))
            {
                if (!NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out var sourceCount))
                {
                    response = default;

                    return false;
                }

                if (sourceCount == 0)
                {
                    response = true;

                    return true;
                }
            }

            return _nodeT.TryPushOptimization<Optimizations.Contains<T>, bool>(in request, out response);
        }

        // the copy follows a dry run that answered, so the source answers both too
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (request.IsDryRun)
            {
                if (!Optimizations.CopyTo.TryGetCount<T, NodeT>(ref _nodeT, out var count))
                {
                    response = default;

                    return false;
                }

                response = Math.Max(1, count);

                return true;
            }

            var sourceCount = Optimizations.CopyTo.Copy<T, NodeT>(ref _nodeT, in request);
            if (sourceCount == 0)
                request.Span[0] = _defaultValue;

            response = Math.Max(1, sourceCount);

            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            if (!Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var reversedSource))
            {
                response = default;

                return false;
            }

            var reversed = new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in reversedSource), _defaultValue);

            Unsafe.SkipInit(out response);
            response.SetNode(reversed);

            return true;
        }

        // past the first element this sequence is its source's, since an empty source has nothing past the default
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            Unsafe.SkipInit(out response);
            if (request.Skip > 0)
                return Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, request.Skip, request.Take, ref response);

            if (request.Take == 0)
            {
                response.SetEmpty();

                return true;
            }

            response = default;

            return false;
        }

        // a non-empty source's span is this sequence; an empty one's holds no default
        private bool AsSpanSource(in Optimizations.AsSpanSource request, out Optimizations.SpanSource<T> response)
        {
            if (_nodeT.TryPushOptimization<Optimizations.AsSpanSource, Optimizations.SpanSource<T>>(in request, out response)
                && !response.GetSpan().IsEmpty)
            {
                return true;
            }

            response = default;

            return false;
        }

        // a source with no first element (or no last) is empty, so the default is this sequence's first and last
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in request, out response))
                return false;

            var index = request.Index;
            var firstOrLast = index.Value == (index.IsFromEnd ? 1 : 0);
            if (!response.Item1
                && firstOrLast)
            {
                response = (true, _defaultValue);
            }

            return true;
        }
    }

    partial struct Push<T, Next>
    {
        bool _sourceHadElements;

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => Next.AllowBatchesWhen;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            if (batch.Count > 0)
                _sourceHadElements = true;

            return _next.ProcessBatch(ref batch);
        }

        public bool ProcessNext(T input)
        {
            _sourceHadElements = true;

            return _next.ProcessNext(input);
        }

        public TPushResult GetResult<TPushResult>()
        {
            if (!_sourceHadElements)
                _ = _next.ProcessNext(_defaultValue);

            return _next.GetResult<TPushResult>();
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        const int SourceEmpty = 0;
        const int InSource = 1;
        const int AtDefault = 2;
        const int Done = 3;

        private int _state;

        public bool MoveNext()
        {
            if (_state <= InSource
                && _enumerator.MoveNext())
            {
                _state = InSource;

                return true;
            }

            _state = _state == SourceEmpty ? AtDefault : Done;

            return _state == AtDefault;
        }

        public T Current => _state == AtDefault ? _defaultValue : _enumerator.Current;
    }
}
