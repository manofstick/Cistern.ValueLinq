using System;

namespace Cistern.ValueLinq.Containers
{
    struct RangeFastEnumerator
        : IFastEnumerator<int>
    {
        private int _current;
        private int _max;

        public RangeFastEnumerator(int current, int max) => (_current, _max) = (current, max);

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
        : INode<int>
    {
        private int _start;
        private int _max;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_max - _start + 1, true);

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

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
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

            if (typeof(TRequest) == typeof(Optimizations.TryLast))
            {
                result = (TResult)(object)(_max > _start, _max);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<int>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
                => RangeNode.FastEnumerate<TResult, FEnumerator>(_start, _max, fenum);

        private static TResult FastEnumerate<TResult, FEnumerator>(int start, int max, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<int>
        {
            try
            { 
                Loop<FEnumerator>(start, max, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<FEnumerator>(int start, int max, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<int>
        {
            var i = start - 1;
            while (i < max)
            {
                if (!fenum.ProcessNext(++i))
                    break;
            }
        }
    }
}
