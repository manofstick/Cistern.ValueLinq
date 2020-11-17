using Cistern.ValueLinq.Containers;
using System;
using System.Collections.Generic;

namespace Cistern.ValueLinq.Optimizations
{
    static class UseSpan<T>
    {
        public static readonly GetSpan<List<T>, T> FromList = x => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(x);

        public static readonly GetSpan<T[], T> FromArray = x => x.AsSpan();

        public static readonly GetSpan<ReadOnlyMemory<T>, T> FromMemory = x => x.Span;
    }
}
