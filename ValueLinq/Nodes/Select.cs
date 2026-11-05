namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using System;

public static partial class Select
{
    [Operator]
    public partial struct Node<T, U, NodeT>
        : INode<U>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<U>
        , Optimizations.IOptimizationCopyTo<U>
        , Optimizations.IOptimizationToArray<U>
        , Optimizations.IOptimizationSkipTake<U>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, U> _selector;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            info.PotentialSideEffects = true;
        }

        // only the dry run, which runs no selector; a count that enumerates runs the selector on every element, as
        // System.Linq's does, so it is pushed
        private bool Count(in Optimizations.Count request, out int response)
        {
            if (!request.DryRun)
            {
                response = default;

                return false;
            }

            return NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out response);
        }

        // over a Where over a span, Where's two passes with the selector run on the survivors
        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out U[] response)
        {
            if (!Optimizations.FilteredSpanSource.TryGet<T, NodeT>(ref _nodeT, out var filtered))
            {
                response = default;
                return false;
            }

            response = Where.ToArray(filtered.Source.GetSpan(), filtered.Predicate, _selector);
            return true;
        }

        // the source finds the element and only it is projected, as System.Linq's Select iterators answer First, Last
        // and ElementAt (an ordering below selects rather than sorts); a source that declines an index from the start
        // or the last is run to the element, the selector not run for the elements passed over
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, U) response)
        {
            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in request, out var answer))
            {
                var index = request.Index;
                if (index.IsFromEnd && index.Value != 1)
                {
                    response = default;
                    return false;
                }

                answer = _nodeT.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(index)));
            }

            response = answer.Item1 ? (true, _selector(answer.Item2)) : (false, default!);
            return true;
        }

        // the slice of the source, projected, so the selector never runs for an element skipped, as System.Linq's
        // Select iterators over a list or range slice their source; only the source itself answers, so one that cannot
        // slice is pushed with its user code run
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<U> response)
        {
            if (!Optimizations.SkipTake.Try<T, NodeT>(ref _nodeT, request.Skip, request.Take, out var sourceSlice))
            {
                response = default;

                return false;
            }

            response = default;
            if (sourceSlice.Type == NodeType.Empty)
                response.SetEmpty();
            else
                response.SetNode(new Node<T, U, NodeContainerNode<T>>(new NodeContainerNode<T>(in sourceSlice), _selector));

            return true;
        }

        // over a source with a span the copy projects straight into the destination, the selector run once per element
        // in order, as the push would run it, and as System.Linq's Select fills an array or list of known size
        private bool CopyTo(in Optimizations.CopyTo<U> request, out int response)
        {
            if (!Optimizations.AsSpanSource.TryGet<T, NodeT>(ref _nodeT, out var source))
            {
                if (typeof(T) == typeof(int))
                    return CopyRangeTo(in request, out response);

                response = default;
                return false;
            }

            var span = source.GetSpan();
            if (!request.IsDryRun)
            {
                var destination = request.Span.Slice(0, span.Length);
                var selector = _selector;
                for (var i = 0; i < span.Length; ++i)
                    destination[i] = selector(span[i]);
            }

            response = span.Length;
            return true;
        }

        // over a range its values are computed in one counted loop rather than pulled from it, as System.Linq's
        // RangeSelectIterator fills its result
        private bool CopyRangeTo(in Optimizations.CopyTo<U> request, out int response)
        {
            if (!Optimizations.AsRange.TryGet(ref _nodeT, out var range))
            {
                response = default;
                return false;
            }

            if (!request.IsDryRun)
            {
                var destination = request.Span.Slice(0, range.Count);
                var selector = (Func<int, U>)(object)_selector;
                for (var i = 0; i < destination.Length; ++i)
                    destination[i] = selector(range.Start + i);
            }

            response = range.Count;
            return true;
        }
    }

    // a selector over an aggregator, as the selector overloads of the maths aggregations run it
    internal static Push<T, U, Next> CreatePush<T, U, Next>(Func<T, U> selector, in Next next)
        where Next : IPushEnumerator<U>
    {
        if (selector == null)
            throw new ArgumentNullException(nameof(selector));

        var push = default(Push<T, U, Next>);
        push.Fill(selector) = next;
        return push;
    }

    internal partial struct Push<T, U, Next>
    {
        // a forwarder's batch only over a promoted leaf: Docs/BatchFromLocalCopy.md, "The fix"; a cold batch only over a
        // downstream that reads every element, as one does that takes cold
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen =>
            (typeof(Next).IsAssignableTo(typeof(IInternal__Aggregator__NotForManualUse))
                ? BatchKinds.Container | BatchKinds.Forwarder
                : BatchKinds.Container)
            | ((Next.AllowBatchesWhen & BatchKinds.Cold) != 0
                ? BatchKinds.ColdSpan
                : BatchKinds.None);

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => ProcessSpan(batch.GetSpan());

        private bool ProcessSpan(ReadOnlySpan<T> span)
        {
            // the downstream runs the span from a local copy: Docs/BatchFromLocalCopy.md, "The fix" and "Constraints"
            var next = _next;
            var selector = _selector;
            var more = true;
            foreach (var item in span)
            {
                if (!next.ProcessNext(selector(item)))
                {
                    more = false;
                    break;
                }
            }
            _next = next;
            return more;
        }

        public bool ProcessNext(T input) => _next.ProcessNext(_selector(input));
    }

    public partial struct Pull<T, U, NodeT, TInEnumerator>
    {
        public bool MoveNext() => _enumerator.MoveNext();

        public U Current => _selector(_enumerator.Current);
    }
}
