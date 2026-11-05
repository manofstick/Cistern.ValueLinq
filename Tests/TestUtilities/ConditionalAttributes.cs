using System;
using System.Reflection;

namespace Xunit
{
    // Stand-ins for the runtime repo's Microsoft.DotNet.XUnitExtensions attributes: the test is skipped unless every
    // named static bool member of the type is true.
    internal static class Conditions
    {
        public static string SkipReason(Type calleeType, string[] conditionMemberNames)
        {
            foreach (var name in conditionMemberNames)
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                var value =
                    calleeType.GetProperty(name, flags)?.GetValue(null)
                    ?? calleeType.GetField(name, flags)?.GetValue(null)
                    ?? calleeType.GetMethod(name, flags, Type.EmptyTypes)?.Invoke(null, null)
                    ?? throw new ArgumentException($"{calleeType.Name}.{name} is not a static bool member");

                if (!(bool)value)
                    return $"Condition(s) not met: \"{name}\"";
            }
            return null;
        }
    }

    public sealed class ConditionalFactAttribute : FactAttribute
    {
        public ConditionalFactAttribute(Type calleeType, params string[] conditionMemberNames)
        {
            var skip = Conditions.SkipReason(calleeType, conditionMemberNames);
            if (skip != null)
                Skip = skip;
        }
    }

    public sealed class ConditionalTheoryAttribute : TheoryAttribute
    {
        public ConditionalTheoryAttribute(Type calleeType, params string[] conditionMemberNames)
        {
            var skip = Conditions.SkipReason(calleeType, conditionMemberNames);
            if (skip != null)
                Skip = skip;
        }
    }
}
