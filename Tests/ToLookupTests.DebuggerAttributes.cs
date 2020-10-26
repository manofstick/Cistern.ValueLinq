// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
    public partial class ToLookupTests : EnumerableTests
    {
        [Theory]
        [MemberData(nameof(DebuggerAttributesValid_Data))]
        public void DebuggerAttributesValid<TKey, TElement>(System.Linq.ILookup<TKey, TElement> lookup)
        {
            Assert.Equal($"Count = {lookup.Count}", DebuggerAttributes.ValidateDebuggerDisplayReferences(lookup));

            object proxyObject = DebuggerAttributes.GetProxyObject(lookup);

            // Validate proxy fields
            Assert.Empty(DebuggerAttributes.GetDebuggerVisibleFields(proxyObject.GetType()));

            // Validate proxy properties
            IEnumerable<PropertyInfo> properties = DebuggerAttributes.GetDebuggerVisibleProperties(proxyObject.GetType());
            Assert.Equal(1, properties.Count());

            // Groupings
            PropertyInfo groupingsProperty = properties.Single(property => property.Name == "Groupings");
            Assert.Equal(DebuggerBrowsableState.RootHidden, DebuggerAttributes.GetDebuggerBrowsableState(groupingsProperty));
            var groupings = (System.Linq.IGrouping<TKey, TElement>[])groupingsProperty.GetValue(proxyObject);
            Assert.IsType<System.Linq.IGrouping<TKey, TElement>[]>(groupings); // Arrays can be covariant / of assignment-compatible types

            Assert.All(groupings.Zip(lookup, (l, r) => Tuple.Create(l, r)), tuple =>
            {
                Assert.Same(tuple.Item1, tuple.Item2);
            });

            Assert.Same(groupings, groupingsProperty.GetValue(proxyObject)); // The result should be cached, as Lookup is immutable.
        }

        public static IEnumerable<object[]> DebuggerAttributesValid_Data()
        {
            IEnumerable<int> source = new[] { 1 };
            yield return new object[] { source.ToLookup(i => i) };
            yield return new object[] { source.ToLookup(i => i.ToString(), i => i) };
            yield return new object[] { source.ToLookup(i => TimeSpan.FromSeconds(i), i => i) };

            yield return new object[] { new string[] { null }.ToLookup(x => x) };
            yield return new object[] { new int?[] { null }.ToLookup(x => x) };
        }
    }
}
