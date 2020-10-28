using System;

namespace Cistern.ValueLinq.Aggregation
{
#if OLD_WAY
    struct SumInt
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Impl.CreateObjectDescent<CreationType>();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
            {
                try
                {
                    var total = 0;
                    while (enumerator.TryGetNext(out var current))
                        total += (int)(object)current;
                    return (CreationType)(object)total;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
            => Impl.CheckForOptimization(out result);

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }

    struct SumDouble
        : INode
    {
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotImplementedException();

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator enumerator)
        {
            checked
            {
                try
                {
                    var total = 0.0;
                    while (enumerator.TryGetNext(out var current))
                        total += (double)(object)current;
                    return (CreationType)(object)total;
                }
                finally
                {
                    enumerator.Dispose();
                }
            }
        }

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum)
            => Impl.CreateObjectViaFastEnumerator<TResult>();
    }
#endif
    struct SumIntForward
        : IForwardEnumerator<int>
    {
        private int _sum;

        public void Init(int? size) { }

        public TResult GetResult<TResult>() => (TResult)(object)_sum;

        public bool ProcessNext(int input)
        {
            checked
            {
                _sum += input;
                return true;
            }
        }
    }

    struct SumDoubleForward
        : IForwardEnumerator<double>
    {
        private double _sum;

        public TResult GetResult<TResult>() => (TResult)(object)_sum;

        public void Init(int? _) { }

        public bool ProcessNext(double input)
        {
            _sum += input;
            return true;
        }
    }

}
