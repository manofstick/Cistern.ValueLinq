using System;
using System.Collections.Generic;
using System.Text;

namespace Cistern.ValueLinq.Tests.TestUtilities
{
    class OuterLoopAttribute : Attribute
    {
        public OuterLoopAttribute(string ignore) { }
    }
}
