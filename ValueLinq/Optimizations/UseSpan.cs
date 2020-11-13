using Cistern.ValueLinq.Containers;
using System;

namespace Cistern.ValueLinq.Optimizations
{
    static class UseSpan<T>
    {
        public static GetSpan<T[], T> FromArray = x => x.AsSpan();

        public static GetSpan<ReadOnlyMemory<T>, T> FromMemory = x => x.Span;
    }
}
