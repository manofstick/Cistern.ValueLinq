using BenchmarkDotNet.Running;

namespace Cistern.Benchmarks.StructLinq
{
    public class Program
    {
        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
