using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Cistern.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 0; i < 5; ++i)
            {
                var sw = Stopwatch.StartNew();

                var x = new Linqs.Tests.ConcatTests();
                foreach (var items in Linqs.Tests.ConcatTests.ManyConcatsData())
                {
                    foreach (IEnumerable<IEnumerable<int>> item in items)
                        x.ManyConcats(item);
                }

                Console.WriteLine(sw.ElapsedMilliseconds);
            }
        }
    }
}
