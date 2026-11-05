namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Numerics;

public static partial class CisternInfiniteSequence
{
    [Answerer]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationSkipTake<T>
        where T : IAdditionOperators<T, T, T>
    {
        internal readonly T _start;
        internal readonly T _step;
        internal readonly int? _length;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_length, true);

        public Node(T start, T step)
        {
            if (start is null)
                throw new ArgumentNullException(nameof(start));

            if (step is null)
                throw new ArgumentNullException(nameof(step));

            _start = start;
            _step = step;
            _length = null;
        }

        private Node(T start, T step, int? length)
        {
            _start = start;
            _step = step;
            _length = length;
        }

        // an addition that wraps or is exact is associative, so a jump to an element gives what the additions give
        private static bool IsAssociative =>
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
            || typeof(T) == typeof(nuint)
            || typeof(T) == typeof(Int128)
            || typeof(T) == typeof(UInt128)
            || typeof(T) == typeof(BigInteger);

        // the step added offset times by doubling, as the constraint gives no multiplication; only for IsAssociative
        private T At(long offset)
        {
            if (offset == 0)
                return _start;

            var multiple = _step;

            for (var bit = 62 - BitOperations.LeadingZeroCount((ulong)offset); bit >= 0; --bit)
            {
                multiple += multiple;
                if (((offset >> bit) & 1) != 0)
                    multiple += _step;
            }

            var element = _start + multiple;

            return element;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            int? offset =
                _length switch
                {
                    int length => request.OffsetIn(length),
                    null when request.Index.IsFromEnd => null,
                    null => request.Index.Value,
                };

            switch (offset)
            {
                case < 0:
                    response = (false, default!);

                    return true;

                case 0:
                    response = (true, _start);

                    return true;

                case int jump when IsAssociative:
                    response = (true, At(jump));

                    return true;

                default:
                    response = default;

                    return false;
            }
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
            response = default;

            if (_length is int length)
            {
                request.Of(length, out var sliceStart, out var sliceCount);

                return TrySlice(sliceStart, sliceCount, ref response);
            }

            if (request.TakesAll)
                return TrySlice(request.Skip, null, ref response);

            if (request.Take <= int.MaxValue)
                return TrySlice(request.Skip, (int)request.Take, ref response);

            return false;
        }

        private bool TrySlice(long skip, int? count, ref NodeContainer<T> response)
        {
            if (count is 0)
            {
                response.SetEmpty();

                return true;
            }

            if (skip > 0
                && !IsAssociative)
            {
                return false;
            }

            var first = At(skip);
            var slice = new Node<T>(first, _step, count);
            response.SetNode(slice);

            return true;
        }

        // POTENTIAL OPTIMIZATION: Reverse of a node of known length over an IsAssociative type, as CisternSequence's (its last element, the step negated); IAdditionOperators gives no negation, so it needs a typeof dispatch to the integer type's own
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
                    PushUnbounded(_start, _step, ref fenum);

                var result = fenum.GetResult<TResult>();

                return result;
            }
            finally
            {
                fenum.Dispose();
            }
        }

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

        private static void PushUnbounded<TPushEnumerator>(T start, T step, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            var current = start;
            while (fenum.ProcessNext(current))
                current += step;
        }
    }

    [Puller]
    public partial struct Pull<T>
        where T : IAdditionOperators<T, T, T>
    {
        private readonly T _step;
        private T _current;
        private int? _remaining;
        private bool _started;

        internal Pull(in Node<T> node)
        {
            _step = node._step;
            _current = node._start;
            _remaining = node._length - 1;
        }

        private static Pull<T> Create(in Node<T> node) =>
            new(in node);

        public bool MoveNext()
        {
            if (!_started)
            {
                _started = true;

                return true;
            }

            if (_remaining is 0)
                return false;

            --_remaining;
            _current += _step;

            return true;
        }

        public T Current => _current;
    }
}
