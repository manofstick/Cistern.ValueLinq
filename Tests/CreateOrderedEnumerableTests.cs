// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
    public sealed class CreateOrderedEnumerableTests
    {
        [Fact(Skip = "CISTERN.VALUELINQ: Doesn't implement IOrderedEnumerable")]
        public void ThrowsNullKeySelector()
        {
#if !CISTERN_VALUELINQ
            var enumerable = new int[0].Order();
            Assert.Throws<ArgumentNullException>(() => enumerable.CreateOrderedEnumerable((Func<int, int>)null!, null, false));
#endif
        }
    }
}
