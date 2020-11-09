using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct IReadOnlyListFastEnumerator<T, List>
        : IFastEnumerator<T>
        where List : IReadOnlyList<T>
    {
        private readonly List _list;
        private int _idx;

        public IReadOnlyListFastEnumerator(List list) => (_list, _idx) = (list, -1);

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

    public struct IReadOnlyListNode<T, List>
        : INode<T>
        where List : IReadOnlyList<T>
    {
        private readonly List _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public IReadOnlyListNode(List list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => IReadOnlyListNode.Create<T, Head, Tail, CreationType, List>(_list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

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

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) 
            => IReadOnlyListNode.FastEnumerate<T, TResult, FEnumerator, List>(_list, fenum);
    }

    static class IReadOnlyListNode
    {
        public static CreationType Create<T, Head, Tail, CreationType, List>(List list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where List : IReadOnlyList<T>
        {
            var enumerator = new IReadOnlyListFastEnumerator<T, List>(list);
            return nodes.CreateObject<CreationType, T, IReadOnlyListFastEnumerator<T, List>>(ref enumerator);
        }

        internal static TResult FastEnumerate<T, TResult, FEnumerator, List>(List list, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where List : IReadOnlyList<T>
        {
            DoLoop<T, FEnumerator, List>(list, ref fenum);
            return fenum.GetResult<TResult>();
        }

        internal static void DoLoop<T, FEnumerator, List>(List list, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where List : IReadOnlyList<T>
        {
            var count = list.Count;
            for (var i = 0; i < count; ++i)
            {
                if (!fenum.ProcessNext(list[i]))
                    break;
            }
        }
    }

}
