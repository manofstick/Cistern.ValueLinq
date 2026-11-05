namespace Cistern.ValueLinq.Chain;

/// <summary>
/// An IPushEnumerator with no downstream, which a batch's local copy promotes whole: a SelectMany with a result
/// selector runs each inner span from a copy of its downstream only when it is one, and a Select takes a
/// forwarder's batch only over one. [Aggregator] writes it on every leaf aggregator: Docs/BatchFromLocalCopy.md,
/// "The fix".
/// </summary>
interface IInternal__Aggregator__NotForManualUse { }

/// <summary>
/// An aggregator that writes its own Dispose, which [Aggregator] marks: a loop that runs a copy of its chain runs one
/// of these in place, as what its Dispose releases may be in a copy an exception discards.
/// </summary>
// It exists for Vanilla's local copy, whose gain is measured but not understood. An empty Dispose inlines to nothing,
// so the JIT would be expected to drop the handlers around the loop and keep the chain in registers without any
// copy; a version written that way, and so without this, measured slower on plain aggregators. Once the JIT does
// what is expected, or the gap is explained, Vanilla loops on its chain directly and this interface goes.
interface IInternal__ImplementsDispose__NotForManualUse { }
