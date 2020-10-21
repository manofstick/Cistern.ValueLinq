﻿namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        bool TryGetNext(out T current);
        void Dispose();
        int? InitialSize { get; }
    }
}
