// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

#if CISTERN_LINQ
using Cistern.Linq;
#elif CISTERN_VALUELINQ
using Cistern.ValueLinq;
#else
using System.Linq;
#endif

namespace Linqs.Tests
{
    public class EmptyEnumerableTest : EnumerableTests
    {
        private void TestEmptyCached<T>()
        {
            var enumerable1 = Enumerable.Empty<T>();
            var enumerable2 = Enumerable.Empty<T>();

            Assert.Same(enumerable1, enumerable2); // Enumerable.Empty is not cached if not the same.
        }

        [Fact(Skip = "CISTERN.VALUELINQ: There rules are irrelevant")]
        public void EmptyEnumerableCachedTest()
        {
            TestEmptyCached<int>();
            TestEmptyCached<string>();
            TestEmptyCached<object>();
            TestEmptyCached<EmptyEnumerableTest>();
        }

        private void TestEmptyEmpty<T>()
        {
            Assert.Equal(new T[0], Enumerable.Empty<T>());
            Assert.Equal(0, Enumerable.Empty<T>().Count());
            Assert.Same(Enumerable.Empty<T>().GetEnumerator(), Enumerable.Empty<T>().GetEnumerator());
        }

        [Fact(Skip = "CISTERN.VALUELINQ: There rules are irrelevant")]
        public void EmptyEnumerableIsIndeedEmpty()
        {
            TestEmptyEmpty<int>();
            TestEmptyEmpty<string>();
            TestEmptyEmpty<object>();
            TestEmptyEmpty<EmptyEnumerableTest>();
        }
    }
}
