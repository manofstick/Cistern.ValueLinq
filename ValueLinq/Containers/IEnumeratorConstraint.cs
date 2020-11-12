using System;
using System.Collections;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct GenericEnumeratorFastEnumerator<T, Enumerator>
        : IFastEnumerator<T>
        where Enumerator : IEnumerator<T>
    {
        private Enumerator _enumerator;
        private readonly int? _count;

        public GenericEnumeratorFastEnumerator(Enumerator e, int? count) => (_enumerator, _count) = (e, count);

        public void Dispose() =>_enumerator.Dispose();

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

        CreationType INode.CreateObjectDescent<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
            => GenericEnumeratorNode.Create<T, Head, Tail, CreationType, Enumerator>(_f(_e), _count, ref nodes);

        CreationType INode.CreateObjectAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.CheckForOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_e);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateObjectViaFastEnumerator<TResult, FEnumerator>(in FEnumerator fenum)
            => GenericEnumeratorNode.FastEnumerate<T, TResult, FEnumerator, Enumerator>(_f(_e), _count, fenum);
    }

    static class GenericEnumeratorNode
    {
        public static CreationType Create<T, Head, Tail, CreationType, Enumerator>(Enumerator list, int? count, ref Nodes<Head, Tail> nodes)
            where Head : INode
            where Tail : INodes
            where Enumerator : IEnumerator<T>
        {
            var enumerator = new GenericEnumeratorFastEnumerator<T, Enumerator>(list, count);
            return nodes.CreateObject<CreationType, T, GenericEnumeratorFastEnumerator<T, Enumerator>>(ref enumerator);
        }

        internal static TResult FastEnumerate<T, TResult, FEnumerator, Enumerator>(Enumerator enumerator, int? count, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where Enumerator : IEnumerator<T>
        {
            try
            {
                InnerLoop<T, FEnumerator, Enumerator>(enumerator, fenum);
                return fenum.GetResult<TResult>();
            }
            finally
            {
                enumerator.Dispose();
                fenum.Dispose();
            }
        }

        private static void InnerLoop<T, FEnumerator, Enumerator>(Enumerator enumerator, FEnumerator fenum)
            where FEnumerator : IForwardEnumerator<T>
            where Enumerator : IEnumerator<T>
        {
            while (enumerator.MoveNext())
                fenum.ProcessNext(enumerator.Current);
        }
    }
}