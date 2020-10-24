using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        /// <summary>
        /// Valid prior to iterating, undefined afterwards
        /// </summary>
        int? InitialSize { get; }
        bool TryGetNext(out T current);

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
