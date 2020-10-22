namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        bool TryGetNext(out T current);
        void Dispose();
        int? InitialSize { get; }
    }

    abstract class FastEnumerator<EnumeratorElement>
    {
        public abstract void Dispose();
        public abstract bool TryGetNext(out EnumeratorElement current);
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
