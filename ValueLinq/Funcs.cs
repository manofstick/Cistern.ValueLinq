namespace Cistern.ValueLinq
{
    public delegate void InOutFunc<T, U>(in T t, out U u);
    public delegate U InFunc<T, U>(in T t);
}
