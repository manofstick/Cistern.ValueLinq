using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct GenericEnumeratorFastEnumerator<T, Enumerator>
        : IFastEnumerator<T>
        where Enumerator : IEnumerator<T>
    {
        private Enumerator _enumerator;

        public GenericEnumeratorFastEnumerator(Enumerator e) => (_enumerator) = (e);

        public void Dispose() =>_enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetNext(out T current)
        {
            if (!_enumerator.MoveNext())
            {
                current = default;
                return false;
            }
            current = _enumerator.Current;
            return true;
        }
    }

    public struct GenericEnumeratorNode<T, Enumerable, Enumerator>
        : INode<T>
        where Enumerable : IEnumerable<T>
        where Enumerator : IEnumerator<T>
    {
        private readonly Enumerable _e;
        private readonly Func<Enumerable, Enumerator> _f;
        private readonly int? _count;

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(_count, false);

        public GenericEnumeratorNode(Enumerable e, Func<Enumerable, Enumerator> f, int? count)
        {
            if (count == null && e is ICollection c)
                count = c.Count;

            (_e, _f, _count) = (e, f, count);
        }

        CreationType INode.CreateViaPushDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => GenericEnumeratorNode.Create<T, Head, Tail, CreationType, Enumerator>(_f(_e), ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation) { creation = default; return false; }

        bool INode.TryPullOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_e);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPull<TResult, FEnumerator>(in FEnumerator fenum)
            => GenericEnumeratorNode.FastEnumerate<T, TResult, FEnumerator, Enumerator>(_f(_e), fenum);
    }

    static class GenericEnumeratorNode
    {
        public static CreationType Create<T, Head, Tail, CreationType, Enumerator>(Enumerator list, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Enumerator : IEnumerator<T>
        {
            var enumerator = new GenericEnumeratorFastEnumerator<T, Enumerator>(list);
            return nodes.CreateObject<CreationType, T, GenericEnumeratorFastEnumerator<T, Enumerator>>(ref enumerator);
        }

        internal static TResult FastEnumerate<T, TResult, FEnumerator, Enumerator>(Enumerator enumerator, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where Enumerator : IEnumerator<T>
        {
            try
            {
                InnerLoop<T, FEnumerator, Enumerator>(enumerator, ref fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                enumerator.Dispose();
                fenum.Dispose();
            }
        }

        private static void InnerLoop<T, FEnumerator, Enumerator>(Enumerator enumerator, ref FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where Enumerator : IEnumerator<T>
        {
            while (enumerator.MoveNext())
                fenum.ProcessNext(enumerator.Current);
        }
    }
}