using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using Cistern.ValueLinq.Nodes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Cistern.Benchmarks.ValueLambdas.WhereSelectSum
{

    partial class Benchmark
    {
        [Benchmark]
        public double CisternValueLinq_normal() =>
            _ints
            .Where(x => (x & 1) == 0)
            .Select(x => x * 2)
            .Sum();


        struct DoubleAnInt : IFunc<int, int> { public int Invoke(int t) => t * 2; } 
        struct FilterEvenInts : IFunc<int, bool> { public bool Invoke(int t) => (t & 1) == 0; }
        [Benchmark]
        public double CisternValueLinq_struct() =>
            _ints
            .Where(new FilterEvenInts()) // ug, sugar please
            .Select(new DoubleAnInt(), default(int)) // ug, sugar please + better type inference...
            .Sum();


        [Benchmark]
        public double CisternValueLinq_struct_nothing_up_my_sleve()
        {
            IEnumerable<int> collection = GetCollection();
            IEnumerable<int> withWhere = AddWhere(collection);
            IEnumerable<int> andSelect = AddSelect(withWhere);
            
            var result = andSelect.Sum();

            return result;

            IEnumerable<int> GetCollection() => _ints;
            IEnumerable<int> AddWhere(IEnumerable<int> stuff) => stuff.Where(new FilterEvenInts());
            IEnumerable<int> AddSelect(IEnumerable<int> stuff) => stuff.Select(new DoubleAnInt(), default(int));
        }

        // but needs some sugar! Something like >=> to denote a struct based Func?
        //
        // Which would make it look something like...
        //
        //[Benchmark]
        //public double CisternValueLinq_struct() =>
        //    _ints
        //    .Where(t >=> t * 2)
        //    .Select(t >=> (t & 1) == 0, default(int))
        //    .Sum();


        [Benchmark]
        public double Flatout()
        {
            var total = 0.0;
            foreach (var x in _ints)
            {
                if ((x & 1) == 0)
                    total += x * 2;
            }
            return total;
        }

        [Benchmark(Baseline = true)]
        public double Flatout_cast()
        {
            var total = 0.0;

            if (_ints is int[] asArray)
            {
                foreach (var x in asArray)
                {
                    if ((x & 1) == 0)
                        total += x * 2;
                }
                return total;
            }

            if (_ints is List<int> asList)
            {
                foreach (var x in asList)
                {
                    if ((x & 1) == 0)
                        total += x * 2;
                }
                return total;
            }

            foreach (var x in _ints)
            {
                if ((x & 1) == 0)
                    total += x * 2;
            }
            return total;
        }



        //[Benchmark]
        //public double CisternValueLinq_Foreach()
        //{
        //    var total = 0.0;
        //    foreach (var item in 
        //        _ints
        //        .Where(x => (x & 1) == 0)
        //        .Select(x => x * 2)
        //    )
        //    {
        //        total += item;
        //    }
        //    return total;
        //}
    }
}
