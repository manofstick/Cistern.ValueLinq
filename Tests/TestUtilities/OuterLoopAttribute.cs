using System;

namespace Xunit
{
    // Stand-in for the runtime repo's attribute of the same name, which marks a test for its outer-loop runs only.
    class OuterLoopAttribute : Attribute
    {
        public OuterLoopAttribute() { }
        public OuterLoopAttribute(string ignore) { }
    }
}
