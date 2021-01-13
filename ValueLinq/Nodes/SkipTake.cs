using Cistern.ValueLinq.Containers;

namespace Cistern.ValueLinq.Nodes
{
    static class SkipTake
    {
        public static bool CreateArray<T, CreationType, Tail>(Optimizations.SourceArray<T> src, ref Tail nodes, out CreationType creation, int start, int count) where Tail : INodes
        {
            if (count <= 0)
            {
                var empty = new EmptyPullEnumerator<T>();
                creation = nodes.CreateObject<CreationType, T, EmptyPullEnumerator<T>>(ref empty);
                return true;
            }

            if (nodes.TryObjectAscentOptimization<Optimizations.SourceArray<T>, CreationType>(new Optimizations.SourceArray<T> { Array = src.Array, Start = start, Count = count }, out creation))
                return true;

            var enumerator = new ArrayPullEnumerator<T>(src.Array, start, count);
            creation = nodes.CreateObject<CreationType, T, ArrayPullEnumerator<T>>(ref enumerator);
            return true;
        }

    }
}
