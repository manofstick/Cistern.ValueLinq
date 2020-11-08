using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        /// <summary>
        /// Valid prior to iterating, undefined afterwards
        /// </summary>
        (bool NoSelect, int Count)? InitialSize { get; }
        bool TryGetNext(out T current);

        void Dispose();
    }

    public interface IForwardEnumerator<T>
    {
        public abstract bool ProcessNext(T input);
        TResult GetResult<TResult>();
    }

    abstract class FastEnumerator<EnumeratorElement>
    {
        public abstract void Dispose();
        public abstract bool TryGetNext(out EnumeratorElement current);
        public abstract (bool NoSelect, int Count)? InitialSize { get; }

        public static readonly FastEnumerator<EnumeratorElement> Empty = new FastEnumerator<EmptyFastEnumerator<EnumeratorElement>, EnumeratorElement>(default);
    }

    sealed class FastEnumerator<Enumerator, EnumeratorElement>
        : FastEnumerator<EnumeratorElement>
        where Enumerator : IFastEnumerator<EnumeratorElement>
    {
        private Enumerator enumerator;

        public FastEnumerator(Enumerator enumerator) => this.enumerator = enumerator;

        public override (bool NoSelect, int Count)? InitialSize => enumerator.InitialSize;

        public override void Dispose() => enumerator.Dispose();

        public override bool TryGetNext(out EnumeratorElement current) => enumerator.TryGetNext(out current);
    }
}
