using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Nodes
{
    struct ZipNodeEnumerator<T, U, TEnumerator>
        : IFastEnumerator<(T, U)>
        where TEnumerator : IFastEnumerator<T>
    {
        private TEnumerator _enumerator1;
        private FastEnumerator<U> _enumerator2;

        public ZipNodeEnumerator(in TEnumerator enumerator1, in FastEnumerator<U> enumerator2) => (_enumerator1, _enumerator2) = (enumerator1, enumerator2);

        public void Dispose() { try { _enumerator1.Dispose(); } finally { _enumerator2.Dispose(); }; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out (T, U) current)
        {
            if (_enumerator1.TryGetNext(out current.Item1) && _enumerator2.TryGetNext(out current.Item2))
                return true;

            current = default;
            return false;
        }
    }

    public struct ZipNode<T, U, NodeT, NodeU>
        : INode<(T, U)>
        where NodeT : INode<T>
        where NodeU : INode<U>
    {
        private NodeT _nodeT;
        private NodeU _nodeU;

        public void GetCountInformation(out CountInformation info)
        {
            _nodeT.GetCountInformation(out info);
            _nodeU.GetCountInformation(out var infoU);

            if (info.MaximumLength.HasValue && infoU.MaximumLength.HasValue)
            {
                info.MaximumLength = Math.Min(info.MaximumLength.Value, infoU.MaximumLength.Value);
                info.ActualLengthIsMaximumLength &= infoU.ActualLengthIsMaximumLength;
                info.LengthIsImmutable &= infoU.LengthIsImmutable;
                info.IsStale |= infoU.IsStale;
                info.PotentialSideEffects |= infoU.PotentialSideEffects;
            }
            else
            {
                info.MaximumLength = null;
            }
        }

        public ZipNode(in NodeT nodeT, in NodeU nodeU) => (_nodeT, _nodeU) = (nodeT, nodeU);

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => Nodes<CreationType>.Descend(ref _nodeT, in this, in nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail tail, ref Enumerator enumerator)
        {
            var eu = Nodes<U>.CreateFastEnumerator(_nodeU);
            var nextEnumerator = new ZipNodeEnumerator<EnumeratorElement, U, Enumerator>(in enumerator, eu);
            return tail.CreateObject<CreationType, (EnumeratorElement, U), ZipNodeEnumerator<EnumeratorElement, U, Enumerator>>(ref nextEnumerator);
        }

        bool INode.TryPullOptimization<TRequest, CreationType, Tail>(in TRequest request, ref Tail tail, out CreationType creation)
        {
            creation = default;
            return false;
        }

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result) { result = default; return false; }

        TResult INode<(T, U)>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (Optimizations.AsMemory.IsAvailable<T, NodeT>(ref _nodeT))
            {
                if (Optimizations.AsMemory.TryGet<U, NodeU>(ref _nodeU, out var memoryU))
                {
                    if (!Optimizations.AsMemory.TryGet<T, NodeT>(ref _nodeT, out var memoryT))
                        throw new InvalidOperationException();

                    return ZipNode.FastEnumerate<T, U, TResult, FEnumerator>(memoryT.Span, memoryU.Span, fenum);
                }
            }

            return _nodeT.CreateViaPush<TResult, ZipFoward<T, U, FEnumerator>>(new ZipFoward<T, U, FEnumerator>(fenum, Nodes<U>.CreateFastEnumerator(_nodeU)));
        }
    }

    struct ZipFoward<T, U, Next>
        : IForwardEnumerator<T>
        where Next : IForwardEnumerator<(T, U)>
    {
        internal Next _next;

        FastEnumerator<U> _eu;

        public ZipFoward(in Next prior, FastEnumerator<U> eu) => (_next, _eu) = (prior, eu);

        public BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request) => BatchProcessResult.Unavailable;
        public void Dispose() => _next.Dispose();
        public TResult GetResult<TResult>() => _next.GetResult<TResult>();
        public bool ProcessNext(T t)
        {
            if (!_eu.TryGetNext(out var u))
                return false;
            _next.ProcessNext((t, u));
            return true;
        }
    }

    static class ZipNode
    {
        internal static TResult FastEnumerate<T, U, TResult, FEnumerator>(ReadOnlySpan<T> ts, ReadOnlySpan<U> us, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<(T, U)>
        {
            try
            {
                ProcessSpans<T, U, FEnumerator>(ts, us, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        internal static void ProcessSpans<T, U, FEnumerator>(ReadOnlySpan<T> ts, ReadOnlySpan<U> us, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<(T, U)>
        {
            var len = Math.Min(ts.Length, us.Length);
            for (var i = 0; i < len; ++i)
            {
                if (!fenum.ProcessNext((ts[i], us[i])))
                    break;
            }
        }
    }

}
