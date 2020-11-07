using System;

namespace Cistern.ValueLinq.Containers
{
    struct ConcatFastEnumerator<T, Finish>
        : IFastEnumerator<T>
        where Finish : INode
    {
        private FastEnumerator<T> _current;
        private Finish _finish;
        int _state;

        public ConcatFastEnumerator(FastEnumerator<T> start, in Finish finish) => (_current, _finish, _state) = (start, finish, 0);

        public (bool, int)? InitialSize => null;

        public void Dispose()
        {
            if (_state < 2)
                _current.Dispose();
        }

        public bool TryGetNext(out T current)
        {
            tryAgain:
            if (_current.TryGetNext(out current))
                return true;

            if (TryGetNextFalse())
                goto tryAgain;

            return false;
        }

        private bool TryGetNextFalse()
        {
            switch (_state)
            {
                case 0:
                    _current.Dispose();
                    _current = Nodes<T>.CreateValueEnumerator(_finish).FastEnumerator;
                    _state = 1;
                    return true;

                case 1:
                    _current.Dispose();
                    _state = 2;
                    return false;

                default:
                    return false;
            }
        }
    }

    public struct ConcatNode<T, Start, Finish>
        : INode
        where Start : INode
        where Finish : INode
    {
        public void GetCountInformation(out int? maximumLength)
        {
            maximumLength = null;
        }

        //{
        //    get 
        //    {
        //        if (!_start.MaximumLength.HasValue || !_finish.MaximumLength.HasValue)
        //            return null;

        //        var max = (long)_start.MaximumLength + _finish.MaximumLength;
        //        if (max > int.MaxValue)
        //            return null;
        //        return (int)max;
        //    }
        //}

        private Start _start;
        private Finish _finish;

        public ConcatNode(in Start start, in Finish finish) => (_start, _finish) = (start, finish);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
                ConcatNode.Create<T, Start, Finish, Head, Tail, CreationType>(in _start, in _finish, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)(Count());
                return true;
            }

            result = default;
            return false;
        }

        private readonly int Count()
        {
            checked { return Enumerable.Count<T, Start>(_start) + Enumerable.Count<T, Finish>(_finish); }
        }

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            throw new NotImplementedException();
        }
    }

    static class ConcatNode
    {
        public static CreationType Create<T, Start, Finish, Head, Tail, CreationType>(in Start start, in Finish finish, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Start : INode
            where Finish : INode
        {
            var startEnumerator = Nodes<T>.CreateValueEnumerator(start);

            var e = new ConcatFastEnumerator<T, Finish>(startEnumerator.FastEnumerator, finish);
            return nodes.CreateObject<CreationType, T, ConcatFastEnumerator<T, Finish>>(ref e);
        }
    }
}
