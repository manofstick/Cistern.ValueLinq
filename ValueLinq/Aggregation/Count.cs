namespace Cistern.ValueLinq.Aggregation
{
    struct Count<T>
        : IForwardEnumerator<T>
    {
        private int _count;

        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_count;

        void IForwardEnumerator<T>.Init(int? size) { }

        bool IForwardEnumerator<T>.ProcessNext(T input)
        {
            checked
            {
                ++_count;
                return true;
            }
        }
    }
}
