using System;

namespace Cistern.ValueLinq.Containers
{
    public delegate ReadOnlySpan<TElement> GetSpan<TObject, TElement>(TObject obj);

    struct SpanPullEnumerator<TObject, TElement>
        : IPullEnumerator<TElement>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, TElement> _getSpan;
        private int _idx;

        public SpanPullEnumerator(TObject obj, GetSpan<TObject, TElement> getSpan) => (_obj, _getSpan, _idx) = (obj, getSpan, -1);

        public void Dispose() { }

        public bool TryGetNext(out TElement current)
        {
            var idx = _idx + 1;
            var span = _getSpan(_obj);
            if (idx < span.Length)
            {
                current = span[idx];
                _idx = idx;
                return true;
            }
            current = default;
            return false;
        }
    }

    public struct ReversedSpanNode<TObject, T>
        : INode<T>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, T> _getSpan;


        public ReversedSpanNode(TObject obj, GetSpan<TObject, T> getSpan) => (_obj, _getSpan) = (obj, getSpan);

#region "This node is only used in forward context, so most of interface is not supported"
        public void GetCountInformation(out CountInformation info) => throw new NotSupportedException();
        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes) => throw new NotSupportedException();
        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
        #endregion

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => SpanNode.ExecuteReversePush<T, TResult, TPushEnumerator>(_getSpan(_obj), fenum);
    }


    public struct SpanNode<TObject, TElement>
        : INode<TElement>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, TElement> _getSpan;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_getSpan(_obj).Length, true);

        public SpanNode(TObject obj, GetSpan<TObject, TElement> getSpan) => (_obj, _getSpan) = (obj, getSpan);

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => SpanNode.Create<TElement, TNodes, CreationType, TObject>(_obj, _getSpan, ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)SpanNode.ToArray(_getSpan(_obj));
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                NodeContainer<TElement> container = default;
                container.SetNode(new ReversedSpanNode<TObject, TElement>(_obj, _getSpan));
                result = (TResult)(object)container;
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skipRequest = (Optimizations.Skip)(object)request;
                NodeContainer<TElement> container = default;
                if (SpanNode.MaybeSkip(_obj, _getSpan, skipRequest.Count, ref container))
                {
                    result = (TResult)(object)container;
                    return true;
                }
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var takeRequest = (Optimizations.Take)(object)request;
                NodeContainer<TElement> container = default;
                if (SpanNode.MaybeTake(_obj, _getSpan, takeRequest.Count, ref container))
                {
                    result = (TResult)(object)container;
                    return true;
                }
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_getSpan(_obj).Length;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<TElement>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => SpanNode.ExecutePush<TElement, TResult, TPushEnumerator, TObject>(_obj, _getSpan, fenum);
    }

    static class SpanNode
    {
        internal static T[] ToArray<T>(ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                return Array.Empty<T>();

            return span.ToArray();
        }

        public static CreationType Create<T, TNodes, CreationType, TObject>(TObject obj, GetSpan<TObject, T> getSpan, ref TNodes nodes)
            where TNodes : INodes
        {
            var enumerator = new SpanPullEnumerator<TObject, T>(obj, getSpan);
            return nodes.CreateObject<CreationType, T, SpanPullEnumerator<TObject, T>>(ref enumerator);
        }

        internal static TResult ExecutePush<TSource, TResult, TPushEnumerator, TObject>(TObject obj, GetSpan<TObject, TSource> getSpan, TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            try
            {
                if (BatchProcessResult.Unavailable == fenum.TryProcessBatch<TObject, GetSpan<TObject, TSource>>(obj, getSpan))
                    Loop(getSpan(obj), ref fenum);

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void Loop<TSource, TPushEnumerator>(ReadOnlySpan<TSource> span, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!fenum.ProcessNext(span[i]))
                    break;
            }
        }

        internal static TResult ExecuteReversePush<TSource, TResult, FEnumerator>(ReadOnlySpan<TSource> span, FEnumerator fenum)
            where FEnumerator : IPushEnumerator<TSource>
        {
            try
            {
                ReverseLoop<TSource, FEnumerator>(span, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ReverseLoop<TSource, TPushEnumerator>(ReadOnlySpan<TSource> span, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<TSource>
        {
            for (var i = span.Length - 1; i >= 0; --i)
            {
                if (!fenum.ProcessNext(span[i]))
                    break;
            }
        }

        internal static bool MaybeSkip<TObject, TElement>(TObject obj, GetSpan<TObject, TElement> getSpan, int count, ref NodeContainer<TElement> container)
        {
            var span = getSpan(obj);
            if (count > span.Length)
            {
                container.SetEmpty();
                return true;
            }
            else if (span.Length < 100) // magic number; weighing up between skipping the skip and creating some garbage. What's good??
            {
                return false;
            }
            else
            {
                container.SetNode(new SpanNode<TObject, TElement>(obj, o => getSpan(o).Slice(count)));
                return true;
            }
        }

        internal static bool MaybeTake<TObject, TElement>(TObject obj, GetSpan<TObject, TElement> getSpan, int count, ref NodeContainer<TElement> container)
        {
            if (count <= 0)
            {
                container.SetEmpty();
                return true;
            }
            else
            {
                var span = getSpan(obj);
                if (count >= span.Length)
                {
                    container.SetNode(new SpanNode<TObject, TElement>(obj, getSpan));
                    return true;
                }
                else if (span.Length < 100) // magic number; weighing up between skipping the skip and creating some garbage. What's good??
                {
                    return false;
                }
                else
                {
                    container.SetNode(new SpanNode<TObject, TElement>(obj, o => getSpan(o).Slice(0, count)));
                    return true;
                }
            }
        }
    }
}
