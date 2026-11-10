using System;
using System.Collections.Generic;
using Xunit;

namespace Cistern.ValueLinq.Tests
{
    // Map and Filter take a pure function, so the requests Select and Where cannot answer without changing when their
    // user code runs are answered: Map's selector runs only for the elements a result is made of, and Filter's
    // predicate may run from the end.
    public class MapFilterAnswers
    {
        private static readonly int[] Values = [5, 3, 8, 1, 9, 4, 7];

        private static IEnumerable<int> Iterate(int[] values)
        {
            foreach (var value in values)
                yield return value;
        }

        [Fact]
        public void Map_CountAndAny_RunNoSelector()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(7, Iterate(Values).Map(selector).Count());
            Assert.Equal(4, Values.Where(x => x > 4).Map(selector).Count());
            Assert.True(Values.Where(x => x > 4).Map(selector).Any());
            Assert.False(Values.Where(x => x > 100).Map(selector).Any());
            Assert.Equal(0, calls);
        }

        [Fact]
        public void Map_Skip_ProjectsOnlyWhatIsKept()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(new[] { 90, 40, 70 }, Values.Where(_ => true).Map(selector).Skip(4).ToArray());
            Assert.Equal(3, calls);

            calls = 0;
            Assert.Equal(110, Values.Where(_ => true).Map(selector).Skip(1).Take(2).Sum());
            Assert.Equal(2, calls);

            calls = 0;
            Assert.Equal(new[] { 80, 10 }, Values.Map(selector).Skip(2).Take(2).ToList());
            Assert.Equal(2, calls);
        }

        [Fact]
        public void Map_ElementFromEnd_ProjectsOnlyTheElement()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(90, Values.Where(_ => true).Map(selector).ElementAt(^3));
            Assert.Equal(90, Iterate(Values).Map(selector).ElementAt(^3));
            Assert.Equal(0, Iterate(Values).Map(selector).ElementAtOrDefault(^8));
            Assert.Equal(70, Iterate(Values).Map(selector).Last());
            Assert.Equal(3, calls);
        }

        [Fact]
        public void Map_Reverse_IsTheSourceReversed()
        {
            var calls = 0;
            Func<int, int> selector = x => { ++calls; return x * 10; };

            Assert.Equal(70, Values.Map(selector).Reverse().First());
            Assert.Equal(1, calls);
            Assert.Equal(new[] { 70, 40, 90, 10, 80, 30, 50 }, Values.Map(selector).Reverse().ToArray());
        }

        [Fact]
        public void Map_Pulled()
        {
            var pulled = new List<int>();
            foreach (var x in Values.Map(x => x + 1).Skip(5))
                pulled.Add(x);

            Assert.Equal(new[] { 5, 8 }, pulled);
            Assert.Equal(new[] { 6, 4, 9, 2, 10, 5, 8 }, ((IEnumerable<int>)Values.Map(x => x + 1)).ToList());
        }

        [Fact]
        public void Filter_ElementFromEnd_ScansFromTheEnd()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x > 4; };

            // 5, 8, 9, 7 match: ^2 is 9, reached from the end over 7, 4, 9
            Assert.Equal(9, Values.Filter(predicate).ElementAt(^2));
            Assert.Equal(3, calls);

            calls = 0;
            Assert.Equal(9, Values.Map(x => x).Filter(predicate).ElementAt(^2));
            Assert.Equal(3, calls);

            Assert.Equal(0, Values.Filter(predicate).ElementAtOrDefault(^5));
            Assert.Equal(8, Values.Filter(predicate).ElementAt(1));
        }

        [Fact]
        public void Filter_Reverse_IsTheSourceReversed()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x > 4; };

            Assert.Equal(7, Values.Filter(predicate).Reverse().First());
            Assert.Equal(1, calls);

            calls = 0;
            Assert.Equal(9, Values.Filter(predicate).Last(x => x > 8));
            Assert.Equal(3, calls);

            Assert.Equal(new[] { 7, 9, 8, 5 }, Values.Filter(predicate).Reverse().ToArray());
        }

        [Fact]
        public void Filter_PulledAndPushed()
        {
            var pulled = new List<int>();
            foreach (var x in Iterate(Values).Filter(x => x % 2 == 1))
                pulled.Add(x);

            Assert.Equal(new[] { 5, 3, 1, 9, 7 }, pulled);
            Assert.Equal(new[] { 5, 3, 1, 9, 7 }, Values.Filter(x => x % 2 == 1).ToArray());
            Assert.Equal(new[] { 50, 30, 10, 90, 70 }, Values.Filter(x => x % 2 == 1).Map(x => x * 10).ToArray());
            Assert.Equal(25, Values.Filter(x => x % 2 == 1).Sum());
        }

        private static Type NodeOf<T, TNode, TEnumerator>(in ValueEnumerable<T, TNode, TEnumerator> _)
            where TNode : INode<T>
            where TEnumerator : struct, IPullEnumerator<T>, INodeEnumerator<TNode, TEnumerator>
            => typeof(TNode);

        // the reordering stays outermost and the filter goes below it
        [Fact]
        public void Filter_OverAReordering_IsAppliedBelowIt()
        {
            Func<int, bool> predicate = x => x > 4;

            var ordered = NodeOf(Values.OrderBy(x => x).Filter(predicate));
            Assert.Equal(typeof(Nodes.OrderBy.Node<,,>), ordered.GetGenericTypeDefinition());
            Assert.Equal(typeof(Nodes.Filter.Node<,>), ordered.GetGenericArguments()[2].GetGenericTypeDefinition());

            // a Reverse alone keeps the filter above it, so the filter sees the elements last first and stops at a match
            var reversed = NodeOf(Values.Reverse().Filter(predicate));
            Assert.Equal(typeof(Nodes.Filter.Node<,>), reversed.GetGenericTypeDefinition());
            Assert.Equal(typeof(Nodes.Reverse.Node<,>), reversed.GetGenericArguments()[1].GetGenericTypeDefinition());

            var reversedOrdering = NodeOf(Values.OrderBy(x => x).Reverse().Filter(predicate)).GetGenericArguments()[1];
            Assert.Equal(typeof(Nodes.OrderBy.Node<,,>), reversedOrdering.GetGenericTypeDefinition());
            Assert.Equal(typeof(Nodes.Filter.Node<,>), reversedOrdering.GetGenericArguments()[2].GetGenericTypeDefinition());

            var orderedReversal = NodeOf(Values.Reverse().OrderBy(x => x).Filter(predicate)).GetGenericArguments()[2];
            Assert.Equal(typeof(Nodes.Reverse.Node<,>), orderedReversal.GetGenericTypeDefinition());
            Assert.Equal(typeof(Nodes.Filter.Node<,>), orderedReversal.GetGenericArguments()[1].GetGenericTypeDefinition());

            var shuffled = NodeOf(Values.Shuffle().Filter(predicate));
            Assert.Equal(typeof(Nodes.Shuffle.Node<,>), shuffled.GetGenericTypeDefinition());
            Assert.Equal(typeof(Nodes.Filter.Node<,>), shuffled.GetGenericArguments()[1].GetGenericTypeDefinition());
        }

        [Fact]
        public void Filter_OverAReordering_SameElements()
        {
            Func<int, bool> predicate = x => x > 4;

            Assert.Equal(new[] { 5, 7, 8, 9 }, Values.OrderBy(x => x).Filter(predicate).ToArray());
            Assert.Equal(new[] { 9, 8, 7, 5 }, Values.OrderByDescending(x => x).Filter(predicate).ToList());
            Assert.Equal(new[] { 9, 8, 7, 5 }, Values.OrderBy(x => x).Reverse().Filter(predicate).ToArray());
            Assert.Equal(new[] { 7, 9, 8, 5 }, Values.Reverse().Filter(predicate).ToArray());

            // equal keys keep the reversed order
            var pairs = new[] { (1, 'a'), (6, 'b'), (1, 'c'), (6, 'd'), (2, 'e') };
            Assert.Equal(new[] { (6, 'd'), (6, 'b') }, pairs.Reverse().OrderBy(p => p.Item1).Filter(p => p.Item1 > 4).ToArray());
            Assert.Equal((6, 'd'), pairs.Reverse().OrderBy(p => p.Item1).Filter(p => p.Item1 > 4).First());
            Assert.Equal(new[] { 5, 7, 8, 9 }, Values.Shuffle().Filter(predicate).Order().ToArray());

            var pulled = new List<int>();
            foreach (var x in Values.OrderBy(x => x).Reverse().Filter(predicate))
                pulled.Add(x);
            Assert.Equal(new[] { 9, 8, 7, 5 }, pulled);
        }

        [Fact]
        public void Filter_OverAReversal_StopsAtTheFirstMatchFromTheEnd()
        {
            var calls = 0;
            Func<int, bool> predicate = x => { ++calls; return x == 9; };

            // 7, 4, then 9
            Assert.Equal(9, Iterate(Values).Reverse().Filter(predicate).First());
            Assert.Equal(3, calls);
        }

        // the key selector runs only on the elements kept, and the ordering's selection answers First and Last
        [Fact]
        public void Filter_OverAnOrdering_KeysOnlyTheKept()
        {
            var keys = 0;
            Func<int, int> key = x => { ++keys; return x; };

            Assert.Equal(5, Values.OrderBy(key).Filter(x => x > 4).First());
            Assert.Equal(9, Values.OrderBy(key).Filter(x => x > 4).Last());
            Assert.Equal(5, Values.OrderBy(key).Reverse().Filter(x => x > 4).Last());
            Assert.Equal(12, keys);

            Assert.Equal(0, Values.OrderBy(x => x).Filter(x => x > 100).FirstOrDefault());
            Assert.Equal(7, Values.Reverse().Filter(x => x > 4).First());
            Assert.Contains(Values.Shuffle().Filter(x => x > 4).First(), new[] { 5, 7, 8, 9 });
        }
    }
}
