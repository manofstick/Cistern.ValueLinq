namespace Cistern.ValueLinq.Aggregation
{
    struct Count<T>
        : IForwardEnumerator<T>
    {
        private int _count;

        public bool CheckForOptimization<TObject, TRequest, TResult>(TObject obj, in TRequest request, out TResult result) { result = default; return false; }
        public void Dispose() { }
        TResult IForwardEnumerator<T>.GetResult<TResult>() => (TResult)(object)_count;

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
