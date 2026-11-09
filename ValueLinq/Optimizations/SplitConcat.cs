using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq.Optimizations
{
    [PushOptimization]
    internal interface IOptimizationSplitConcat<T>
    {
        bool SplitConcat(in SplitConcat request, out (SystemIEnumerable.Node<T>, SystemIEnumerable.Node<T>) response);
    }

    readonly struct SplitConcat
    {
        public static bool TrySplit<T, Node>(ref Node node, out (SystemIEnumerable.Node<T>, SystemIEnumerable.Node<T>) nodes)
            where Node : INode<T>
            => node.TryPushOptimization(new SplitConcat(), out nodes);

    }
}
