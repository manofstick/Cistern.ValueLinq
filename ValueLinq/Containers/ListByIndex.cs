using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct ListByIndexFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly List<T> _list;
        private int _idx;

        public ListByIndexFastEnumerator(List<T> list) => (_list, _idx) = (list, -1);

        public (bool, int)? InitialSize => (true, _list.Count);

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
        : INode
    {
        private readonly List<T> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListByIndexNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => ListByIndexNode.Create<T, Head, Tail, CreationType>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_list.Count;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode.CreateObjectViaFastEnumerator<TIn, TResult, FEnumerator>(in FEnumerator fenum) 
            => ListByIndexNode.FastEnumerate<TIn, TResult, FEnumerator>((List<TIn>)(object)_list, fenum);
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
            fenum.Init(list.Count);
            DoLoop(list, ref fenum);
            return fenum.GetResult<TResult>();
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
