using Cistern.ValueLinq.Chain;

namespace Cistern.ValueLinq.ValueEnumerable
{
    // a chain reached as IEnumerable<T> is met through these rather than through its node (see SystemIEnumerable.Node): it
    // hands over its own enumerator, or, unless the chain applied to it asks to fuse, pushes into a sink holding that
    // chain
    interface IPullEnumerable<T>
    {
        PullEnumerator<T> CreatePullEnumerator();
        void Push(in ChainOptions options, Containers.ErasedChain.PushSink<T> sink);
    }
}
