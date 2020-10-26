using System;

namespace Cistern.ValueLinq.Containers
{
    struct RangeFastEnumerator
        : IFastEnumerator<int>
    {
        private int _current;
        private int _max;

        public RangeFastEnumerator(int current, int max) => (_current, _max) = (current, max);

        public (bool, int)? InitialSize => (true, _max-_current+1);

        public void Dispose() { }

        public bool TryGetNext(out int current)
        {
            if (_max - _current >= 0)
            {
                current = _current++;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct RangeNode
        : INode
    {
        private int _start;
        private int _max;

        public RangeNode(int start, int count)
        {
            var max = (long)start + count - 1;
            if (count < 0 || max > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(count));

            (_start, _max) = (start, (int)max);
        }

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new RangeFastEnumerator(_start, _max);
            return nodes.CreateObject<CreationType, int, RangeFastEnumerator>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.TryLast))
            {
                result = (TResult)(object)(_max > _start, _max);
                return true;
            }
            result = default;
            return false;
        }
    }
}
