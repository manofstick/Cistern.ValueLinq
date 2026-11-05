using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // ToArray and ToList of a Select over a source with a span project straight into the result, the selector run once
    // per element in source order, alone or as a part of a Concat, an Append or a DefaultIfEmpty.
    public class SelectCopiedToArrayAndList
    {
        private static readonly int[] Data = [10, 11, 12, 13, 14];

        private sealed class Recorder
        {
            public List<int> Seen { get; } = [];

            public int Select(int x)
            {
                Seen.Add(x);
                return x * 2;
            }
        }

        [Fact]
        public void OverEachContainer_ProjectsEachElementOnceInOrder()
        {
            foreach (var shape in new[] { "array", "list", "memory" })
            {
                var toArray = new Recorder();
                var toList = new Recorder();

                var array = shape switch
                {
                    "array" => Data.Select(toArray.Select).ToArray(),
                    "list" => new List<int>(Data).Select(toArray.Select).ToArray(),
                    _ => new ReadOnlyMemory<int>(Data).Select(toArray.Select).ToArray(),
                };
                var list = shape switch
                {
                    "array" => Data.Select(toList.Select).ToList(),
                    "list" => new List<int>(Data).Select(toList.Select).ToList(),
                    _ => new ReadOnlyMemory<int>(Data).Select(toList.Select).ToList(),
                };

                Assert.Equal(new[] { 20, 22, 24, 26, 28 }, array);
                Assert.Equal(new[] { 20, 22, 24, 26, 28 }, list);
                Assert.Equal(Data, toArray.Seen);
                Assert.Equal(Data, toList.Seen);
            }
        }

        [Fact]
        public void AsAPart_ProjectsEachElementOnceInOrder()
        {
            var recorder = new Recorder();

            var concatenated = Data.Select(recorder.Select).Concat(new[] { 1, 2 }).ToList();
            var appended = Data.Select(recorder.Select).Append(3).Prepend(4).ToArray();
            var defaulted = Array.Empty<int>().Select(recorder.Select).DefaultIfEmpty(9).ToArray();

            Assert.Equal(new[] { 20, 22, 24, 26, 28, 1, 2 }, concatenated);
            Assert.Equal(new[] { 4, 20, 22, 24, 26, 28, 3 }, appended);
            Assert.Equal(new[] { 9 }, defaulted);
            Assert.Equal([.. Data, .. Data], recorder.Seen);
        }

        [Fact]
        public void ASelectorThatThrows_Throws()
        {
            Func<int, int> selector = x => x == 12 ? throw new InvalidOperationException() : x;

            Assert.Throws<InvalidOperationException>(() => Data.Select(selector).ToArray());
            Assert.Throws<InvalidOperationException>(() => Data.Select(selector).ToList());
        }
    }
}
