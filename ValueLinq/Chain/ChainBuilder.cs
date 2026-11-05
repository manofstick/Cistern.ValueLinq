namespace Cistern.ValueLinq.Chain;

using System;
using System.Runtime.CompilerServices;

/// <summary>
/// What a node hands its source when a chain is rebuilt over another source: given the source's new node, it builds
/// the node above over it, and hands that on to the builder the node above gave it.
/// </summary>
interface IChainBuilder<T, TBuilt>
{
    TBuilt Build<TNode, TEnumerator>(in TNode node)
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>;
}

static class ChainBuilder
{
    internal static ref readonly TTo As<TFrom, TTo>(in TFrom node)
    {
        if (typeof(TFrom) != typeof(TTo))
            throw new InvalidOperationException("A Rebuild must be handed the node its enumerator enumerates");

        return ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in node));
    }

    internal static TBuilt Identity<T, TNode, TEnumerator, TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct
        => builder.Build<TNode, TEnumerator>(in As<TOwnNode, TNode>(in node));
}
