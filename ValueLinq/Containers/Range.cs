using System;

namespace Cistern.ValueLinq.Containers
{
    struct RangePullEnumerator
        : IPullEnumerator<int>
    {
        private int _current;
        private int _max;

        public RangePullEnumerator(int current, int max) => (_current, _max) = (current, max);

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

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            var enumerator = new RangePullEnumerator(_start, _max);
            return nodes.CreateObject<CreationType, int, RangePullEnumerator>(ref enumerator);
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

            if (typeof(TRequest) == typeof(Optimizations.TryLast))
            {
                result = (TResult)(object)(_max > _start, _max);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<int>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
                => RangeNode.ExecutePush<TResult, TPushEnumerator>(_start, _max, fenum);

        private static TResult ExecutePush<TResult, TPushEnumerator>(int start, int max, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<int>
        {
            try
            { 
                Loop<TPushEnumerator>(start, max, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TPushEnumerator>(int start, int max, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<int>
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
