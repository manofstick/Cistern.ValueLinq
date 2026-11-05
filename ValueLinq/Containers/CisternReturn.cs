namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;

// A sequence of one element.
public static partial class CisternReturn
{
    [Answerer]
    public partial struct Node<T>
        : INode<T>
    {
        internal T _element;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(1, true);

        public Node(T element) => _element = element;

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CisternReturn.ExecutePush<T, TPushResult, TPushEnumerator>(_element, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    public partial struct Pull<T>
    {
        private T _element;
        private bool _iterated;

        internal Pull(T element)
        {
            _element = element;
            _iterated = false;
        }

        private static Pull<T> Create(in Node<T> node)
            => new(node._element);

        public bool MoveNext()
        {
            if (!_iterated)
            {
                _iterated = true;
                return true;
            }
            return false;
        }

        public T Current => _element;
    }

    internal static TResult ExecutePush<TElement, TResult, TPushEnumerator>(TElement element, TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<TElement>
    {
        try
        {
            fenum.ProcessNext(element);
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }
}
