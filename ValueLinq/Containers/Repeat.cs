using System;

namespace Cistern.ValueLinq.Containers
{
    struct RepeatFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private T _element;
        private int _count;

        public RepeatFastEnumerator(T element, int count) => (_element, _count) = (element, count);

        public (bool, int)? InitialSize => (true, _count);

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
        : INode
    {
        private T _element;
        private int _count;

        public RepeatNode(T element, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            (_element, _count) = (element, count);
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new RepeatFastEnumerator<T>(_element, _count);
            return nodes.CreateObject<CreationType, T, RepeatFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            throw new NotImplementedException();
        }
    }
}
