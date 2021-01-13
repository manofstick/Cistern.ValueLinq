using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct ListSegmentPullEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly List<T> _list;
        private readonly int _lastIdx;
        private int _currentIdx;

        public ListSegmentPullEnumerator(in ListSegment<T> list) => (_list, _currentIdx, _lastIdx) = (list.List, list.Start-1, list.Start+list.Count-1);

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
        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) => throw new NotSupportedException();
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
        #endregion

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
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

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<T>
            => ListSegmentNode.FastReverseEnumerate<T, TResult, TPushEnumerator>(in _list, fenum);
    }

    public struct ListSegmentNode<TSource>
        : INode<TSource>
    {
        private readonly ListSegment<TSource> _list;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_list.Count, false);

        public ListSegmentNode(List<TSource> list, int start, int count) => _list = new ListSegment<TSource>(list, start, count);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) =>
            ListSegmentNode.Create<TSource, TNodes, CreationType>(in _list, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        public bool TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
            => ListSegmentNode.CheckForOptimization<TSource, TRequest, TResult>(in _list, in request, out result);

        public TResult CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
            => ListSegmentNode.ExecutePush<TSource, TResult, TPushEnumerator>(in _list, fenum);
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

        public static CreationType Create<T, TNodes, CreationType>(in ListSegment<T> list, ref TNodes nodes)
            where TNodes : INodes
        {
            var enumerator = new ListSegmentPullEnumerator<T>(in list);
            return nodes.CreateObject<CreationType, T, ListSegmentPullEnumerator<T>>(ref enumerator);
        }

        internal static TResult ExecutePush<TSource, TResult, TPushEnumerator>(in ListSegment<TSource> list, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
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

        internal static void ProcessList<TSource, TPushEnumerator>(List<TSource> list, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            if (list == null)
                throw new ArgumentNullException("source"); // name used to match System.Linq's exceptions

            ProcessList<TSource, TPushEnumerator>(new ListSegment<TSource>(list, 0, list.Count), ref fenum);
        }

        internal static void ProcessList<TSource, TPushEnumerator>(in ListSegment<TSource> list, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            if (list.Count < 20) // thumb in the air # from some random testing; depends on multiple things, so impossible to get 'perfect'
            {
                DoLoop<TSource, TPushEnumerator>(in list, ref fenum);
            }
            else if (BatchProcessResult.Unavailable == fenum.TryProcessBatch<ListSegment<TSource>, GetSpan<ListSegment<TSource>, TSource>>(list, in Optimizations.UseSpan<TSource>.FromList))
            {
                SpanNode.Loop<TSource, TPushEnumerator>(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list.List).Slice(list.Start, list.Count), ref fenum);
            }
        }

        private static void DoLoop<TElement, TPushEnumerator>(in ListSegment<TElement> listInfo, ref TPushEnumerator pushee)
            where TPushEnumerator : IPushEnumerator<TElement>
        {
            var list  = listInfo.List;
            var start = listInfo.Start;
            var end   = listInfo.Start + listInfo.Count - 1;

            for (var i = start; i <= end; ++i)
            {
                if (!pushee.ProcessNext(list[i]))
                    break;
            }
        }

        internal static TResult FastReverseEnumerate<TElement, TResult, FEnumerator>(in ListSegment<TElement> list, FEnumerator pushee)
            where FEnumerator : IPushEnumerator<TElement>
        {
            try
            {
                ReverseLoop<TElement, FEnumerator>(in list, ref pushee);
                return pushee.GetResult<TResult>();
            }
            finally
            {
                pushee.Dispose();
            }
        }

        internal static void ReverseLoop<TElement, TPushEnumerator>(in ListSegment<TElement> listInfo, ref TPushEnumerator pushee)
            where TPushEnumerator : IPushEnumerator<TElement>
        {
            var list  = listInfo.List;
            var start = listInfo.Start;
            var end   = listInfo.Start + listInfo.Count - 1;

            for (var i = end; i >= start; --i)
            {
                if (!pushee.ProcessNext(list[i]))
                    break;
            }
        }
    }
}

