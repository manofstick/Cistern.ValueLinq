using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    public class NonEnumeratedCount
    {
        private static int[] Sequence(int count)
        {
            var items = new int[count];
            for (var i = 0; i < count; ++i)
                items[i] = i;

            return items;
        }

        private static int Pulled<T>(IEnumerable<T> source)
        {
            var count = 0;
            foreach (var _ in source)
                ++count;

            return count;
        }

        private static void AnswersItsLength<T>(IEnumerable<T> source)
        {
            Assert.True(source.TryGetNonEnumeratedCount(out var count));
            Assert.Equal(Pulled(source), count);
        }

        private static void Declines<T>(IEnumerable<T> source)
        {
            Assert.False(source.TryGetNonEnumeratedCount(out var count));
            Assert.Equal(0, count);
        }

        private sealed class Watched<T>(IEnumerable<T> inner) : IEnumerable<T>
        {
            public int Enumerations;

            public IEnumerator<T> GetEnumerator()
            {
                ++Enumerations;
                return inner.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static IEnumerable<int> Iterate(int count)
        {
            for (var i = 0; i < count; ++i)
                yield return i;
        }

        public static IEnumerable<object[]> Lengths() => [[0], [1], [10], [1001]];

        [Theory]
        [MemberData(nameof(Lengths))]
        public void EveryCollectionShapeAnswersItsLength(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            ReadOnlyMemory<int> memory = array;

            Assert.True(array.TryGetNonEnumeratedCount(out var arrayCount));
            Assert.Equal(length, arrayCount);
            Assert.True(list.TryGetNonEnumeratedCount(out var listCount));
            Assert.Equal(length, listCount);
            Assert.True(memory.TryGetNonEnumeratedCount(out var memoryCount));
            Assert.Equal(length, memoryCount);

            AnswersItsLength<int>(array);
            AnswersItsLength<int>(list);
            AnswersItsLength(new Stack<int>(array));
            AnswersItsLength(new Queue<int>(array));
            AnswersItsLength(new HashSet<int>(array));
        }

        [Fact]
        public void CorefxSupportedEnumerablesAreAnswered()
        {
            AnswersItsLength(Enumerable.Empty<string>());
            AnswersItsLength(Enumerable.Range(1, 100));
            AnswersItsLength(Enumerable.Repeat(1, 80));
            AnswersItsLength(Enumerable.Range(1, 20).Reverse());
            AnswersItsLength(Enumerable.Range(1, 20).OrderBy(x => -x));
            AnswersItsLength(Enumerable.Range(1, 10).Concat(Enumerable.Range(11, 10)));
            AnswersItsLength(Enumerable.Range(1, 50).Select(x => x + 1));
            AnswersItsLength(new int[] { 1, 2, 3, 4 }.Select(x => x + 1));
            AnswersItsLength(Enumerable.Range(1, 50).Select(x => x + 1).Select(x => x - 1));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void ValueEnumerableAnswersWhatItYields(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);

            var reversed = ((IEnumerable<int>)list).Reverse();
            Assert.True(reversed.TryGetNonEnumeratedCount(out var reversedCount));
            Assert.Equal(Pulled(reversed), reversedCount);

            var taken = array.Select(x => x * 2).Take(7);
            Assert.True(taken.TryGetNonEnumeratedCount(out var takenCount));
            Assert.Equal(Pulled(taken), takenCount);

            var skipped = list.Skip(3);
            Assert.True(skipped.TryGetNonEnumeratedCount(out var skippedCount));
            Assert.Equal(Pulled(skipped), skippedCount);

            AnswersItsLength(array.Append(-1).Prepend(-2));
            AnswersItsLength(array.Concat(Sequence(3)));
            AnswersItsLength(array.OrderBy(x => -x).ThenBy(x => x));
            AnswersItsLength(array.Shuffle());
            AnswersItsLength(array.Zip(Sequence(length / 2)));
        }

        [Theory]
        [MemberData(nameof(Lengths))]
        public void FilteredAndUnknownLengthsAreDeclined(int length)
        {
            var array = Sequence(length + 1);
            var list = new List<int>(array);

            Declines(array.Where(x => x % 2 == 0));
            Declines(array.GroupBy(x => x % 2 == 0));
            Declines(array.SelectMany(x => Sequence(x % 3)));
            Declines(array.TakeWhile(x => x < 5));
            Declines(new object[] { "abc" }.OfType<string>());
            Declines(Iterate(length));
            Declines(Iterate(length).Select(x => x));
            Declines(Iterate(length).Concat(list));
            Declines(list.Concat(Iterate(length)));
        }

        // a count read from the source without running anything: a Select over any collection, and a Concat over
        // collections whose cached lengths may have changed
        [Theory]
        [MemberData(nameof(Lengths))]
        public void SelectAndConcatOverCollectionsAreAnswered(int length)
        {
            var array = Sequence(length);
            var list = new List<int>(array);
            var stack = new Stack<int>(array);

            AnswersItsLength(list.Select(x => x + 1));
            AnswersItsLength(list.Select(x => x + 1).Select(x => x - 1));
            AnswersItsLength(list.AsReadOnly().Select(x => x + 1));
            AnswersItsLength(stack.Select(x => x + 1));
            AnswersItsLength(stack.Select(x => x + 1).Select(x => x - 1));
            AnswersItsLength(new HashSet<int>(array).Select(x => x + 1));
            AnswersItsLength(list.Concat(list));
            AnswersItsLength(stack.Concat(stack).Select(x => x + 1));
            AnswersItsLength(list.Concat(array).Select(x => x + 1).Reverse());
            AnswersItsLength(list.Select(x => x + 1).Append(1).Prepend(2));
            AnswersItsLength(list.Select(x => x + 1).OrderBy(x => -x));
            AnswersItsLength(list.Select(x => x + 1).DefaultIfEmpty());
            AnswersItsLength(list.Select(x => x + 1).Chunk(3));
            AnswersItsLength(list.Select(x => x + 1).Index());
            AnswersItsLength(list.Select(x => x + 1).Shuffle());

            AnswersItsLength(list.Select((x, i) => x + i));
            AnswersItsLength(stack.Select((x, i) => x + i));
            AnswersItsLength(list.Select((in int x) => x + 1));
            AnswersItsLength(stack.Select((in int x) => x + 1));
            AnswersItsLength(list.Select(new AddOne(), 0));
            AnswersItsLength(stack.Select(new AddOne(), 0));
        }

        private readonly struct AddOne : IFunc<int, int>
        {
            public int Invoke(int x) => x + 1;
        }

        [Fact]
        public void ConcatOfListsGrownAfterItWasBuilt()
        {
            var first = new List<int>(Sequence(2));
            var second = new List<int>(Sequence(3));
            var concat = first.Concat(second);
            var selected = concat.Select(x => x + 1);

            second.AddRange(Sequence(4));

            Assert.True(concat.TryGetNonEnumeratedCount(out var count));
            Assert.Equal(9, count);
            Assert.True(selected.TryGetNonEnumeratedCount(out var selectedCount));
            Assert.Equal(9, selectedCount);
        }

        [Fact]
        public void SelectorIsNotRun()
        {
            var calls = 0;
            var selected = Sequence(100).Select(x => { ++calls; return x; });
            var listSelected = new List<int>(Sequence(100)).Select(x => { ++calls; return x; });
            var concatSelected = new List<int>(Sequence(50)).Concat(new Stack<int>(Sequence(50))).Select(x => { ++calls; return x; });

            Assert.True(selected.TryGetNonEnumeratedCount(out var count));
            Assert.Equal(100, count);
            Assert.True(listSelected.TryGetNonEnumeratedCount(out var listCount));
            Assert.Equal(100, listCount);
            Assert.True(concatSelected.TryGetNonEnumeratedCount(out var concatCount));
            Assert.Equal(100, concatCount);
            Assert.Equal(0, calls);
        }

        [Fact]
        public void SourceIsNotEnumerated()
        {
            var watched = new Watched<int>(Iterate(10));

            Declines(watched);
            Declines(watched.Select(x => x + 1));
            Declines(watched.Concat(Sequence(3)));
            Declines(watched.Append(1));
            Declines(watched.OrderBy(x => x));
            Declines(watched.SelectMany(x => Sequence(x)));
            Declines(Sequence(3).SelectMany(x => watched));

            Assert.Equal(0, watched.Enumerations);
        }

        [Fact]
        public void ListChangedAfterTheChainIsBuilt()
        {
            var list = new List<int>(Sequence(5));
            var reversed = ((IEnumerable<int>)list).Reverse();
            var skipped = list.Skip(2);

            list.AddRange(Sequence(4));

            Assert.True(reversed.TryGetNonEnumeratedCount(out var reversedCount));
            Assert.Equal(9, reversedCount);
            Assert.True(skipped.TryGetNonEnumeratedCount(out var skippedCount));
            Assert.Equal(7, skippedCount);
            Assert.Equal(Pulled(skipped), skippedCount);
        }

        [Fact]
        public void LengthBeyondIntIsDeclined()
        {
            Declines(Enumerable.Repeat(7, int.MaxValue).Concat(Enumerable.Repeat(7, 1)));
        }

        [Fact]
        public void NullSourceThrows()
        {
            Assert.Throws<ArgumentNullException>("source", () => ((int[])null!).TryGetNonEnumeratedCount(out _));
            Assert.Throws<ArgumentNullException>("source", () => ((List<int>)null!).TryGetNonEnumeratedCount(out _));
            Assert.Throws<ArgumentNullException>("source", () => ((IEnumerable<int>)null!).TryGetNonEnumeratedCount(out _));
        }
    }
}
