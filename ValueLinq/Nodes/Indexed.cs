namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Runtime.CompilerServices;

public static partial class Indexed
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<(int Index, T Item)>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<(int Index, T Item)>
        , Optimizations.IOptimizationCopyTo<(int Index, T Item)>
        , Optimizations.IOptimizationSkipTake<(int Index, T Item)>
        where NodeT : INode<T>
    {
        // POTENTIAL OPTIMIZATION: Reverse, answered over a source that reverses itself with a node counting its index down from the source's length, which needs that length known when the request is asked
        internal NodeT _nodeT;
        [Propagate] internal int _start;

        public void GetCountInformation(out CountInformation info) => _nodeT.GetCountInformation(out info);

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
                return NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out response);

            response = NodeImpl.Count<T, NodeT>(in _nodeT, request.IgnorePotentialSideEffects);

            return true;
        }

        // POTENTIAL OPTIMIZATION: CopyTo over a source answering it without a span (a Concat, an AppendPrepend), its elements copied into a pooled buffer and paired from there
        private bool CopyTo(in Optimizations.CopyTo<(int Index, T Item)> request, out int response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source)
                || ((long)_start + source.GetSpan().Length) > ((long)int.MaxValue + 1))
            {
                response = default;

                return false;
            }

            var items = source.GetSpan();
            response = items.Length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span;
            for (var i = 0; i < items.Length; ++i)
                destination[i] = (_start + i, items[i]);

            return true;
        }

        // the slice of the source, its index starting where the slice does; a slice whose index would pass int.MaxValue is left to the push, which throws as it reaches it
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<(int Index, T Item)> response)
        {
            var sourceSlice = default(NodeContainer<T>);
            if (request.Skip > int.MaxValue - _start
                || !Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, request.Skip, request.Take, ref sourceSlice))
            {
                response = default;

                return false;
            }

            Unsafe.SkipInit(out response);
            if (sourceSlice.Type == NodeType.Empty)
                response.SetEmpty();
            else
                response.SetNode(new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in sourceSlice), _start + (int)request.Skip));

            return true;
        }

        // an index from the end lands at a position only a known source length gives, and the source answering may run
        // user code, so the length is checked before it is asked
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, (int Index, T Item)) response)
        {
            response = default;

            var index = request.Index;
            var position = index.Value;
            if (index.IsFromEnd)
            {
                _nodeT.GetCountInformation(out var info);
                if (info.IsStale
                    || info.ActualSize is not int length)
                {
                    return false;
                }

                position = length - index.Value;
            }

            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in request, out var answer))
                return false;

            if (!answer.Item1)
                return true;

            response = (true, (checked(_start + position), answer.Item2));

            return true;
        }
    }

    partial struct Push<T, Next>
    {
        private int _index;

        partial void Start() => _index = _start - 1;

        // as Select.Push.AllowBatchesWhen
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            (typeof(Next).IsAssignableTo(typeof(IInternal__Aggregator__NotForManualUse))
                ? BatchKinds.Container | BatchKinds.Forwarder
                : BatchKinds.Container)
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            // the downstream runs the span from a local copy: Docs/BatchFromLocalCopy.md, "The fix" and "Constraints"
            var next = _next;
            var index = _index;
            var more = true;
            foreach (var item in batch.GetSpan())
            {
                index = checked(index + 1);
                if (!next.ProcessNext((index, item)))
                {
                    more = false;
                    break;
                }
            }
            _next = next;
            _index = index;

            return more;
        }

        public bool ProcessNext(T input)
        {
            _index = checked(_index + 1);

            return _next.ProcessNext((_index, input));
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        private int _index;

        partial void Start() => _index = _start - 1;

        public bool MoveNext()
        {
            if (_enumerator.MoveNext())
            {
                _index = checked(_index + 1);

                return true;
            }

            return false;
        }

        public (int Index, T Item) Current => (_index, _enumerator.Current);
    }
}
