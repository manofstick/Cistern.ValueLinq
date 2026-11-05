namespace Cistern.ValueLinq.Optimizations;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// Used only by the generated TryPushOptimization, inside the guard it writes on the request's type: they read the
// request and write the result in place, the request without the copy a cast would make, and the result where the
// asker holds it rather than into a zeroed local copied there. The request's reinterpretation is sound by the guard;
// the result's rests on the asker naming the result type of the request's interface, which nothing at run time checks
// in Release: a check there is not folded for a result whose type is shared code (ToArray's T[], anything over a
// reference type), and reading the generic context for it keeps the dispatch from being inlined. Debug builds, the test
// runs among them, assert it.
static class Request
{
    public static ref readonly TActual As<TRequest, TActual>(in TRequest request)
        where TActual : struct
        => ref Unsafe.As<TRequest, TActual>(ref Unsafe.AsRef(in request));

    public static ref TActual Result<TRequestResult, TActual>([UnscopedRef] out TRequestResult result)
    {
        Debug.Assert(typeof(TRequestResult) == typeof(TActual), $"A {typeof(TRequestResult)} result asked for where the answer writes {typeof(TActual)}: the asker names the wrong result type for its request.");

        Unsafe.SkipInit(out result);

        return ref Unsafe.As<TRequestResult, TActual>(ref result);
    }
}
