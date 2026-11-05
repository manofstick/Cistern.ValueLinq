namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;

// The integers from a start for a count, generated as they are pushed or pulled.
public static partial class SystemRange
{
    [Answerer]
    public readonly partial struct Node
        : INode<int>
        , Optimizations.IOptimizationElementAt<int>
        , Optimizations.IOptimizationCopyTo<int>
        , Optimizations.IOptimizationContains<int>
        , Optimizations.IOptimizationSkipTake<int>
        , Optimizations.IOptimizationAsRange
    {
        internal readonly int _start;
        internal readonly int _max;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_max - _start + 1, true);

        public Node(int start, int count)
        {
            var max = (long)start + count - 1;
            if (count < 0 || max > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(count));

            _start = start;
            _max = (int)max;
        }

        // _max is the last element; the length is read from the count, as an empty range starting at int.MinValue
        // wraps _max round to int.MaxValue, and the count wraps back to 0
        private bool ElementAt(in Optimizations.ElementAt request, out (bool, int) response)
        {
            var offset = request.OffsetIn(_max - _start + 1);
            response = (offset >= 0, offset < 0 ? 0 : _start + offset);
            return true;
        }

        private bool AsRange(in Optimizations.AsRange request, out (int Start, int Count) response)
        {
            response = (_start, _max - _start + 1);
            return true;
        }

        private bool CopyTo(in Optimizations.CopyTo<int> request, out int response)
        {
            var count = _max - _start + 1;
            response = count;
            if (request.IsDryRun)
                return true;

            CisternSequence.Fill(request.Span[..count], _start, 1);
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<int> response)
        {
            request.Of(_max - _start + 1, out var start, out var count);

            response = default;
            if (count == 0)
                response.SetEmpty();
            else
                response.SetRange(_start + start, count);

            return true;
        }

        private bool Contains(in Optimizations.Contains<int> request, out bool response)
        {
            response = (uint)(request.Value - _start) < (uint)(_max - _start + 1);
            return true;
        }

        TPushResult INode<int>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
                => ExecutePush<int, TPushResult, TPushEnumerator>(_start, _max, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));

        // generic over the element for a range held in a NodeContainer<TElement>, whose TElement is then int
        internal static TResult ExecutePush<TElement, TResult, TPushEnumerator>(int start, int max, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TElement>
        {
            try
            {
                var i = start - 1;
                while (i < max)
                {
                    if (!fenum.ProcessNext((TElement)(object)++i))
                        break;
                }
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }
    }

    // MoveNext is a single comparison, as an inlined MoveNext with two returns leaves its result in a register its
    // caller tests again; the index steps one past the count at the end, where nothing reads Current
    [Puller]
    public partial struct Pull
    {
        private readonly int _start;
        private readonly int _count;
        private int _index;

        internal Pull(int start, int count)
        {
            _start = start;
            _count = count;
            _index = -1;
        }

        // the count as the node reads it, which wraps back to 0 for an empty range starting at int.MinValue
        private static Pull Create(in Node node)
            => new(node._start, node._max - node._start + 1);

        public bool MoveNext() => ++_index < _count;

        public int Current => _start + _index;
    }
}
