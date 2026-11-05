namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System.Diagnostics.CodeAnalysis;

public interface IPullEnumerator<TElement>
{
    internal bool MoveNext();
    internal TElement Current { get; }
    internal void Dispose();


    internal bool TryPushRest(ErasedChain.PushSink<TElement> sink);


    internal static abstract bool RebuildChanges { get; }
    internal static abstract bool PreferRebuilt { get; }
    internal static abstract TBuilt Rebuild<TOwnNode, TBuilt, TBuilder>(in TOwnNode node, in TBuilder builder)
        where TBuilder : IChainBuilder<TElement, TBuilt>, allows ref struct;


    internal static abstract bool TryPullOptimization<TRequest, TOwnNode, TBuilt, TBuilder>(in TRequest request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
        where TRequest : notnull
        where TBuilder : IChainBuilder<TElement, TBuilt>, allows ref struct;
}
