namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// One element repeated a count of times.
public static partial class CisternRepeat
{
    [Answerer]
    public partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationCopyTo<T>
        , Optimizations.IOptimizationSkipTake<T>
        , Optimizations.IOptimizationElementAt<T>
        , Optimizations.IOptimizationContains<T>
    {
        internal T _element;
        internal int _count;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, true);

        public Node(T element, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            _element = element;
            _count = count;
        }

        private bool CopyTo(in Optimizations.CopyTo<T> request, out int response)
        {
            response = _count;
            // the destination is newly allocated, so a reference slot already holds null
            if (!request.IsDryRun
             && !(RuntimeHelpers.IsReferenceOrContainsReferences<T>() && _element is null))
            {
                request.Span[.._count].Fill(_element);
            }

            return true;
        }

        private bool ElementAt(in Optimizations.ElementAt request, out (bool, T) response)
        {
            var found = request.OffsetIn(_count) >= 0;
            response = (found, found ? _element : default!);
            return true;
        }

        // as System.Linq's, the element compared once
        private bool Contains(in Optimizations.Contains<T> request, out bool response)
        {
            response = _count > 0 && EqualityComparer<T>.Default.Equals(request.Value, _element);
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            request.Of(_count, out _, out var count);

            response = default;
            if (count == 0)
                response.SetEmpty();
            else
                response.SetNode(new Node<T>(_element, count));

            return true;
        }

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CisternRepeat.ExecutePush<T, TPushResult, TPushEnumerator>(_element, _count, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    public partial struct Pull<T>
    {
        private T _element;
        private int _count;

        internal Pull(T element, int count)
        {
            _element = element;
            _count = count;
        }

        private static Pull<T> Create(in Node<T> node)
            => new(node._element, node._count);

        public bool MoveNext()
        {
            if (_count > 0)
            {
                --_count;
                return true;
            }
            return false;
        }

        public T Current => _element;
    }

    internal static TResult ExecutePush<TElement, TResult, TPushEnumerator>(TElement element, int count, TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<TElement>
    {
        try
        {
            Loop<TElement, TPushEnumerator>(element, count, ref fenum);
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void Loop<TElement, TPushEnumerator>(TElement element, int count, ref TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<TElement>
    {
        for(var i=0; i < count; ++i)
        {
            if (!fenum.ProcessNext(element))
                break;
        }
    }
}
