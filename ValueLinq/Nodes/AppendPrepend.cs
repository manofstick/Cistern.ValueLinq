namespace Cistern.ValueLinq.Nodes;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

public static partial class AppendPrepend
{
    [Operator]
    public partial struct Node<T, NodeT>
        : INode<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationContains<T>
        where NodeT : INode<T>
    {
        internal NodeT _nodeT;
        [Propagate(ByRef = true)] internal Elements<T> _elements;

        // Elements<T> is internal, so the constructor cannot be the generated public one
        internal Node(in NodeT nodeT, in Elements<T> elements)
        {
            _nodeT = nodeT;
            _elements = elements;
        }

        // an Append or Prepend onto this node extends it rather than wrapping it, so a chain of them is one node
        internal readonly Node<T, NodeT> Append(T element) => new(in _nodeT, _elements.Append(element));
        internal readonly Node<T, NodeT> Prepend(T element) => new(in _nodeT, _elements.Prepend(element));

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            if (info.MaximumLength.HasValue)
                info.MaximumLength = info.MaximumLength.Value + _elements.Count;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (!request.DryRun)
            {
                response = checked(NodeImpl.Count<T, NodeT>(in _nodeT, request.IgnorePotentialSideEffects) + _elements.Count);
                return true;
            }

            if (NodeImpl.TryGetNonEnumeratedCount<T, NodeT>(in _nodeT, out var sourceCount)
                && (long)sourceCount + _elements.Count <= int.MaxValue)
            {
                response = sourceCount + _elements.Count;
                return true;
            }

            response = default;
            return false;
        }

        // the added elements are compared first and the source then asked in this node's place, as System.Linq's
        // AppendPrepend asks it (a collection's own lookup)
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            var value = request.Value;
            for (var i = 0; i < _elements.PrependCount; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(value, _elements.GetPrepended(i)))
                {
                    response = true;
                    return true;
                }
            }
            for (var i = 0; i < _elements.AppendCount; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(value, _elements.GetAppended(i)))
                {
                    response = true;
                    return true;
                }
            }

            response = NodeImpl.Contains<T, NodeT>(_nodeT, value);
            return true;
        }

        // the copy follows a dry run that answered, so the source answers both too
        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (request.IsDryRun)
            {
                if (Optimizations.CopyTo.TryGetCount<T, NodeT>(ref _nodeT, out var count) && (long)count + _elements.Count <= int.MaxValue)
                {
                    response = count + _elements.Count;
                    return true;
                }

                response = default;
                return false;
            }

            var span = request.Span;
            var prependCount = _elements.PrependCount;
            for (var i = 0; i < prependCount; ++i)
                span[i] = _elements.GetPrepended(i);

            var sourceCount = Optimizations.CopyTo.Copy<T, NodeT>(ref _nodeT, request.Advance(prependCount));

            var appendsStart = prependCount + sourceCount;
            for (var i = 0; i < _elements.AppendCount; ++i)
                span[appendsStart + i] = _elements.GetAppended(i);

            response = sourceCount + _elements.Count;
            return true;
        }

        // the reversal of a source that reverses itself, with the ends swapped and each reversed; a boxed node in
        // place of the array the reversal would otherwise be buffered into
        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            if (Optimizations.Reverse.Try<T, NodeT>(ref _nodeT, out var reversedSource))
            {
                Unsafe.SkipInit(out response);
                response.SetNode(new Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in reversedSource), _elements.Reversed()));
                return true;
            }

            response = default;
            return false;
        }

        // a slice taking nothing is empty; one skipping nothing is left to the Push path, as a take alone always was;
        // otherwise the skip is answered as below, and a take over it is a Take node over the boxed answer
        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            if (request.Take == 0)
            {
                Unsafe.SkipInit(out response);
                response.SetEmpty();
                return true;
            }

            if (request.Skip == 0 || !TrySkip(request.Skip, out NodeContainer<T> skipped))
            {
                response = default;
                return false;
            }

            if (!request.TakesAll && skipped.Type != NodeType.Empty)
            {
                var count = (int)Math.Min(request.Take, int.MaxValue);
                var taken = new Take.Node<T, NodeContainerNode<T>>(new NodeContainerNode<T>(in skipped), count);
                skipped = default;
                skipped.SetNode(taken);
            }

            response = skipped;
            return true;
        }

        // a skip that reaches the source is answered with the source skipped and the appends kept, which costs a boxed
        // node and lets a container source skip without being run; one that stops in the prepends saves too little to
        // pay for the box, and one whose landing point depends on a source length that is not known is left to the
        // Push path. A source with side effects is never skipped whole, as Skip.Node never does.
        private bool TrySkip(long count, out NodeContainer<T> container)
        {
            _nodeT.GetCountInformation(out var info);
            if (count < _elements.PrependCount || info.IsStale || info.ActualSize is not int sourceLength)
            {
                container = default;
                return false;
            }

            var inSource = count - _elements.PrependCount;
            Unsafe.SkipInit(out container);
            if (inSource < sourceLength)
            {
                container.SetNode(new Node<T, Skip.Node<T, NodeT>>(new Skip.Node<T, NodeT>(in _nodeT, inSource), _elements.AppendsFrom(0)));
            }
            else if (info.PotentialSideEffects)
            {
                container = default;
                return false;
            }
            else
            {
                var inAppends = inSource - sourceLength;
                if (inAppends >= _elements.AppendCount)
                    container.SetEmpty();
                else
                    container.SetNode(new Node<T, CisternEmpty.Node<T>>(new CisternEmpty.Node<T>(), _elements.AppendsFrom((int)inAppends)));
            }

            return true;
        }

        // an index in the prepends or appends is answered here and one in the source is passed to it translated; one
        // the source finds past its end needs the source's length to land in the other end, and where that length will
        // not be known it is left to the container path
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var index = request.Index;
            var prependCount = _elements.PrependCount;
            var appendCount = _elements.AppendCount;

            if (!index.IsFromEnd)
            {
                if (index.Value < prependCount)
                    return Answer(true, _elements.GetPrepended(index.Value), out response);

                var inSource = index.Value - prependCount;
                if (!TryElementAtInSource(new Index(inSource), appendCount > 0, out var sourceLength, out response))
                    return false;
                if (response.Item1)
                    return true;

                var inAppends = inSource - sourceLength;
                return (uint)inAppends < (uint)appendCount
                    ? Answer(true, _elements.GetAppended(inAppends), out response)
                    : Answer(false, default!, out response);
            }

            if (index.Value == 0)
                return Answer(false, default!, out response);

            if (index.Value <= appendCount)
                return Answer(true, _elements.GetAppended(appendCount - index.Value), out response);

            var fromSourceEnd = index.Value - appendCount;
            if (!TryElementAtInSource(new Index(fromSourceEnd, true), prependCount > 0, out var length, out response))
                return false;
            if (response.Item1)
                return true;

            var inPrepends = prependCount - (fromSourceEnd - length);
            return (uint)inPrepends < (uint)prependCount
                ? Answer(true, _elements.GetPrepended(inPrepends), out response)
                : Answer(false, default!, out response);
        }

        private static bool Answer(bool found, T element, out (bool, T) response)
        {
            response = (found, element);
            return true;
        }

        // the source's answer, with its length where it has no such element and the other end's elements need it. The
        // source answering may run user code, so whether that length will be known is settled before it is asked: known
        // already, or no element at its first or last, which is an empty source
        private bool TryElementAtInSource(Index index, bool lengthNeeded, out int sourceLength, out (bool, T) response)
        {
            sourceLength = 0;
            _nodeT.GetCountInformation(out var info);
            var knownLength = info.IsStale ? null : info.ActualSize;
            if (lengthNeeded
                && knownLength is null
                && index.Value != (index.IsFromEnd ? 1 : 0))
            {
                response = default;
                return false;
            }

            if (!_nodeT.TryPushOptimization<Optimizations.ElementAt, (bool, T)>(new Optimizations.ElementAt(index), out response))
                return false;

            if (!response.Item1)
                sourceLength = knownLength ?? 0;

            return true;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult,TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var appendPrependFiller = new Filler<T, TPushEnumerator, TPushFiller>(in _elements, in filler);
            if (_elements.PrependCount == 0)
                return _nodeT.CreateViaPush<TPushResult,Push<T, TPushEnumerator>, Filler<T, TPushEnumerator, TPushFiller>>(in options, in appendPrependFiller);

            // the prepends run before the source is started, so a downstream they satisfy (First) never starts it;
            // until the source has the chain, disposing it is this method's
            var forward = PushFiller.Fill<Push<T, TPushEnumerator>, Filler<T, TPushEnumerator, TPushFiller>>(in appendPrependFiller);
            var sourceWanted = false;
            try
            {
                sourceWanted = forward.ProcessPrepends();
                if (!sourceWanted)
                    return forward.GetResult<TPushResult>();
            }
            finally
            {
                if (!sourceWanted)
                    forward.Dispose();
            }

            // the source's push owns the chain once it has started it; disposed here when that push fails first
            var started = false;
            try
            {
                var downstream = new Downstream<Push<T, TPushEnumerator>>(in forward);
                var guard = new Guard<Push<T, TPushEnumerator>, Downstream<Push<T, TPushEnumerator>>>(ref started, in downstream);
                return _nodeT.CreateViaPush<TPushResult, Push<T, TPushEnumerator>, Guard<Push<T, TPushEnumerator>, Downstream<Push<T, TPushEnumerator>>>>(in options, in guard);
            }
            finally
            {
                if (!started)
                    forward.Dispose();
            }
        }
    }

    partial struct Push<T, Next>
    {
        bool _halted;

        public bool ProcessPrepends()
        {
            for (var i = 0; i < _elements.PrependCount; ++i)
            {
                if (!_next.ProcessNext(_elements.GetPrepended(i)))
                {
                    _halted = true;
                    return false;
                }
            }
            return true;
        }

        static BatchKinds IPushEnumerator<T>.AllowBatchesWhen => Next.AllowBatchesWhen;

        bool IPushEnumerator<T>.ProcessBatch(scoped ref Batch<T> batch)
        {
            if (_next.ProcessBatch(ref batch))
                return true;
            _halted = true;
            return false;
        }

        public bool ProcessNext(T input)
        {
            if (_next.ProcessNext(input))
                return true;
            _halted = true;
            return false;
        }

        // the appends follow the source unless the downstream has already halted
        public TPushResult GetResult<TPushResult>()
        {
            if (!_halted)
            {
                for (var i = 0; i < _elements.AppendCount; ++i)
                {
                    if (!_next.ProcessNext(_elements.GetAppended(i)))
                        break;
                }
            }
            return _next.GetResult<TPushResult>();
        }
    }

    public partial struct Pull<T, NodeT, TInEnumerator>
    {
        const int Prepends = 0;
        const int Source = 1;
        const int Appends = 2;
        const int Done = 3;

        private int _state;
        private int _index;

        // a prepended or appended element; while in the source, Current is the source's
        [AllowNull] private T _item;

        partial void Start() => _state = _elements.PrependCount > 0 ? Prepends : Source;

        public bool MoveNext()
        {
            if (_state == Source && _enumerator.MoveNext())
                return true;

            return MoveNextOutsideSource();
        }

        public T Current => _state == Source ? _enumerator.Current : _item;

        private bool MoveNextOutsideSource()
        {
            switch (_state)
            {
                case Prepends:
                    if (_index < _elements.PrependCount)
                    {
                        _item = _elements.GetPrepended(_index++);
                        return true;
                    }
                    _state = Source;
                    if (_enumerator.MoveNext())
                        return true;
                    goto case Source;

                case Source:
                    _state = Appends;
                    _index = 0;
                    goto case Appends;

                case Appends:
                    if (_index < _elements.AppendCount)
                    {
                        _item = _elements.GetAppended(_index++);
                        return true;
                    }
                    _state = Done;
                    return false;

                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// The elements Append and Prepend add to a source. Prepends come out latest call first and appends earliest call
    /// first, whatever the interleaving of the calls, so only each end's order is kept. Inline, appends fill the buffer
    /// from the front and prepends from the back; past the buffer both move to one ImmutableList in output order with
    /// the prepends leading. Neither is ever changed in place, so a sequence extended twice from the same point gives
    /// two independent sequences.
    /// </summary>
    internal struct Elements<T>
    {
        const int Capacity = 4;

        // named fields rather than an inline array, which written at a variable index is never promoted, so every copy
        // of the node reloaded it whole; the spill paths take them as values so nothing takes the struct's address
        T _inline0;
        T _inline1;
        T _inline2;
        T _inline3;
        ImmutableList<T>? _spilled;
        int _prependCount;
        int _appendCount;

        public readonly int PrependCount => _prependCount;
        public readonly int AppendCount => _appendCount;
        public readonly int Count => _prependCount + _appendCount;

        public readonly T GetPrepended(int index) =>
            _spilled is null ? GetInline(Capacity - _prependCount + index) : _spilled[index];

        public readonly T GetAppended(int index) =>
            _spilled is null ? GetInline(index) : _spilled[_prependCount + index];

        public readonly Elements<T> Append(T element)
        {
            var next = this;
            if (_spilled is not null)
                next._spilled = _spilled.Add(element);
            else if (Count < Capacity)
                next.SetInline(_appendCount, element);
            else
            {
                var builder = Spill(_inline0, _inline1, _inline2, _inline3, _appendCount);
                builder.Add(element);
                next._spilled = builder.ToImmutable();
                next.ClearInline();
            }
            ++next._appendCount;
            return next;
        }

        public readonly Elements<T> Prepend(T element)
        {
            var next = this;
            if (_spilled is not null)
                next._spilled = _spilled.Insert(0, element);
            else if (Count < Capacity)
                next.SetInline(Capacity - 1 - _prependCount, element);
            else
            {
                var builder = Spill(_inline0, _inline1, _inline2, _inline3, _appendCount);
                builder.Insert(0, element);
                next._spilled = builder.ToImmutable();
                next.ClearInline();
            }
            ++next._prependCount;
            return next;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly T GetInline(int index) =>
            Pick(index, _inline0, _inline1, _inline2, _inline3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetInline(int index, T element)
        {
            switch (index)
            {
                case 0: _inline0 = element; break;
                case 1: _inline1 = element; break;
                case 2: _inline2 = element; break;
                default: _inline3 = element; break;
            }
        }

        // so the spilled copy does not keep the buffer's elements alive
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearInline()
        {
            _inline0 = default!;
            _inline1 = default!;
            _inline2 = default!;
            _inline3 = default!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Pick(int index, T inline0, T inline1, T inline2, T inline3) =>
            index switch
            {
                0 => inline0,
                1 => inline1,
                2 => inline2,
                _ => inline3,
            };

        // the appends from the given one on, with no prepends
        public readonly Elements<T> AppendsFrom(int start)
        {
            var result = default(Elements<T>);
            for (var i = start; i < _appendCount; ++i)
                result = result.Append(GetAppended(i));
            return result;
        }

        // the elements of the reversed sequence: the appends, reversed, lead it, and the prepends, reversed, follow it
        public readonly Elements<T> Reversed()
        {
            var result = default(Elements<T>);
            for (var i = 0; i < _appendCount; ++i)
                result = result.Prepend(GetAppended(i));
            for (var i = _prependCount - 1; i >= 0; --i)
                result = result.Append(GetPrepended(i));
            return result;
        }

        // a full buffer in output order: the prepends hold its slots from the appends' count on, the appends those before
        private static ImmutableList<T>.Builder Spill(T inline0, T inline1, T inline2, T inline3, int appendCount)
        {
            var builder = ImmutableList.CreateBuilder<T>();
            for (var i = appendCount; i < Capacity; ++i)
                builder.Add(Pick(i, inline0, inline1, inline2, inline3));
            for (var i = 0; i < appendCount; ++i)
                builder.Add(Pick(i, inline0, inline1, inline2, inline3));
            return builder;
        }
    }
}
