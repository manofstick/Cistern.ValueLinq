namespace Cistern.ValueLinq.Chain;

using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/// <summary>
/// A pull chain held where its type cannot be named: what IEnumerable&lt;T&gt;.GetEnumerator hands out, and the handle
/// on a sequence whose type is known only at run time (an IEnumerable&lt;T&gt; source, a Concat part, a SelectMany
/// inner sequence). Met again by Cistern, it is pulled with one TryGetNext rather than MoveNext and Current, and what
/// is left of it can be pushed rather than pulled.
/// </summary>
abstract class PullEnumerator<TElement>
    : IEnumerator<TElement>
{
    public abstract TElement Current { get; }
    object? System.Collections.IEnumerator.Current => Current;

    public abstract bool MoveNext();
    public abstract void Dispose();
    public void Reset() => throw new NotSupportedException();

    public abstract bool TryGetNext([MaybeNullWhen(false)] out TElement current);

    // what is left of the sequence goes into the sink, until the sink stops taking elements; the enumerator is
    // still disposed by whoever owns it
    public virtual void PushRest(ErasedChain.PushSink<TElement> sink)
    {
        while (TryGetNext(out var current))
        {
            if (!sink.ProcessNext(current))
                return;
        }
    }

    public static readonly PullEnumerator<TElement> Empty = new PullEnumerator<CisternEmpty.Pull<TElement>, TElement>(default);
}

sealed class PullEnumerator<TPullEnumerator, TElement>
    : PullEnumerator<TElement>
    where TPullEnumerator : struct, IPullEnumerator<TElement>
{
    private TPullEnumerator _enumerator;
    [AllowNull] private TElement _current;
    private bool _finished;

    public PullEnumerator(in TPullEnumerator enumerator)
    {
        _enumerator = enumerator;
        _current = default;
        _finished = false;
    }

    public override TElement Current => _current;

    public override void Dispose()
    {
        if (_finished)
            return;
        _finished = true;
        _enumerator.Dispose();
    }

    // the chain is disposed on reaching its end, as System.Linq's iterators dispose their source there, and is
    // not touched again after that
    public override bool MoveNext()
    {
        if (_finished)
            return false;
        if (_enumerator.MoveNext())
        {
            _current = _enumerator.Current;
            return true;
        }
        _current = default;
        Dispose();
        return false;
    }

    // as MoveNext; Current is kept up to date for whoever handed this enumerator on
    public override bool TryGetNext([MaybeNullWhen(false)] out TElement current)
    {
        if (!_finished && _enumerator.MoveNext())
        {
            current = _current = _enumerator.Current;
            return true;
        }
        current = _current = default;
        Dispose();
        return false;
    }

    public override void PushRest(ErasedChain.PushSink<TElement> sink)
    {
        if (_finished)
            return;
        if (!_enumerator.TryPushRest(sink))
            base.PushRest(sink);
    }
}

static class PullEnumerator
{
    // a chain over an IEnumerable<T> is rebuilt over the container the source holds, so the heap enumerator pulls a
    // chain specialised to it; a source with nothing applied to it is already handed out as its own enumerator
    public static PullEnumerator<T> Create<T, TNode, TEnumerator>(in TNode node)
        where TNode : INode<T>
        where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
    {
        if (TEnumerator.RebuildChanges
            && typeof(TEnumerator) != typeof(ErasedEnumerator<T, SystemIEnumerable.Node<T>>))
        {
            return TEnumerator.Rebuild<TNode, PullEnumerator<T>, BoxBuilder<T>>(in node, default);
        }

        return Box<T, TEnumerator>(TEnumerator.Create(in node));
    }

    private readonly struct BoxBuilder<T>
        : IChainBuilder<T, PullEnumerator<T>>
    {
        PullEnumerator<T> IChainBuilder<T, PullEnumerator<T>>.Build<TNode, TEnumerator>(in TNode node)
            => Box<T, TEnumerator>(TEnumerator.Create(in node));
    }

    // a chain whose enumerator is itself a handle on an erased sequence (an IEnumerable<T> source with nothing
    // applied to it) is handed out as that handle rather than boxed a second time
    private static PullEnumerator<T> Box<T, TEnumerator>(in TEnumerator enumerator)
        where TEnumerator : struct, IPullEnumerator<T>
    {
        if (typeof(TEnumerator) == typeof(CisternEmpty.Pull<T>))
            return PullEnumerator<T>.Empty;

        if (typeof(TEnumerator) == typeof(ErasedEnumerator<T, SystemIEnumerable.Node<T>>))
            return Unsafe.As<TEnumerator, ErasedEnumerator<T, SystemIEnumerable.Node<T>>>(ref Unsafe.AsRef(in enumerator)).Inner;

        return new PullEnumerator<TEnumerator, T>(in enumerator);
    }

    // what is left of a typed enumerator goes into the sink, pushed where the enumerator can push it, until the sink
    // stops taking elements; the enumerator is still disposed by whoever owns it
    public static void PushRest<T, TEnumerator>(ref TEnumerator enumerator, ErasedChain.PushSink<T> sink)
        where TEnumerator : struct, IPullEnumerator<T>
    {
        if (enumerator.TryPushRest(sink))
            return;

        while (enumerator.MoveNext())
        {
            if (!sink.ProcessNext(enumerator.Current))
                return;
        }
    }
}
