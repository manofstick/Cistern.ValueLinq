using BenchmarkDotNet.Attributes;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class B6
    {
        [Benchmark]
        public int[] B49a_Handcoded()
        {
            var result = new int[_a.Length + _b.Length];
            var index = 0;
            foreach (var x in _a)
                result[index++] = x;
            foreach (var x in _b)
                result[index++] = x;
            return result;
        }

        [Benchmark]
        public int[] B49b_Handcoded()
        {
            var result = new int[_listA.Count + _listB.Count];
            var index = 0;
            foreach (var x in _listA)
                result[index++] = x;
            foreach (var x in _listB)
                result[index++] = x;
            return result;
        }

        [Benchmark]
        public int[] B49c_Handcoded()
        {
            var result = new int[_a.Length + _listB.Count + _c.Length];
            var index = 0;
            foreach (var x in _a)
                result[index++] = x;
            foreach (var x in _listB)
                result[index++] = x;
            foreach (var x in _c)
                result[index++] = x;
            return result;
        }

        [Benchmark]
        public int[] B50a_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _a)
                result.Add(x);
            foreach (var x in Data.Iterator(_b))
                result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public int[] B50b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in Data.Iterator(_a))
                result.Add(x);
            foreach (var x in _b)
                result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public int B51a_Handcoded() =>
            _listA.Count + _listB.Count;

        [Benchmark]
        public int B51b_Handcoded()
        {
            var count = _a.Length;
            foreach (var _ in Data.Iterator(_b))
                ++count;
            return count;
        }

        [Benchmark]
        public int B51c_Handcoded() =>
            _listA.Count + _listB.Count;

        [Benchmark]
        public int B52a_Handcoded() =>
            _index < _listA.Count
                ? _listA[_index]
                : _listB[_index - _listA.Count];

        [Benchmark]
        public int B52b_Handcoded()
        {
            var position = 0;
            foreach (var x in Data.Iterator(_a))
            {
                if (position == _index)
                    return x;
                ++position;
            }
            return _listB[_index - position];
        }

        [Benchmark]
        public int B53a_Handcoded() =>
            _listA[0];

        [Benchmark]
        public int B53b_Handcoded() =>
            _listB[_listB.Count - 1];

        [Benchmark]
        public int B53c_Handcoded() =>
            _listB[_listB.Count - 1];

        [Benchmark]
        public bool B54a_Handcoded()
        {
            foreach (var x in _a)
                if (x == _presentB)
                    return true;
            foreach (var x in _b)
                if (x == _presentB)
                    return true;
            return false;
        }

        [Benchmark]
        public bool B54b_Handcoded() =>
            _setA.Contains(_presentB) || _setB.Contains(_presentB);

        [Benchmark]
        public bool B54c_Handcoded()
        {
            foreach (var x in _a)
                if (x == _presentC)
                    return true;
            foreach (var x in _b)
                if (x == _presentC)
                    return true;
            foreach (var x in _c)
                if (x == _presentC)
                    return true;
            return false;
        }

        [Benchmark]
        public List<int> B55a_Handcoded()
        {
            var result = new List<int>(_listA.Count + _listB.Count);
            result.AddRange(_listA);
            result.AddRange(_listB);
            return result;
        }

        [Benchmark]
        public List<int> B55b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _a)
                result.Add(x);
            foreach (var x in Data.Iterator(_b))
                result.Add(x);
            return result;
        }
    }
}
