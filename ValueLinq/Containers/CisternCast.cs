namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

// A non-generic IEnumerable whose elements are cast to the element type: one that turns out to be an IEnumerable<T>
// is pulled or pushed as that, else each element is cast as it is read.
public static partial class CisternCast
{
    public readonly struct Node<TElement>
        : IErasedNode<TElement>
    {
        private readonly System.Collections.IEnumerable _enumerable;

        public void GetCountInformation(out CountInformation info) =>
            SystemIEnumerable.GetCountInformation(_enumerable, out info);

        public Node(System.Collections.IEnumerable source) => _enumerable = source;

        PullEnumerator<TElement> IErasedNode<TElement>.CreatePullEnumerator()
        {
            if (_enumerable is IEnumerable<TElement> e)
                return SystemIEnumerable.CreatePullEnumerator(e);
            return CisternCast.CreatePullEnumerator<TElement>(_enumerable);
        }

        readonly bool INode.TryPushOptimization<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
        {
            if (_enumerable is IEnumerable<TElement> e)
                return SystemIEnumerable.TryPushOptimization<TElement, TRequest, TRequestResult>(e, in request, out result);

            result = default;
            return false;
        }

        TPushResult INode<TElement>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var fenum = PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler);

            if (_enumerable is IEnumerable<TElement> e)
                return SystemIEnumerable.ExecutePush<TElement, TPushResult, TPushEnumerator>(in options, e, in fenum);

            return CisternCast.CreateViaPush<TElement, TPushResult, TPushEnumerator>(_enumerable, fenum);
        }
    }

    [Puller]
    internal partial struct Pull<TElement>
    {
        private readonly System.Collections.IEnumerator _enumerator;

        public Pull(System.Collections.IEnumerable enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { if (_enumerator is IDisposable d) d.Dispose(); }

        public bool MoveNext() => _enumerator.MoveNext();

        public TElement Current => (TElement)_enumerator.Current!;
    }

    [Puller]
    internal partial struct ObjectPull<TElement>
    {
        private readonly IEnumerator<object> _enumerator;

        public ObjectPull(IEnumerable<object> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        public bool MoveNext() => _enumerator.MoveNext();

        public TElement Current => (TElement)_enumerator.Current;
    }

    internal static PullEnumerator<T> CreatePullEnumerator<T>(System.Collections.IEnumerable enumerable)
    {
        if (enumerable is IEnumerable<object> eo)
            return new PullEnumerator<ObjectPull<T>, T>(new ObjectPull<T>(eo));

        return new PullEnumerator<Pull<T>, T>(new Pull<T>(enumerable));
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
