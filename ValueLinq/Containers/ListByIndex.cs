using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListByIndexFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly List<T> _list;
        private int _idx;

        public ListByIndexFastEnumerator(List<T> list) => (_list, _idx) = (list, -1);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _idx + 1;
            if (idx >= _list.Count)
            {
                current = default;
                return false;
            }
            current = _list[idx];
            _idx = idx;
            return true;
        }
    }

    public struct ListByIndexNode<T>
        : INode<T>
    {
        private readonly List<T> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListByIndexNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => ListByIndexNode.Create<T, Head, Tail, CreationType>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_list.Count;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) 
            => ListByIndexNode.FastEnumerate<T, TResult, FEnumerator>(_list, fenum);
    }

    static class ListByIndexNode
    {
        public static CreationType Create<T, Head, Tail, CreationType>(List<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListByIndexFastEnumerator<T>(list);
            return nodes.CreateObject<CreationType, T, ListByIndexFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(List<TIn> list, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                if (list == null)
                    throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

                if (list.Count < 20) // thumb in the air # from some random testing; depends on multiple things, so impossible to get 'perfect'
                {
                    DoLoop<TIn, FEnumerator>(list, ref fenum);
                }
                else if (BatchProcessResult.Unavailable == fenum.TryProcessBatch<List<TIn>, GetSpan<List<TIn>, TIn>>(list, in Optimizations.UseSpan<TIn>.FromList))
                {
                    SpanNode.Loop<TIn, FEnumerator>(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list), ref fenum);
                }
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void DoLoop<TIn, FEnumerator>(List<TIn> list, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = 0; i < list.Count; ++i)
            {
                if (!fenum.ProcessNext(list[i]))
                    break;
            }
        }
    }
}
