using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Containers
{
    struct CastEnumerableFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly System.Collections.IEnumerator _enumerator;

        public CastEnumerableFastEnumerator(System.Collections.IEnumerable enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { if (_enumerator is IDisposable d) d.Dispose(); }

        public bool TryGetNext(out T current)
        {
            if (_enumerator.MoveNext())
            {
                current = (T)_enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }

    struct CastObjectEnumerableFastEnumerator<T>
        : IFastEnumerator<T>
    {
        private readonly IEnumerator<object> _enumerator;

        public CastObjectEnumerableFastEnumerator(IEnumerable<object> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        public bool TryGetNext(out T current)
        {
            if (_enumerator.MoveNext())
            {
                current = (T)_enumerator.Current;
                return true;
            }
            current = default;
            return false;
        }
    }


    public struct CastNode<T>
        : INode<T>
    {
        private readonly System.Collections.IEnumerable _enumerable;

        public void GetCountInformation(out CountInformation info) =>
            EnumerableNode.GetCountInformation(_enumerable, out info);

        public CastNode(System.Collections.IEnumerable source) => _enumerable = source;

        CreationType INode.CreateViaPullDescend<CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes)
        {
            if (_enumerable is IEnumerable<T> e)
                return EnumerableNode.CreateObjectDescent<T, CreationType, Head, Tail>(ref nodes, e);
            return CastNode.CreateObjectDescent<T, CreationType, Head, Tail>(ref nodes, _enumerable);
        }

        CreationType INode.CreateViaPullAscent<CreationType, EnumeratorElement, Enumerator, Tail>(ref Tail _, ref Enumerator __) =>
            throw new InvalidOperationException();

        bool INode.TryPullOptimization<TRequest, TResult, Nodes>(in TRequest request, ref Nodes nodes, out TResult creation)
            => throw new InvalidOperationException();
        readonly bool INode.TryPushOptimization<TRequest, TResult>(in TRequest request, out TResult result)
        {
            if (_enumerable is IEnumerable<T> e)
                return EnumerableNode.CheckForOptimization<T, TRequest, TResult>(e, in request, out result);

            result = default;
            return false;
        }

        TResult INode<T>.CreateViaPush<TResult, FEnumerator>(in FEnumerator fenum)
        {
            if (_enumerable is IEnumerable<T> e)
                return EnumerableNode.FastEnumerateSwitch<T, TResult, FEnumerator>(e, in fenum);

            return CastNode.CreateViaPush<T, TResult, FEnumerator>(_enumerable, fenum);
        }
    }

    static class CastNode
    {
        public static CreationType CreateObjectDescent<T, CreationType, Head, Tail>(ref Nodes<Head, Tail> nodes, System.Collections.IEnumerable enumerable)
            where Head : INode
            where Tail : INodes
        {
            if (enumerable is IEnumerable<object> eo)
            {
                var e = new CastObjectEnumerableFastEnumerator<T>(eo);
                return nodes.CreateObject<CreationType, T, CastObjectEnumerableFastEnumerator<T>>(ref e);
            }
            else
            {
                var e = new CastEnumerableFastEnumerator<T>(enumerable);
                return nodes.CreateObject<CreationType, T, CastEnumerableFastEnumerator<T>>(ref e);
            }
        }

        internal static TResult CreateViaPush<T, TResult, FEnumerator>(System.Collections.IEnumerable _enumerable, FEnumerator fenum)
             where FEnumerator : IForwardEnumerator<T>
        {
            try
            {
                if (_enumerable is IEnumerable<object> eo)
                    Loop<FEnumerator, T>(eo, ref fenum);
                else
                    Loop<FEnumerator, T>(_enumerable, ref fenum);

                return fenum.GetResult<TResult>();
            }
            finally
            {
                fenum.Dispose();
            }
        }

        private static void Loop<FEnumerator, T>(IEnumerable<object> e, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext((T)item))
                    break;
            }
        }

        private static void Loop<FEnumerator, T>(System.Collections.IEnumerable e, ref FEnumerator fenum) where FEnumerator : IForwardEnumerator<T>
        {
            foreach (var item in e)
            {
                if (!fenum.ProcessNext((T)item))
                    break;
            }
        }
    }
}