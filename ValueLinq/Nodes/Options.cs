namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using System;

public static partial class Options
{
    // applies its override to the ChainOptions of the part of the chain below it when pushed, and is otherwise not
    // there: it hands each request straight to its source
    [Operator(Forwards = true)]
    public partial struct Node<T, NodeT>
        : INode<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        internal readonly ChainOptionsOverride _override;

        public Node(in NodeT nodeT, in ChainOptionsOverride @override)
        {
            _nodeT = nodeT;
            _override = @override;
        }

        public void GetCountInformation(out CountInformation info) => _nodeT.GetCountInformation(out info);

        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
            => _nodeT.TryPushOptimization<TRequest, TRequestResult>(in request, out result);

        TPushResult INode<T>.CreateViaPush<TPushResult,TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => _nodeT.CreateViaPush<TPushResult,TPushEnumerator, TPushFiller>(options.Apply(in _override), in filler);
    }

    // pulled, the options change nothing, so the source's own enumerator is pulled through
    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        // rebuilt over a runtime container, the chain is pulled, where the options do nothing, so the node is left out
        static TBuilt IPullEnumerator<T>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        {
            ref readonly var options = ref ChainBuilder.As<TOwnNode, Node<T, NodeT>>(in node);
            return TInEnumerator.Rebuild<NodeT, TBuilt, TBuilder>(in options._nodeT, in builder);
        }

        public bool MoveNext() => _enumerator.MoveNext();

        public T Current => _enumerator.Current;

        bool IPullEnumerator<T>.TryPushRest(Containers.ErasedChain.PushSink<T> sink)
        {
            PullEnumerator.PushRest<T, TInEnumerator>(ref _enumerator, sink);
            return true;
        }
    }
}
