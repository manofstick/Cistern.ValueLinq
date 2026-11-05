namespace Cistern.ValueLinq;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

enum NodeType : byte
{
    Empty = 0,
    Reference,
    Array,
    Memory,
    ListSegment,
    ReversedMemory,
    ReversedListSegment,
    IListSegment,
    IReadOnlyListSegment,
    EnumerableSlice,
    Range,
}

// The node a request is answered with, held as a tag and the parts the node is rebuilt from on dispatch: one
// reference (the INode, the array, the List<T>, the IList<T>, the IReadOnlyList<T> or the IEnumerable<T> of a
// slice) with a start and count, a range's start and count, or a memory. Kept small, with two references, as it is
// zeroed, copied and returned on every request descent and sits in the frame of whatever inlines one.
struct NodeContainer<TElement>
{
    public NodeType Type;

    private object? _reference;
    private int _start;
    private int _count;
    private ReadOnlyMemory<TElement> _memory;

    public void SetEmpty() => Type = NodeType.Empty;

    public void SetNode(INode<TElement> node)
    {
        Type = NodeType.Reference;
        _reference = node;
    }

    public void SetNode(SystemArray.Node<TElement> node)
    {
        Type = NodeType.Array;
        _reference = node._array;
    }

    public void SetNode(SystemMemory.Node<TElement> node)
    {
        Type = NodeType.Memory;
        _memory = node._memory;
    }

    public void SetNode(SystemListSkipTake.Node<TElement> node)
    {
        Type = NodeType.ListSegment;
        (_reference, _start, _count) = (node._list, node._start, node._count);
    }

    public void SetNode(SystemMemory.ReversedNode<TElement> node)
    {
        Type = NodeType.ReversedMemory;
        _memory = node._memory;
    }

    public void SetNode(SystemListSkipTake.ReversedNode<TElement> node)
    {
        Type = NodeType.ReversedListSegment;
        (_reference, _start, _count) = (node._list, node._start, node._count);
    }

    public void SetNode(SystemIList.Node<TElement> node)
    {
        Type = NodeType.IListSegment;
        (_reference, _start, _count) = (node._list, node._start, node._count);
    }

    public void SetNode(SystemIReadOnlyList.Node<TElement> node)
    {
        Type = NodeType.IReadOnlyListSegment;
        (_reference, _start, _count) = (node._list, node._start, node._count);
    }

    // a take of all is held as -1; a skip or take past int.MaxValue, which only an unbounded source reaches, is boxed
    public void SetNode(SystemEnumerableSkipTake.Node<TElement> node)
    {
        if (node._skip > int.MaxValue
            || (node._take > int.MaxValue && node._take != long.MaxValue))
        {
            SetNode((INode<TElement>)node);
            return;
        }

        Type = NodeType.EnumerableSlice;
        (_reference, _start, _count) = (node._enumerable, (int)node._skip, node._take == long.MaxValue ? -1 : (int)node._take);
    }

    // only a NodeContainer<int> is given one
    public void SetRange(int start, int count)
    {
        Type = NodeType.Range;
        (_start, _count) = (start, count);
    }

    // each read as the tag says it is
    private INode<TElement> Node => (INode<TElement>)_reference!;
    private SystemArray.Node<TElement> ArrayNode => new((TElement[])_reference!, true);
    private SystemMemory.Node<TElement> MemoryNode => new(_memory);
    private SystemListSkipTake.Node<TElement> ListSegmentNode => new((List<TElement>)_reference!, _start, _count);
    private SystemMemory.ReversedNode<TElement> ReversedMemoryNode => new(_memory);
    private SystemListSkipTake.ReversedNode<TElement> ReversedListNode => new((List<TElement>)_reference!, _start, _count);
    private SystemIList.Node<TElement> IListSegmentNode => new((IList<TElement>)_reference!, _start, _count);
    private SystemIReadOnlyList.Node<TElement> IReadOnlyListSegmentNode => new((IReadOnlyList<TElement>)_reference!, _start, _count);
    private SystemEnumerableSkipTake.Node<TElement> EnumerableSliceNode => new((IEnumerable<TElement>)_reference!, _start, _count < 0 ? long.MaxValue : _count);
    private SystemRange.Node RangeNode => new(_start, _count);

    public bool TryPushOptimization<TRequest, TResult>(in TRequest request, [MaybeNullWhen(false)] out TResult result)
        where TRequest : notnull =>
        Type switch
        {
            NodeType.Empty          => CisternEmpty.Node<TElement>.Empty.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.Reference      => Node.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.Array          => ArrayNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.Memory         => MemoryNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.ListSegment    => ListSegmentNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.ReversedMemory => ReversedMemoryNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.ReversedListSegment => ReversedListNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.IListSegment   => IListSegmentNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.IReadOnlyListSegment => IReadOnlyListSegmentNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.EnumerableSlice => EnumerableSliceNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            NodeType.Range          => RangeNode.TryPushOptimization<TRequest, TResult>(in request, out result),
            _ => throw new InvalidOperationException(),
        };

    public TResult CreateViaPush<TResult, TPushEnumerator, TFiller>(in ChainOptions options, in TFiller filler)
        where TPushEnumerator : IPushEnumerator<TElement>
        where TFiller : IPushFiller<TPushEnumerator>, allows ref struct =>
        Type switch
        {
            NodeType.Empty          => CisternEmpty.Node<TElement>.Empty.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.Reference      => Node.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.Array          => ArrayNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.Memory         => MemoryNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.ListSegment    => ListSegmentNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.ReversedMemory => ReversedMemoryNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.ReversedListSegment => ReversedListNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.IListSegment   => IListSegmentNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.IReadOnlyListSegment => IReadOnlyListSegmentNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.EnumerableSlice => EnumerableSliceNode.CreateViaPush<TResult, TPushEnumerator, TFiller>(in options, in filler),
            NodeType.Range          => SystemRange.Node.ExecutePush<TElement, TResult, TPushEnumerator>(_start, _start + _count - 1, PushFiller.Fill<TPushEnumerator, TFiller>(in filler)),
            _ => throw new InvalidOperationException(),
        };
}
