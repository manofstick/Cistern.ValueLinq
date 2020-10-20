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

    public interface IValueEnumerable<T> 
        : IEnumerable<T>
        , INode
    {
        new ValueEnumerator<T> GetEnumerator();
    }

    interface ITryFastToListOuter<T>
    {
        List<T> MaybeToList();
    }

    interface ITryFastToListInner<T>
    { 
        List<U> MaybeMapToList<U>(Func<T,U> map);
    }
}
