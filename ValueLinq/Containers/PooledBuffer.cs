namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using Cistern.ValueLinq.Utils;
using System.Threading;

// What a buffering node (Reverse, Shuffle, OrderBy) materialises into when it is pulled: its buffer, rented for the one
// enumeration, which the enumerator returns to the pool when it is disposed.
//
// The enumerator is a public struct, which a caller can copy, and copies share the rented array. A copy disposed
// would otherwise return the array while another still reads it, or return it a second time, so that the pool lends
// it out twice and an unrelated renter's data is overwritten. The array is therefore held through a Lease, one object
// every copy shares: it is returned once however many copies are disposed, and a copy pulled after that ends. A copy
// is never safe to use (one of a chain over an IEnumerator<T> pulls an enumerator another copy disposed), but through
// the lease the harm stays with the copy, which at worst reads one element of an array lent out again.
public static partial class PooledBuffer
{
    internal sealed class Lease<T>(PooledArray<T> array)
    {
        private readonly PooledArray<T> _array = array;
        private int _returned;

        internal bool Returned => _returned != 0;

        internal void Return()
        {
            if (Interlocked.Exchange(ref _returned, 1) == 0)
                _array.Return();
        }
    }

    [Answerer]
    public readonly partial struct Node<T>
        : INode<T>
    {
        internal readonly PooledArray<T> _array;

        internal Node(PooledArray<T> array) => _array = array;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_array.Count, true);

        // pushed, the buffer is returned once the loop ends, so the node is pushed or enumerated once
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => SystemArray.ExecutePush<T, TPushResult, TPushEnumerator>(_array, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    public partial struct Pull<T>
    {
        private readonly T[] _array;
        // null where the array is not rented (the source's own ToArray answer, a buffer too small to rent)
        private readonly Lease<T>? _lease;
        private readonly int _end;
        private int _idx;

        private Pull(in PooledArray<T> array)
        {
            _array = array.Array;
            _lease = array.IsRented ? new Lease<T>(array) : null;
            _end = array.Count;
            _idx = -1;
        }

        private static Pull<T> Create(in Node<T> node) => new(in node._array);

        public bool MoveNext() => ++_idx < _end && (_lease is null || !_lease.Returned);

        public T Current => _array[_idx];

        public void Dispose() => _lease?.Return();
    }
}
