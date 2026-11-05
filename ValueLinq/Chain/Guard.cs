namespace Cistern.ValueLinq.Chain;

// put before the filler of a node that holds something its chain will own once started (an enumerator it opened, a
// chain it hands on through Downstream): started tells the node, in a finally around the push, whether to release it,
// as nothing does if the push fails before its chain has started. A finally rather than a catch: a catch around the
// push, into which the source's loop is inlined, keeps Dynamic PGO from what it does for the loop
readonly ref struct Guard<TPushEnumerator, TFiller>
    : IPushFiller<TPushEnumerator>
    where TFiller : IPushFiller<TPushEnumerator>, allows ref struct
{
    private readonly ref bool _started;
    private readonly ref readonly ErasedRef _next;

    public Guard(ref bool started, in TFiller next)
    {
        _started = ref started;
        _next = ref Erase.Ref(in next);
    }

    void IPushFiller<TPushEnumerator>.Fill(ref TPushEnumerator target) =>
        Erase.Deref<TFiller>(in _next).Fill(ref target);

    void IPushFiller<TPushEnumerator>.Started()
    {
        _started = true;
        Erase.Deref<TFiller>(in _next).Started();
    }
}
