using System;

namespace Cistern.ValueLinq.Containers
{
    struct RepeatPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private T _element;
        private int _count;

        public RepeatPullEnumerator(T element, int count) => (_element, _count) = (element, count);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            if (_count > 0)
            {
                --_count;
                current = _element;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct RepeatNode<T>
        : INode<T>
    {
        private T _element;
        private int _count;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, true);

        public RepeatNode(T element, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            (_element, _count) = (element, count);
        }

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            var enumerator = new RepeatPullEnumerator<T>(_element, _count);
            return nodes.CreateObject<CreationType, T, RepeatPullEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                // TODO:
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                // TODO:
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                // TODO:
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                // TODO:
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                // TODO:
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => RepeatNode.ExecutePush<T, TResult, TPushEnumerator>(_element, _count, fenum);
    }

    static class RepeatNode
    {
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
}
