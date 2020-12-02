using Cistern.ValueLinq.Containers;
using System;

namespace Cistern.ValueLinq.Optimizations
{
    static class UseSpan<T>
    {
        public static readonly GetSpan<ListSegment<T>, T> FromList = x => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(x.List).Slice(x.Start, x.Count);

        public static readonly GetSpan<T[], T> FromArray = x => x.AsSpan();

        public static readonly GetSpan<ReadOnlyMemory<T>, T> FromMemory = x => x.Span;
    }
}
