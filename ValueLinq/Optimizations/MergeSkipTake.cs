namespace Cistern.ValueLinq.Optimizations;

using Cistern.ValueLinq.Chain;
using System.Diagnostics.CodeAnalysis;

[PullOptimization]
interface IPullOptimizationMergeSkipTake<T>
{
    static abstract bool MergeSkipTake<TOwnNode, TBuilt, TBuilder>(in MergeSkipTake request, in TOwnNode node, in TBuilder builder, [MaybeNullWhen(false)] out TBuilt built)
        where TBuilder : IChainBuilder<T, TBuilt>, allows ref struct;
}

// asked by a Skip's or Take's chain builder of the enumerator below it: the window of that enumerator's node that the
// skip and take leave, built in place of the Skip or Take rebuilt over the node
readonly struct MergeSkipTake(long skip, long take)
{
    public readonly long Skip = skip;
    public readonly long Take = take;
}
