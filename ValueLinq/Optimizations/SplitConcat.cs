using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq.Optimizations
{
    struct SplitConcat
    {
        public static bool TrySplit<T, Node>(ref Node node, out (EnumerableNode<T>, EnumerableNode<T>) nodes)
            where Node : INode<T>
            => node.TryPushOptimization(new SplitConcat(), out nodes);

    }
}
