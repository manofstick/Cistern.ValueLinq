namespace Cistern.ValueLinq;

// what an Options.Node sets; an option left null keeps the value from above
public readonly record struct ChainOptionsOverride
{
    public bool? Fuse { get; init; }
}
