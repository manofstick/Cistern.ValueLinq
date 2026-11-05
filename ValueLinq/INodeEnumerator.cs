namespace Cistern.ValueLinq;

/// <summary>
/// A pull enumerator that builds itself from the node it enumerates, so a ValueEnumerable can name its chain's type
/// and hand it to foreach; the node knows nothing of it. Each node type has one.
/// </summary>
public interface INodeEnumerator<TNode, TSelf>
    where TSelf : struct, INodeEnumerator<TNode, TSelf>
{
    static abstract TSelf Create(in TNode node);
}
