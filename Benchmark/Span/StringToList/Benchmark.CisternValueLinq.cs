using BenchmarkDotNet.Attributes;
using Cistern.ValueLinq;
using System;
using System.Collections.Generic;

namespace Cistern.Benchmarks.Span
{
    partial class StringToList
    {
        [Benchmark]
        public List<char> CisternValueLinq() =>_text.ToList();


        [Benchmark]
        public List<char> CisternValueLinq_ViaSpan() =>
            Enumerable
            .FromSpan(_text, text => text.AsSpan())
            .ToList();
    }
}
