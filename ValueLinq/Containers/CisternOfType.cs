namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections.Generic;

// A non-generic IEnumerable filtered to the elements of the element type, each tested as it is read.
public static partial class CisternOfType
{
    [Answerer]
    public readonly partial struct Node<T>
        : IErasedNode<T>
    {
        private readonly System.Collections.IEnumerable _enumerable;

        public void GetCountInformation(out CountInformation info)
        {
            SystemIEnumerable.GetCountInformation(_enumerable, out info);
            info.ActualLengthIsMaximumLength &= info.MaximumLength == 0;
        }

        public Node(System.Collections.IEnumerable source) => _enumerable = source;

        PullEnumerator<T> IErasedNode<T>.CreatePullEnumerator()
            => CisternOfType.CreatePullEnumerator<T>(_enumerable);

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CisternOfType.CreateViaPush<T, TPushResult, TPushEnumerator>(_enumerable, PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    internal partial struct Pull<T>
    {
        private readonly System.Collections.IEnumerator _enumerator;

        public Pull(System.Collections.IEnumerable enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { if (_enumerator is IDisposable d) d.Dispose(); }

        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current is T t)
                {
                    _current = t;
                    return true;
                }
            }
            return false;
        }

        public T Current => _current;
    }

    [Puller]
    internal partial struct EnumerablePull<T>
    {
        private readonly IEnumerator<object> _enumerator;

        public EnumerablePull(IEnumerable<object> enumerable) => _enumerator = enumerable.GetEnumerator();

        public void Dispose() { _enumerator.Dispose(); }

        [AllowNull] private T _current;

        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_enumerator.Current is T t)
                {
                    _current = t;
                    return true;
                }
            }
            return false;
        }

        public T Current => _current;
    }

    internal static PullEnumerator<T> CreatePullEnumerator<T>(System.Collections.IEnumerable enumerable)
    {
        if (enumerable is IEnumerable<object> eo)
            return new PullEnumerator<EnumerablePull<T>, T>(new EnumerablePull<T>(eo));

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
