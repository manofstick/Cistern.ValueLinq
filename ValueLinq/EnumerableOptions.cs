using System;

namespace Cistern.ValueLinq
{
    /// <summary>
    /// How a chain handed out by ToEnumerable behaves as an <see cref="System.Collections.Generic.IEnumerable{T}"/>.
    /// </summary>
    [Flags]
    public enum EnumerableOptions
    {
        /// <summary>
        /// A Cistern chain pushed through it meets it as an IEnumerable&lt;T&gt; boundary, fused only where that chain asks
        /// to be with Fuse().
        /// </summary>
        None = 0,

        /// <summary>
        /// Any Cistern chain pushed through it is fused with it, whether or not that chain asks to be with Fuse().
        /// </summary>
        Fuse = 1 << 0,
    }
}
