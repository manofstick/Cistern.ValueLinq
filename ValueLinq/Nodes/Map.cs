namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using System;
using System.Runtime.CompilerServices;

// Select under the caller's promise that the selector is pure: no side effects, and a result that depends only on its
// argument. Nothing then pins when or how often it runs, so it runs only for the elements a result is made of: a count,
// an Any or a skip never runs it, an element is found in the source and only it projected, and a reversal or a slice
// is the source's with the projection over it.
public static partial class Map
{
    [Operator]
    public partial struct Node<T, U, NodeT>
        : INode<U>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationAny
        , Optimizations.IOptimizationElementAt<U>
        , Optimizations.IOptimizationCopyTo<U>
        , Optimizations.IOptimizationToArray<U>
        , Optimizations.IOptimizationSkipTake<U>
        , Optimizations.IOptimizationReverse<U>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate] internal Func<T, U> _selector;

        public void GetCountInformation(out CountInformation info) =>
            _nodeT.GetCountInformation(out info);

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
                return NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out response);

            response = NodeImpl.Count<T, NodeT>(in _nodeT, request.IgnorePotentialSideEffects);
            return true;
        }

        private bool Any(in Optimizations.Any request, out bool response)
        {
            response = NodeImpl.Any<T, NodeT>(_nodeT);
            return true;
        }

        // any index, from the start or the end, is found in the source, run without the selector where the source
        // declines it
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, U) response)
        {
            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(in request, out var answer))
                answer = FindByPush(in _nodeT, request.Index);

            response = answer.Item1 ? (true, _selector(answer.Item2)) : (false, default!);
            return true;
        }

        private static (bool, T) FindByPush(in NodeT node, Index index)
        {
            if (index.IsFromEnd && index.Value != 1)
            {
                if (index.Value == 0)
                    return (false, default!);

                if (NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in node, out var count))
                {
                    if (count < index.Value)
                        return (false, default!);

                    index = new Index(count - index.Value);
                }
                else
                {
                    var last = new TakeRange.Node<T, NodeT>(in node, new Range(index, new Index(index.Value - 1, fromEnd: true)));
                    return Found<TakeRange.Node<T, NodeT>>(in last, new Index(0));
                }
            }

            return Found<NodeT>(in node, index);
        }

        private static (bool, T) Found<TSource>(in TSource node, Index index)
            where TSource : INode<T>
            => node.CreateViaPush<(bool, T), Aggregation.FoundElementAt<T>, Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>>(default(ChainOptions), new Construct<Aggregation.FoundElementAt<T>, Aggregation.FoundElementAt<T>.Args>(new Aggregation.FoundElementAt<T>.Args(index)));

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out U[] response) =>
            Select.FilteredToArray(ref _nodeT, _selector, out response);

        private bool CopyTo(in Optimizations.CopyTo<U> request, out int response) =>
            Select.ProjectedCopyTo(ref _nodeT, _selector, in request, out response);

        // the source's slice where it slices; otherwise a skip is moved below the projection, so the skipped elements
        // are passed over unprojected (a take alone gains nothing from the move, as the push halts at its end either way)
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<U> response)
        {
            response = default;

            var sourceSlice = default(NodeContainer<T>);
            if (Optimizations.SkipTake.Answer<T, NodeT>(ref _nodeT, request.Skip, request.Take, ref sourceSlice))
            {
                if (sourceSlice.Type == NodeType.Empty)
                    response.SetEmpty();
                else
                    response.SetNode(new Node<T, U, NodeContainerNode<T>>(new NodeContainerNode<T>(in sourceSlice), _selector));

                return true;
            }

            if (request.Skip == 0)
                return false;

            var skipped = new Skip.Node<T, NodeT>(in _nodeT, request.Skip);
            if (request.TakesAll)
                response.SetNode(new Node<T, U, Skip.Node<T, NodeT>>(in skipped, _selector));
            else if (request.Take <= int.MaxValue)
                response.SetNode(new Node<T, U, Take.Node<T, Skip.Node<T, NodeT>>>(new Take.Node<T, Skip.Node<T, NodeT>>(in skipped, (int)request.Take), _selector));
            else
                return false;

            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<U> response)
        {
            if (!Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var reversedSource))
            {
                response = default;
                return false;
            }

            Unsafe.SkipInit(out response);
            response.SetNode(new Node<T, U, NodeContainerNode<T>>(new NodeContainerNode<T>(in reversedSource), _selector));
            return true;
        }

        // pushed through Select's forwarder, whose batch paths it shares
        TPushResult INode<U>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            _nodeT.CreateViaPush<TPushResult, Select.Push<T, U, TPushEnumerator>, Select.Filler<T, U, TPushEnumerator, TPushFiller>>(in options, new Select.Filler<T, U, TPushEnumerator, TPushFiller>(_selector, in filler));
    }

    public partial struct Pull<T, U, NodeT, TInEnumerator>
    {
        public bool MoveNext() => _enumerator.MoveNext();

        public U Current => _selector(_enumerator.Current);
    }
}
