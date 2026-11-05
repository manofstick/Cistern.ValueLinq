namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;

/// <summary>
/// A node whose sequence is chosen at run time (an IEnumerable&lt;T&gt;, Cast, OfType): its enumerator is an
/// ErasedEnumerator over the PullEnumerator the node picks.
/// </summary>
public interface IErasedNode<T>
    : INode<T>
{
    internal PullEnumerator<T> CreatePullEnumerator();
}
