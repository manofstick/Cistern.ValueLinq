using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct CastEnumerablePullEnumerator<TElement>
        : IPullEnumerator<TElement>
    {
        private readonly System.Collections.IEnumerator _enumerator;

        public CastEnumerablePullEnumerator(System.Collections.IEnumerable enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { if (_enumerator is IDisposable d) d.Dispose(); }

        public bool TryGetNext(out TElement current)
        {
            if (_enumerator.MoveNext())
            {
                current = (TElement)_enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }

    struct CastObjectEnumerablePullEnumerator<TElement>
        : IPullEnumerator<TElement>
    {
        private readonly IEnumerator<object> _enumerator;

        public CastObjectEnumerablePullEnumerator(IEnumerable<object> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out TElement current)
        {
            if (_enumerator.MoveNext())
            {
                current = (TElement)_enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }


    public struct CastNode<TElement>
        : INode<TElement>
    {
        private readonly System.Collections.IEnumerable _enumerable;

        public void GetCountInformation(out CountInformation info) =>
            EnumerableNode.GetCountInformation(_enumerable, out info);

        public CastNode(System.Collections.IEnumerable source) => _enumerable = source;

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
        {
            if (_enumerable is IEnumerable<TElement> e)
                return EnumerableNode.CreateObjectDescent<TElement, CreationType, TNodes>(ref nodes, e);
            return CastNode.CreateObjectDescent<TElement, CreationType, TNodes>(ref nodes, _enumerable);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();
        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (_enumerable is IEnumerable<TElement> e)
                return EnumerableNode.TryPushOptimization<TElement, TRequest, TResult>(e, in request, out result);

            result = default;
            return false;
        }

        TResult INode<TElement>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
        {
            if (_enumerable is IEnumerable<TElement> e)
                return EnumerableNode.ExecutePush<TElement, TResult, TPushEnumerator>(e, in fenum);

            return CastNode.CreateViaPush<TElement, TResult, TPushEnumerator>(_enumerable, fenum);
        }
    }

    static class CastNode
    {
        public static CreationType CreateObjectDescent<T, CreationType, TNodes>(ref TNodes nodes, System.Collections.IEnumerable enumerable)
            where TNodes : INodes
        {
            if (enumerable is IEnumerable<object> eo)
            {
                var e = new CastObjectEnumerablePullEnumerator<T>(eo);
                return nodes.CreateObject<CreationType, T, CastObjectEnumerablePullEnumerator<T>>(ref e);
            }
            else
            {
                var e = new CastEnumerablePullEnumerator<T>(enumerable);
                return nodes.CreateObject<CreationType, T, CastEnumerablePullEnumerator<T>>(ref e);
            }
        }

        internal static TResult CreateViaPush<T, TResult, TPushEnumerator>(System.Collections.IEnumerable _enumerable, TPushEnumerator fenum)
             where TPushEnumerator : IPushEnumerator<T>
        {
            try
            {
                if (_enumerable is IEnumerable<object> eo)
                    Loop<TPushEnumerator, T>(eo, ref fenum);
                else
                    Loop<TPushEnumerator, T>(_enumerable, ref fenum);

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<TPushEnumerator, T>(IEnumerable<object> e, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext((T)item))
                    break;
            }
        }

        private static void Loop<TPushEnumerator, T>(System.Collections.IEnumerable e, ref TPushEnumerator fenum)
            where TPushEnumerator : IPushEnumerator<T>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext((T)item))
                    break;
            }
        }
    }
}