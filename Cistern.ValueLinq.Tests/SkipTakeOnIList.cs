using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Skip and Take over an IList<T> or IReadOnlyList<T> that is neither an array nor a List<T> keep indexed access: an
    // element is read by its index, never by walking the source to it.
    public class SkipTakeOnIList
    {
        private abstract class Counting
        {
            protected readonly int[] Items;

            protected Counting(int count)
            {
                Items = new int[count];
                for (var i = 0; i < count; ++i)
                    Items[i] = i;
            }

            public int Reads { get; protected set; }
            public int Enumerations { get; private set; }

            protected int Read(int index)
            {
                ++Reads;
                return Items[index];
            }

            public IEnumerator<int> GetEnumerator()
            {
                ++Enumerations;
                for (var i = 0; i < Items.Length; ++i)
                    yield return Read(i);
            }
        }

        private sealed class CountingList : Counting, IList<int>
        {
            public CountingList(int count) : base(count) { }

            public int this[int index]
            {
                get => Read(index);
                set => throw new NotSupportedException();
            }

            public int Count => Items.Length;
            public bool IsReadOnly => true;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void CopyTo(int[] array, int arrayIndex) => throw new NotSupportedException();
            public bool Contains(int item) => throw new NotSupportedException();
            public int IndexOf(int item) => throw new NotSupportedException();
            public void Add(int item) => throw new NotSupportedException();
            public void Clear() => throw new NotSupportedException();
            public void Insert(int index, int item) => throw new NotSupportedException();
            public bool Remove(int item) => throw new NotSupportedException();
            public void RemoveAt(int index) => throw new NotSupportedException();
        }

        private sealed class CountingReadOnlyList : Counting, IReadOnlyList<int>
        {
            public CountingReadOnlyList(int count) : base(count) { }

            public int this[int index] => Read(index);
            public int Count => Items.Length;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private static Counting Create(string kind) =>
            kind switch
            {
                "IList" => new CountingList(1000),
                "IReadOnlyList" => new CountingReadOnlyList(1000),
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };

        private static IEnumerable<int> AsEnumerable(Counting source) => (IEnumerable<int>)source;

        [Theory]
        [InlineData("IList")]
        [InlineData("IReadOnlyList")]
        public void ElementAt_ReadsOneElement(string kind)
        {
            var source = Create(kind);

            Assert.Equal(810, AsEnumerable(source).Skip(10).Take(900).ElementAt(800));
            Assert.Equal(0, source.Enumerations);
            Assert.Equal(1, source.Reads);
        }

        [Theory]
        [InlineData("IList")]
        [InlineData("IReadOnlyList")]
        public void ElementAtOrDefault_ReadsOneElement(string kind)
        {
            var source = Create(kind);

            Assert.Equal(810, AsEnumerable(source).Skip(10).Take(900).ElementAtOrDefault(800));
            Assert.Equal(0, source.Enumerations);
            Assert.Equal(1, source.Reads);
        }

        [Theory]
        [InlineData("IList")]
        [InlineData("IReadOnlyList")]
        public void Last_ReadsOneElement(string kind)
        {
            var source = Create(kind);

            Assert.Equal(909, AsEnumerable(source).Skip(10).Take(900).Last());
            Assert.Equal(0, source.Enumerations);
            Assert.Equal(1, source.Reads);
        }
    }
}
