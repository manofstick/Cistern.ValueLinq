using System;
using System.Runtime.CompilerServices;

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

        public int? MaximumLength => _max-_start+1;

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

        public TResult CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
                => RangeNode.FastEnumerate<TIn, TResult, FEnumerator>(_start, _max, fenum);

        private static TResult FastEnumerate<TIn, TResult, FEnumerator>(int start, int max, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            fenum.Init(max - start + 1);
            Loop<TIn, FEnumerator>(start, max, ref fenum);
            return fenum.GetResult<TResult>();
        }

        private static void Loop<TIn, FEnumerator>(int start, int max, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            var i = start - 1;
            while (i < max)
            {
                fenum.ProcessNext((TIn)(object)++i);
            }
        }
    }
}
