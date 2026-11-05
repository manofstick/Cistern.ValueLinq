namespace Cistern.ValueLinq;

using System;

// the batches a push enumerator takes (its AllowBatchesWhen), and what a batch needs a taker to take (its Requires): a
// taker is handed a batch when the two share a flag
[Flags]
enum BatchKinds
{
    None = 0,
    // a span a container hands over whole
    Container = 1 << 0,
    // a chunk a forwarder built
    Forwarder = 1 << 1,
    // a collection's elements not yet copied out (cold), copied straight into the taker's own storage through
    // Batch.CopyTo or AddTo
    ColdCopy = 1 << 2,
    // a cold batch read through Batch.GetSpan, which copies the elements into a rented buffer, by a taker that reads
    // every element
    ColdSpan = 1 << 3,
    Cold = ColdCopy | ColdSpan,
}

static class BatchKindsExtensions
{
    // past this the buffer's memory, rather than the enumerator's calls, is the cost
    public const int ColdSpanLimit = 1 << 20;

    public static bool Takes(this BatchKinds accepts, BatchKinds requires) =>
        (accepts & requires) != 0;

    // what a cold batch of a collection requires: read through a span only where the collection would otherwise be
    // read through its boxed IEnumerator<T> (direct: its container calls an enumerator without boxing it, which the
    // copy would only add a pass to), and up to ColdSpanLimit
    public static BatchKinds ColdRequires(int count, bool direct) =>
        direct || count > ColdSpanLimit
            ? BatchKinds.ColdCopy
            : BatchKinds.Cold;
}
