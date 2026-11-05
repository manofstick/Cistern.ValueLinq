namespace Cistern.ValueLinq;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// Elements handed to a push enumerator together: a span, or a collection whose elements are copied out only when
/// asked for (cold). It carries what a taker must accept to be handed it, so a forwarder handing it on, or a sink or
/// fork asking what lies behind it, tests the same thing its maker did. The container that made a cold batch
/// releases it.
/// </summary>
ref struct Batch<T>
{
    private ReadOnlySpan<T> _span;
    private readonly ICollection<T>? _collection;
    // non-null only once GetSpan has rented it; the only array ever returned to the pool
    private T[]? _rented;

    public Batch(ReadOnlySpan<T> span, BatchKinds requires)
    {
        _span = span;
        Requires = requires;
        Count = span.Length;
    }

    internal Batch(ICollection<T> collection, int count, BatchKinds requires)
    {
        _collection = collection;
        Requires = requires;
        Count = count;
    }

    public readonly BatchKinds Requires { get; }

    public readonly int Count { get; }

    public readonly bool IsCold => _collection is not null && _rented is null;

    public ReadOnlySpan<T> GetSpan()
    {
        if (IsCold && Count > 0)
        {
            _rented = ArrayPool<T>.Shared.Rent(Count);
            _collection!.CopyTo(_rented, 0);
            _span = _rented.AsSpan(0, Count);
        }

        return _span;
    }

    public readonly void CopyTo(T[] array, int index)
    {
        if (IsCold)
            _collection!.CopyTo(array, index);
        else
            _span.CopyTo(array.AsSpan(index));
    }

    public readonly void AddTo(List<T> list)
    {
        if (IsCold)
            list.AddRange(_collection!);
        else
            list.AddRange(_span);
    }

    internal readonly void Release()
    {
        if (_rented is not null)
            ArrayPool<T>.Shared.Return(_rented, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
    }
}
