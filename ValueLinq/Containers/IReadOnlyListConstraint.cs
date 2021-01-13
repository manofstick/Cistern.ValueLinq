using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct IReadOnlyListPullEnumerator<T, List>
        : IPullEnumerator<T>
        where List : IReadOnlyList<T>
    {
        private readonly List _list;
        private int _idx;

        public IReadOnlyListPullEnumerator(List list) => (_list, _idx) = (list, -1);

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

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => IReadOnlyListNode.Create<T, TNodes, CreationType, List>(_list, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_list.Count;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) 
            => IReadOnlyListNode.ExecutePush<T, TResult, TPushEnumerator, List>(_list, fenum);
    }

    static class IReadOnlyListNode
    {
        public static CreationType Create<T, TNodes, CreationType, List>(List list, ref TNodes nodes)
            where TNodes : INodes
            where List : IReadOnlyList<T>
        {
            var enumerator = new IReadOnlyListPullEnumerator<T, List>(list);
            return nodes.CreateObject<CreationType, T, IReadOnlyListPullEnumerator<T, List>>(ref enumerator);
        }

        internal static TResult ExecutePush<T, TResult, TPushEnumerator, List>(List list, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
            where List : IReadOnlyList<T>
        {
            try
            { 
                DoLoop<T, TPushEnumerator, List>(list, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void DoLoop<T, TPushEnumerator, List>(List list, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
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
