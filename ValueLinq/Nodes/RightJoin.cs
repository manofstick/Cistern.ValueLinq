namespace Cistern.ValueLinq.Nodes;

using System;

public static class RightJoin
{
    public readonly struct FuncSelector<TInner, TOuter, TResult>(Func<TOuter?, TInner, TResult> resultSelector)
        : Zip.ISelector<TInner, TOuter, TResult>
    {
        static bool Zip.ISelector<TInner, TOuter, TResult>.PotentialSideEffects => true;

        TResult Zip.ISelector<TInner, TOuter, TResult>.Invoke(TInner inner, TOuter outer) => resultSelector(outer, inner);
    }
}
