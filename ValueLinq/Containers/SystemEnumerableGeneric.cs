namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections;
using System.Collections.Generic;

// A collection reached through its own GetEnumerator, so a struct enumerator is never boxed the way
// IEnumerable<T>.GetEnumerator boxes it.
public static partial class SystemEnumerableGeneric
{
    // The enumerator is fetched per enumeration, never at construction, so the node can be enumerated more than once
    // and a collection modified between construction and execution is not rejected by the enumerator's version check.
    [Answerer]
    public readonly partial struct Node<T, Enumerable, Enumerator>
        : INode<T>
        , Optimizations.IOptimizationCount
        where Enumerable : IEnumerable<T>
        where Enumerator : IEnumerator<T>
    {
        internal readonly Enumerable _e;
        internal readonly Func<Enumerable, Enumerator> _f;
        private readonly Func<Enumerable, int>? _getCount;

        // the count is asked for each time, never held, as the collection may have changed since construction
        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(SystemEnumerableGeneric.TryGetCount<T, Enumerable>(_e, _getCount), false);

        public Node(Enumerable e, Func<Enumerable, Enumerator> f, Func<Enumerable, int>? getCount)
        {
            _e = e;
            _f = f;
            _getCount = getCount;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            if (request.DryRun)
            {
                var known = SystemEnumerableGeneric.TryGetCount<T, Enumerable>(_e, _getCount);
                response = known.GetValueOrDefault();
                return known.HasValue;
            }

            response = SystemEnumerableGeneric.Count<T, Enumerable, Enumerator>(_e, _f, _getCount);
            return true;
        }

        // the chain is filled, and so initialised, before the enumerator is opened, and disposed when opening it throws
        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
        {
            var fenum = PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler);

            Enumerator enumerator;
            try
            {
                enumerator = _f(_e);
            }
            catch
            {
                fenum.Dispose();
                throw;
            }

            return SystemEnumerableGeneric.ExecutePush<T, TPushResult, TPushEnumerator, Enumerator>(enumerator, fenum);
        }
    }

    // also built for a collection met as IEnumerable<T> (SystemIEnumerable.Node), where there is no Enumerable type
    // to name the node by
    [Puller]
    internal partial struct Pull<T, Enumerator>
        where Enumerator : IEnumerator<T>
    {
        private Enumerator _enumerator;

        public Pull(Enumerator e) => (_enumerator) = (e);

        public void Dispose() =>_enumerator.Dispose();

        public bool MoveNext() => _enumerator.MoveNext();

        public T Current => _enumerator.Current;
    }

    // Node's enumerator
    [Puller]
    public partial struct NodeEnumerator<T, Enumerable, Enumerator>
        where Enumerable : IEnumerable<T>
        where Enumerator : IEnumerator<T>
    {
        private Pull<T, Enumerator> _enumerator;

        private NodeEnumerator(Enumerator e) => _enumerator = new(e);

        private static NodeEnumerator<T, Enumerable, Enumerator> Create(in Node<T, Enumerable, Enumerator> node)
            => new(node._f(node._e));

        public void Dispose() => _enumerator.Dispose();

        public bool MoveNext() => _enumerator.MoveNext();

        public T Current => _enumerator.Current;
    }

    internal static int? TryGetCount<T, Enumerable>(Enumerable e, Func<Enumerable, int>? getCount)
        where Enumerable : IEnumerable<T>
    {
        if (getCount != null)
            return getCount(e);

        return e switch
        {
            ICollection<T> c          => c.Count,
            IReadOnlyCollection<T> r  => r.Count,
            ICollection c             => c.Count,
            _                         => null,
        };
    }

    // a collection with no way to count it is walked with its own enumerator, as boxing it through IEnumerable<T>
    // is what this node avoids
    internal static int Count<T, Enumerable, Enumerator>(Enumerable e, Func<Enumerable, Enumerator> f, Func<Enumerable, int>? getCount)
        where Enumerable : IEnumerable<T>
        where Enumerator : IEnumerator<T>
    {
        if (TryGetCount<T, Enumerable>(e, getCount) is int known)
            return known;

        var enumerator = f(e);
        try
        {
            checked
            {
                var count = 0;
                while (enumerator.MoveNext())
                    ++count;
                return count;
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    internal static TResult ExecutePush<TElement, TResult, TPushEnumerator, TEnumerator>(TEnumerator enumerator, TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TElement>
        where TEnumerator : IEnumerator<TElement>
    {
        try
        {
            // by ref, so the Dispose below reaches the enumerator that ran rather than an unstarted copy; disposed before
            // the result is taken
            try
            {
                Loop<TElement, TPushEnumerator, TEnumerator>(ref enumerator, ref fenum);
            }
            finally
            {
                enumerator.Dispose();
            }

            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }

    private static void Loop<TElement, TPushEnumerator, TEnumerator>(ref TEnumerator enumerator, ref TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TElement>
        where TEnumerator : IEnumerator<TElement>
    {
        while (enumerator.MoveNext())
        {
            if (!fenum.ProcessNext(enumerator.Current))
                break;
        }
    }
}
