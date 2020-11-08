using System;

namespace Cistern.ValueLinq.Containers
{
    struct ReturnFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private T _element;
        private bool _iterated;

        public ReturnFastEnumerator(T element) => (_element, _iterated) = (element, false);

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
        : INode
    {
        private T _element;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(1, true);

        public ReturnNode(T element) => _element = element;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new ReturnFastEnumerator<T>(_element);
            return nodes.CreateObject<CreationType, T, ReturnFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
            => ReturnNode.FastEnumerate<TIn, TResult, FEnumerator>((TIn)(object)_element, fenum);
    }

    static class ReturnNode
    {
        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(TIn element, FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            fenum.ProcessNext(element);
            return fenum.GetResult<TResult>();
        }
    }
}
