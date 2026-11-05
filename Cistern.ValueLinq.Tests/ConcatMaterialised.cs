using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToArray and ToList over a Concat whose last parts are counted: the parts up to the last uncounted one are pushed
    // into a builder, and each counted part after it is copied once, straight into the result at its place, counted only
    // once the parts before it have run. A counted part ahead of an uncounted one goes through the builder, and a Concat
    // ending in an uncounted part is pushed. A Concat counted throughout is copied part by part, a collection by its own
    // CopyTo straight into the result. Chains built through IEnumerable<T> and typed chains alike, nested either way.
    public class ConcatMaterialised
    {
        private sealed class Counted<T>(IList<T> items) : ICollection<T>
        {
            public int CopyTos;
            public int Enumerations;
            public T[]? CopiedInto;
            public int CopiedAt;

            public int Count => items.Count;
            public bool IsReadOnly => true;

            public void CopyTo(T[] array, int arrayIndex)
            {
                ++CopyTos;
                CopiedInto = array;
                CopiedAt = arrayIndex;
                items.CopyTo(array, arrayIndex);
            }

            public IEnumerator<T> GetEnumerator()
            {
                ++Enumerations;
                return items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Contains(T item) => items.Contains(item);
            public void Add(T item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public bool Remove(T item) => throw new NotSupportedException();
        }

        private static IEnumerable<int> Iterate(int start, int count)
        {
            for (var i = 0; i < count; ++i)
                yield return start + i;
        }

        private static int[] Values(int start, int count)
        {
            var values = new int[count];
            for (var i = 0; i < count; ++i)
                values[i] = start + i;
            return values;
        }

        public static IEnumerable<object[]> Lengths()
        {
            foreach (var head in new[] { 0, 3, 8, 9, 100 })
            {
                foreach (var tail in new[] { 0, 1, 10, 1000 })
                    yield return [head, tail];
            }
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ATailCollectionIsCopiedOnceStraightIntoTheArray(int head, int tail)
        {
            var collection = new Counted<int>(Values(head, tail));
            var result = Iterate(0, head).Concat(collection).ToArray();

            Assert.Equal(Values(0, head + tail), result);
            Assert.Equal(0, collection.Enumerations);
            if (tail == 0)
                return;

            Assert.Equal(1, collection.CopyTos);
            Assert.Same(result, collection.CopiedInto);
            Assert.Equal(head, collection.CopiedAt);
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ATailCollectionIsCopiedOnceStraightIntoTheList(int head, int tail)
        {
            var collection = new Counted<int>(Values(head, tail));
            var result = Iterate(0, head).Concat(collection).ToList();

            Assert.Equal(Values(0, head + tail), result);
            Assert.Equal(0, collection.Enumerations);
            if (tail == 0)
                return;

            Assert.Equal(1, collection.CopyTos);
            Assert.Equal(head, collection.CopiedAt);
            Assert.Equal(result.Capacity, collection.CopiedInto!.Length);
        }

        [Fact]
        public void EveryTailPartIsCopiedStraightInInOrder()
        {
            var first = new Counted<int>(Values(5, 10));
            var second = new Counted<int>(Values(20, 3));
            var array = Values(15, 5);
            var list = new List<int>(Values(23, 4));
            var result = Iterate(0, 5).Concat(first).Concat(array).Concat(second).Concat(list).ToArray();

            Assert.Equal(Values(0, 27), result);
            Assert.Equal((result, 5), (first.CopiedInto, first.CopiedAt));
            Assert.Equal((result, 20), (second.CopiedInto, second.CopiedAt));
            Assert.Equal(Values(0, 27), Iterate(0, 5).Concat(first).Concat(array).Concat(second).Concat(list).ToList());
        }

        [Fact]
        public void AHeadCollectionIsCopiedStraightIn()
        {
            var head = new Counted<int>(Values(0, 10));
            var tail = new Counted<int>(Values(13, 10));
            var result = head.Concat(Iterate(10, 3)).Concat(tail).ToArray();

            Assert.Equal(Values(0, 23), result);
            Assert.Equal(1, head.CopyTos);
            Assert.Same(result, head.CopiedInto);
            Assert.Equal(0, head.CopiedAt);
            Assert.Same(result, tail.CopiedInto);
            Assert.Equal(13, tail.CopiedAt);
        }

        [Fact]
        public void AConcatEndingInAnUncountedPartIsPushed()
        {
            var collection = new Counted<int>(Values(3, 10));
            var result = Iterate(0, 3).Concat(collection).Concat(Iterate(13, 2)).ToArray();

            Assert.Equal(Values(0, 15), result);
            Assert.NotSame(result, collection.CopiedInto);
            Assert.Equal(Values(0, 15), Iterate(0, 3).Concat(collection).Concat(Iterate(13, 2)).ToList());
        }

        [Fact]
        public void ACountedConcatIsCopied()
        {
            var array = Values(0, 4);
            var list = new List<int>(Values(4, 6));
            Assert.Equal(Values(0, 10), array.Concat(list).ToArray());
            Assert.Equal(Values(0, 10), array.Concat(list).ToList());
            Assert.Equal(Values(0, 14), array.OfArray().Concat(list.OfList()).Concat(Values(10, 4).OfArray()).ToArray());
        }

        [Fact]
        public void ACollectionInACountedConcatIsCopiedStraightIn()
        {
            var first = new Counted<int>(Values(0, 4));
            var second = new Counted<int>(Values(6, 5));
            var array = Values(4, 2);

            var result = first.Concat(array).Concat(second).ToArray();
            Assert.Equal(Values(0, 11), result);
            Assert.Equal((result, 0), (first.CopiedInto, first.CopiedAt));
            Assert.Equal((result, 6), (second.CopiedInto, second.CopiedAt));

            var list = first.Concat(array).Concat(second).ToList();
            Assert.Equal(Values(0, 11), list);
            Assert.Equal((list.Capacity, 6), (second.CopiedInto!.Length, second.CopiedAt));
            Assert.Equal(0, first.Enumerations + second.Enumerations);
        }

        [Fact]
        public void TheTailIsCountedAfterTheHeadHasRun()
        {
            IEnumerable<int> Growing(List<int> tail)
            {
                for (var i = 0; i < 3; ++i)
                {
                    tail.Add(100 + i);
                    yield return i;
                }
            }

            int[] expected = [0, 1, 2, 7, 100, 101, 102];

            var toArray = new List<int> { 7 };
            Assert.Equal(expected, Growing(toArray).Concat(toArray).ToArray());

            var toList = new List<int> { 7 };
            Assert.Equal(expected, Growing(toList).Concat(toList).ToList());

            var typed = new List<int> { 7 };
            Assert.Equal(expected, Growing(typed).OfEnumerable().Concat(typed.OfList()).ToArray());
        }

        [Fact]
        public void ATypedChainCopiesItsTailStraightIn()
        {
            var collection = new Counted<int>(Values(7, 5));
            var result = Iterate(0, 3).OfEnumerable().Concat(Values(3, 4).OfArray()).Concat(collection.OfEnumerable()).ToArray();

            Assert.Equal(Values(0, 12), result);
            Assert.Same(result, collection.CopiedInto);
            Assert.Equal(7, collection.CopiedAt);

            var list = Iterate(0, 3).OfEnumerable().Concat(Values(3, 4).OfArray()).Concat(collection.OfEnumerable()).ToList();
            Assert.Equal(Values(0, 12), list);
            Assert.Equal(7, collection.CopiedAt);
            Assert.Equal(list.Capacity, collection.CopiedInto!.Length);
        }

        [Fact]
        public void ATypedChainKeepsItsOrderNestedEitherWay()
        {
            var head = new Counted<int>(Values(0, 3));
            var tail = new Counted<int>(Values(9, 3));
            var middle = new List<int>(Values(5, 4));

            // the uncounted part at the end of the start, inside the finish, and alone at the front
            Assert.Equal(Values(0, 12), head.OfEnumerable().Concat(Iterate(3, 2).OfEnumerable()).Concat(middle.OfList().Concat(tail.OfEnumerable())).ToArray());
            Assert.Equal(Values(0, 12), head.OfEnumerable().Concat(Iterate(3, 2).OfEnumerable().Concat(middle.OfList())).Concat(tail.OfEnumerable()).ToList());
            Assert.Equal(Values(0, 12), Iterate(0, 5).OfEnumerable().Concat(middle.OfList().Concat(tail.OfEnumerable())).ToArray());
            Assert.Equal(Values(0, 12), Iterate(0, 5).OfEnumerable().Concat(middle.OfList()).Concat(tail.OfEnumerable()).ToList());
        }

        [Fact]
        public void AReferenceTypeTailIsCopiedIntoTheList()
        {
            static IEnumerable<string> Head()
            {
                yield return "a";
                yield return "b";
            }

            var tail = new Counted<string>(["c", "d"]);
            Assert.Equal(["a", "b", "c", "d", "e"], Head().Concat(tail).Concat(new List<string> { "e" }).ToList());
            Assert.Equal(["a", "b", "c", "d", "e"], Head().Concat(tail).Concat(new List<string> { "e" }).ToArray());
        }
    }
}
