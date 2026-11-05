namespace System
{
    // The members of the runtime repo's PlatformDetection and TestEnvironment that this suite reads, with the values
    // they take on CoreCLR on a desktop OS.
    public static class PlatformDetection
    {
        public static bool IsDebuggerTypeProxyAttributeSupported => true;
        public static bool IsLinqSpeedOptimized => true;
        public static bool IsMultithreadingSupported => true;
        public static bool IsNotBuiltWithAggressiveTrimming => true;
        public static bool IsWasm => false;
        public static bool IsNaNPayloadPreservationExpected => true;
    }

    public static class TestEnvironment
    {
        public static bool IsStressModeEnabled =>
            Environment.GetEnvironmentVariable("DOTNET_TEST_STRESS") is string value
            && (value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase));
    }
}
