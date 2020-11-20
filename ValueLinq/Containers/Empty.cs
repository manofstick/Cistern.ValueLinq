using System;

namespace Cistern.ValueLinq.Containers
{
    struct EmptyFastEnumerator<T>
        : IFastEnumerator<T>
    {
        public void Dispose() { }

        public bool TryGetNext(out T current) { current = default; return false; }

        public static readonly FastEnumerator<T> Instance = new FastEnumerator<EmptyFastEnumerator<T>, T>(new EmptyFastEnumerator<T>());
    }

    

    public struct EmptyNode<T>
        : INode<T>
    {
        public void GetCountInformation(out CountInformation info) => 
            info = new CountInformation(0, true);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => EmptyNode.Create<T, Nodes<Head, Tail>, CreationType>(ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => EmptyNode.FastEnumerate<T, TResult, FEnumerator>(fenum);
    }

    static class EmptyNode
    {
        public static CreationType Create<T, Nodes, CreationType>(ref Nodes nodes)
            where Nodes : INodes
        {
            var enumerator = new EmptyFastEnumerator<T>();
            return nodes.CreateObject<CreationType, T, EmptyFastEnumerator<T>>(0, ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(FEnumerator fenum) where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }
    }
}
