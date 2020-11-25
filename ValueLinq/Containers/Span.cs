using System;

namespace Cistern.ValueLinq.Containers
{
    public delegate ReadOnlySpan<TElement> GetSpan<TObject, TElement>(TObject obj);

    struct SpanFastEnumerator<TObject, TElement>
        : IFastEnumerator<TElement>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, TElement> _getSpan;
        private int _idx;

        public SpanFastEnumerator(TObject obj, GetSpan<TObject, TElement> getSpan) => (_obj, _getSpan, _idx) = (obj, getSpan, -1);

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
        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => throw new NotSupportedException();
        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();
        #endregion

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => SpanNode.FastReverseEnumerate<T, TResult, FEnumerator>(_getSpan(_obj), fenum);
    }


    public struct SpanNode<TObject, TElement>
        : INode<TElement>
    {
        private TObject _obj;
        private readonly GetSpan<TObject, TElement> _getSpan;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_getSpan(_obj).Length, true);

        public SpanNode(TObject obj, GetSpan<TObject, TElement> getSpan) => (_obj, _getSpan) = (obj, getSpan);

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes) => SpanNode.Create<TElement, Head, Tail, CreationType, TObject>(_obj, _getSpan, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.ToArray))
            {
                result = (TResult)(object)SpanNode.ToArray(_getSpan(_obj));
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Reverse))
            {
                result = (TResult)(object)new ReversedSpanNode<TObject, TElement>(_obj, _getSpan);
                return true;
            }

            if (typeof(TRequest) == typeof(Optimizations.Skip))
            {
                var skipRequest = (Optimizations.Skip)(object)request;
                var node = SpanNode.MaybeSkip(_obj, _getSpan, skipRequest.Count);
                result = (TResult)(object)node;
                return node != null;
            }

            if (typeof(TRequest) == typeof(Optimizations.Take))
            {
                var takeRequest = (Optimizations.Take)(object)request;
                var node = SpanNode.MaybeTake(_obj, _getSpan, takeRequest.Count);
                result = (TResult)(object)node;
                return node != null;
            }

            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)_getSpan(_obj).Length;
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<TElement>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => SpanNode.FastEnumerate<TElement, TResult, FEnumerator, TObject>(_obj, _getSpan, fenum);
    }

    static class SpanNode
    {
        internal static T[] ToArray<T>(ReadOnlySpan<T> span)
        {
            if (span.Length == 0)
                return Array.Empty<T>();

            return span.ToArray();
        }

        public static CreationType Create<T, Head, Tail, CreationType, TObject>(TObject obj, GetSpan<TObject, T> getSpan, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
        {
            var enumerator = new SpanFastEnumerator<TObject, T>(obj, getSpan);
            return nodes.CreateObject<CreationType, T, SpanFastEnumerator<TObject, T>>(0, ref enumerator);
        }

        internal static TResult FastEnumerate<TIn, TResult, FEnumerator, TObject>(TObject obj, GetSpan<TObject, TIn> getSpan, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                if (BatchProcessResult.Unavailable == fenum.TryProcessBatch<TObject, GetSpan<TObject, TIn>>(obj, getSpan))
                    Loop(getSpan(obj), ref fenum);

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void Loop<TIn, FEnumerator>(ReadOnlySpan<TIn> span, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = 0; i < span.Length; ++i)
            {
                if (!fenum.ProcessNext(span[i]))
                    break;
            }
        }

        internal static TResult FastReverseEnumerate<TIn, TResult, FEnumerator>(ReadOnlySpan<TIn> span, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            try
            {
                ReverseLoop<TIn, FEnumerator>(span, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ReverseLoop<TIn, FEnumerator>(ReadOnlySpan<TIn> span, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<TIn>
        {
            for (var i = span.Length - 1; i >= 0; --i)
            {
                if (!fenum.ProcessNext(span[i]))
                    break;
            }
        }

        internal static INode<TElement> MaybeSkip<TObject, TElement>(TObject obj, GetSpan<TObject, TElement> getSpan, int count)
        {
            var span = getSpan(obj);
            if (count > span.Length)
                return EmptyNode<TElement>.Empty;

            if (span.Length < 100) // magic number; weighing up between skipping the skip and creating some garbage. What's good??
                return null;

            return new SpanNode<TObject, TElement>(obj, o => getSpan(o).Slice(count));
        }

        internal static object MaybeTake<TObject, TElement>(TObject obj, GetSpan<TObject, TElement> getSpan, int count)
        {
            if (count <= 0)
                return EmptyNode<TElement>.Empty;

            var span = getSpan(obj);
            if (count >= span.Length)
                return new SpanNode<TObject, TElement>(obj, getSpan);

            if (span.Length < 100) // magic number; weighing up between skipping the skip and creating some garbage. What's good??
                return null;

            return new SpanNode<TObject, TElement>(obj, o => getSpan(o).Slice(0, count));
        }
    }
}
