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
            result = default;
            return false;
        }

        TResult INode<TElement>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => SpanNode.FastEnumerate<TElement, TResult, FEnumerator, TObject>(_obj, _getSpan, fenum);
    }

    static class SpanNode
    {
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
    }
}
