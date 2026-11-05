namespace Cistern.ValueLinq;

// decisions taken while a chain is pushed, passed down its push and on into every sequence an operator pushes from
// inside it; an Options.Node changes them for the part of the chain below it, and an aggregation starts from the
// default. A pull chain takes none.
public readonly record struct ChainOptions
{
    // a Cistern chain reached as IEnumerable<T> is pushed through by the chain applied to it, rather than met through
    // types that depend only on T (see SystemIEnumerable.ExecutePush)
    public bool Fuse { get; init; }

    public ChainOptions Apply(in ChainOptionsOverride o) =>
        this with { Fuse = o.Fuse ?? Fuse };
}
