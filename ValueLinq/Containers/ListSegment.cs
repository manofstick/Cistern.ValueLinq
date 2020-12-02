using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListSegmentFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly List<T> _list;
        private readonly int _lastIdx;
        private int _currentIdx;

        public ListSegmentFastEnumerator(in ListSegment<T> list) => (_list, _currentIdx, _lastIdx) = (list.List, list.Start-1, list.Start+list.Count-1);

        public void Dispose() { }

        public bool TryGetNext(out T current)
        {
            var idx = _currentIdx + 1;
            if (idx > _lastIdx)
            {
                current = default;
                return false;
            }
            current = _list[idx];
            _currentIdx = idx;
            return true;
        }
    }

    struct ListSegment<T>
    {
        public ListSegment(List<T> list, int start, int count) => (List, Start, Count) = (list, start, count);

        public List<T> List { get; }
        public int Start { get; }
        public int Count { get; }
    }

    public struct ReversedListSegmentNode<T>
        : INode<T>
    {
        private readonly ListSegment<T> _list; // list is still in forward order

        internal ReversedListSegmentNode(List<T> list, int start, int count) => _list = new ListSegment<T>(list, start, count);

        #region "This node is only used in forward context, so most of interface is not supported"
        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException();
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotSupportedException();
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
        #endregion

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        public bool CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.ReverseSkip(in _list, skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.ReverseTake(in _list, take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new ListSegmentNode<T>(_list.List, _list.Start, _list.Count));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)ListSegmentNode.ToArrayReverse(_list.List, _list.Start, _list.Count);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList))
            {
                result = default;
                return false;
                // TODO
            }

            result = default;
            return false;
        }

        public TResult CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
            => ListSegmentNode.FastReverseEnumerate<T, TResult, FEnumerator>(in _list, fenum);
    }

    public struct ListSegmentNode<T>
        : INode<T>
    {
        private readonly ListSegment<T> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListSegmentNode(List<T> list, int start, int count) => _list = new ListSegment<T>(list, start, count);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) =>
            ListSegmentNode.Create<T, Head, Tail, CreationType>(in _list, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.TryObjectAscentOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
        { creation = default; return false; }

        public bool CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => ListSegmentNode.CheckForOptimization<T, TRequest, TResult>(in _list, in request, out result);

        public TResult CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            => ListSegmentNode.FastEnumerate<T, TResult, FEnumerator>(in _list, fenum);
    }

    static class ListSegmentNode
    {
        public static bool CheckForOptimization<T, TRequest, TResult>(in ListSegment<T> _list, in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skip = (Optimizations.Skip)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.Skip(in _list, skip.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var take = (Optimizations.Take)(object)request;
                NodeContainer<T> container = default;
                ListSegmentNode.Take(in _list, take.Count, ref container);
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<T> container = default;
                container.SetNode(new ReversedListSegmentNode<T>(_list.List, _list.Start, _list.Count));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)ListSegmentNode.ToArray(_list.List, _list.Start, _list.Count);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.ToList))
            {
                result = default;
                return false;
                // TODO
            }

            result = default;
            return false;
        }

        public static T[] ToArray<T>(List<T> srcList, int start, int count) =>
            srcList.Count switch
            {
                0 => Array.Empty<T>(),
                _ => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(srcList).Slice(start, count).ToArray()
            };

        public static T[] ToArrayReverse<T>(List<T> srcList, int start, int count)
        {
            var array = ToArray(srcList, start, count);
            Array.Reverse(array);
            return array;
        }

        internal static void Skip<T>(in ListSegment<T> list, int count, ref NodeContainer<T> container)
        {
            if (count >= list.Count)
            {
                container.SetEmpty();
            }
            else
            {
                container.SetNode(new ListSegmentNode<T>(list.List, list.Start+count, list.Count-count));
            }
        }

        internal static void Take<T>(in ListSegment<T> list, int count, ref NodeContainer<T> container)
        {
            if (count <= 0)
            {
                container.SetEmpty();
            }
            else
            {
                container.SetNode(new ListSegmentNode<T>(list.List, list.Start, Math.Min(count, list.Count)));
            }
        }

        internal static void ReverseSkip<T>(in ListSegment<T> list, int count, ref NodeContainer<T> container)
        {
            if (count >= list.Count)
            {
                container.SetEmpty();
            }
            else
            {
                container.SetNode(new ReversedListSegmentNode<T>(list.List, list.Start, list.Count - count));
            }
        }

        internal static void ReverseTake<T>(in ListSegment<T> list, int count, ref NodeContainer<T> container)
        {
            if (count <= 0)
            {
                container.SetEmpty();
            }
            else
            {
                var length = Math.Min(list.Count, count);
                container.SetNode(new ReversedListSegmentNode<T>(list.List, list.Start + list.Count - length, length));
            }
        }

        public static CreationType Create<T, Head, Tail, CreationType>(in ListSegment<T> list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new ListSegmentFastEnumerator<T>(in list);
            return nodes.CreateObject<CreationType, T, ListSegmentFastEnumerator<T>>(ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator>(in ListSegment<TIn> list, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ProcessList(in list, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ProcessList<TIn, FEnumerator>(List<TIn> list, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            if (list == null)
                throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

            ProcessList<TIn, FEnumerator>(new ListSegment<TIn>(list, 0, list.Count), ref fenum);
        }

        internal static void ProcessList<TIn, FEnumerator>(in ListSegment<TIn> list, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            if (list.Count < 20) // thumb in the air # from some random testing; depends on multiple things, so impossible to get 'perfect'
            {
                DoLoop<TIn, FEnumerator>(in list, ref fenum);
            }
            else if (BatchProcessResult.Unavailable == fenum.TryProcessBatch<ListSegment<TIn>, GetSpan<ListSegment<TIn>, TIn>>(list, in Optimizations.UseSpan<TIn>.FromList))
            {
                SpanNode.Loop<TIn, FEnumerator>(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list.List).Slice(list.Start, list.Count), ref fenum);
            }
        }

        private static void DoLoop<TIn, FEnumerator>(in ListSegment<TIn> listInfo, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            var list  = listInfo.List;
            var start = listInfo.Start;
            var end   = listInfo.Start + listInfo.Count - 1;

            for (var i = start; i <= end; ++i)
            {
                if (!fenum.ProcessNext(list[i]))
                    break;
            }
        }

        internal static TResult FastReverseEnumerate<TIn, TResult, FEnumerator>(in ListSegment<TIn> list, FEnumerator fenum)
        where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ReverseLoop<TIn, FEnumerator>(in list, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ReverseLoop<TIn, FEnumerator>(in ListSegment<TIn> listInfo, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            var list  = listInfo.List;
            var start = listInfo.Start;
            var end   = listInfo.Start + listInfo.Count - 1;

            for (var i = end; i >= start; --i)
            {
                if (!fenum.ProcessNext(list[i]))
                    break;
            }
        }
    }
}

