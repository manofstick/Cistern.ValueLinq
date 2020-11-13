using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        /// <summary>
        /// Valid prior to iterating, undefined afterwards
        /// </summary>
        bool TryGetNext(out T current);

        void Dispose();
    }

    public enum BatchProcessResult
    {
        Unavailable,
        SuccessAndHalt,
        SuccessAndContinue,
    }

    public interface IForwardEnumerator<T>
    {
        BatchProcessResult TryProcessBatch<TObject, TRequest>(TObject obj, in TRequest request);
        public bool ProcessNext(T input);
        TResult GetResult<TResult>();
        void Dispose();
    }

    abstract class FastEnumerator<EnumeratorElement>
    {
        public abstract void Dispose();
        public abstract bool TryGetNext(out EnumeratorElement current);
        public static readonly FastEnumerator<EnumeratorElement> Empty = new FastEnumerator<EmptyFastEnumerator<EnumeratorElement>, EnumeratorElement>(default);
    }

    sealed class FastEnumerator<Enumerator, EnumeratorElement>
        : FastEnumerator<EnumeratorElement>
        where Enumerator : IFastEnumerator<EnumeratorElement>
    {
        private Enumerator enumerator;

        public FastEnumerator(Enumerator enumerator) => this.enumerator = enumerator;

        public override void Dispose() => enumerator.Dispose();

        public override bool TryGetNext(out EnumeratorElement current) => enumerator.TryGetNext(out current);
    }
}
