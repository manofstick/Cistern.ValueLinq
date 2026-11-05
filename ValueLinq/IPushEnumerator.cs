namespace Cistern.ValueLinq;

using System;

interface IPushEnumerator
    : IDisposable
{
    static virtual bool AllowInitialise => false;
    void Initialise() => throw new NotSupportedException("Should not be called when IPushEnumerator.AllowInitialise is false");
}

interface IPushEnumerator<TElement>
    : IPushEnumerator
{
    static virtual BatchKinds AllowBatchesWhen => BatchKinds.None;
    bool ProcessBatch(scoped ref Batch<TElement> batch) => throw new NotSupportedException("Should not be called when IPushEnumerator<TElement>.AllowBatchesWhen is BatchKinds.None");

    bool ProcessNext(TElement input);
    TPushResult GetResult<TPushResult>();
}
