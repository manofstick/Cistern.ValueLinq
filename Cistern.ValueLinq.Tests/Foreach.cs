using System;
using System.Collections.Generic;
using Xunit;
using System.Globalization;

namespace Cistern.ValueLinq.Tests
{
    public class Foreach : EnumerableTests
    {
        [Fact]
        public void SameResultsRepeatCallsIntQuery()
        {
            var numbers = Enumerable.Range(0, 100);

            (int count, int sum) =
                numbers
                .ForEach((0, 0),
                    (ref (int count, int sum) state, int item) =>
                    {
                        state.count++;
                        state.sum += item;
                    });

            var expected = numbers.Average();
            var actual = (double)sum / count;
            
            Assert.Equal(expected, actual);
        }
    }
}
