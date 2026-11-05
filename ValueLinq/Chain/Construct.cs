namespace Cistern.ValueLinq.Chain;

/// <summary>
/// A push enumerator built in its slot by Construct, from its Args: the arguments of its one constructor, which
/// [Create] writes it from.
/// </summary>
interface ICreate<TSelf, TArgs>
    where TSelf : ICreate<TSelf, TArgs>
{
    static abstract TSelf Create(in TArgs args);
}

// fills the slot by building the push enumerator in it. One built elsewhere and copied in is read back as a single
// wide load over the narrower stores that built it, which defeats store forwarding: 6-10 ns a push.
readonly ref struct Construct<TPushEnumerator, TArgs>
    : IPushFiller<TPushEnumerator>
    where TPushEnumerator : ICreate<TPushEnumerator, TArgs>
{
    private readonly TArgs _args;

    public Construct(TArgs args) => _args = args;

    void IPushFiller<TPushEnumerator>.Fill(ref TPushEnumerator target) => target = TPushEnumerator.Create(in _args);

    void IPushFiller<TPushEnumerator>.Started() { }
}
