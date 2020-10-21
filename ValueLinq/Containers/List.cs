using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly List<T> _list;
        private int _idx;

        public ListFastEnumerator(List<T> list) => (_list, _idx) = (list, -1);

        public int? InitialSize => _list.Count;

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

    public struct ListNode<T>
        : INode
    {
        private readonly List<T> _list;

        public ListNode(List<T> list) => _list = list;

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            var enumerator = new ListFastEnumerator<T>(_list);
            return nodes.CreateObject<CreationType, T, ListFastEnumerator<T>>(ref enumerator);
        }

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        bool INode.CheckForOptimization<TOuter, TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }
    }
}
