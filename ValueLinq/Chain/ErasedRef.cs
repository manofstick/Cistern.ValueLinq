namespace Cistern.ValueLinq.Chain;

using System.Runtime.CompilerServices;

// stands in for a builder or filler held by ref: a ref field cannot refer to a ref struct (CS9050), so Erase.Ref and
// Deref erase its type. Safe: the GC follows a ref only to the location it points at, and a ref struct's own references
// are reported by the frame that holds it, with their real types; Deref is only ever given back the type that Ref took.
// becomes ref readonly T once that is allowed, https://github.com/dotnet/csharplang/discussions/9152#discussioncomment-12240032
readonly struct ErasedRef { }

static class Erase
{
    internal static ref readonly ErasedRef Ref<T>(in T value)
        where T : allows ref struct
        => ref Unsafe.As<T, ErasedRef>(ref Unsafe.AsRef(in value));

    internal static ref readonly T Deref<T>(in ErasedRef value)
        where T : allows ref struct
        => ref Unsafe.As<ErasedRef, T>(ref Unsafe.AsRef(in value));
}
