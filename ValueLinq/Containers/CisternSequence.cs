namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

public static partial class CisternSequence
{
    [Answerer]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationContains<T>
        , Optimizations.IOptimizationAsRange
        where T : INumber<T>
    {
        internal readonly T _start;
        internal readonly T _endInclusive;
        internal readonly T _step;
        internal readonly int? _length;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_length, true);

        public Node(T start, T endInclusive, T step)
        {
            if (start is null)
                throw new ArgumentNullException(nameof(start));

            if (T.IsNaN(start))
                throw new ArgumentOutOfRangeException(nameof(start));

            if (endInclusive is null)
                throw new ArgumentNullException(nameof(endInclusive));

            if (T.IsNaN(endInclusive))
                throw new ArgumentOutOfRangeException(nameof(endInclusive));

            if (step is null)
                throw new ArgumentNullException(nameof(step));

            if (T.IsNaN(step))
                throw new ArgumentOutOfRangeException(nameof(step));

            if (T.IsZero(step)
                && start != endInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(step));
            }

            if (T.IsPositive(step)
                && endInclusive < start)
            {
                throw new ArgumentOutOfRangeException(nameof(endInclusive));
            }

            if (!T.IsPositive(step)
                && endInclusive > start)
            {
                throw new ArgumentOutOfRangeException(nameof(endInclusive));
            }

            _start = start;
            _endInclusive = endInclusive;
            _step = step;
            _length = LengthOf(start, endInclusive, step);
        }

        internal Node(T start, T endInclusive, T step, int length)
        {
            _start = start;
            _endInclusive = endInclusive;
            _step = step;
            _length = length;
        }

        private static bool IsPrimitiveInteger =>
            typeof(T) == typeof(byte)
            || typeof(T) == typeof(sbyte)
            || typeof(T) == typeof(short)
            || typeof(T) == typeof(ushort)
            || typeof(T) == typeof(char)
            || typeof(T) == typeof(int)
            || typeof(T) == typeof(uint)
            || typeof(T) == typeof(long)
            || typeof(T) == typeof(ulong)
            || typeof(T) == typeof(nint)
            || typeof(T) == typeof(nuint);

        // POTENTIAL OPTIMIZATION: the length of an Int128, UInt128 or BigInteger sequence, from its distance in BigInteger (which allocates past a long's range, at construction); a floating-point walk's length is known only by walking it
        private static int? LengthOf(T start, T endInclusive, T step)
        {
            if (T.IsZero(step))
                return 1;

            if (!IsPrimitiveInteger)
                return null;

            // a primitive integer is at most 64 bits, so its distance and stride fit in a ulong, the subtraction of the two
            // ends wrapping to their distance whichever sides of zero they are on
            var distance =
                endInclusive >= start
                    ? unchecked(ulong.CreateTruncating(endInclusive) - ulong.CreateTruncating(start))
                    : unchecked(ulong.CreateTruncating(start) - ulong.CreateTruncating(endInclusive));

            var steps = distance / Stride(step);

            int? fitted =
                steps < int.MaxValue
                    ? (int)steps + 1
                    : null;

            return fitted;
        }

        private static ulong Stride(T step) =>
            T.IsNegative(step)
                ? unchecked(0UL - ulong.CreateTruncating(step))
                : ulong.CreateTruncating(step);

        // an integer is held where it lies between the walk's ends a whole number of steps from its start; a
        // floating-point walk's values depend on how its additions round, so it is walked
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            if (!IsPrimitiveInteger)
            {
                response = false;

                return false;
            }

            var value = request.Value;
            if (T.IsZero(_step))
            {
                response = value == _start;

                return true;
            }

            var ascending = T.IsPositive(_step);
            var within =
                ascending
                    ? value >= _start && value <= _endInclusive
                    : value <= _start && value >= _endInclusive;

            if (!within)
            {
                response = false;

                return true;
            }

            var distance =
                ascending
                    ? unchecked(ulong.CreateTruncating(value) - ulong.CreateTruncating(_start))
                    : unchecked(ulong.CreateTruncating(_start) - ulong.CreateTruncating(value));

            response = distance % Stride(_step) == 0;

            return true;
        }

        // an int walk of step one is a range
        private bool AsRange(in Optimizations.AsRange request, out (int Start, int Count) response)
        {
            if (typeof(T) == typeof(int)
                && _step == T.One
                && _length is int length)
            {
                response = ((int)(object)_start!, length);

                return true;
            }

            response = default;

            return false;
        }

        // exact for an integer type, whose multiplication wraps as the additions of the walk would
        private T At(int offset)
        {
            var element =
                offset == 0
                    ? _start
                    : _start + (_step * T.CreateTruncating(offset));

            return element;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            if (_length is not int length)
            {
                response = (true, _start);
                var isFirst = request.Index is { IsFromEnd: false, Value: 0 };

                return isFirst;
            }

            response =
                request.OffsetIn(length) switch
                {
                    < 0 => (false, default!),
                    var offset => (true, At(offset)),
                };

            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            if (_length is not int length)
            {
                response = 0;

                return false;
            }

            response = length;
            if (request.IsDryRun)
                return true;

            var destination = request.Span[..length];
            if (Fill(destination, _start, _step))
                return true;

            var current = _start;
            destination[0] = current;
            for (var i = 1; i < destination.Length; ++i)
            {
                current += _step;
                destination[i] = current;
            }

            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            if (_length is not int length)
            {
                response = default;

                return false;
            }

            request.Of(length, out var start, out var count);

            response = default;
            if (count == 0)
                response.SetEmpty();
            else if (typeof(T) == typeof(int) && _step == T.One)
                response.SetRange((int)(object)At(start)!, count); // a container's range, where a node would be boxed
            else
                response.SetNode(Slice(start, count));

            return true;
        }

        private Node<T> Slice(int start, int count)
        {
            var lastOffset = start + count - 1;
            var first = At(start);
            var last = At(lastOffset);

            var slice = new Node<T>(first, last, _step, count);

            return slice;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            if (_length is not int length)
            {
                response = default;

                return false;
            }

            var reversed =
                length == 1
                    ? this
                    : Reversed(length);

            response = default;
            response.SetNode(reversed);

            return true;
        }

        private Node<T> Reversed(int length)
        {
            var lastOffset = length - 1;
            var last = At(lastOffset);
            // an unsigned step negates by wrapping, which the additions wrap back
            var step = -_step;

            var reversed = new Node<T>(last, _start, step, length);

            return reversed;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler) =>
            ExecutePush<TPushResult, TPushEnumerator>(PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        private TResult ExecutePush<TResult, TPushEnumerator>(TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            try
            {
                if (_length is int length)
                    PushLength(_start, _step, length, ref fenum);
                else
                    PushWalk(_start, _endInclusive, _step, ref fenum);

                var result = fenum.GetResult<TResult>();

                return result;
            }
            finally
            {
                fenum.Dispose();
            }
        }

        // POTENTIAL OPTIMIZATION: the elements generated into a StackBuffer<T> and offered as batches (ProcessBatch, BatchKinds.Container), so an aggregator's span path (Sum's SIMD loop) runs over them; Range would take the same
        private static void PushLength<TPushEnumerator>(T start, T step, int length, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            var current = start;
            var remaining = length;
            while (fenum.ProcessNext(current)
                && --remaining > 0)
            {
                current += step;
            }
        }

        private static void PushWalk<TPushEnumerator>(T start, T endInclusive, T step, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            var ascending = T.IsPositive(step);
            var current = start;
            var more = fenum.ProcessNext(current);
            while (more)
            {
                var next = current + step;
                if (StepsShortOfEnd(current, next, endInclusive, ascending))
                {
                    more = fenum.ProcessNext(next);
                    current = next;
                }
                else
                {
                    if (LandsOnEnd(current, next, endInclusive))
                        fenum.ProcessNext(next);

                    more = false;
                }
            }
        }
    }

    [Puller]
    public partial struct Pull<T>
        where T : INumber<T>
    {
        private enum State : byte
        {
            LengthFromStart,
            Length,
            WalkFromStart,
            Walk,
            Ended,
        }

        private readonly T _endInclusive;
        private readonly T _step;
        private readonly bool _ascending;
        private T _current;
        private int _remaining;
        private State _state;

        internal Pull(in Node<T> node)
        {
            _endInclusive = node._endInclusive;
            _step = node._step;
            _ascending = T.IsPositive(node._step);
            _current = node._start;
            (_state, _remaining) =
                node._length switch
                {
                    int length => (State.LengthFromStart, length),
                    null => (State.WalkFromStart, 0),
                };
        }

        private static Pull<T> Create(in Node<T> node) =>
            new(in node);

        public bool MoveNext()
        {
            var moved =
                _state switch
                {
                    State.LengthFromStart => Begin(State.Length),
                    State.Length => MoveAlongLength(),
                    State.WalkFromStart => Begin(State.Walk),
                    State.Walk => MoveAlongWalk(),
                    _ => false,
                };

            return moved;
        }

        public T Current => _current;

        private bool Begin(State state)
        {
            _state = state;

            return true;
        }

        private bool MoveAlongLength()
        {
            var moved = --_remaining > 0;
            if (moved)
                _current += _step;
            else
                _state = State.Ended;

            return moved;
        }

        private bool MoveAlongWalk()
        {
            var next = _current + _step;
            var moved = StepsShortOfEnd(_current, next, _endInclusive, _ascending);
            if (!moved)
            {
                _state = State.Ended;
                moved = LandsOnEnd(_current, next, _endInclusive);
            }

            if (moved)
                _current = next;

            return moved;
        }
    }

    private static bool StepsShortOfEnd<T>(T current, T next, T endInclusive, bool ascending)
        where T : INumber<T>
    {
        var shortOfEnd =
            ascending
                ? next < endInclusive
                    && next > current
                : next > endInclusive
                    && next < current;

        return shortOfEnd;
    }

    private static bool LandsOnEnd<T>(T current, T next, T endInclusive)
        where T : INumber<T>
    {
        var lands =
            next == endInclusive
            && current != next;

        return lands;
    }

    // a primitive integer walk written a vector at a time, which wraps as its additions do, as a range's is with a step of
    // one; reached through IFill, as T is not constrained to a struct
    internal static bool Fill<T>(Span<T> destination, T start, T step)
        where T : INumber<T>
    {
        if (typeof(T) == typeof(int))
            return ((IFill<T>)(object)default(VectorFill<int>)).Fill(destination, start, step);
        if (typeof(T) == typeof(long))
            return ((IFill<T>)(object)default(VectorFill<long>)).Fill(destination, start, step);
        if (typeof(T) == typeof(short))
            return ((IFill<T>)(object)default(VectorFill<short>)).Fill(destination, start, step);
        if (typeof(T) == typeof(byte))
            return ((IFill<T>)(object)default(VectorFill<byte>)).Fill(destination, start, step);
        if (typeof(T) == typeof(uint))
            return ((IFill<T>)(object)default(VectorFill<uint>)).Fill(destination, start, step);
        if (typeof(T) == typeof(ulong))
            return ((IFill<T>)(object)default(VectorFill<ulong>)).Fill(destination, start, step);
        if (typeof(T) == typeof(ushort))
            return ((IFill<T>)(object)default(VectorFill<ushort>)).Fill(destination, start, step);
        if (typeof(T) == typeof(sbyte))
            return ((IFill<T>)(object)default(VectorFill<sbyte>)).Fill(destination, start, step);
        if (typeof(T) == typeof(nint))
            return ((IFill<T>)(object)default(VectorFill<nint>)).Fill(destination, start, step);
        if (typeof(T) == typeof(nuint))
            return ((IFill<T>)(object)default(VectorFill<nuint>)).Fill(destination, start, step);

        return false;
    }

    private interface IFill<T>
    {
        bool Fill(Span<T> destination, T start, T step);
    }

    private readonly struct VectorFill<T>
        : IFill<T>
        where T : struct, INumber<T>
    {
        bool IFill<T>.Fill(Span<T> destination, T start, T step)
        {
            var i = 0;
            if (Vector.IsHardwareAccelerated
                && Vector<T>.IsSupported
                && destination.Length >= Vector<T>.Count)
            {
                // stored through the span of whole vectors, as CopyTo into a slice keeps the slice and its length check in
                // the loop
                var vectors = MemoryMarshal.Cast<T, Vector<T>>(destination);
                var steps = new Vector<T>(step);
                var current = new Vector<T>(start) + Vector<T>.Indices * steps;
                var increment = steps * new Vector<T>(T.CreateTruncating(Vector<T>.Count));
                foreach (ref var slot in vectors)
                {
                    slot = current;
                    current += increment;
                }
                i = vectors.Length * Vector<T>.Count;
            }

            var value = start + T.CreateTruncating(i) * step;
            for (; i < destination.Length; ++i)
            {
                destination[i] = value;
                value += step;
            }

            return true;
        }
    }
}
