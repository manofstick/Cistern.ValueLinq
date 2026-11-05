namespace Cistern.ValueLinq.Containers;

using Cistern.ValueLinq.Chain;
using System;
using System.Collections;
using System.Collections.Generic;

// The empty sequence: nothing is pushed or pulled, and every request is answered without a source.
public static partial class CisternEmpty
{
    [Answerer(Forwards = true)]
    public readonly partial struct Node<T>
        : INode<T>
        , Optimizations.IOptimizationToArray<T>
        , Optimizations.IOptimizationCount
        , Optimizations.IOptimizationReverse<T>
        , Optimizations.IOptimizationSkipTake<T>
    {
        public static INode<T> Empty { get; } = new Node<T>();

        public void GetCountInformation(out CountInformation info) =>
            info = new CountInformation(0, true);

        private bool ToArray(in Optimizations.ToArray request, [MaybeNullWhen(false)] out T[] response)
        {
            response = Array.Empty<T>();
            return true;
        }

        private bool Count(in Optimizations.Count request, out int response)
        {
            response = 0;
            return true;
        }

        private bool Reverse(in Optimizations.Reverse request, out NodeContainer<T> response)
        {
            response = default;
            return true;
        }

        private bool SkipTake(in Optimizations.SkipTake request, out NodeContainer<T> response)
        {
            response = default;
            return true;
        }

        // an empty container is often what a materialising node hands back (an OrderBy skipped past its end), and
        // a request it declines sends the asker back to redo that materialisation, so it answers everything a
        // memory container answers
        private bool Forward<TRequest, TRequestResult>(in TRequest request, [MaybeNullWhen(false)] out TRequestResult result)
            where TRequest : notnull
            => new SystemMemory.Node<T>(ReadOnlyMemory<T>.Empty).TryPushOptimization<TRequest, TRequestResult>(in request, out result);

        TPushResult INode<T>.CreateViaPush<TPushResult, TPushEnumerator, TPushFiller>(in ChainOptions options, in TPushFiller filler)
            => CisternEmpty.ExecutePush<T, TPushResult, TPushEnumerator>(PushFiller.Fill<TPushEnumerator, TPushFiller>(in filler));
    }

    [Puller]
    public partial struct Pull<T>
    {
        private static Pull<T> Create(in Node<T> node)
            => default;

        public bool MoveNext() => false;

        public T Current => default!;
    }

    internal class InstanceOf<T>
        : IEnumerator<T>
        , IEnumerable<T>
    {
        private static InstanceOf<T> _instance = new InstanceOf<T>();

        public static readonly IEnumerator<T> AsEnumerator = _instance;
        public static readonly IEnumerable<T> AsEnumerable = _instance;
        private InstanceOf() { }
        public T Current => Aggregation.Impl.ResultCast<T?, T>(default);
        object? System.Collections.IEnumerator.Current => Current;
        public void Dispose() { }
        public bool MoveNext() => false;
        public void Reset() { }
        IEnumerator IEnumerable.GetEnumerator() => this;
        public IEnumerator<T> GetEnumerator() => this;
    }

    internal static TResult ExecutePush<TElement, TResult, TPushEnumerator>(TPushEnumerator fenum)
        where TPushEnumerator : IPushEnumerator<TElement>
    {
        try
        {
            return fenum.GetResult<TResult>();
        }
        finally
        {
            fenum.Dispose();
        }
    }
}
