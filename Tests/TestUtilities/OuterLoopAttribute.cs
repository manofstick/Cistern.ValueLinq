using System;
using System.Collections.Generic;
using System.Text;

#if CISTERN_LINQ
using Cistern.Linq;
#elif CISTERN_VALUELINQ
using Cistern.ValueLinq;
#else
using System.Linq;
#endif

namespace Linqs.Tests.TestUtilities
{
    class OuterLoopAttribute : Attribute
    {
        public OuterLoopAttribute(string ignore) { }
    }
}
