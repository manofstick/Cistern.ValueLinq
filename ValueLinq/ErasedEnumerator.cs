namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// The enumerator of an IErasedNode: one virtual call per element, into whatever the node chose.
/// </summary>
[Puller]
public partial struct ErasedEnumerator<T, TNode>
    where TNode : IErasedNode<T>
{
    private readonly PullEnumerator<T> _enumerator;
    [AllowNull] private T _current;

    private ErasedEnumerator(PullEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
        _current = default;
    }

    private static ErasedEnumerator<T, TNode> Create(in TNode node)
        => new(node.CreatePullEnumerator());

    static bool IPullEnumerator<T>.RebuildChanges => typeof(TNode) == typeof(SystemIEnumerable.Node<T>);

    static TBuilt IPullEnumerator<T>.Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
    {
        if (typeof(TNode) == typeof(SystemIEnumerable.Node<T>))
            return SystemIEnumerable.Rebuild<T, TBuilt, TBuilder>(in ChainBuilder.As<TOwnNode, SystemIEnumerable.Node<T>>(in node), in builder);

        return ChainBuilder.Identity<T, TNode, ErasedEnumerator<T, TNode>, TOwnNode, TBuilt, TBuilder>(in node, in builder);
    }

    internal PullEnumerator<T> Inner => _enumerator;

    public void Dispose() => _enumerator.Dispose();

    public bool MoveNext() => _enumerator.TryGetNext(out _current);

    public T Current => _current;

    bool IPullEnumerator<T>.TryPushRest(ErasedChain.PushSink<T> sink)
    {
        _enumerator.PushRest(sink);
        return true;
    }
}
