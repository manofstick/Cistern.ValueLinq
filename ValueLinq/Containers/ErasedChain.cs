namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;

// A chain applied to another chain reached as IEnumerable<T> meets it through types that depend only on T, so each
// chain is instantiated once rather than once per pairing of the two. Pull goes through the inner chain's own
// PullEnumerator<T>; push goes through a PushSink<T> holding the outer chain, which the inner chain pushes into. A
// Concat pushes its parts into a PushSink<T> the same way (see CisternConcat.Node.CreateViaPush), as does a
// SelectMany its inner sequences (see SelectMany.Push).
public static partial class ErasedChain
{
    internal abstract class PushSink<T>
    {
        // the outer chain has stopped taking elements, so nothing more is pushed into it
        public bool Halted { get; protected set; }

        // whether the chain takes a batch with these requirements, which a container asks before making a cold one, as
        // PushForward takes every batch statically
        public abstract bool Takes(BatchKinds requires);
        public abstract bool ProcessBatch(scoped ref Batch<T> batch);
        public abstract bool ProcessNext(T input);
    }

    internal sealed class PushSink<T, TPushEnumerator>
        : PushSink<T>
        where TPushEnumerator : IPushEnumerator<T>
    {
        private TPushEnumerator _chain;

        public PushSink(in TPushEnumerator chain) => _chain = chain;

        // for an owner that also runs the chain itself (SelectMany offering an inner span to it as a batch)
        public ref TPushEnumerator Chain => ref _chain;

        // the sink takes every batch, as what the chain takes is known only here; one the chain does not take is
        // looped into it
        public override bool Takes(BatchKinds requires) => TPushEnumerator.AllowBatchesWhen.Takes(requires);

        public override bool ProcessBatch(scoped ref Batch<T> batch)
        {
            var more =
                Takes(batch.Requires)
                    ? _chain.ProcessBatch(ref batch)
                    : ProcessElements(batch.GetSpan());

            if (!more)
                Halted = true;
            return more;
        }

        private bool ProcessElements(ReadOnlySpan<T> span)
        {
            foreach (var item in span)
            {
                if (!_chain.ProcessNext(item))
                    return false;
            }
            return true;
        }

        public override bool ProcessNext(T input)
        {
            if (_chain.ProcessNext(input))
                return true;
            Halted = true;
            return false;
        }

        public TResult GetResult<TResult>() => _chain.GetResult<TResult>();
        public void Dispose() => _chain.Dispose();
    }

    // what the inner chain, or a Concat part, pushes into; the result and disposal belong to the outer chain, so the
    // inner chain's end leaves both to the sink's owner
    [Create]
    internal partial struct PushForward<T>
        : IPushEnumerator<T>
    {
        private readonly PushSink<T> _sink;

        public PushForward(PushSink<T> sink) => _sink = sink;

        public PushSink<T> Sink => _sink;

        // a cold batch is made for it only once its sink's Takes has said yes
        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => BatchKinds.Container | BatchKinds.Forwarder | BatchKinds.Cold;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch) => _sink.ProcessBatch(ref batch);
        public bool ProcessNext(T input) => _sink.ProcessNext(input);

        public TPushResult GetResult<TPushResult>() => default!;
        public void Dispose() { }
    }

    // the result is taken from the outer chain once the inner chain has finished pushing, and the outer chain is
    // disposed after that, as a container does with the chain pushed into it. Pushed from a sink already, the inner
    // chain is given that sink, whose owner takes the result and disposes it
    internal static TResult Push<T, TResult, TPushEnumerator>(in ChainOptions options, ValueEnumerable.IPullEnumerable<T> inner, in TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        if (typeof(TPushEnumerator) == typeof(PushForward<T>))
        {
            inner.Push(in options, ((PushForward<T>)(object)fenum).Sink);
            return default!;
        }

        var sink = new PushSink<T, TPushEnumerator>(in fenum);
        try
        {
            inner.Push(in options, sink);
            return sink.GetResult<TResult>();
        }
        finally
        {
            sink.Dispose();
        }
    }

    // a Cistern chain's enumerator, handed out by a wrapper's GetEnumerator, pushes what is left of the chain into
    // the outer chain and is disposed before the result is taken, as a container disposes its source
    internal static TResult PushRest<T, TResult, TPushEnumerator>(PullEnumerator<T> enumerator, in TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<T>
    {
        if (typeof(TPushEnumerator) == typeof(PushForward<T>))
        {
            try
            {
                enumerator.PushRest(((PushForward<T>)(object)fenum).Sink);
            }
            finally
            {
                enumerator.Dispose();
            }
            return default!;
        }

        var sink = new PushSink<T, TPushEnumerator>(in fenum);
        try
        {
            try
            {
                enumerator.PushRest(sink);
            }
            finally
            {
                enumerator.Dispose();
            }
            return sink.GetResult<TResult>();
        }
        finally
        {
            sink.Dispose();
        }
    }
}
