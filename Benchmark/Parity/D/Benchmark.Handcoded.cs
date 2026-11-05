using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Parity
{
    partial class D
    {
        [Benchmark]
        public int D01a_Handcoded()
        {
            var sum = 0;
            foreach (var x in _ints)
                sum += x;
            return sum;
        }

        [Benchmark]
        public int D01b_Handcoded()
        {
            var sum = 0;
            foreach (var x in _intList)
                sum += x;
            return sum;
        }

        [Benchmark]
        public long D01c_Handcoded()
        {
            var sum = 0L;
            foreach (var x in _longs)
                sum += x;
            return sum;
        }

        [Benchmark]
        public long D01d_Handcoded()
        {
            var sum = 0L;
            foreach (var x in _longList)
                sum += x;
            return sum;
        }

        [Benchmark]
        public float D02a_Handcoded()
        {
            var sum = 0f;
            foreach (var x in _floats)
                sum += x;
            return sum;
        }

        [Benchmark]
        public double D02b_Handcoded()
        {
            var sum = 0d;
            foreach (var x in _doubleList)
                sum += x;
            return sum;
        }

        [Benchmark]
        public decimal D02c_Handcoded()
        {
            var sum = 0m;
            foreach (var x in _decimals)
                sum += x;
            return sum;
        }

        [Benchmark]
        public double D03a_Handcoded()
        {
            if (_ints.Length == 0)
                throw new InvalidOperationException();
            var sum = 0L;
            foreach (var x in _ints)
                sum += x;
            return (double)sum / _ints.Length;
        }

        [Benchmark]
        public double D03b_Handcoded()
        {
            if (_intList.Count == 0)
                throw new InvalidOperationException();
            var sum = 0L;
            foreach (var x in _intList)
                sum += x;
            return (double)sum / _intList.Count;
        }

        [Benchmark]
        public double D04a_Handcoded()
        {
            if (_longList.Count == 0)
                throw new InvalidOperationException();
            var sum = 0L;
            foreach (var x in _longList)
                sum += x;
            return (double)sum / _longList.Count;
        }

        [Benchmark]
        public float D04b_Handcoded()
        {
            if (_floats.Length == 0)
                throw new InvalidOperationException();
            var sum = 0d;
            foreach (var x in _floats)
                sum += x;
            return (float)(sum / _floats.Length);
        }

        [Benchmark]
        public double D04c_Handcoded()
        {
            if (_doubleList.Count == 0)
                throw new InvalidOperationException();
            var sum = 0d;
            foreach (var x in _doubleList)
                sum += x;
            return sum / _doubleList.Count;
        }

        [Benchmark]
        public decimal D04d_Handcoded()
        {
            if (_decimals.Length == 0)
                throw new InvalidOperationException();
            var sum = 0m;
            foreach (var x in _decimals)
                sum += x;
            return sum / _decimals.Length;
        }

        [Benchmark]
        public float D05a_Handcoded()
        {
            if (_floats.Length == 0)
                throw new InvalidOperationException();
            var min = _floats[0];
            foreach (var x in _floats)
                if (x < min)
                    min = x;
            return min;
        }

        [Benchmark]
        public double D05b_Handcoded()
        {
            if (_doubleList.Count == 0)
                throw new InvalidOperationException();
            var max = _doubleList[0];
            foreach (var x in _doubleList)
                if (x > max)
                    max = x;
            return max;
        }

        [Benchmark]
        public decimal D06a_Handcoded()
        {
            if (_decimals.Length == 0)
                throw new InvalidOperationException();
            var min = _decimals[0];
            foreach (var x in _decimals)
                if (x < min)
                    min = x;
            return min;
        }

        [Benchmark]
        public decimal D06b_Handcoded()
        {
            if (_decimalList.Count == 0)
                throw new InvalidOperationException();
            var max = _decimalList[0];
            foreach (var x in _decimalList)
                if (x > max)
                    max = x;
            return max;
        }

        [Benchmark]
        public int D07a_Handcoded()
        {
            if (_ints.Length == 0)
                throw new InvalidOperationException();
            var min = _ints[0];
            foreach (var x in _ints)
                if (x < min)
                    min = x;
            return min;
        }

        [Benchmark]
        public long D07b_Handcoded()
        {
            if (_longList.Count == 0)
                throw new InvalidOperationException();
            var max = _longList[0];
            foreach (var x in _longList)
                if (x > max)
                    max = x;
            return max;
        }

        [Benchmark]
        public short D07c_Handcoded()
        {
            if (_shorts.Length == 0)
                throw new InvalidOperationException();
            var min = _shorts[0];
            foreach (var x in _shorts)
                if (x < min)
                    min = x;
            return min;
        }

        [Benchmark]
        public int D07d_Handcoded()
        {
            if (_intList.Count == 0)
                throw new InvalidOperationException();
            var max = _intList[0];
            foreach (var x in _intList)
                if (x > max)
                    max = x;
            return max;
        }

        [Benchmark]
        public int D08a_Handcoded()
        {
            if (_ints.Length == 0)
                throw new InvalidOperationException();
            var accumulator = _ints[0];
            for (var i = 1; i < _ints.Length; ++i)
                accumulator = _add(accumulator, _ints[i]);
            return accumulator;
        }

        [Benchmark]
        public long D08b_Handcoded()
        {
            var accumulator = 0L;
            foreach (var x in _intList)
                accumulator = _addLong(accumulator, x);
            return accumulator;
        }

        [Benchmark]
        public int D08c_Handcoded()
        {
            var accumulator = 0L;
            foreach (var x in _ints)
                accumulator = _addLong(accumulator, x);
            return _half(accumulator);
        }

        [Benchmark]
        public bool D08d_Handcoded()
        {
            foreach (var x in _found)
                if (_isLast(x))
                    return true;
            return false;
        }

        [Benchmark]
        public bool D08e_Handcoded()
        {
            foreach (var x in _foundList)
                if (!_belowRange(x))
                    return false;
            return true;
        }

        [Benchmark]
        public int D08f_Handcoded()
        {
            var count = 0;
            foreach (var x in _ints)
                if (_predicate(x))
                    ++count;
            return count;
        }

        [Benchmark]
        public int D08g_Handcoded()
        {
            foreach (var x in _foundList)
                if (_isLast(x))
                    return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int D08h_Handcoded()
        {
            foreach (var x in _found)
                if (_isLast(x))
                    return x;
            return default;
        }

        [Benchmark]
        public int D08i_Handcoded()
        {
            var found = false;
            var result = 0;
            foreach (var x in _foundList)
            {
                if (!_isLast(x))
                    continue;
                if (found)
                    throw new InvalidOperationException();
                found = true;
                result = x;
            }
            if (!found)
                throw new InvalidOperationException();
            return result;
        }

        [Benchmark]
        public int D08j_Handcoded()
        {
            if (_intList.Count == 0)
                throw new InvalidOperationException();
            var accumulator = _intList[0];
            for (var i = 1; i < _intList.Count; ++i)
                accumulator = _add(accumulator, _intList[i]);
            return accumulator;
        }

        [Benchmark]
        public long D08k_Handcoded()
        {
            var accumulator = 0L;
            foreach (var x in _ints)
                accumulator = _addLong(accumulator, x);
            return accumulator;
        }

        [Benchmark]
        public int D08l_Handcoded()
        {
            var accumulator = 0L;
            foreach (var x in _intList)
                accumulator = _addLong(accumulator, x);
            return _half(accumulator);
        }

        [Benchmark]
        public bool D08m_Handcoded()
        {
            foreach (var x in _foundList)
                if (_isLast(x))
                    return true;
            return false;
        }

        [Benchmark]
        public bool D08n_Handcoded()
        {
            foreach (var x in _found)
                if (!_belowRange(x))
                    return false;
            return true;
        }

        [Benchmark]
        public int D08o_Handcoded()
        {
            var count = 0;
            foreach (var x in _intList)
                if (_predicate(x))
                    ++count;
            return count;
        }

        [Benchmark]
        public int D08p_Handcoded()
        {
            foreach (var x in _found)
                if (_isLast(x))
                    return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int D08q_Handcoded()
        {
            foreach (var x in _foundList)
                if (_isLast(x))
                    return x;
            return default;
        }

        [Benchmark]
        public int D08r_Handcoded()
        {
            var found = false;
            var result = 0;
            foreach (var x in _found)
            {
                if (!_isLast(x))
                    continue;
                if (found)
                    throw new InvalidOperationException();
                found = true;
                result = x;
            }
            if (!found)
                throw new InvalidOperationException();
            return result;
        }

        [Benchmark]
        public int D11a_Handcoded()
        {
            var count = 0;
            foreach (var x in _intList)
                if (_predicate(x))
                    ++count;
            return count;
        }

        [Benchmark]
        public int[] D11b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _intList)
                if (_predicate(x))
                    result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> D11c_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _intList)
                if (_predicate(x))
                    result.Add(x);
            return result;
        }

        [Benchmark]
        public int D11d_Handcoded()
        {
            foreach (var x in _intList)
                if (_predicate(x))
                    return x;
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int D11e_Handcoded()
        {
            var found = false;
            var last = 0;
            foreach (var x in _intList)
            {
                if (!_predicate(x))
                    continue;
                found = true;
                last = x;
            }
            if (!found)
                throw new InvalidOperationException();
            return last;
        }

        [Benchmark]
        public int D11f_Handcoded()
        {
            var index = 0;
            foreach (var x in _intList)
            {
                if (!_predicate(x))
                    continue;
                if (index == 2000)
                    return x;
                ++index;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public bool D11g_Handcoded()
        {
            foreach (var x in _intList)
                if (_predicate(x) && x == Data.Range - 1)
                    return true;
            return false;
        }

        [Benchmark]
        public int D11h_Handcoded()
        {
            var count = 0;
            foreach (var x in _intList)
                if (_predicate(x))
                {
                    _selector(x);
                    ++count;
                }
            return count;
        }

        [Benchmark]
        public int[] D11i_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _intList)
                if (_predicate(x))
                    result.Add(_selector(x));
            return result.ToArray();
        }

        [Benchmark]
        public List<int> D11j_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _intList)
                if (_predicate(x))
                    result.Add(_selector(x));
            return result;
        }

        [Benchmark]
        public int D11k_Handcoded()
        {
            foreach (var x in _intList)
                if (_predicate(x))
                    return _selector(x);
            throw new InvalidOperationException();
        }

        [Benchmark]
        public int D11l_Handcoded()
        {
            var found = false;
            var last = 0;
            foreach (var x in _intList)
            {
                if (!_predicate(x))
                    continue;
                found = true;
                last = x;
            }
            if (!found)
                throw new InvalidOperationException();
            return _selector(last);
        }

        [Benchmark]
        public int D11m_Handcoded()
        {
            var index = 0;
            foreach (var x in _intList)
            {
                if (!_predicate(x))
                    continue;
                if (index == 2000)
                    return _selector(x);
                ++index;
            }
            throw new ArgumentOutOfRangeException("index");
        }

        [Benchmark]
        public bool D11n_Handcoded()
        {
            foreach (var x in _intList)
                if (_predicate(x) && _selector(x) == 2)
                    return true;
            return false;
        }

        [Benchmark]
        public int[] D11o_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _sortedList)
                if (_ninety(x))
                    result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public List<int> D11p_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _sortedList)
                if (_ninety(x))
                    result.Add(x);
            return result;
        }

        [Benchmark]
        public int[] D12a_Handcoded()
        {
            var result = new int[_intList.Count];
            for (var i = 0; i < result.Length; ++i)
                result[i] = _selector(_intList[i]);
            return result;
        }

        [Benchmark]
        public List<int> D12b_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _intList)
                result.Add(_selector(x));
            return result;
        }

        [Benchmark]
        public int[] D15a_Handcoded()
        {
            var result = new List<int>();
            foreach (var array in _arrays)
                foreach (var x in array)
                    result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public int[] D15b_Handcoded()
        {
            var result = new List<int>();
            foreach (var list in _lists)
                foreach (var x in list)
                    result.Add(x);
            return result.ToArray();
        }

        [Benchmark]
        public int[] D15c_Handcoded()
        {
            var result = new List<int>();
            foreach (var x in _short)
                result.Add(x);
            foreach (var x in _intList)
                result.Add(x);
            return result.ToArray();
        }
    }
}
