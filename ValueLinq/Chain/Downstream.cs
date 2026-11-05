namespace Cistern.ValueLinq.Chain;

using System;

// fills the slot with a chain built already: the downstream a node pushes its parts into
readonly ref struct Downstream<TPushEnumerator>
    : IPushFiller<TPushEnumerator>
    where TPushEnumerator : IDisposable
{
    private readonly ref readonly TPushEnumerator _value;

    public Downstream(in TPushEnumerator value) => _value = ref value;

    void IPushFiller<TPushEnumerator>.Fill(ref TPushEnumerator target) => target = _value;

    void IPushFiller<TPushEnumerator>.Started() { }
}
