using System;

namespace Cistern.ValueLinq.Containers
{
    struct ReturnPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private T _element;
        private bool _iterated;

        public ReturnPullEnumerator(T element) => (_element, _iterated) = (element, false);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (!_iterated)
            {
                _iterated = true;
                current = _element;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ReturnNode<T>
        : INode<T>
    {
        private T _element;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(1, true);

        public ReturnNode(T element) => _element = element;

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) =>
            ReturnNode.Create<T, CreationType, TNodes>(_element, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) =>
            ReturnNode.TryPushOptimization<T, TRequest, TResult>(_element, in request, out result);

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => ReturnNode.ExecutePush<T, TResult, TPushEnumerator>(_element, fenum);
    }

    static class ReturnNode
    {
        internal static bool TryPushOptimization<TElement, TRequest, TResult>(TElement element, in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        internal static CreationType Create<T, CreationType, TNodes>(T _element, ref TNodes nodes)
            where TNodes : INodes
        {
            var enumerator = new ReturnPullEnumerator<T>(_element);
            return nodes.CreateObject<CreationType, T, ReturnPullEnumerator<T>>(ref enumerator);
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
}
