using System.Runtime.CompilerServices;

namespace Cistern.ValueLinq.Utils
{
    [InlineArray(Capacity)]
    struct StackBuffer<T>
    {
        public const int Capacity = 64;

        T _element0;
    }
}
