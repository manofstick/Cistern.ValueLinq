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

    public readonly struct PairSelector<TInner, TOuter>
        : Zip.ISelector<TInner, TOuter, (TOuter? Outer, TInner Inner)>
    {
        static bool Zip.ISelector<TInner, TOuter, (TOuter? Outer, TInner Inner)>.PotentialSideEffects => false;

        (TOuter? Outer, TInner Inner) Zip.ISelector<TInner, TOuter, (TOuter? Outer, TInner Inner)>.Invoke(TInner inner, TOuter outer) => (outer, inner);
    }
}
