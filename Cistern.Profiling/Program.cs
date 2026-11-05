using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cistern.ValueLinq;

namespace Cistern.Profiling
{
    class Program
    {
        static void Main(string[] args)
        {
            SelectManyReturnedResultSelector();
        }

        // the query of Benchmark\Synthetic\SelectManyChainSum's ReturnedResultSelector category over a few short rows: the
        // selector returns a Cistern chain, so the result-selector overload taking a chain binds and each inner chain is
        // pushed, fused, into SelectManyResult's ResultSelectorSink
        static void SelectManyReturnedResultSelector()
        {
            // row r has (r * 37) % 20 elements: 0, 17, 14, 11, 8
            var rows = new int[5][];
            for (var r = 0; r < rows.Length; ++r)
            {
                var row = new int[(r * 37) % 20];
                for (var j = 0; j < row.Length; ++j)
                    row[j] = (r * 1000 + j) % 1000;
                rows[r] = row;
            }

            var expected = 0;
            foreach (var row in rows)
                foreach (var x in row)
                    expected += x;

            if (Debugger.IsAttached)
                Debugger.Break();

            var sum =
                rows
                .SelectMany(row => row.Select(x => x), (row, x) => x)
                .Sum();

            Console.WriteLine($"SelectManyReturnedResultSelector {sum} expected {expected}");
        }

        // the queries of Benchmark\Synthetic\SelectManyChainCount over a few short rows: each inner sequence is a Cistern
        // chain over an array held as IEnumerable<int>, which cannot count itself without running, so SelectMany's Count
        // pushes it into the ErasedChain.PushSink<int, Count<int>> it shares across the inner sequences
        static void SelectManyChainCount()
        {
            // row r has (r * 37) % 20 elements: 0, 17, 14, 11, 8
            var rows = new int[5][];
            for (var r = 0; r < rows.Length; ++r)
            {
                var row = new int[(r * 37) % 20];
                for (var j = 0; j < row.Length; ++j)
                    row[j] = (r * 1000 + j) % 1000;
                rows[r] = row;
            }

            var where = new IEnumerable<int>[rows.Length];
            var select = new IEnumerable<int>[rows.Length];
            for (var r = 0; r < rows.Length; ++r)
            {
                where[r] = rows[r].Where(x => (x & 1) == 0);
                select[r] = rows[r].Select(x => x);
            }

            var expectedWhere = 0;
            var expectedSelect = 0;
            foreach (var row in rows)
            {
                expectedSelect += row.Length;
                foreach (var x in row)
                    if ((x & 1) == 0)
                        ++expectedWhere;
            }

            if (Debugger.IsAttached)
                Debugger.Break();

            var whereCount =
                where
                .SelectMany(row => row)
                .Count();

            var selectCount =
                select
                .SelectMany(row => row)
                .Count();

            Console.WriteLine($"SelectManyChainCount Where {whereCount} expected {expectedWhere}, Select {selectCount} expected {expectedSelect}");
        }

        // the query of Benchmark\Synthetic\SelectManyWhereSum over a few short rows: the inner arrays expose spans, so
        // SelectMany.Push offers each to the Where as a batch
        static void SelectManyWhereSum()
        {
            // row r has (r * 37) % 20 elements: 0, 17, 14, 11, 8
            var rows = new int[5][];
            for (var r = 0; r < rows.Length; ++r)
            {
                var row = new int[(r * 37) % 20];
                for (var j = 0; j < row.Length; ++j)
                    row[j] = (r * 1000 + j) % 1000;
                rows[r] = row;
            }

            var expected = 0L;
            foreach (var row in rows)
                foreach (var x in row)
                    if (x % 3 == 0)
                        expected += (long)x * x;

            if (Debugger.IsAttached)
                Debugger.Break();

            var sum =
                rows
                .SelectMany(row => row)
                .Where(x => x % 3 == 0)
                .Select(x => (long)x * x)
                .Sum();

            Console.WriteLine($"SelectManyWhereSum {sum} expected {expected}");
        }

        // the query of Benchmark\Synthetic\SelectManyEnumerableSum over a few short rows: a Queue<int> exposes no span,
        // so each inner sequence is pushed into SelectMany's ErasedChain.PushSink through SystemIEnumerable.Node's struct-enumerator dispatch
        static void SelectManyEnumerableSum()
        {
            // row r has (r * 37) % 20 elements: 0, 17, 14, 11, 8
            var rows = new Queue<int>[5];
            for (var r = 0; r < rows.Length; ++r)
            {
                var length = (r * 37) % 20;
                var row = new Queue<int>(length);
                for (var j = 0; j < length; ++j)
                    row.Enqueue((r * 1000 + j) % 1000);
                rows[r] = row;
            }

            var expected = 0;
            foreach (var row in rows)
                foreach (var x in row)
                    expected += x;

            if (Debugger.IsAttached)
                Debugger.Break();

            var sum =
                rows
                .SelectMany(row => row)
                .Sum();

            Console.WriteLine($"SelectManyEnumerableSum {sum} expected {expected}");
        }

        // the shape of Benchmark\Coverage\ElementAt: the container is held as IEnumerable<int>, so the call binds to
        // the IEnumerable overload and dispatches on the runtime type
        static void ElementAt()
        {
            IEnumerable<int> array = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            IEnumerable<int> list = new List<int>(array);

            if (Debugger.IsAttached)
                Debugger.Break();

            var fromArray = array.ElementAt(5);
            var fromList = list.ElementAt(5);

            Console.WriteLine($"{fromArray} {fromList}");
        }

        static void ManyConcats()
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
