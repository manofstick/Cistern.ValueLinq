using Cistern.ValueLinq.Containers;
using System;

namespace Cistern.ValueLinq
{
    public interface IPullEnumerator<TElement>
    {
        /// <summary>
        /// Valid prior to iterating, undefined afterwards
        /// </summary>
        bool TryGetNext(out TElement current);

        void Dispose();
    }

    public enum BatchProcessResult
    {
        Unavailable,
        SuccessAndHalt,
        SuccessAndContinue,
    }

    public interface IPushEnumerator<TElement>
    {
        BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request);
        public bool ProcessNext(TElement input);
        TResult GetResult<TResult>();
        void Dispose();
    }

    abstract class PullEnumerator<TElement>
        : IDisposable
    {
        public abstract void Dispose();
        public abstract bool TryGetNext(out TElement current);

        public static readonly PullEnumerator<TElement> Empty = new PullEnumerator<EmptyPullEnumerator<TElement>, TElement>(default);
    }

    sealed class PullEnumerator<TPullEnumerator, TElement>
        : PullEnumerator<TElement>
        where TPullEnumerator : struct, IPullEnumerator<TElement>
    {
        private TPullEnumerator enumerator;

        public PullEnumerator(TPullEnumerator enumerator)
            => this.enumerator = enumerator;

        public override void Dispose() => enumerator.Dispose();

        public override bool TryGetNext(out TElement current)
            => enumerator.TryGetNext(out current);
    }
}
