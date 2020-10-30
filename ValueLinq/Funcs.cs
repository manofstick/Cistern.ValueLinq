namespace Cistern.ValueLinq
{
    public delegate U InFunc<T, U>(in T t);

    public interface IFunc<T, U>
    {
        U Invoke(T t);
    }
}
