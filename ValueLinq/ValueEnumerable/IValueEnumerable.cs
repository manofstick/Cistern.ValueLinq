using System.Collections.Generic;

namespace Cistern.ValueLinq.ValueEnumerable
{
    public interface IValueEnumerable<T>
        : IEnumerable<T>
    {
        new ValueEnumerator<T> GetEnumerator();
    }
}
