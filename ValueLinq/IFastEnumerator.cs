using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq
{
    public interface IFastEnumerator<T>
    {
        bool TryGetNext(out T current);
        void Dispose();
        int? InitialSize { get; }
    }

    interface IOptimizedCreateCollectionOuter<T>
    {
        List<T> ToList();
    }

    interface IOptimizedCreateCollectionInner<T>
    { 
        List<U> ToList<U>(Func<T,U> map);
        List<T> ToList(Func<T, bool> map);
    }
}
