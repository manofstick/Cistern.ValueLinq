using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct OfTypeFastEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly System.Collections.IEnumerator _enumerator;

        public OfTypeFastEnumerator(System.Collections.IEnumerable enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { if (_enumerator is IDisposable d) d.Dispose(); }

        public bool TryGetNext(out T current)
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current is T t)
                {
                    current = t;
                    return true;
                }
            }
            current = default;
            return false;
        }
    }

    struct OfTypeEnumerableFastEnumerator<T>
        : IPullEnumerator<T>
    {
        private readonly IEnumerator<object> _enumerator;

        public OfTypeEnumerableFastEnumerator(IEnumerable<object> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out T current)
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current is T t)
                {
                    current = t;
                    return true;
                }
            }
            current = default;
            return false;
        }
    }


    public struct OfTypeNode<T>
        : INode<T>
    {
        private readonly System.Collections.IEnumerable _enumerable;

        public void GetCountInformation(out CountInformation info) =>
            EnumerableNode.GetCountInformation(_enumerable, out info);

        public OfTypeNode(System.Collections.IEnumerable source) => _enumerable = source;

        CreationType INode.CreateViaPullDescend<CreationType, TNodes>(ref TNodes nodes)
            => OfTypeNode.CreateObjectDescent<T, CreationType, TNodes>(ref nodes, _enumerable);

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();

        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, TPushEnumerator>(in TPushEnumerator fenum)
            => OfTypeNode.CreateViaPush<T, TResult, TPushEnumerator>(_enumerable, fenum);
    }

    static class OfTypeNode
    {
        public static CreationType CreateObjectDescent<T, CreationType, TNodes>(ref TNodes nodes, System.Collections.IEnumerable enumerable)
            where TNodes : INodes
        {
            if (enumerable is IEnumerable<object> eo)
            {
                var e = new OfTypeEnumerableFastEnumerator<T>(eo);
                return nodes.CreateObject<CreationType, T, OfTypeEnumerableFastEnumerator<T>>(ref e);
            }
            else
            {
                var e = new OfTypeFastEnumerator<T>(enumerable);
                return nodes.CreateObject<CreationType, T, OfTypeFastEnumerator<T>>(ref e);
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

        private static void Loop<TPushEnumerator, T>(IEnumerable<object> e, ref TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<T>
        {
            foreach (var item in e)
            {
                if (item is T t && !fenum.ProcessNext(t))
                    break;
            }
        }

        private static void Loop<TPushEnumerator, T>(System.Collections.IEnumerable e, ref TPushEnumerator fenum) where TPushEnumerator : IPushEnumerator<T>
        {
            foreach (var item in e)
            {
                if (item is T t && !fenum.ProcessNext(t))
                    break;
            }
        }
    }
}