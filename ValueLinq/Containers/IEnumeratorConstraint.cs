using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Containers
{
    struct GenericEnumeratorPullEnumerator<T, Enumerator>
        : IPullEnumerator<T>
        where Enumerator : IEnumerator<T>
    {
        private Enumerator _enumerator;

        public GenericEnumeratorPullEnumerator(Enumerator e) => (_enumerator) = (e);

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

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => GenericEnumeratorNode.Create<T, TNodes, CreationType, Enumerator>(_f(_e), ref nodes);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator_, Tail>(ref Tail _, ref Enumerator_ __)
            => throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (typeof(TRequest) == typeof(Optimizations.Count))
            {
                result = (TResult)(object)EnumerableNode.Count(_e);
                return true;
            }

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => GenericEnumeratorNode.ExecutePush<T, TResult, TPushEnumerator, Enumerator>(_f(_e), fenum);
    }

    static class GenericEnumeratorNode
    {
        public static CreationType Create<T, TNodes, CreationType, Enumerator>(Enumerator list, ref TNodes nodes)
            where TNodes : INodes
            where Enumerator : IEnumerator<T>
        {
            var enumerator = new GenericEnumeratorPullEnumerator<T, Enumerator>(list);
            return nodes.CreateObject<CreationType, T, GenericEnumeratorPullEnumerator<T, Enumerator>>(ref enumerator);
        }

        internal static TResult ExecutePush<TElement, TResult, TPushEnumerator, TEnumerator>(TEnumerator enumerator, TPushEnumerator pushee)
            where TPushEnumerator : IPushEnumerator<TElement>
            where TEnumerator : IEnumerator<TElement>
        {
            try
            {
                InnerLoop<TElement, TPushEnumerator, TEnumerator>(enumerator, ref pushee);
                return pushee.GetResult<TResult>();
            }
            finally
            {
                enumerator.Dispose();
                pushee.Dispose();
            }
        }

        private static void InnerLoop<TElement, TPushEnumerator, TEnumerator>(TEnumerator enumerator, ref TPushEnumerator pushee)
            where TPushEnumerator : IPushEnumerator<TElement>
            where TEnumerator : IEnumerator<TElement>
        {
            while (enumerator.MoveNext())
                pushee.ProcessNext(enumerator.Current);
        }
    }
}