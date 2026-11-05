using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // A value selector and a value predicate over elements of a reference type, whose node code is shared across them,
    // pushed and pulled, element by element and as spans.
    public class ValueFuncsOverReferences
    {
        private readonly struct Length : IFunc<string, int>
        {
            public int Invoke(string t) => t.Length;
        }

        private readonly struct Short : IFunc<string, bool>
        {
            public bool Invoke(string t) => t.Length < 3;
        }

        private static IEnumerable<T> Iterate<T>(T[] items)
        {
            foreach (var item in items)
                yield return item;
        }

        private static List<T> Pull<T>(IEnumerable<T> source)
        {
            var items = new List<T>();
            foreach (var item in source)
                items.Add(item);

            return items;
        }

        private static readonly string[] Words = ["a", "bbbb", "cc", "", "ddddd", "eee", "f"];

        [Fact]
        public void ASelector_ProjectsEachElement()
        {
            int[] expected = [1, 4, 2, 0, 5, 3, 1];

            Assert.Equal(expected, Words.OfArray().Select(new Length(), default(int)).ToArray());
            Assert.Equal(expected, Iterate(Words).OfEnumerable().Select(new Length(), default(int)).ToList());
            Assert.Equal(expected, Pull(Words.OfArray().Select(new Length(), default(int))));
            Assert.Equal(16, Words.OfArray().Select(new Length(), default(int)).Sum());
        }

        [Fact]
        public void APredicate_KeepsTheElementsItPasses()
        {
            string[] expected = ["a", "cc", "", "f"];

            Assert.Equal(expected, Words.OfArray().Where(new Short()).ToArray());
            Assert.Equal(expected, Iterate(Words).OfEnumerable().Where(new Short()).ToList());
            Assert.Equal(expected, Pull(Words.OfArray().Where(new Short())));
            Assert.Equal("cc", Words.OfArray().Where(new Short()).ElementAt(1));
            Assert.Equal(4, Words.OfArray().Where(new Short()).Select(new Length(), default(int)).Sum());
        }

        [Fact]
        public void ASelectManyOverArrays_FlattensThem()
        {
            int[][] arrays = [[1, 2], [], [3], [4, 5, 6]];

            Assert.Equal([1, 2, 3, 4, 5, 6], arrays.SelectMany(x => x).ToList());
            Assert.Equal([1, 2, 3, 4, 5, 6], Pull(arrays.SelectMany(x => x)));
            Assert.Equal(21, arrays.SelectMany(x => x).Sum());
        }
    }
}
